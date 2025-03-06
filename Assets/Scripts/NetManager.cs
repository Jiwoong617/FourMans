using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public partial class NetManager : MonoBehaviour
{
    //Lobby
    private Lobby currentLobby;
    private const int maxPlayers = 4;

    private async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync(); //유니티 서비스 초기화 작업
            Debug.Log("UGS Init");
        }
        catch(System.Exception e)
        {
            Debug.LogError("UGS Init Failed: " + e.Message);
        }

        if (!AuthenticationService.Instance.IsSignedIn) // 서비스 내부에 로그인이 되어있지 않다면
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync(); // 비동기로 유니티 서비스 내부에 로그인
        }

        //StartMatchButton.onClick.AddListener(() => StartMatchmaking());
        //JoinMatchButton.onClick.AddListener(() => JoinGameWithCode(FieldText.text));
    }


    #region match making
    public async void StartMatchmaking()
    {
        //로그인이 되지 않았을 때
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("로그인 되지 않았습니다.");
            return;
        }

        //로그인이 되었을 때
        currentLobby = await FindAvailableLobby(); //로비 찾기


        if (currentLobby == null) //로비가 없으면
        {
            await CreateNewLobby(); //새로운 로비 생성 - host
            Debug.Log(currentLobby.LobbyCode);
        }
        else //로비 존재 시
        {
            await JoinLobbyWithID(currentLobby.Id);
            await JoinRelayServer(GetRelayCodeInLobby());
        }
    }

    public async void MakeGameRoom(string lobbyName)
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("로그인 되지 않았습니다.");
            return;
        }

        await CreateNewLobby(lobbyName, isPrivte:true);
    }

    public async void JoinGameRoom(string lobbyCode)
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("로그인 되지 않았습니다.");
            return;
        }

        await JoinLobbyWithCode(lobbyCode);

        await JoinRelayServer(GetRelayCodeInLobby());
    }


    #endregion

    //호스트와 클라이언트 네트워크 매니저를 통해 전달
    private void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        Debug.Log("호스트 시작");

        //클라이언트가 연결될 때마다 실행됨
        //NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        //NetworkManager.Singleton.OnClientDisconnectCallback += OnHostDisconnected;
    }

    private void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        Debug.Log("클라이언트 시작");
    }

    private void OnClientConnected(ulong clientId)
    {
        //최대 인원이 다 차면 바로 실행시키는 함수
        OnPlayerJoined();
    }

    private void OnHostDisconnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId && NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnHostDisconnected;
        }
    }


    private void OnPlayerJoined()
    {
        if (NetworkManager.Singleton.ConnectedClients.Count >= maxPlayers)
        {
            ChangeSceneForAllPlayers();
        }
    }

    //씬 변경 함수
    private void ChangeSceneForAllPlayers()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            //NetworkManager.Singleton.SceneManager.LoadScene(gameplaySceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
            //싱글로 처리하는 이유 - 서로다른 기계에서 처리하기 때문에 싱글로 처리
        }
    }
}

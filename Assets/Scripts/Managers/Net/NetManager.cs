using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using UnityEngine;

public partial class NetManager : Singleton<NetManager>
{
    //Lobby
    public Lobby currentLobby { get; private set; }

    private string playerID;
    private float heartbeatTimer;
    private float heartbeatTimerMax = 15f;
    private float refreshTimer;
    private float refreshTimerMax = 1f;
    private const int maxPlayers = 4;

    private async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync(); //유니티 서비스 초기화 작업
            if (!AuthenticationService.Instance.IsSignedIn) // 서비스 내부에 로그인이 되어있지 않다면
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync(); // 비동기로 유니티 서비스 내부에 로그인
            }
            playerID = AuthenticationService.Instance.PlayerId;
            Debug.Log("UGS Init");
        }
        catch(System.Exception e)
        {
            Debug.LogError("UGS Init Failed: " + e.Message);
        }



        //StartMatchButton.onClick.AddListener(() => StartMatchmaking());
        //JoinMatchButton.onClick.AddListener(() => JoinGameWithCode(FieldText.text));
    }

    private void Update()
    {
        HandleLobbyHeartbeat();
        RefreshLobby();
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

    public async void LeaveGame()
    {
        try
        {
            //2. 현재 로비 정보 가져오기
            if (currentLobby != null)
            {
                //3. 로비 떠나기
                await LobbyService.Instance.RemovePlayerAsync(currentLobby.Id, AuthenticationService.Instance.PlayerId);
                Debug.Log("로비에서 나감");
            }

            NetworkManager.Singleton.Shutdown(); // 호스트가 shutdown 시 다른 클라이언트들도 shutdown됨
            //로비 생성 후, 4명이 모였을 때, 릴레이 할당, 게임 중 인원수가 가득차지 않으면 로비 연결 해제도 만들어야됨.
            currentLobby = null;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"로비 또는 Relay 종료 실패: {e.Message}");
        }
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

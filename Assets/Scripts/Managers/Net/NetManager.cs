using System.Collections;
using System.Threading.Tasks;
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
    private const int maxPlayers = 4;

    private string playerID;
    private float heartbeatTimer;
    private float heartbeatTimerMax = 15f;
    private bool isHeartbeating = false;
    private float refreshTimer;
    private float refreshTimerMax = 1f;
    private bool isRefreshing = false;

    private bool isRelayExist = false;
    private Coroutine matchmakingCo = null;

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
        if (currentLobby != null || matchmakingCo != null) return;

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
            //await JoinRelayServer(GetRelayCodeInLobby());
        }

        matchmakingCo = StartCoroutine(CheckToRelayStart());
    }

    public async void MakeGameRoom(string lobbyName)
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("로그인 되지 않았습니다.");
            return;
        }

        if (currentLobby != null || matchmakingCo != null)
        {
            return;
        }

        await CreateNewLobby(lobbyName, isPrivte:true);

        matchmakingCo = StartCoroutine(CheckToRelayStart());
    }

    public async void JoinGameRoom(string lobbyCode)
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("로그인 되지 않았습니다.");
            return;
        }

        if (currentLobby != null || matchmakingCo != null)
        {
            return;
        }

        await JoinLobbyWithCode(lobbyCode);
        //await JoinRelayServer(GetRelayCodeInLobby());
        matchmakingCo = StartCoroutine(CheckToRelayStart());
    }

    //로비에서 매치메이킹 중 나갔을 때
    public void LeaveGame()
    {
        NetworkManager.Singleton.Shutdown();
        isRelayExist = false;
            
        StopMatchMaking();
    }

    private async void StopMatchMaking()
    {
        await LeaveLobby();

        if (matchmakingCo != null)
        {
            StopCoroutine(matchmakingCo);
            matchmakingCo = null;
        }

        //혹시 모를 이벤트 콜백 제거
        NetworkManager.Singleton.OnClientStopped -= OnClientStopped;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;

        isRelayExist = false;
    }
    #endregion


    private IEnumerator CheckToRelayStart()
    {
        while(!isRelayExist)
        {
            if (currentLobby == null) yield break;

            yield return new WaitForSeconds(1f);
            if(currentLobby.Players.Count == maxPlayers)
            {
                Task<bool> task;
                if (currentLobby.HostId == playerID)
                    task = CreateRelayServer(currentLobby);
                else
                    task = JoinRelayServer(GetRelayCodeInLobby());

                yield return new WaitUntil(() => task.IsCompleted);
                isRelayExist = task.Result;
            }
        }

        new WaitUntil(() => NetworkManager.Singleton.ConnectedClients.Count == maxPlayers);

        matchmakingCo = null;
    }


    //호스트와 클라이언트 네트워크 매니저를 통해 전달
    private void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        Debug.Log("호스트 시작");

        //클라이언트가 연결될 때마다 실행됨
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        Debug.Log("클라이언트 시작");

        NetworkManager.Singleton.OnClientStopped -= OnClientStopped;
        NetworkManager.Singleton.OnClientStopped += OnClientStopped;
    }

    private void OnClientConnected(ulong clientId)
    {
        //최대 인원이 다 차면 바로 실행
        if (NetworkManager.Singleton.ConnectedClients.Count >= maxPlayers)
        {
            ChangeSceneForAllPlayers();
        }
    }

    //누구 한명 게임 중 끊기면 전부 끊기게 설정
    private void OnClientDisconnected(ulong clientId)
    {
        LeaveGame();
    }
    private void OnClientStopped(bool isHost)
    {
        LeaveGame();
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

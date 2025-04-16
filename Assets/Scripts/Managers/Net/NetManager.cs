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
            await UnityServices.InitializeAsync(); //����Ƽ ���� �ʱ�ȭ �۾�
            if (!AuthenticationService.Instance.IsSignedIn) // ���� ���ο� �α����� �Ǿ����� �ʴٸ�
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync(); // �񵿱�� ����Ƽ ���� ���ο� �α���
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
        //�α����� ���� �ʾ��� ��
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("�α��� ���� �ʾҽ��ϴ�.");
            return;
        }
        if (currentLobby != null || matchmakingCo != null) return;

        //�α����� �Ǿ��� ��
        currentLobby = await FindAvailableLobby(); //�κ� ã��


        if (currentLobby == null) //�κ� ������
        {
            await CreateNewLobby(); //���ο� �κ� ���� - host
            Debug.Log(currentLobby.LobbyCode);
        }
        else //�κ� ���� ��
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
            Debug.Log("�α��� ���� �ʾҽ��ϴ�.");
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
            Debug.Log("�α��� ���� �ʾҽ��ϴ�.");
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

    //�κ񿡼� ��ġ����ŷ �� ������ ��
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

        //Ȥ�� �� �̺�Ʈ �ݹ� ����
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


    //ȣ��Ʈ�� Ŭ���̾�Ʈ ��Ʈ��ũ �Ŵ����� ���� ����
    private void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        Debug.Log("ȣ��Ʈ ����");

        //Ŭ���̾�Ʈ�� ����� ������ �����
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        Debug.Log("Ŭ���̾�Ʈ ����");

        NetworkManager.Singleton.OnClientStopped -= OnClientStopped;
        NetworkManager.Singleton.OnClientStopped += OnClientStopped;
    }

    private void OnClientConnected(ulong clientId)
    {
        //�ִ� �ο��� �� ���� �ٷ� ����
        if (NetworkManager.Singleton.ConnectedClients.Count >= maxPlayers)
        {
            ChangeSceneForAllPlayers();
        }
    }

    //���� �Ѹ� ���� �� ����� ���� ����� ����
    private void OnClientDisconnected(ulong clientId)
    {
        LeaveGame();
    }
    private void OnClientStopped(bool isHost)
    {
        LeaveGame();
    }


    //�� ���� �Լ�
    private void ChangeSceneForAllPlayers()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            //NetworkManager.Singleton.SceneManager.LoadScene(gameplaySceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
            //�̱۷� ó���ϴ� ���� - ���δٸ� ��迡�� ó���ϱ� ������ �̱۷� ó��
        }
    }
}

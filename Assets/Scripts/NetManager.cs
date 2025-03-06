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
            await UnityServices.InitializeAsync(); //����Ƽ ���� �ʱ�ȭ �۾�
            Debug.Log("UGS Init");
        }
        catch(System.Exception e)
        {
            Debug.LogError("UGS Init Failed: " + e.Message);
        }

        if (!AuthenticationService.Instance.IsSignedIn) // ���� ���ο� �α����� �Ǿ����� �ʴٸ�
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync(); // �񵿱�� ����Ƽ ���� ���ο� �α���
        }

        //StartMatchButton.onClick.AddListener(() => StartMatchmaking());
        //JoinMatchButton.onClick.AddListener(() => JoinGameWithCode(FieldText.text));
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
            await JoinRelayServer(GetRelayCodeInLobby());
        }
    }

    public async void MakeGameRoom(string lobbyName)
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("�α��� ���� �ʾҽ��ϴ�.");
            return;
        }

        await CreateNewLobby(lobbyName, isPrivte:true);
    }

    public async void JoinGameRoom(string lobbyCode)
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("�α��� ���� �ʾҽ��ϴ�.");
            return;
        }

        await JoinLobbyWithCode(lobbyCode);

        await JoinRelayServer(GetRelayCodeInLobby());
    }


    #endregion

    //ȣ��Ʈ�� Ŭ���̾�Ʈ ��Ʈ��ũ �Ŵ����� ���� ����
    private void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        Debug.Log("ȣ��Ʈ ����");

        //Ŭ���̾�Ʈ�� ����� ������ �����
        //NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        //NetworkManager.Singleton.OnClientDisconnectCallback += OnHostDisconnected;
    }

    private void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        Debug.Log("Ŭ���̾�Ʈ ����");
    }

    private void OnClientConnected(ulong clientId)
    {
        //�ִ� �ο��� �� ���� �ٷ� �����Ű�� �Լ�
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

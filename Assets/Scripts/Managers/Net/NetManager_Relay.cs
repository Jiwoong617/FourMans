using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using Unity.Services.Lobbies;
using System.Collections.Generic;

public partial class NetManager // relay
{

    private async Task<bool> CreateRelayServer(Lobby lobby) //���� ���� �ߴٸ� relay��� ������ ����
    {
        try
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(lobby.MaxPlayers); //������ �κ��� �ִ� �÷��̾�
            // �÷��̾ �� ���� �κ� �������� �ʴ� �κ�ó�� ���̱� ����
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId); //ģ���� �Ҷ��� ���� �����ڵ� ����

            //Debug.Log("Relay ���� �Ҵ� �Ϸ�. Join Code : " + joinCode);

            await LobbyService.Instance.UpdateLobbyAsync(currentLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject> { {"relayCode", new DataObject(DataObject.VisibilityOptions.Member, joinCode) } }
            });

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
                );

            StartHost();
            return true;
        }
        catch (RelayServiceException e)
        {
            Debug.Log("Relay ���� �Ҵ� ���� : " + e);
            return false;
        }
    }

    private async Task<bool> JoinRelayServer(string joinCode)
    {
        if (string.IsNullOrEmpty(joinCode)) //�����ڵ尡 ������
        {
            Debug.Log("��ȿ���� ���� JoinCode �Դϴ�.");
            return false;
        }

        try
        {
            // Relay ������ ����
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            // Relay ���� ������ ���� (NGO�� ����)
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                //unitytransport �� ���� �����͸� joinAllocation�� ����
                //Ŭ���̾�Ʈ�� ȣ��Ʈ�� �����Ϸ��� ȣ��Ʈ�� ���� �ִ� �����Ϳ� ������ �� �� �־����                                                              
                //�� �����͸� ���� �ִ°� unitytransport - �׷��� �� ��⸶�� ���� �ٸ�                                                    
                //�� ���� �濡 ������ unitytransport ���� �����ؾߵ�
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            StartClient(); // Ŭ���̾�Ʈ ����
            Debug.Log("Joined Relay successfully");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to join Relay: " + e.Message);
            return false;
        }
    }

    private string GetRelayCodeInLobby()
    {
        if (currentLobby == null || currentLobby.Data == null) return null;

        if (currentLobby.Data.TryGetValue("relayCode", out var relayJoinCode))
        {
            Debug.Log("successfully get relayjoincode");
            return relayJoinCode.Value;
        }

        Debug.Log("fail to get relayjoincode");
        return null;
    }
}

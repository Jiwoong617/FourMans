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

    private async Task<bool> CreateRelayServer(Lobby lobby) //방을 생성 했다면 relay라는 서버에 저장
    {
        try
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(lobby.MaxPlayers); //생성한 로비의 최대 플레이어
            // 플레이어가 꽉 차면 로비를 존재하지 않는 로비처럼 보이기 위함
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId); //친구와 할때를 위한 조인코드 생성

            //Debug.Log("Relay 서버 할당 완료. Join Code : " + joinCode);

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
            Debug.Log("Relay 서버 할당 실패 : " + e);
            return false;
        }
    }

    private async Task<bool> JoinRelayServer(string joinCode)
    {
        if (string.IsNullOrEmpty(joinCode)) //조인코드가 없으면
        {
            Debug.Log("유효하지 않은 JoinCode 입니다.");
            return false;
        }

        try
        {
            // Relay 서버에 참가
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            // Relay 서버 데이터 설정 (NGO와 연결)
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                //unitytransport 의 서버 데이터를 joinAllocation로 변경
                //클라이언트가 호스트에 접근하려면 호스트가 갖고 있는 데이터에 접근을 할 수 있어야함                                                              
                //그 데이터를 갖고 있는게 unitytransport - 그러나 각 기기마다 값이 다름                                                    
                //즉 같은 방에 들어가려면 unitytransport 값이 동일해야됨
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            StartClient(); // 클라이언트 시작
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

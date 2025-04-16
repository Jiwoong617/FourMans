using Unity.Services.Lobbies;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;

public partial class NetManager //lobby
{
    //로비 생성
    private async Task CreateNewLobby(string lobbyName = "NewLobby", bool isPrivte = false)
    {
        try
        {
            //로비 옵션
            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = isPrivte // 공개 로비
            };

            currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);//ugs 의 로비 생성(로비생성, 최대 플레이어 수)
            Debug.Log("new lobby Created" + currentLobby.Id);
            Debug.Log("new lobby Created CODE : " + currentLobby.LobbyCode);

            await LobbyService.Instance.UpdateLobbyAsync(currentLobby.Id, new UpdateLobbyOptions
            {
                Data = new()
            });

            //로비를 생성했다면 host가 되어야됨
            //await CreateRelayServer(currentLobby); //로비 생성 후 릴레이 서버 내부에 접근
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("Failed to Create Lobby: " + e);
        }
    }

    private async Task JoinLobbyWithID(string lobbyId)
    {
        try
        {
            currentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId); //전달받은 로비 id로 접근
            Debug.Log("Join Lobby" + currentLobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("Failed to Join Lobby: " + e);
        }
    }

    private async Task JoinLobbyWithCode(string lobbyCode)
    {
        try
        {
            currentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode); //전달받은 로비 code로 접근

            Debug.Log("Join Lobby" + currentLobby.LobbyCode);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("Failed to Join Lobby: " + e);
        }
    }

    private async Task LeaveLobby()
    {
        try
        {
            if (currentLobby != null)
            {
                //로비 떠나기
                await LobbyService.Instance.RemovePlayerAsync(currentLobby.Id, playerID);
                Debug.Log("로비에서 나감");

                currentLobby = null;
            }

        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"로비 또는 Relay 종료 실패: {e.Message}");
        }
    }

    //로비 찾기
    private async Task<Lobby> FindAvailableLobby()
    {
        try //예외처리
        {
            var queryResponse = await LobbyService.Instance.QueryLobbiesAsync(); //ugs 안쪽의 쿼리로 저장된 로비 찾기
            if (queryResponse.Results.Count > 0) //로비가 하나라도 있다면
            {
                return queryResponse.Results[0]; //가장 먼저 만들어진 로비 반환
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("로비 찾기 실패" + e);
        }
        return null;
    }

    //로비 연결을 위해서 호스트가 신호 보내기
    private async void HandleLobbyHeartbeat()
    {
        if (currentLobby != null && currentLobby.HostId == playerID && !isHeartbeating)
        {
            heartbeatTimer += Time.deltaTime;
            if (heartbeatTimer > heartbeatTimerMax)
            {
                heartbeatTimer = 0f;
                isHeartbeating = true;
                await LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
                isHeartbeating = false;
            }
        }
    }

    private async void RefreshLobby()
    {
        if(currentLobby != null)
        {
            refreshTimer += Time.deltaTime;
            if(refreshTimer > refreshTimerMax && !isRefreshing)
            {
                refreshTimer = 0f;
                isRefreshing = true;
                currentLobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);
                isRefreshing = false;
            }
        }
    }
}

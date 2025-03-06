using Unity.Services.Lobbies;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Services.Lobbies.Models;
using System;

public partial class NetManager : MonoBehaviour //Lobby
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

            //로비를 생성했다면 host가 되어야됨
            await CreateRelayServer(currentLobby); //로비 생성 후 릴레이 서버 내부에 접근
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
}

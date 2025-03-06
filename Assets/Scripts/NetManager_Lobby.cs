using Unity.Services.Lobbies;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Services.Lobbies.Models;
using System;

public partial class NetManager : MonoBehaviour //Lobby
{
    //�κ� ����
    private async Task CreateNewLobby(string lobbyName = "NewLobby", bool isPrivte = false)
    {
        try
        {
            //�κ� �ɼ�
            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = isPrivte // ���� �κ�
            };

            currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);//ugs �� �κ� ����(�κ����, �ִ� �÷��̾� ��)
            Debug.Log("new lobby Created" + currentLobby.Id);
            Debug.Log("new lobby Created CODE : " + currentLobby.LobbyCode);

            //�κ� �����ߴٸ� host�� �Ǿ�ߵ�
            await CreateRelayServer(currentLobby); //�κ� ���� �� ������ ���� ���ο� ����
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
            currentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId); //���޹��� �κ� id�� ����
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
            currentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode); //���޹��� �κ� code�� ����

            Debug.Log("Join Lobby" + currentLobby.LobbyCode);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("Failed to Join Lobby: " + e);
        }
    }

    //�κ� ã��
    private async Task<Lobby> FindAvailableLobby()
    {
        try //����ó��
        {
            var queryResponse = await LobbyService.Instance.QueryLobbiesAsync(); //ugs ������ ������ ����� �κ� ã��
            if (queryResponse.Results.Count > 0) //�κ� �ϳ��� �ִٸ�
            {
                return queryResponse.Results[0]; //���� ���� ������� �κ� ��ȯ
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("�κ� ã�� ����" + e);
        }
        return null;
    }
}

using System;
using Unity.Netcode;
using UnityEngine;

public class InputManager : NetworkBehaviour
{
    public static NetworkVariable<byte> playerInput = new NetworkVariable<byte>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Update()
    {
        if (NetworkManager.Singleton == null || !IsClient) return;

        CheckPlayerInputServerRpc(Input.GetMouseButton(0), (int)NetworkManager.Singleton.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void CheckPlayerInputServerRpc(bool isPressed, int playerId)
    {
        byte mask = (byte)(1 << (playerId));

        if (isPressed)
            playerInput.Value |= mask;
        else
            playerInput.Value &= (byte)~mask;
    }
}

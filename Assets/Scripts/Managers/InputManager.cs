using System;
using Unity.Netcode;
using UnityEngine;

public class InputManager : NetworkBehaviour
{
    private static NetworkVariable<byte> _playerInput = new NetworkVariable<byte>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private bool preMouseState = false;

    public static byte PlayerInput
    {
        get { return _playerInput.Value; }
    }

    private void Update()
    {
        if (NetworkManager.Singleton == null || !IsClient) return;

        //네트워크 부하 감소를 위해 입력 상태 변경 시에만 호출
        bool currentMouseState = Input.GetMouseButton(0);
        if(preMouseState != currentMouseState)
        {
            CheckPlayerInputServerRpc(currentMouseState, (int)NetworkManager.Singleton.LocalClientId);
            preMouseState = currentMouseState;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CheckPlayerInputServerRpc(bool isPressed, int playerId)
    {
        byte mask = (byte)(1 << (playerId));

        if (isPressed)
            _playerInput.Value |= mask;
        else
            _playerInput.Value &= (byte)~mask;
    }
}

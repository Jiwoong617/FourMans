using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    readonly Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

    Vector2 dir = Vector2.zero;
    int rand;

    private void Start()
    {
        SetRandClientRpc();
    }

    private void Update()
    {
        //서버에서 스폰 후, 이동하도록 변경
        if(IsSpawned && IsServer)
            Move();
    }


    private void Move()
    {
        SetDir();
        transform.Translate(dir * Time.deltaTime);

        MoveClientRpc(transform.position);
    }

    private void SetDir()
    {
        dir = Vector2.zero;
        for (int i = 0; i < 4; i++)
        {
            if((InputManager.PlayerInput & (1 << i)) != 0)
                dir += directions[(i + rand) % 4];
        }
    }

    [ClientRpc]
    private void MoveClientRpc(Vector3 pos)
    {
        transform.position = pos;
    }

    [ClientRpc]
    private void SetRandClientRpc()
    {
        if(IsServer)
            rand = Random.Range(0, NetManager.Instance.currentLobby.MaxPlayers);
    }
}

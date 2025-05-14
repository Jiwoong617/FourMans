using Unity.Netcode;
using UnityEngine;

public class Obstacle : NetworkBehaviour
{
    [SerializeField]
    protected float speed;

    protected Vector2 restartPos;

    public void SetRestartPos(Vector2 pos) => restartPos = pos;

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if(IsServer)
        {
            OnPlayerHitted(collision.GetComponent<PlayerController>());
        }
    }

    private void OnPlayerHitted(PlayerController pc)
    {
        if (pc == null)
            return;

        pc.SetPosClientRpc(restartPos);
    }
}

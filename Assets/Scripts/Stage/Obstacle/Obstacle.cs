using Unity.Netcode;
using UnityEngine;

public class Obstacle : NetworkBehaviour
{
    [SerializeField]
    protected float speed;

    protected Vector2 restartPos;

    public virtual void Init(float speed, params Vector3[] pos) 
    {
        this.speed = speed;
    }

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

    [ClientRpc]
    protected void SyncTransfomClientRpc(Vector3 pos)
    {
        transform.position = pos;
    }
}

using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ProjectileObstacle : Obstacle
{
    [SerializeField] private List<Vector3> movingPos = new();
    Vector3 dir = Vector3.zero;

    private void Update()
    {
        Move();
    }

    //pos[0] - startpos, pos[1] - endpos
    public override void Init(float speed, params Vector3[] pos)
    {
        base.Init(speed);
        foreach (Vector3 p in pos)
            movingPos.Add(p);

        dir = (pos[1] - pos[0]).normalized;
    }

    public override void SetRestartPos(Vector2 pos)
    {
        base.SetRestartPos(pos);
    }

    private void Move()
    {
        if (!IsServer) return;

        Vector3 pos = transform.position + dir * speed * Time.deltaTime;
        if (Vector3.Distance(transform.position, movingPos[1]) < 0.1f)
            pos = movingPos[0];

        SyncTransfomClientRpc(pos);
    }

    [ClientRpc]
    private void SyncRotationClientRpc(float angle) => transform.rotation = Quaternion.Euler(0, 0, angle);
}

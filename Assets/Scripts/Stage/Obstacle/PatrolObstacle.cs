using System.Collections.Generic;
using UnityEngine;

public class PatrolObstacle : Obstacle
{
    [SerializeField] private List<Vector3> patrolPos = new();
    private int idx = 0;

    private void Update()
    {
        Patrol();
    }

    public override void Init(float speed, params Vector3[] pos)
    {
        base.Init(speed);
        foreach(Vector3 p in pos)
            patrolPos.Add(p);
    }

    private void Patrol ()
    {
        if (!IsServer) return;

        if (Vector3.Distance(transform.position, patrolPos[idx]) < 0.1f)
            idx = (idx + 1) % patrolPos.Count;

        Vector3 dir = (patrolPos[idx] - transform.position).normalized;
        Vector3 pos = transform.position + dir * speed * Time.deltaTime;
        SyncTransfomClientRpc(pos);
    }
}

using UnityEngine;

public class RotatingObstacle : Obstacle
{
    Vector3 rotatePoint;

    void Update()
    {
        Rotating();
    }

    public override void Init(float speed, params Vector3[] pos)
    {
        base.Init(speed);
        if (pos.Length > 0)
            rotatePoint = pos[0];
    }

    private void Rotating()
    {
        if (!IsServer) return;
        transform.RotateAround(rotatePoint, Vector3.back, speed * Time.deltaTime);

        SyncTransfomClientRpc(transform.position);
    }
}

using Unity.Netcode;
using UnityEngine;

public class SpinningObstacle : Obstacle
{
    void Update()
    {
        transform.Rotate(Vector3.forward * speed * Time.deltaTime);
    }
}

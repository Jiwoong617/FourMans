using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class Stage : NetworkBehaviour
{
    const string spinningObs = "SpinningObstacle";

    List<Obstacle> obstacles = new List<Obstacle>();
    public int stageNum;

    public override void OnNetworkSpawn()
    {
        Init();
    }

    private void Init()
    {
        //수정 필요
        InstantiateSpinningObstacle();

        SetObstacleRestartPos();
        SpawnObstacle();
    }

    private void InstantiateSpinningObstacle()
    {
        if (!IsServer) return;

        GameObject obstacle = Utils.FindChild(gameObject, spinningObs);
        foreach (Transform t in obstacle.transform)
        {
            //spawn
            SpinningObstacle so = Instantiate(Managers.Resource.LoadSpinningObstacle(), t.position, Quaternion.identity);
            obstacles.Add(so);
        }
    }

    private void SpawnObstacle()
    {
        foreach (Obstacle ob in obstacles)
            ob.GetComponent<NetworkObject>()?.Spawn();
    }

    public void SetObstacleRestartPos()
    {
        foreach (Obstacle o in obstacles)
            o.SetRestartPos(Vector2.zero);
    }

    public void DespawnStage()
    {
        foreach(Obstacle ob in obstacles)
            ob.GetComponent<NetworkObject>().Despawn();

        transform.GetComponent<NetworkObject>().Despawn();
    }
}

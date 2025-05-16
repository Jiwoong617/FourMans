using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Stage : NetworkBehaviour
{
    const string spinningObs = "SpinningObstacle";
    const string patrolObs = "PatrolObstacle";

    List<Obstacle> obstacles = new List<Obstacle>();
    public int stageNum;

    public override void OnNetworkSpawn()
    {
        Init();
    }

    private void Init()
    {
        InstantiateObstacle();

        SetObstacleRestartPos();
        SpawnObstacle();
    }

    private void InstantiateObstacle()
    {
        if (!IsServer) return;
        InstantiateSpinningObstacle();
        InstantiatePatrolObstacle();
    }

    private void InstantiateSpinningObstacle()
    {
        GameObject obstacle = Utils.FindChild(gameObject, spinningObs);
        foreach (Transform t in obstacle.transform)
        {
            //spawn
            SpinningObstacle so = Instantiate(Managers.Resource.LoadSpinningObstacle(), t.position, Quaternion.identity);
            obstacles.Add(so);
        }
    }

    private void InstantiatePatrolObstacle()
    {
        GameObject obstacle = Utils.FindChild(gameObject, patrolObs);
        foreach (Transform t in obstacle.transform)
        {
            //spawn
            PatrolObstacle po = Instantiate(Managers.Resource.LoadPatrolObstacle(), t.position, Quaternion.identity);
            Vector3[] pos = t.GetComponentsInChildren<Transform>().Select(x => x.position).ToArray();
            
            //TODO : obstacle speed
            po.Init(5f, pos);

            obstacles.Add(po);
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

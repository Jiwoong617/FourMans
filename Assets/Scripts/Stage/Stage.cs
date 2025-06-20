using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Stage : NetworkBehaviour
{
    const string spinningObs = "SpinningObstacle";
    const string patrolObs = "PatrolObstacle";
    const string rotatingObs = "RotatingObstacle";
    const string projectileObs = "ProjectileObstacle";

    List<Obstacle> obstacles = new List<Obstacle>();

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
        InstantiateRotatingObstacdle();
        InstantiateProjectileObstacdle();
    }

    private void InstantiateSpinningObstacle()
    {
        GameObject obstacle = Utils.FindChild(gameObject, spinningObs);
        if (obstacle == null) return;

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
        if (obstacle == null) return;

        foreach (Transform t in obstacle.transform)
        {
            //spawn
            PatrolObstacle po = Instantiate(Managers.Resource.LoadPatrolObstacle(), t.GetChild(0).transform.position, Quaternion.identity);
            Vector3[] pos = t.GetComponentsInChildren<Transform>().Where(x => x != t.transform).Select(x => x.position).ToArray();

            float speed;
            if (!float.TryParse(t.name.Split('_')[^1], out speed))
                speed = 3f;

            po.Init(speed, pos);

            obstacles.Add(po);
        }
    }

    private void InstantiateRotatingObstacdle()
    {
        GameObject obstacle = Utils.FindChild(gameObject, rotatingObs);
        if (obstacle == null) return;

        foreach (Transform rotateP in obstacle.transform)
        {
            //spawn
            foreach(Transform roTrans in rotateP)
            {
                RotatingObstacle ro = Instantiate(Managers.Resource.LoadRotatingObstacle(), roTrans.position, Quaternion.identity);
                float speed;
                if (!float.TryParse(rotateP.name.Split('_')[^1], out speed))
                    speed = 60f;

                ro.Init(speed, rotateP.position);

                obstacles.Add(ro);
            }
        }
    }

    private void InstantiateProjectileObstacdle()
    {
        GameObject obstacle = Utils.FindChild(gameObject, projectileObs);
        if (obstacle == null) return;

        foreach (Transform proj in obstacle.transform)
        {
            ProjectileObstacle pro = Instantiate(Managers.Resource.LoadProjectileObstacle(), proj.GetChild(0).transform.position, Quaternion.identity);
            Vector3[] pos = proj.GetComponentsInChildren<Transform>().Where(x => x != proj.transform).Select(x => x.position).ToArray();
            float speed;
            if (!float.TryParse(proj.name.Split('_')[^1], out speed))
                speed = 3f;

            pro.Init(speed, pos);

            obstacles.Add(pro);
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
            ob.GetComponent<NetworkObject>().Despawn(true);

        transform.GetComponent<NetworkObject>().Despawn(true);
    }
}

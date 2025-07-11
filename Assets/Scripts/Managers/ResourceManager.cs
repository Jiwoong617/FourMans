using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager
{
    private SpinningObstacle so = null;
    private PatrolObstacle po = null;
    private RotatingObstacle ro = null;
    private ProjectileObstacle pro = null;

    public SpinningObstacle LoadSpinningObstacle()
    {
        if (so == null)
            return Load<SpinningObstacle>("Prefabs/Obstacle/SpinningObstacle");
        return so;
    }
    
    public PatrolObstacle LoadPatrolObstacle()
    {
        if (po == null)
            return Load<PatrolObstacle>("Prefabs/Obstacle/PatolObstacle");
        return po;
    }

    public RotatingObstacle LoadRotatingObstacle()
    {
        if (ro == null)
            return Load<RotatingObstacle>("Prefabs/Obstacle/RotatingObstacle");
        return ro;
    }
    
    public ProjectileObstacle LoadProjectileObstacle()
    {
        if (pro == null)
            return Load<ProjectileObstacle>("Prefabs/Obstacle/ProjectileObstacle");
        return pro;
    }

    public T Load<T>(string path) where T : Object
    {
        return Resources.Load<T>(path);
    }

    public GameObject Instantiate(string path, Transform parent = null)
    {
        GameObject prefab = Load<GameObject>($"Prefabs/{path}");
        if (prefab == null)
        {
            Debug.Log($"Failed to load prefab : {path}");
            return null;
        }

        GameObject go = Object.Instantiate(prefab, parent);
        int index = go.name.IndexOf("(Clone)");
        if (index > 0)
            go.name = go.name.Substring(0, index);

        return go;
    }

    public void Destroy(GameObject go)
    {
        if (go == null)
            return;

        Object.Destroy(go);
    }
}

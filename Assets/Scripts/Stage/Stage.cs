using Unity.Netcode;
using UnityEngine;

public class Stage : NetworkBehaviour
{
    public int stageNum;
    public Vector2 startPos;

    private void Start()
    {
        
    }

    public void Init()
    {
        InitObstacle();
    }

    private void InitObstacle()
    {
        GameObject obstacle = Utils.FindChild(gameObject, "Obstacle");
        Obstacle[] ob = obstacle.GetComponentsInChildren<Obstacle>();
        foreach(Obstacle o in ob)
            o.SetRestartPos(startPos);
    }

}

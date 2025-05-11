using Unity.Netcode;
using UnityEngine;

public class EndPos : NetworkBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.GetComponent<PlayerController>() != null)
        {
            if(IsServer)
                GameManager.instance.OnStageClear(transform.parent.GetComponent<Stage>().stageNum + 1);
        }
    }
}

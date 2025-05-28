using Unity.Netcode;
using UnityEngine;

public class EndPos : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.GetComponent<PlayerController>() != null)
        {
            GameManager.instance.OnStageClear(transform.root.GetComponent<Stage>().stageNum + 1);
        }
    }
}

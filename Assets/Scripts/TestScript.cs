using Unity.Netcode;
using UnityEngine;

public class TestScript : NetworkBehaviour
{
    Transform t;
    private void Start()
    {
        t = GetComponent<Transform>();
    }

    private void Update()
    {
        //�������� ���� ��, �̵��ϵ��� ����
        if(IsSpawned && IsServer)
            Move();
    }


    private void Move()
    { 
        if ((InputManager.playerInput.Value & (1 << 0)) != 0)
            t.Translate(Vector2.up * Time.deltaTime);
        if ((InputManager.playerInput.Value & (1 << 1)) != 0)
            t.Translate(Vector2.down * Time.deltaTime);
        if ((InputManager.playerInput.Value & (1 << 2)) != 0)
            t.Translate(Vector2.left * Time.deltaTime);
        if ((InputManager.playerInput.Value & (1 << 3)) != 0)
            t.Translate(Vector2.right * Time.deltaTime);

        MoveClientRpc(transform.position);
    }

    [ClientRpc]
    private void MoveClientRpc(Vector3 pos)
    {
        t.position = pos;
    }
}

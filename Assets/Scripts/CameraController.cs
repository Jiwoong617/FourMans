using Unity.Netcode;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float bias = 0.5f;
    private ulong networkPlayerId;
    private Transform player;

    public void SetPlayer(ulong networkPlayerId) => this.networkPlayerId = networkPlayerId;

    private void Update()
    {
        if (player == null)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkPlayerId, out var netObj))
                player = netObj.transform;
        }

        transform.position = Vector3.Lerp(transform.position, new Vector3(player.position.x, player.position.y, transform.position.z), bias);
    }
}

using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI ConnectedPlayerText;

    private void Start()
    {
        StartCoroutine(testco());
    }

    public void QuitLobby()
    {
        NetManager.Instance.LeaveGame();
    }

    IEnumerator testco()
    {
        while(true)
        {
            yield return new WaitForSeconds(1f);
            Debug.Log($"lobby player : {(NetManager.Instance.currentLobby != null ? NetManager.Instance.currentLobby.Players.Count : 0)}");
            Debug.Log($"connected client : {NetworkManager.Singleton.ConnectedClients.Count}");
        }
    }
}

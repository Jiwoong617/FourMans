using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;

    private static Stage[] StageList;
    private Stage currentStage;
    [SerializeField]
    private PlayerController player;

    private void Awake()
    {
        instance = this;
        //���ҽ� �ε�
        StageList = Resources.LoadAll<Stage>("Prefabs/Stage");
        Debug.Log(StageList.Length);
    }

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        if(IsServer)
        {
            //�÷��̾� ����
            player = Instantiate(player);
            player.GetComponent<NetworkObject>().Spawn();

            OnStageClear(0);
        }
    }

    public void OnStageClear(int nextStage)
    {
        if (StageList == null || nextStage >= StageList.Length)
            return;

        if(IsServer)
        {
            Debug.Log("StageClear");
            //TODO
            //���� �������� ����
            if (currentStage != null)
            {
                currentStage.GetComponent<NetworkObject>().Despawn();
                Destroy(currentStage);
            }
            //���� �������� ����
            currentStage = Instantiate(StageList[nextStage]);
            currentStage.GetComponent<NetworkObject>().Spawn();
            //ĳ���� ���� �������� startpos�� �̵�
        }
    }
}

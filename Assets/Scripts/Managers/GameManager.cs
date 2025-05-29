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
    private int stageNum = 0;
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
            NetworkObject obj = player.GetComponent<NetworkObject>();
            obj.Spawn();

            OnStageClear();

            InitCameraClientRpc(obj.NetworkObjectId);
        }
    }

    [ClientRpc]
    private void InitCameraClientRpc(ulong playerId)
    {
        Utils.GetOrAddComponent<CameraController>(Camera.main.gameObject).SetPlayer(playerId);
    }

    public void OnStageClear()
    {
        if (StageList == null || stageNum >= StageList.Length)
            return;

        if(IsServer)
        {
            Debug.Log("StageClear");
            //startpos�� 0,0���� ����
            //ĳ���� ���� �������� startpos�� �̵�
            player.SetPosClientRpc(Vector2.zero);

            //���� �������� ����
            if (currentStage != null)
            {
                currentStage.DespawnStage();
            }
            //���� �������� ����
            currentStage = Instantiate(StageList[stageNum]);
            currentStage.GetComponent<NetworkObject>().Spawn();

        }

        stageNum++;
    }
}

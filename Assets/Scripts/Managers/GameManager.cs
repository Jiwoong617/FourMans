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
        //리소스 로드
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
            //플레이어 생성
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
            //startpos는 0,0으로 고정
            //캐릭터 다음 스테이지 startpos로 이동
            player.SetPosClientRpc(Vector2.zero);

            //현재 스테이지 삭제
            if (currentStage != null)
            {
                currentStage.DespawnStage();
            }
            //다음 스테이지 생성
            currentStage = Instantiate(StageList[stageNum]);
            currentStage.GetComponent<NetworkObject>().Spawn();

        }

        stageNum++;
    }
}

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
            //현재 스테이지 삭제
            if (currentStage != null)
            {
                currentStage.GetComponent<NetworkObject>().Despawn();
                Destroy(currentStage);
            }
            //다음 스테이지 생성
            currentStage = Instantiate(StageList[nextStage]);
            currentStage.GetComponent<NetworkObject>().Spawn();
            //캐릭터 다음 스테이지 startpos로 이동
        }
    }
}

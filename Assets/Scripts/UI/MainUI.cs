using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class MainUI : UIBase
{
    enum Buttons
    {
        RandomMatchBtn,
        MakeRoomBtn,
        JoinMatchBtn,
        CancleMatchBtn,
    }
    enum Texts
    {
        JoinCodeText,
        MatchingText,
        PlayerCountText,
    }
    enum Images
    {
        MatchingPanel,  
    }
    enum InputFields
    { 
        JoinCodeInputField,
    }

    private bool isMatching = false;
    private TextMeshProUGUI joinCode;
    private TextMeshProUGUI matchingText;
    private TextMeshProUGUI playerCountText;
    private Image matchingPanel;

    private Sequence matchingTextSeq;
    private Coroutine checkPlayerCountCo;

    private void Start()
    {
        Init();
        //StartCoroutine(testco());
    }

    private void Update()
    {

    }

    public override void Init()
    {
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));
        Bind<TMP_InputField>(typeof(InputFields));

        GetButton((int)Buttons.RandomMatchBtn).gameObject.BindEvent(RandomMatchBtn);
        GetButton((int)Buttons.MakeRoomBtn).gameObject.BindEvent(MakeRoomBtn);
        GetButton((int)Buttons.JoinMatchBtn).gameObject.BindEvent(JoinMatchBtn);
        GetButton((int)Buttons.CancleMatchBtn).gameObject.BindEvent(CancleMatchBtn);

        joinCode = GetText((int)Texts.JoinCodeText);
        matchingText = GetText((int)Texts.MatchingText);
        playerCountText = GetText((int)Texts.PlayerCountText);

        matchingPanel = GetImage((int)Images.MatchingPanel);
    }



    #region Buttons
    private void RandomMatchBtn(PointerEventData data)
    {
        if (isMatching) return;
        isMatching = true;

        NetManager.Instance.StartMatchmaking();
        ActiveMatchingPanel();
    }

    private async void MakeRoomBtn(PointerEventData data)
    {
        if (isMatching) return;

        bool isCreated = await NetManager.Instance.MakeGameRoom("GameRoom");
        if(isCreated)
        {
            joinCode.text = $"JoinCode : {NetManager.Instance.currentLobby.LobbyCode}";
            isMatching = true;
            ActiveMatchingPanel();
        }
        else
        {
        }
    }

    private void JoinMatchBtn(PointerEventData data)
    {
        if (isMatching) return;
        isMatching = true;

        TMP_InputField input = Get<TMP_InputField>((int)InputFields.JoinCodeInputField);
        if (input != null)
        {
            NetManager.Instance.JoinGameRoom(input.text);
            ActiveMatchingPanel();
        }
    }

    private void CancleMatchBtn(PointerEventData data)
    {
        if (matchingTextSeq != null)
            matchingTextSeq.Kill();
        if (checkPlayerCountCo != null)
            StopCoroutine(checkPlayerCountCo);

        isMatching = false;
        NetManager.Instance.LeaveGame();
        matchingPanel.gameObject.SetActive(false);

    }

    private void ActiveMatchingPanel()
    {
        matchingPanel.gameObject.SetActive(true);
        MatchingText();
        checkPlayerCountCo = StartCoroutine(CheckPlayerCountCo());
    }
    #endregion

    #region Texts
    private void MatchingText()
    {
        if (matchingTextSeq != null)
            matchingTextSeq.Kill();

        matchingTextSeq = DOTween.Sequence()
            .AppendCallback(() => matchingText.text = "Matching.")
            .AppendInterval(0.5f)
            .AppendCallback(() => matchingText.text = "Matching..")
            .AppendInterval(0.5f)
            .AppendCallback(() => matchingText.text = "Matching...")
            .AppendInterval(0.5f)
            .SetLoops(-1);
    }

    IEnumerator CheckPlayerCountCo()
    {
        if (checkPlayerCountCo != null)
            yield break;

        while (true)
        {
            yield return new WaitForSeconds(1f);

            playerCountText.text = $"( {(NetManager.Instance.currentLobby != null ? NetManager.Instance.currentLobby.Players.Count : 0)} / 4 )";
            //Debug.Log($"connected client : {NetworkManager.Singleton.ConnectedClients.Count}");
        }
    }
    #endregion
}

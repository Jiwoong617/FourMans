using System.Collections;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainUI : UIBase
{
    enum Buttons
    {
        RandomMatchBtn,
        MakeRoomBtn,
        JoinMatchBtn,
        QuitMatchBtn,
    }
    enum Texts
    {
        RandomMatchText,
        MakeRoomText,
        JoinMatchText,
        JoinCodeText,
    }
    enum InputFields
    { 
        JoinCodeInputField,
    }

    private bool isMakingGameroom = false;
    private TextMeshProUGUI joinCode;

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
        Bind<TMP_InputField>(typeof(InputFields));

        GetButton((int)Buttons.RandomMatchBtn).gameObject.BindEvent(RandomMatchBtn);
        GetButton((int)Buttons.MakeRoomBtn).gameObject.BindEvent(MakeRoomBtn);
        GetButton((int)Buttons.JoinMatchBtn).gameObject.BindEvent(JoinMatchBtn);
        GetButton((int)Buttons.QuitMatchBtn).gameObject.BindEvent(QuitMatchBtn);

        joinCode = GetText((int)Texts.JoinCodeText);
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

    private void RandomMatchBtn(PointerEventData data)
    {
        NetManager.Instance.StartMatchmaking();
    }

    private async void MakeRoomBtn(PointerEventData data)
    {
        if (isMakingGameroom) return;
        isMakingGameroom = true;

        bool isCreted = await NetManager.Instance.MakeGameRoom("GameRoom");
        if(isCreted)
        {
            joinCode.text = $"JoinCode : {NetManager.Instance.currentLobby.LobbyCode}";
            isMakingGameroom = false;
        }
        else
        {

        }
    }

    private void JoinMatchBtn(PointerEventData data)
    {
        TMP_InputField input = Get<TMP_InputField>((int)InputFields.JoinCodeInputField);
        if (input != null)
        {
            NetManager.Instance.JoinGameRoom(input.text);
        }
    }

    private void QuitMatchBtn(PointerEventData data)
    {
    }
}

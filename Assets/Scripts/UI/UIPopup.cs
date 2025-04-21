using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UIPopup : UIBase
{
    public override void Init()
    {
        //Managers.UI.SetCanvas(gameObject, true);
    }

    public virtual void ClosePopupUI()
    {
        //Managers.UI.ClosePopupUI(this);
    }
}

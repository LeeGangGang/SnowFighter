using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DlgBoxCtrl : MonoBehaviour
{
    public Text titleTxt;
    public Text messageTxt;

    public Button OkBtn;
    public Button CancelBtn;

    public delegate void OK_Act();
    OK_Act OK_Click;

    // Start is called before the first frame update
    void Start()
    {
        if (!ReferenceEquals(CancelBtn, null))
            CancelBtn.onClick.AddListener(CancelBtnClick);

        if (!ReferenceEquals(OkBtn, null))
        {
            if (!ReferenceEquals(OK_Click, null))
                OkBtn.onClick.AddListener(OkBtnClick);
            else
                OkBtn.gameObject.SetActive(false);
        }
    }

    void OkBtnClick()
    {
        OK_Click();
        this.gameObject.SetActive(false);
    }

    void CancelBtnClick()
    {
        this.gameObject.SetActive(false);
    }

    public void InitData(string a_title, string a_Mess, OK_Act a_OKClick = null)
    {
        titleTxt.text = a_title;
        messageTxt.text = a_Mess;

        OK_Click = a_OKClick;
    }
}

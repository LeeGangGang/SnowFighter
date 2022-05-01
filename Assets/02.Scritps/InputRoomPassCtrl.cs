using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputRoomPassCtrl : MonoBehaviour
{
    private string m_RoomName = "";
    private string m_RoomPass = "";

    public Text m_RoomName_Txt;
    public InputField m_RoomPW_If;

    public Button m_OK_Btn;
    public Button m_Cancel_Btn;

    // Start is called before the first frame update
    void Start()
    {
        if (!ReferenceEquals(m_RoomName_Txt, null))
            m_RoomName_Txt.text = m_RoomName;
        
        if (!ReferenceEquals(m_OK_Btn, null))
            m_OK_Btn.onClick.AddListener(JoinRoom_Click);

        if (!ReferenceEquals(m_Cancel_Btn, null))
            m_Cancel_Btn.onClick.AddListener(Cancel_Click);
    }

    public void InitData(string RoomName, string RoomPass)
    {
        m_RoomName = RoomName;
        m_RoomPass = RoomPass;
    }

    public void JoinRoom_Click()
    {
        SoundManager.Instance.PlayUISound("Button");

        if (m_RoomPW_If.text.Equals(m_RoomPass))
        {
            string a_RoomName = string.Format($"{m_RoomPass}_{m_RoomName}");
            PhotonNetwork.LocalPlayer.NickName = GlobalValue.nickName;
            PhotonNetwork.JoinRoom(a_RoomName);
            Destroy(this.gameObject);
        }
        else
        {
            m_RoomPW_If.text = "";
            m_RoomPW_If.placeholder.GetComponent<Text>().text = "비밀번호가 틀렸습니다.";
        }
    }

    private void Cancel_Click()
    {
        SoundManager.Instance.PlayUISound("Button");

        Destroy(this.gameObject);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomData : MonoBehaviour
{
    [HideInInspector] public string m_RoomName = string.Empty;
    [HideInInspector] public string m_RoomPass = string.Empty;
    [HideInInspector] public int m_ConnectPlayer = 0;
    [HideInInspector] public int m_MaxPlayer = 0;

    // 룸 이름 표시할 Text UI
    public Text m_RoomNameTxt;
    public Image m_PrivateImg;
    // 룸 접속자 수와 최대 접속자 수를 표시할 Text UI
    public Text m_ConnectInfoTxt;

    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<Button>().onClick.AddListener(() =>
        {
            PhotonInit RefPtInit = FindObjectOfType<PhotonInit>();
            if (!ReferenceEquals(RefPtInit, null))
            {
                RefPtInit.OnClickRoomItem(m_RoomName, m_RoomPass);
            }
        });
    }

    public void DispRoomData(bool a_IsOpen)
    {
        if (a_IsOpen == true)
        {
            m_RoomNameTxt.color = new Color32(0, 0, 0, 255);
            m_ConnectInfoTxt.color = new Color32(0, 0, 0, 255);
        }
        else
        {
            m_RoomNameTxt.color = new Color32(0, 0, 255, 255);
            m_ConnectInfoTxt.color = new Color32(0, 0, 255, 255);
        }

        m_RoomNameTxt.text = m_RoomName;
        if (!string.IsNullOrEmpty(m_RoomPass))
            m_PrivateImg.gameObject.SetActive(true);

        m_ConnectInfoTxt.text = string.Format("({0}/{1})", m_ConnectPlayer, m_MaxPlayer);
    }
}

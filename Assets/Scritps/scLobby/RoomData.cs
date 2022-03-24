using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomData : MonoBehaviour
{
    [HideInInspector] public string roomName = string.Empty;
    [HideInInspector] public int connectPlayer = 0;
    [HideInInspector] public int maxPlayer = 0;

    // 룸 이름 표시할 Text UI
    public Text textRoomName;
    // 룸 접속자 수와 최대 접속자 수를 표시할 Text UI
    public Text textConnectInfo;

    [HideInInspector] public string ReadyState = string.Empty; // 레디 상태 표시

    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<Button>().onClick.AddListener(() =>
        {
            PhotonInit RefPtInit = FindObjectOfType<PhotonInit>();
            if (RefPtInit != null)
                RefPtInit.OnClickRoomItem(roomName);
        });
    }

    public void DispRoomData(bool a_IsOpen)
    {
        if (a_IsOpen == true)
        {
            textRoomName.color = new Color32(0, 0, 0, 255);
            textConnectInfo.color = new Color32(0, 0, 0, 255);
        }
        else
        {
            textRoomName.color = new Color32(0, 0, 255, 255);
            textConnectInfo.color = new Color32(0, 0, 255, 255);
        }

        textRoomName.text = roomName;
        textConnectInfo.text = string.Format("({0}/{1})", connectPlayer, maxPlayer);
    }
}

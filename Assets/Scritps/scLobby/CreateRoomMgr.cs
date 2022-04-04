using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoomMgr : MonoBehaviour
{
    public InputField m_RoomName_If;
    public InputField m_RoomPW_If;

    public Dropdown m_Match_Drop;

    public Button m_OK_Btn;
    public Button m_Cancel_Btn;

    // Start is called before the first frame update
    void Start()
    {
        if (m_OK_Btn != null)
            m_OK_Btn.onClick.AddListener(CreateRoom_Click);

        if (m_Cancel_Btn != null)
            m_Cancel_Btn.onClick.AddListener(Cancel_Click);
    }

    public void CreateRoom_Click()
    {
        string a_roomName = m_RoomName_If.text; // roomName.text;
        string a_playerName = MyPlayerInfo.g_Name;
        Debug.Log("name : " + a_playerName);
        // 물 이름이 없거나 Null일 경우 룸이름 지정
        if (string.IsNullOrEmpty(a_roomName))
        {
            a_roomName = "Room" + Random.Range(0, 999).ToString("000");
        }

        // 로컬 플레이어의 이름을 설정
        PhotonNetwork.LocalPlayer.NickName = a_playerName;
        // 플레이어 이름을 저장
        PlayerPrefs.SetString("USER_ID", a_playerName);

        // 생성할 룸의 조건 설정
        RoomOptions roomOptions = new RoomOptions(); // using Photon.Realtime;
        roomOptions.IsOpen = true;  // 입장 가능 여부
        roomOptions.IsVisible = true;   // 로비에서 룸의 노출 여부
        roomOptions.MaxPlayers = 8; // 룸에 입장할 수 있는 최대 접속자 수

        // 지정한 조건에 맞는 룸 생성 함수
        PhotonNetwork.CreateRoom(a_roomName, roomOptions, TypedLobby.Default);
        // TypedLobby.Default 어느 로비에서 방을 만들껀지?
    }

    private void Cancel_Click()
    {
        Destroy(this.gameObject);
    }
}

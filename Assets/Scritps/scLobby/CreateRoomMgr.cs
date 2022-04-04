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
        // �� �̸��� ���ų� Null�� ��� ���̸� ����
        if (string.IsNullOrEmpty(a_roomName))
        {
            a_roomName = "Room" + Random.Range(0, 999).ToString("000");
        }

        // ���� �÷��̾��� �̸��� ����
        PhotonNetwork.LocalPlayer.NickName = a_playerName;
        // �÷��̾� �̸��� ����
        PlayerPrefs.SetString("USER_ID", a_playerName);

        // ������ ���� ���� ����
        RoomOptions roomOptions = new RoomOptions(); // using Photon.Realtime;
        roomOptions.IsOpen = true;  // ���� ���� ����
        roomOptions.IsVisible = true;   // �κ񿡼� ���� ���� ����
        roomOptions.MaxPlayers = 8; // �뿡 ������ �� �ִ� �ִ� ������ ��

        // ������ ���ǿ� �´� �� ���� �Լ�
        PhotonNetwork.CreateRoom(a_roomName, roomOptions, TypedLobby.Default);
        // TypedLobby.Default ��� �κ񿡼� ���� ���鲫��?
    }

    private void Cancel_Click()
    {
        Destroy(this.gameObject);
    }
}

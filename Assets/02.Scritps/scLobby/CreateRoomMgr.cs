using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoomMgr : MonoBehaviour
{
    public InputField m_RoomName_If;
    public InputField m_RoomPW_If;

    public Button m_OK_Btn;
    public Button m_Cancel_Btn;

    // Start is called before the first frame update
    void Start()
    {
        if (!ReferenceEquals(m_OK_Btn, null))
            m_OK_Btn.onClick.AddListener(CreateRoom_Click);

        if (!ReferenceEquals(m_Cancel_Btn, null))
            m_Cancel_Btn.onClick.AddListener(Cancel_Click);

        if (!ReferenceEquals(m_RoomName_If, null))
            m_RoomName_If.onValueChanged.AddListener((text) => { m_RoomName_If.text = Regex.Replace(text, @"[_]", ""); });

        if (!ReferenceEquals(m_RoomPW_If, null))
            m_RoomPW_If.onValueChanged.AddListener((text) => { m_RoomPW_If.text = Regex.Replace(text, @"[_]", ""); });
    }

    public void CreateRoom_Click()
    {
        if (string.IsNullOrEmpty(m_RoomName_If.text))
            return;
        
        SoundManager.Instance.PlayUISound("Button");

        string a_roomName = string.Format($"{m_RoomPW_If.text}_{m_RoomName_If.text}");
        string a_playerName = GlobalValue.nickName;

        PhotonNetwork.LocalPlayer.NickName = a_playerName;

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = 8;

        PhotonNetwork.CreateRoom(a_roomName, roomOptions, TypedLobby.Default);
    }

    private void Cancel_Click()
    {
        SoundManager.Instance.PlayUISound("Button");

        Destroy(this.gameObject);
    }
}

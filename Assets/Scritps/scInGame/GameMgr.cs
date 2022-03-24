using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class GameMgr : MonoBehaviourPunCallbacks
{
    // RPC ȣ���� ���� PhotonView
    private PhotonView pv;

    public Text m_SnowCntText;

    void Awake()
    {
        // PhotonView ������Ʈ �Ҵ�
        pv = GetComponent<PhotonView>();

        CreatePlayer();

        //��� Ŭ������ ��Ʈ��ũ �޽��� ������ �ٽ� ����
        PhotonNetwork.IsMessageQueueRunning = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        m_SnowCntText.text = Camera.main.GetComponent<CameraCtrl>().Player.GetComponent<PlayerCtrl>().m_CurSnowCnt.ToString();
    }

    void CreatePlayer()
    {
        float pos = Random.Range(-5.0f, 5.0f);
        Camera.main.GetComponent<CameraCtrl>().Player = PhotonNetwork.Instantiate("Player", new Vector3(pos, 5.0f, pos), Quaternion.identity, 0);
    }
}

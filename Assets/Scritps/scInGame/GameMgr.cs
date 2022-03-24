using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class GameMgr : MonoBehaviourPunCallbacks
{
    // RPC È£­„À» À§ÇÑ PhotonView
    private PhotonView pv;

    public Text m_SnowCntText;

    void Awake()
    {
        // PhotonView ÄÄÆ÷³ÍÆ® ÇÒ´ç
        pv = GetComponent<PhotonView>();

        CreatePlayer();

        //¸ðµç Å¬¶ó¿ìµåÀÇ ³×Æ®¿öÅ© ¸Þ½ÃÁö ¼ö½ÅÀ» ´Ù½Ã ¿¬°á
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

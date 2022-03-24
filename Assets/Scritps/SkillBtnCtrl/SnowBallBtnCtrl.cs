using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SnowBallBtnCtrl : MonoBehaviourPunCallbacks
{
    public GameObject m_SnowBallPrefab;

    private PlayerCtrl m_PlayerCtrl;
    private Transform m_PlayerTr;

    private PhotonView pv = null;


    void Awake()
    {
        m_PlayerCtrl = Camera.main.GetComponent<CameraCtrl>().Player.GetComponent<PlayerCtrl>();
        m_PlayerTr = Camera.main.GetComponent<CameraCtrl>().Player.transform;

        this.GetComponent<Button>().onClick.AddListener(Btn_Click);

        //PhotonView 컴포넌트를 pv 변수에 할당
        pv = GetComponent<PhotonView>();
    }

    private void Btn_Click()
    {
        if (m_PlayerCtrl == null || m_PlayerTr == null || m_SnowBallPrefab == null)
            return;

        if (m_PlayerCtrl.m_CurSnowCnt <= 0)
            return;

        Vector3 SnowPos = m_PlayerTr.position + m_PlayerTr.forward * 1.5f;
        SnowPos.y += 0.2f;
        Quaternion SnowRot = Quaternion.LookRotation(SnowPos - m_PlayerTr.position);

        Shot(SnowPos, SnowRot);
        pv.RPC("Shot", RpcTarget.Others, SnowPos, SnowRot);

        int a_SnowCnt = m_PlayerCtrl.m_CurSnowCnt - 1;
        m_PlayerCtrl.SendSnowCnt(a_SnowCnt);
    }

    [PunRPC]
    void Shot(Vector3 a_Pos, Quaternion a_Rot)
    {
        GameObject a_SnowBall = GameObject.Instantiate(m_SnowBallPrefab, a_Pos, a_Rot);
    }
}

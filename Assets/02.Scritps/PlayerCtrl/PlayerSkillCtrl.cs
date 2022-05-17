using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillCtrl : MonoBehaviour
{
    //PhotonView ÄÄÆ÷³ÍÆ®¸¦ ÇÒ´çÇÒ º¯¼ö
    [HideInInspector] public PhotonView pv = null;

    public GameObject m_SnowBallPrefab;
    public GameObject m_SnowWallPrefab;
    public GameObject m_SnowBowlingPrefab;

    public GameObject m_CatapultPrefab;
    public GameObject m_CatapultAttProjectorPrefab;

    void Awake()
    {
        //PhotonView ÄÄÆ÷³ÍÆ® ÇÒ´ç
        pv = GetComponent<PhotonView>();
    }

    #region --- ´«µ¢ÀÌ ´øÁö±â
    public void Shot(Vector3 a_Pos, Quaternion a_Rot)
    {
        if (m_SnowBallPrefab == null)
            return;

        Debug.Log("Shot");
        ShotRPC(a_Pos, a_Rot);
        pv.RPC("ShotRPC", RpcTarget.Others, a_Pos, a_Rot);
    }

    [PunRPC]
    void ShotRPC(Vector3 a_Pos, Quaternion a_Rot)
    {
        GameObject a_SnowBall = GameObject.Instantiate(m_SnowBallPrefab, a_Pos, a_Rot);
        a_SnowBall.GetComponent<SnowBallCtrl>().SnowData.AttackerId = pv.Owner.ActorNumber;
        if (pv.Owner.CustomProperties.ContainsKey("MyTeam") == true)
            a_SnowBall.GetComponent<SnowBallCtrl>().SnowData.AttackerTeam = (int)pv.Owner.CustomProperties["MyTeam"];
    }
    #endregion

    #region --- ´«º® ¼³Ä¡
    public void CreateSnowWall(Vector3 a_Pos, Quaternion a_Rot)
    {
        if (m_SnowWallPrefab == null)
            return;

        CreateSnowWallRPC(a_Pos, a_Rot);
        pv.RPC("CreateSnowWallRPC", RpcTarget.Others, a_Pos, a_Rot);
    }

    [PunRPC]
    void CreateSnowWallRPC(Vector3 a_Pos, Quaternion a_Rot)
    {
        GameObject a_SnowWall = GameObject.Instantiate(m_SnowWallPrefab, a_Pos, a_Rot);
        a_SnowWall.GetComponent<SnowWallCtrl>().SnowData.AttackerId = pv.Owner.ActorNumber;
        if (pv.Owner.CustomProperties.ContainsKey("MyTeam") == true)
            a_SnowWall.GetComponent<SnowWallCtrl>().SnowData.AttackerTeam = (int)pv.Owner.CustomProperties["MyTeam"];
    }
    #endregion

    #region --- ´«µ¢ÀÌ ±¼¸®±â
    private GameObject m_CurSnowBowling = null;
    public void CreateSnowBowling(Vector3 a_Pos, Quaternion a_Rot)
    {
        if (m_SnowBowlingPrefab == null)
            return;

        CreateSnowBowlingRPC(a_Pos, a_Rot);
        pv.RPC("CreateSnowBowlingRPC", RpcTarget.Others, a_Pos, a_Rot);
    }

    [PunRPC]
    void CreateSnowBowlingRPC(Vector3 a_Pos, Quaternion a_Rot)
    {
        m_CurSnowBowling = GameObject.Instantiate(m_SnowBowlingPrefab, a_Pos, a_Rot);
        m_CurSnowBowling.name = "SnowBowling";
        float a_Radius = m_CurSnowBowling.transform.localScale.x / 2.0f;
        m_CurSnowBowling.transform.SetParent(this.gameObject.transform);
        m_CurSnowBowling.GetComponent<SnowBowlingCtrl>().SnowData.AttackerId = pv.Owner.ActorNumber;
        if (pv.Owner.CustomProperties.ContainsKey("MyTeam") == true)
            m_CurSnowBowling.GetComponent<SnowBowlingCtrl>().SnowData.AttackerTeam = (int)pv.Owner.CustomProperties["MyTeam"];
    }

    public void RollingSnow( Vector3 a_Dir, float a_Speed )
    {
        if(m_SnowBowlingPrefab == null)
            return;

        RollingSnowRPC( a_Dir, a_Speed );
        pv.RPC("RollingSnowRPC", RpcTarget.Others, a_Dir, a_Speed );
    }

    [PunRPC]
    public void RollingSnowRPC( Vector3 a_Dir, float a_Speed)
    {
        if (m_CurSnowBowling != null)
        {
            m_CurSnowBowling.GetComponent<SnowBowlingCtrl>().RollingSnow( a_Dir, a_Speed );
        }
    }
    #endregion

    #region --- ´«»ç¶÷ ¼ÒÈ¯
    public void CreateSnowMan(Vector3 a_Pos, Quaternion a_Rot)
    {
        GameObject a_SnowMan = PhotonNetwork.Instantiate("SkillPrefabs/SnowMan", a_Pos, a_Rot, 0);
        a_SnowMan.GetComponent<SnowManCtrl>().SnowData.AttackerId = pv.Owner.ActorNumber;
        if (pv.Owner.CustomProperties.ContainsKey("MyTeam") == true)
            a_SnowMan.GetComponent<SnowManCtrl>().SnowData.AttackerTeam = (int)pv.Owner.CustomProperties["MyTeam"];
    }
    #endregion

    #region --- Åõ¼®±â ¼³Ä¡
    [HideInInspector] public GameObject m_Catapult;
    [HideInInspector] public GameObject m_Projector;
    public void CreateCatapult()
    {
        if (m_CatapultPrefab == null)
            return;

        CreateCatapultRPC();
        pv.RPC("CreateCatapultRPC", RpcTarget.Others);
    }

    [PunRPC]
    void CreateCatapultRPC()
    {
        m_Catapult = GameObject.Instantiate(m_CatapultPrefab, this.transform);
        m_Catapult.GetComponentInChildren<CatapultSnowBallCtrl>().SnowData.AttackerId = pv.Owner.ActorNumber;
        if (pv.Owner.CustomProperties.ContainsKey("MyTeam") == true)
            m_Catapult.GetComponentInChildren<CatapultSnowBallCtrl>().SnowData.AttackerTeam = (int)pv.Owner.CustomProperties["MyTeam"];

        if (pv.IsMine)
        {
            Vector3 a_AttProPos = m_Catapult.transform.position;
            a_AttProPos.y = 5f;
            m_Projector = PhotonNetwork.Instantiate("SkillPrefabs/AttProjector", a_AttProPos, Quaternion.identity);
        }
    }

    public void DestoryCatapult()
    {
        if (m_Catapult == null)
            return;

        DestoryCatapultRPC();
        pv.RPC("DestoryCatapultRPC", RpcTarget.Others);
    }

    [PunRPC]
    void DestoryCatapultRPC()
    {
        Destroy(m_Catapult);
    }

    public void CatapultShot(Vector3 a_Pos)
    {
        if (m_Catapult == null)
            return;

        CatapultShotRPC(a_Pos);
        pv.RPC("CatapultShotRPC", RpcTarget.Others, a_Pos);
    }

    [PunRPC]
    void CatapultShotRPC(Vector3 a_Pos)
    {
        m_Catapult.GetComponent<CatapultCtrl>().Shot(a_Pos);
    }

    public void DestoryAttProjector()
    {
        if (m_Projector == null)
            return;

        PhotonNetwork.Destroy(m_Projector);
    }
    #endregion
}
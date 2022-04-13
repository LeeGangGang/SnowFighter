using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillCtrl : MonoBehaviour
{
    //PhotonView 컴포넌트를 할당할 변수
    [HideInInspector] public PhotonView pv = null;

    public GameObject m_SnowBallPrefab;
    public GameObject m_SnowWallPrefab;
    public GameObject m_SnowBowlingPrefab;

    void Awake()
    {
        //PhotonView 컴포넌트 할당
        pv = GetComponent<PhotonView>();
    }

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
        GameObject a_SnowBall = GameObject.Instantiate(m_SnowBowlingPrefab, a_Pos, a_Rot);
        a_SnowBall.name = "SnowBowling";
        float a_Radius = a_SnowBall.transform.localScale.x / 2.0f;
        a_SnowBall.transform.SetParent(this.gameObject.transform);
        a_SnowBall.GetComponent<SnowBowlingCtrl>().SnowData.AttackerId = pv.Owner.ActorNumber;
        if (pv.Owner.CustomProperties.ContainsKey("MyTeam") == true)
            a_SnowBall.GetComponent<SnowBowlingCtrl>().SnowData.AttackerTeam = (int)pv.Owner.CustomProperties["MyTeam"];
    }
}

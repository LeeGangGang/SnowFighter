using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class TeamGameMgr : MonoBehaviourPunCallbacks
{
    private PhotonView pv;

    [HideInInspector] public static Vector3[] m_Team1Pos = new Vector3[4];
    [HideInInspector] public static Vector3[] m_Team2Pos = new Vector3[4];

    ExitGames.Client.Photon.Hashtable m_Team1WinProps = new ExitGames.Client.Photon.Hashtable();
    ExitGames.Client.Photon.Hashtable m_Team2WinProps = new ExitGames.Client.Photon.Hashtable();

    void OnLevelWasLoaded()
    {
        int PosIdx = 0;
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("TeamIdx") == true)
            PosIdx = (int)PhotonNetwork.LocalPlayer.CustomProperties["TeamIdx"];

        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("MyTeam") == true)
        {
            if ((int)PhotonNetwork.LocalPlayer.CustomProperties["MyTeam"] == 0)
                GameMgr.Inst.CreatePlayer(m_Team1Pos[PosIdx]);
            else
                GameMgr.Inst.CreatePlayer(m_Team2Pos[PosIdx]);
        }
        else
        {
            GameMgr.Inst.CreatePlayer(new Vector3(0f, 1.5f, 0f));
        }
    }

    void Awake()
    {
        //PhotonView
        pv = GetComponent<PhotonView>();

        // Player Spawn
        GameObject Team1Pos = GameObject.Find("SpawnPos_1Team");
        if (!ReferenceEquals(Team1Pos, null))
        {
            int nIdx = 0;
            for (int i = 0; i < Team1Pos.transform.childCount; i++)
            {
                m_Team1Pos[nIdx] = Team1Pos.transform.GetChild(i).transform.position;
                m_Team1Pos[nIdx].y = 1.5f;
                nIdx++;
            }
        }
        GameObject Team2Pos = GameObject.Find("SpawnPos_2Team");
        if (!ReferenceEquals(Team2Pos, null))
        {
            int nIdx = 0;
            for (int i = 0; i < Team2Pos.transform.childCount; i++)
            {
                m_Team2Pos[nIdx] = Team2Pos.transform.GetChild(i).transform.position;
                m_Team2Pos[nIdx].y = 1.5f;
                nIdx++;
            }
        }
    }
}

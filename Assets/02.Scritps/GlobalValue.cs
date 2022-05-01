using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalValue
{
    public static string userID = "";
    public static string nickName = "";
    public static int win = 0;
    public static int lose = 0;
    public static int kill = 0;

    public static void ClearData()
    {
        userID = "";
        nickName = "";
        win = 0;
        lose = 0;
        kill = 0;
    }
}

public class ConfigValue
{
    public static int UseBgmSound = 1; // 0 : 사용 안함 , 1 : 사용함
    public static int UseEffSound = 1;
    public static float BgmSdVolume = 1f; // 0.0f ~ 1.0f
    public static float EffSdVolume = 1f;
}

public class SnowData
{
    public float m_MaxHp = 100;
    public float m_CurHp = 0;
    public float m_Attck = 0;

    // 공격한한 플레이어의 ID 저장
    public int AttackerId = -1;
    // 공격한 플레이어의 팀
    public int AttackerTeam = -1;
}
using System.Collections;
using System.Collections.Generic;

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

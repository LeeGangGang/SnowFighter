using System.Collections;
using System.Collections.Generic;

public class SnowData
{
    public float m_MaxHp = 100;
    public float m_CurHp = 0;

    public float m_Attck = 0;
}

interface IDamageCtrl
{
    public void Init();

    public void GetDamage(float a_Damaage);
    public float SetDamage();

    public void DestroyThisObj();
}

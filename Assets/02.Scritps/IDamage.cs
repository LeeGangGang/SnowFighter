public interface IDamage
{
    void Init();

    void DestroyThisObj(float tm = 0.0f);

    bool IsMyTeam(int a_Team);

    void GetDamage(float a_Dmg, int a_AttackerId);

    SnowData GetData();
}

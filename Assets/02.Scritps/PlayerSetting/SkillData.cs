using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class SkillData
{
    public enum SkillType
    {
        [Description("눈벽 설치")]
        SnowWall = 0,
        [Description("눈 굴리기")]
        SnowBowling,
        [Description("눈사람 소환")]
        SpawnSnowMan,
        [Description("투석기")]
        Catapult,
    }

    public static string DescriptionAttr<T>(T source)
    {
        System.Reflection.FieldInfo fi = source.GetType().GetField(source.ToString());

        DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

        if (attributes != null && attributes.Length > 0)
            return attributes[0].Description;
        else 
            return source.ToString();
    }

    public static string InfoTxt(SkillType type)
    {
        string str = string.Empty;
        switch (type)
        {
            case SkillType.SnowWall:
                str = string.Format("소모 눈덩이 : {0}\n체력 : {1}", 1, 60);
                break;
            case SkillType.SnowBowling:
                str = string.Format("소모 눈덩이 : {0}\n공격력 : {1}", 1, 45);
                break;
            case SkillType.SpawnSnowMan:
                str = string.Format("소모 눈덩이 : {0}\n체력 : {1}\n공격력 : {2}", 2, 50, 20);
                break;
            case SkillType.Catapult:
                str = string.Format("소모 눈덩이 : {0}\n공격력 : {1}", 2, 40);
                break;
        }
        return str;
    }

    public static string ExplainTxt(SkillType type)
    {
        string str = DescriptionAttr(type) + "\n\n";
        switch (type)
        {
            case SkillType.SnowWall:
                str += "눈 벽을 설치하여 경로를 방해하고 자신을 방어한다.";
                break;
            case SkillType.SnowBowling:
                str += "눈덩이를 굴리며 돌진한다.\n굴리는 동안 이동속도가 점차 상승한다.";
                break;
            case SkillType.SpawnSnowMan:
                str += "눈사람을 소환한다.\n공격 범위안에 들어올 경우 적을 공격한다.";
                break;
            case SkillType.Catapult:
                str += "눈덩이 투석기를 설치한다.\n멀리있는 적을 범위공격한다.";
                break;
        }
        return str;
    }
}

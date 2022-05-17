using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class SkillData
{
    public enum SkillType
    {
        [Description("���� ��ġ")]
        SnowWall = 0,
        [Description("�� ������")]
        SnowBowling,
        [Description("����� ��ȯ")]
        SpawnSnowMan,
        [Description("������")]
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
                str = string.Format("�Ҹ� ������ : {0}\nü�� : {1}", 1, 60);
                break;
            case SkillType.SnowBowling:
                str = string.Format("�Ҹ� ������ : {0}\n���ݷ� : {1}", 1, 45);
                break;
            case SkillType.SpawnSnowMan:
                str = string.Format("�Ҹ� ������ : {0}\nü�� : {1}\n���ݷ� : {2}", 2, 50, 20);
                break;
            case SkillType.Catapult:
                str = string.Format("�Ҹ� ������ : {0}\n���ݷ� : {1}", 2, 40);
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
                str += "�� ���� ��ġ�Ͽ� ��θ� �����ϰ� �ڽ��� ����Ѵ�.";
                break;
            case SkillType.SnowBowling:
                str += "�����̸� ������ �����Ѵ�.\n������ ���� �̵��ӵ��� ���� ����Ѵ�.";
                break;
            case SkillType.SpawnSnowMan:
                str += "������� ��ȯ�Ѵ�.\n���� �����ȿ� ���� ��� ���� �����Ѵ�.";
                break;
            case SkillType.Catapult:
                str += "������ �����⸦ ��ġ�Ѵ�.\n�ָ��ִ� ���� ���������Ѵ�.";
                break;
        }
        return str;
    }
}

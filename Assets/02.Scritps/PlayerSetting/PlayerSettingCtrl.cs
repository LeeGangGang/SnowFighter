using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public enum SkillType
{
    SnowWall = 0,
    SnowBowling,
    SnowMan,
}

public class PlayerSettingCtrl : MonoBehaviour
{
    public GameObject Skill_Item; // Prefab

    public Transform[] Slot = new Transform[2]; // 선택한 스킬 슬롯
    public Transform Content;

    public Button m_HideBtn;
    Vector3 ShowPos_PS = new Vector3(25f, 0f, 0f);
    Vector3 HidePos_PS = new Vector3(510f, 0f, 0f);
    public bool IsShow = false;
    float MvSpeed_PS = 2000f;

    public Button m_SaveBtn;
    public int[] m_SelectSkill;

    private readonly string m_SaveSkillSetUrl = "http://schd153.dothome.co.kr/SnowFighter/SaveSkillSet.php";

    // Start is called before the first frame update
    void Start()
    {
        m_SelectSkill = StrToArr(GlobalValue.StrskillSet);

        foreach (SkillType type in Enum.GetValues(typeof(SkillType)))
        {
            GameObject a_Itme = Instantiate(Skill_Item);
            Texture a_Img = (Texture)Resources.Load("Skill_Img/" + type.ToString());
            a_Itme.transform.Find("Img").GetComponent<RawImage>().texture = a_Img;
            a_Itme.GetComponent<SkillDragHandler>().Type = type;
            if (GlobalValue.skillSet[0] == (int)type)
            {
                a_Itme.transform.SetParent(Slot[0], false);
                Slot[0].GetComponent<SkillSlotCtrl>().CurInObj = a_Itme;
            }
            else if (GlobalValue.skillSet[1] == (int)type)
            {
                a_Itme.transform.SetParent(Slot[1], false);
                Slot[1].GetComponent<SkillSlotCtrl>().CurInObj = a_Itme;
            }
            else
                a_Itme.transform.SetParent(Content, false);
        }

        if (!ReferenceEquals(m_SaveBtn, null))
            m_SaveBtn.onClick.AddListener(SaveBtn_Click);

        if (!ReferenceEquals(m_HideBtn, null))
            m_HideBtn.onClick.AddListener(HideBtn_Click);
    }

    // Update is called once per frame
    void Update()
    {
        PlayerSettingMoveCtrl();
    }

    void SaveBtn_Click()
    {
        m_SelectSkill[0] = Slot[0].childCount == 0 ? -1 : (int)Slot[0].GetComponentInChildren<SkillDragHandler>().Type;
        m_SelectSkill[1] = Slot[1].childCount == 0 ? -1 : (int)Slot[1].GetComponentInChildren<SkillDragHandler>().Type;
        
        StartCoroutine(SaveSkillSetCo());
    }

    void ShowBtn_Click()
    {
        IsShow = true;
    }

    void HideBtn_Click()
    {
        IsShow = false;
    }

    private void PlayerSettingMoveCtrl()
    {
        if (IsShow)
        {
            this.transform.localPosition = Vector3.MoveTowards(this.transform.localPosition, ShowPos_PS, MvSpeed_PS * Time.deltaTime);
        }
        else
        {
            this.transform.localPosition = Vector3.MoveTowards(this.transform.localPosition, HidePos_PS, MvSpeed_PS * Time.deltaTime);
        }
    }

    IEnumerator SaveSkillSetCo()
    {
        WWWForm form = new WWWForm();
        string MyAttSet = ArrToStr(m_SelectSkill);
        form.AddField("Input_id", GlobalValue.userID, System.Text.Encoding.UTF8);
        form.AddField("Input_skillSet", MyAttSet, System.Text.Encoding.UTF8);

        UnityWebRequest a_www = UnityWebRequest.Post(m_SaveSkillSetUrl, form);
        yield return a_www.SendWebRequest();

        if (a_www.error == null)
        {
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            string sReturn = enc.GetString(a_www.downloadHandler.data);
            if (sReturn.Contains("OK_"))
            {
                Array.Copy(m_SelectSkill, GlobalValue.skillSet, GlobalValue.skillSet.Length);
                IsShow = true;
            }
        }
        else
        {
            Debug.Log(a_www.error);
        }
    }

    public string ArrToStr(int[] Skill)
    {
        string Return = string.Empty;
        try
        {
            var jsonAllTower = new JSONObject();
            var json = new JSONArray();
            for (int i = 0; i < Skill.Length; i++)
                json.Add(Skill[i]);

            jsonAllTower.Add("skillSet", json);
            Return = jsonAllTower.ToString();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
        }
        return Return;
    }


    public static int[] StrToArr( string str)
    {
        int[] ArrReturn = new int[2] { -1, -1 };
        try
        {
            if (string.IsNullOrEmpty(str) || str == "")
                return ArrReturn;

            var json = JSON.Parse(str);
            var value = json["skillSet"];
            for (int ii = 0; ii < value.Count; ii++)
                ArrReturn[ii] = value[ii].AsInt;
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
        }
        return ArrReturn;
    }
}

using SimpleJSON;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class TitleMgr : MonoBehaviour
{
    private string m_Msg = "";

    [Header("LoginPanel")]
    public GameObject m_LoginPanel;
    public InputField m_LoginIdIF;
    public InputField m_LoginePwIF;
    public Button m_LoginBtn = null;
    public Button m_OpenCreateUserPanelBtn = null;

    [Header("CreateAccountPanel")]
    public GameObject m_CreateUserPanel;
    public InputField m_CreateIdIF;
    public InputField m_CreatePwIF;
    public InputField m_CreateNickIF;
    public Button m_CreateUserBtn = null;
    public Button m_CancelBtn = null;

    [Header("Normal")]
    public GameObject Popup_Canvas = null;
    public Button m_ConfigBtn;
    public GameObject m_Config_Pop;
    public Text m_MsgTxt;
    private float m_ShowMsgTimer = 0.0f;

    private readonly string m_LoginUrl = "http://schd153.dothome.co.kr/SnowFighter/Login.php";
    private readonly string m_CreateUrl = "http://schd153.dothome.co.kr/SnowFighter/CreateID.php";

    // Start is called before the first frame update
    void Start()
    {
        if (!ReferenceEquals(m_LoginBtn, null))
            m_LoginBtn.onClick.AddListener(LoginBtn_Click);

        if (!ReferenceEquals(m_OpenCreateUserPanelBtn, null))
            m_OpenCreateUserPanelBtn.onClick.AddListener(OpenCreateUserBtn_Click);

        if (!ReferenceEquals(m_CancelBtn, null))
            m_CancelBtn.onClick.AddListener(CreateCancelBtn_Click);

        if (!ReferenceEquals(m_CreateUserBtn, null))
            m_CreateUserBtn.onClick.AddListener(CreateUserBtn_Click);

        if (!ReferenceEquals(m_ConfigBtn, null))
            m_ConfigBtn.onClick.AddListener(ConfigBtn_Click);

        SoundManager.Instance.PlayBGM("MainBgm");
    }

    // Update is called once per frame
    void Update()
    {
        if (0.0f < m_ShowMsgTimer)
        {
            m_ShowMsgTimer -= Time.deltaTime;
            if (m_ShowMsgTimer <= 0.0f)
            {
                MessageOnOff("", false);
            }
        }
    }

    public void LoginBtn_Click()
    {
        SoundManager.Instance.PlayUISound("Button");

        StartCoroutine(LoginCo());
    }

    IEnumerator LoginCo()
    {
        GlobalValue.userID = "";

        WWWForm form = new WWWForm();
        form.AddField("Input_id", m_LoginIdIF.text, System.Text.Encoding.UTF8);
        form.AddField("Input_pw", m_LoginePwIF.text);

        WWW webRequest = new WWW(m_LoginUrl, form);
        yield return webRequest;

        System.Text.Encoding enc = System.Text.Encoding.UTF8;
        string sz = enc.GetString(webRequest.bytes);

        if (sz.Contains("OK_") == false)
        {
            ErrorMsg(sz);
            yield break;
        }

        GlobalValue.userID = m_LoginIdIF.text;

        string a_GetStr = sz.Substring(sz.IndexOf("{\""));

        var N = JSON.Parse(a_GetStr);
        if (N == null)
            yield break;

        if (!ReferenceEquals(N["nickname"], null))
            GlobalValue.nickName = N["nickname"];

        if (!ReferenceEquals(N["winCnt"], null))
            GlobalValue.win = N["winCnt"].AsInt;

        if (!ReferenceEquals(N["loseCnt"], null))
            GlobalValue.lose = N["loseCnt"].AsInt;

        if (!ReferenceEquals(N["killCnt"], null))
            GlobalValue.lose = N["killCnt"].AsInt;

        SceneManager.LoadScene("LobbyScene");
    }

    void ErrorMsg(string a_Str)
    {
        if (a_Str.Contains("ID does not exist.") == true)
            MessageOnOff("ID가 존재하지 않습니다.");
        else if (a_Str.Contains("Pass does not Match.") == true)
            MessageOnOff("패스워드가 일치하지 않습니다.");
        else
            MessageOnOff(a_Str);
    }

    void MessageOnOff(string Mess = "", bool isOn = true)
    {
        if (m_MsgTxt == null)
            return;

        if (isOn == true)
        {
            m_MsgTxt.text = Mess;
            m_MsgTxt.gameObject.SetActive(true);
            m_ShowMsgTimer = 7.0f;
        }
        else
        {
            m_MsgTxt.text = "";
            m_MsgTxt.gameObject.SetActive(false);
        }
    }

    public void OpenCreateUserBtn_Click()
    {
        SoundManager.Instance.PlayUISound("Button");

        if (!ReferenceEquals(m_LoginPanel, null))
            m_LoginPanel.SetActive(false);

        if (!ReferenceEquals(m_CreateUserPanel, null))
            m_CreateUserPanel.SetActive(true);
    }

    public void CreateCancelBtn_Click()
    {
        SoundManager.Instance.PlayUISound("Button");

        if (!ReferenceEquals(m_LoginPanel, null))
            m_LoginPanel.SetActive(true);

        if (!ReferenceEquals(m_CreateUserPanel, null))
            m_CreateUserPanel.SetActive(false);
    }

    public void CreateUserBtn_Click()
    {
        SoundManager.Instance.PlayUISound("Button");

        StartCoroutine(CreateUserCo());
    }

    IEnumerator CreateUserCo()
    {
        string a_IdStr = m_CreateIdIF.text.Trim();
        string a_PwStr = m_CreatePwIF.text.Trim();
        string a_NickStr = m_CreateNickIF.text.Trim();

        if (string.IsNullOrEmpty(a_IdStr) || string.IsNullOrEmpty(a_PwStr) || string.IsNullOrEmpty(a_NickStr))
        {
            MessageOnOff("ID, PW, 별명 빈칸 없이 입력해 주셔야 합니다.");
            yield break;
        }

        if (!(3 <= a_IdStr.Length && a_IdStr.Length < 20))
        {
            MessageOnOff("ID는 3글자 이상 20글자 이하로 작성해 주세요.");
            yield break;
        }

        if (!(4 <= a_PwStr.Length && a_PwStr.Length < 20))
        {
            MessageOnOff("비밀번호는 4글자 이상 20글자 이하로 작성해 주세요.");
            yield break;
        }

        if (!(2 <= a_NickStr.Length && a_NickStr.Length < 20))
        {
            MessageOnOff("별명은 2글자 이상 20글자 이하로 작성해 주세요.");
            yield break;
        }

        WWWForm form = new WWWForm();
        form.AddField("Input_id", m_CreateIdIF.text, System.Text.Encoding.UTF8);
        form.AddField("Input_pw", m_CreatePwIF.text);
        form.AddField("Input_nick", m_CreateNickIF.text, System.Text.Encoding.UTF8);

        WWW webRequest = new WWW(m_CreateUrl, form);
        yield return webRequest;

        if (webRequest.text.Contains("OK_"))
        {
            MessageOnOff("회원가입 성공");
            CreateCancelBtn_Click();
        }
        else
            MessageOnOff(webRequest.text);
    }

    public void ConfigBtn_Click()
    {
        SoundManager.Instance.PlayUISound("Button");

        if (m_Config_Pop == null)
            m_Config_Pop = Resources.Load("Prefabs/ConfigPanel") as GameObject;

        GameObject a_Config_Pop = (GameObject)Instantiate(m_Config_Pop);
        a_Config_Pop.transform.SetParent(Popup_Canvas.transform, false);
    }
}
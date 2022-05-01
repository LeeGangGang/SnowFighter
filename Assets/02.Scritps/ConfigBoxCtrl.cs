using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConfigBoxCtrl : MonoBehaviour
{
    public Button m_OkBtn;
    public Button m_CancelBtn;

    //배경음악
    public Toggle m_BGMOnOffToggle;
    public Slider m_BGMVolSld;
    bool m_IsBgmOn = true;

    //효과음
    public Toggle m_SEOnOffToggle;
    public Slider m_SEVolSld;
    bool m_IsSEOn = true;

    // Start is called before the first frame update
    void Start()
    {
        if (!ReferenceEquals(m_OkBtn, null))
            m_OkBtn.onClick.AddListener(OkBtnClick);

        if (!ReferenceEquals(m_CancelBtn, null))
            m_CancelBtn.onClick.AddListener(CancelBtnClick);

        m_BGMOnOffToggle.isOn = ConfigValue.UseBgmSound == 0;
        if (!ReferenceEquals(m_BGMOnOffToggle, null))
            m_BGMOnOffToggle.onValueChanged.AddListener(BGMOnOff);

        m_BGMVolSld.value = ConfigValue.BgmSdVolume;
        if (!ReferenceEquals(m_BGMVolSld, null))
            m_BGMVolSld.onValueChanged.AddListener(BGMVolChanged);

        m_SEOnOffToggle.isOn = ConfigValue.UseEffSound == 0;
        if (!ReferenceEquals(m_SEOnOffToggle, null))
            m_SEOnOffToggle.onValueChanged.AddListener(SEOnOff);

        m_SEVolSld.value = ConfigValue.EffSdVolume;
        if (!ReferenceEquals(m_SEVolSld, null))
            m_SEVolSld.onValueChanged.AddListener(SEVolChanged);
    }

    void OkBtnClick()
    {
        SoundManager.Instance.PlayUISound("Button");

        ConfigValue.UseBgmSound = m_IsBgmOn ? 1 : 0;
        PlayerPrefs.SetInt("SoundOnOff_Bgm", ConfigValue.UseBgmSound);
        ConfigValue.BgmSdVolume = m_BGMVolSld.value;
        PlayerPrefs.SetFloat("SoundVolume_Bgm", ConfigValue.BgmSdVolume);

        ConfigValue.UseEffSound = m_IsSEOn ? 1 : 0;
        PlayerPrefs.SetInt("SoundOnOff_Eff", ConfigValue.UseEffSound);
        ConfigValue.EffSdVolume = m_SEVolSld.value;
        PlayerPrefs.SetFloat("SoundVolume_Eff", ConfigValue.EffSdVolume);

        Destroy(gameObject);
    }

    void CancelBtnClick()
    {
        SoundManager.Instance.PlayUISound("Button");

        SoundManager.Instance.SoundOnOff_Bgm(ConfigValue.UseBgmSound == 1);
        SoundManager.Instance.SoundVolume_Bgm(ConfigValue.BgmSdVolume);

        SoundManager.Instance.SoundOnOff_Eff(ConfigValue.UseEffSound == 1);
        SoundManager.Instance.SoundVolume_Eff(ConfigValue.EffSdVolume);

        Destroy(gameObject);
    }

    void BGMOnOff(bool isOn)
    {
        //토글 on 상태가 bgm off로 되어야 함...
        m_IsBgmOn = !isOn;
        SoundManager.Instance.SoundOnOff_Bgm(m_IsBgmOn);
    }

    void BGMVolChanged(float volume)
    {
        SoundManager.Instance.SoundVolume_Bgm(volume);
    }

    void SEOnOff(bool isOn)
    {
        //토글 on 상태가 se off로 되어야 함...
        m_IsSEOn = !isOn;
        SoundManager.Instance.SoundOnOff_Eff(m_IsSEOn);
    }

    void SEVolChanged(float volume)
    {
        SoundManager.Instance.SoundVolume_Eff(volume);
    }
}

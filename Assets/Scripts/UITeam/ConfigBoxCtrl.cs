using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConfigBoxCtrl : MonoBehaviour
{
    public Button OkBtn;
    public Button CancelBtn;

    //배경음악
    public Toggle BGMOnOffToggle;
    public Slider BGMVolSld;
    bool isBGMOn = true;

    //효과음
    public Toggle SEOnOffToggle;
    public Slider SEVolSld;
    bool isSEOn = true;

    // Start is called before the first frame update
    void Start()
    {
        if (OkBtn != null)
            OkBtn.onClick.AddListener(OkBtnClick);
        if (CancelBtn != null)
            CancelBtn.onClick.AddListener(CancelBtnClick);

        //bgm이 on 일 때 false이므로
        BGMOnOffToggle.isOn = ConfigValue.UseBgmSound == 0;
        BGMVolSld.value = ConfigValue.BgmSdVolume;
        if (BGMOnOffToggle != null)
            BGMOnOffToggle.onValueChanged.AddListener(BGMOnOff);
        if (BGMVolSld != null)
            BGMVolSld.onValueChanged.AddListener(BGMVolChanged);

        SEOnOffToggle.isOn = ConfigValue.UseEffSound == 0;
        SEVolSld.value = ConfigValue.EffSdVolume;
        if (SEOnOffToggle != null)
            SEOnOffToggle.onValueChanged.AddListener(SEOnOff);
        if (SEVolSld != null)
            SEVolSld.onValueChanged.AddListener(SEVolChanged);
    }

    void OkBtnClick()
    {
        //ok 버튼을 눌렀을 때는 configValue의 static 변수를 변경하고
        //playerprefs에 값을 저장한다.
        ConfigValue.UseBgmSound = isBGMOn ? 1 : 0;
        PlayerPrefs.SetInt("SoundOnOff_Bgm", ConfigValue.UseBgmSound);
        ConfigValue.BgmSdVolume = BGMVolSld.value;
        PlayerPrefs.SetFloat("SoundVolume_Bgm", ConfigValue.BgmSdVolume);

        ConfigValue.UseEffSound = isSEOn ? 1 : 0;
        PlayerPrefs.SetInt("SoundOnOff_Eff", ConfigValue.UseEffSound);
        ConfigValue.EffSdVolume = SEVolSld.value;
        PlayerPrefs.SetFloat("SoundVolume_Eff", ConfigValue.EffSdVolume);

        Destroy(gameObject);
    }

    void CancelBtnClick()
    {
        //cancel 버튼을 눌렀을 때는 configValue의 static 변수를 이용해 초기화 시키고 destroy한다.
        SoundManager.Instance.SoundOnOff_Bgm(ConfigValue.UseBgmSound == 1);
        SoundManager.Instance.SoundVolume_Bgm(ConfigValue.BgmSdVolume);

        SoundManager.Instance.SoundOnOff_Eff(ConfigValue.UseEffSound == 1);
        SoundManager.Instance.SoundVolume_Eff(ConfigValue.EffSdVolume);

        Destroy(gameObject);
    }

    void BGMOnOff(bool _bool)
    {
        //토글 on 상태가 bgm off로 되어야 함...
        isBGMOn = !_bool;
        SoundManager.Instance.SoundOnOff_Bgm(isBGMOn);
    }

    void BGMVolChanged(float value)
    {
        SoundManager.Instance.SoundVolume_Bgm(value);
    }

    void SEOnOff(bool _bool)
    {
        //토글 on 상태가 se off로 되어야 함...
        isSEOn = !_bool;
        SoundManager.Instance.SoundOnOff_Eff(isSEOn);
    }

    void SEVolChanged(float value)
    {
        SoundManager.Instance.SoundVolume_Eff(value);
    }
}

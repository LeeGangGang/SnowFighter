using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//싱글 톤 패턴
//어느 씬에서든 로딩할 수 있는 구조로 작성

//MonoBehaviour를 상속받는 게 아니라 MonoSingleton의 G_Singleton 클래스를 상속받는다.
public class SoundManager : MonoBehaviour
{
    // Bgm + UI AudioSource
    [HideInInspector] public AudioSource audioSrc = null;
    // 모든 AudioClip 
    Dictionary<string, AudioClip> audioClipList = new Dictionary<string, AudioClip>();

    [HideInInspector] public bool soundOnOff = true;

    //효과음 버퍼
    int effSdCount = 10; //최대 10번 플레이
    int iSdCount = 0;
    List<GameObject> sdObjList = new List<GameObject>();

    // Effect AudioSource
    List<AudioSource> sdSrcList = new List<AudioSource>();

    static public SoundManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadChildGameObj();
            DontDestroyOnLoad(this.gameObject);
        }
        else
            Destroy(this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        AudioClip clip = null;
        object[] temp = Resources.LoadAll("Sounds");
        for (int i = 0; i < temp.Length; i++)
        {
            clip = temp[i] as AudioClip;
            if (audioClipList.ContainsKey(clip.name))
                continue;
            audioClipList.Add(clip.name, clip);
        }

        ConfigValue.UseBgmSound = PlayerPrefs.GetInt("SoundOnOff_Bgm", 1);
        bool a_soundOnOff = (ConfigValue.UseBgmSound == 1);
        SoundOnOff_Bgm(a_soundOnOff);

        ConfigValue.UseEffSound = PlayerPrefs.GetInt("SoundOnOff_Eff", 1);
        a_soundOnOff = (ConfigValue.UseEffSound == 1);
        SoundOnOff_Eff(a_soundOnOff);

        ConfigValue.BgmSdVolume = PlayerPrefs.GetFloat("SoundVolume_Bgm", 1f);
        ConfigValue.EffSdVolume = PlayerPrefs.GetFloat("SoundVolume_Eff", 1f);
        SoundVolume_Bgm(ConfigValue.BgmSdVolume);
        SoundVolume_Eff(ConfigValue.EffSdVolume);
    }

    void LoadChildGameObj()
    {
        if (this == null)
            return;

        audioSrc = gameObject.AddComponent<AudioSource>();

        for (int i = 0; i < effSdCount; i++)
        {
            GameObject newSdObj = new GameObject();
            newSdObj.transform.SetParent(transform);
            newSdObj.transform.localPosition = Vector3.zero;
            AudioSource audioSource = newSdObj.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            newSdObj.name = "SoundEffObj";

            sdSrcList.Add(audioSource);
            sdObjList.Add(newSdObj);
        }
    }

    public void PlayBGM(string fileName, bool loop = true)
    {
        if (!soundOnOff)
            return;
        
        if (!audioClipList.ContainsKey(fileName))
            audioClipList.Add(fileName, Resources.Load("Sounds/" + fileName) as AudioClip);

        if (ReferenceEquals(audioSrc, null))
            return;
        
        if (!ReferenceEquals(audioSrc.clip, null) && audioSrc.clip.name == fileName)
            return;

        audioSrc.clip = audioClipList[fileName];
        audioSrc.loop = loop;
        audioSrc.Play();
    }

    public void PlayUISound(string fileName, float volume = 1f)
    {
        if (!soundOnOff)
            return;

        if (!audioClipList.ContainsKey(fileName))
            audioClipList.Add(fileName, Resources.Load("Sounds/" + fileName) as AudioClip);

        if (audioSrc == null)
            return;

        audioSrc.PlayOneShot(audioClipList[fileName], volume * ConfigValue.BgmSdVolume);
    }

    public void PlayEffSound(string fileName, float volume = 1f)
    {
        if (!soundOnOff)
            return;

        if (!audioClipList.ContainsKey(fileName))
            audioClipList.Add(fileName, Resources.Load("Sounds/" + fileName) as AudioClip);

        if (!ReferenceEquals(audioClipList[fileName], null) && !ReferenceEquals(sdSrcList[iSdCount], null))
        {
            sdSrcList[iSdCount].clip = audioClipList[fileName];
            sdSrcList[iSdCount].loop = false;
            sdSrcList[iSdCount].volume = volume;
            sdSrcList[iSdCount].Play();

            iSdCount++;
            if (effSdCount <= iSdCount)
                iSdCount = 0;
        }
    }

    public float GetDistVolume(Vector3 OtherPos)
    {
        Vector3 MyPos = Camera.main.GetComponent<CameraCtrl>().Player.GetComponent<PlayerCtrl>().tr.position;
        float Volume = 1f;
        float Dist = (OtherPos - MyPos).magnitude;
        if (Dist >= 20f)
            return 0f;
        else
        {
            Volume = 1f - Dist / 20f;
            if (Volume < 0.1f)
                Volume = 0.1f;
        }

        return Volume;
    }

    public void SoundOnOff_Bgm(bool soundOn = true)
    {
        if (!ReferenceEquals(audioSrc, null))
        {
            audioSrc.mute = !soundOn;
        }
    }

    public void SoundOnOff_Eff(bool soundOn = true)
    {
        for (int i = 0; i < sdSrcList.Count; i++)
        {
            if (!ReferenceEquals(sdSrcList[i], null))
            {
                sdSrcList[i].mute = !soundOn;
            }
        }
    }

    public void SoundVolume_Bgm(float volume)
    {
        if (!ReferenceEquals(audioSrc, null))
        {
            audioSrc.volume = volume;
        }
    }

    public void SoundVolume_Eff(float volume)
    {
        for (int i = 0; i < sdSrcList.Count; i++)
        {
            sdSrcList[i].volume = volume;
        }
    }
}
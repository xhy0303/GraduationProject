using System.Collections;
using UnityEngine;

public class BGMManager : Manager<BGMManager>
{

    public AudioSource bgmSource;
    public float fadeDuration = 1.5f;

    public AudioClip indoorPeace;
    public AudioClip indoorAlert;
    public AudioClip outdoorPeace;
    public AudioClip outdoorAlert;

    private Coroutine currentCoroutine;
    private string currentKey = "";

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);

        UpdateBGM(false, PlayerStats.ExposureState.Peace); // 默认室外和平音乐
    }

    public void UpdateBGM(bool isIndoor, PlayerStats.ExposureState state)
    {
        string key = (isIndoor ? "Indoor" : "Outdoor") + "_" + state.ToString();
        if (key == currentKey) return;

        currentKey = key;
        AudioClip newClip = GetClip(isIndoor, state);

        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(SwitchBGM(newClip));
    }

    private AudioClip GetClip(bool isIndoor, PlayerStats.ExposureState state)
    {
        switch (state)
        {
            case PlayerStats.ExposureState.Peace:
                return isIndoor ? indoorPeace : outdoorPeace;
            case PlayerStats.ExposureState.Alert:
            case PlayerStats.ExposureState.Chase: // 追击也用警觉音乐
                return isIndoor ? indoorAlert : outdoorAlert;
            default:
                return null;
        }
    }

    [SerializeField] private float defaultVolume = 0.5f;  // 默认音量

    IEnumerator SwitchBGM(AudioClip newClip)
    {
        // 淡出当前音乐
        float t = 0f;

        while (t < fadeDuration)
        {
            bgmSource.volume = Mathf.Lerp(defaultVolume, 0, t / fadeDuration);
            t += Time.deltaTime;
            yield return null;
        }

        bgmSource.Stop();
        bgmSource.clip = newClip;
        bgmSource.Play();

        // 淡入新音乐
        t = 0f;
        while (t < fadeDuration)
        {
            bgmSource.volume = Mathf.Lerp(0, defaultVolume, t / fadeDuration);
            t += Time.deltaTime;
            yield return null;
        }

        bgmSource.volume = defaultVolume;
    }

    public void OnVolumeChanged(float newVolume)
    {
        defaultVolume = newVolume;

        // 如果正在播放音乐，实时更新音量
        if (bgmSource.isPlaying)
        {
            bgmSource.volume = newVolume;
        }
    }

    public float GetVolume()
    {
        return defaultVolume;
    }


}

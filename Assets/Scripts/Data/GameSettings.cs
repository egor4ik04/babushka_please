using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Scriptable Objects/GameSettings")]
public class GameSettings : ScriptableObject
{
    [SerializeField] private float masterVolume = 1f;
    //[SerializeField] private float musicVolume = 100f;
    //[SerializeField] private float soundVolume = 100f;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Resolution resolution;
    [SerializeField] private ScreenResolution screenResolution;
    [SerializeField] private FullScreenMode fullScreenMode;
    public static bool IsLoaded { get; private set; } = false;
    public static event Action OnInstanceLoaded;

    public void SaveSettings()
    {
        PlayerPrefs.SetInt("FullScreenMode", (int)fullScreenMode);
        PlayerPrefs.SetInt("ResolutionWidth", resolution.width);
        PlayerPrefs.SetInt("ResolutionHeight", resolution.height);
        PlayerPrefs.SetInt("ResolutionRefreshRateNumerator", (int)resolution.refreshRateRatio.numerator);
        PlayerPrefs.SetInt("ResolutionRefreshRateDenominator", (int)resolution.refreshRateRatio.denominator);
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.Save();
    }
    public void LoadSettings()
    {
        fullScreenMode = (FullScreenMode)PlayerPrefs.GetInt("FullScreenMode", (int)FullScreenMode.FullScreenWindow);
        if (screenResolution == null)
        {
            screenResolution = new ScreenResolution(Screen.currentResolution);
        }
        screenResolution.Width = PlayerPrefs.GetInt("ResolutionWidth", Screen.currentResolution.width);
        screenResolution.Height = PlayerPrefs.GetInt("ResolutionHeight", Screen.currentResolution.height);
        screenResolution.RefreshRate.Numerator = (uint)PlayerPrefs.GetInt("ResolutionRefreshRateNumerator", (int)Screen.currentResolution.refreshRateRatio.numerator);
        screenResolution.RefreshRate.Denominator = (uint)PlayerPrefs.GetInt("ResolutionRefreshRateDenominator", (int)Screen.currentResolution.refreshRateRatio.denominator);
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
    }
    public void Initialize()
    {
        bool wasLoaded = IsLoaded;
        IsLoaded = false;
        if (!wasLoaded)
        {
            //LoadAudioMixer();
            LoadSettings();
            LoadResolution();
            Application.quitting -= OnAppQuit;
            Application.quitting += OnAppQuit;
            SceneManager.sceneUnloaded -= OnSceneUnload;
            SceneManager.sceneUnloaded += OnSceneUnload;
            IsLoaded = true;
            OnInstanceLoaded?.Invoke();
        }
        else
            IsLoaded = true;
    }
    
    public void OnAppQuit()
    {
        SaveSettings();
    }
    public void OnSceneUnload(Scene scene)
    {
        SaveSettings();
    }
    public void ResetSave()
    {
        FullScreenMode = FullScreenMode.FullScreenWindow;
        MasterVolume = 1f;

        var resolutions = Screen.resolutions.ToList();
        Resolution = resolutions.Last();

        SaveSettings();
    }

    /*public AudioMixer AudioMixer
    {
        get
        {
            LoadAudioMixer();
            return audioMixer;
        }
        set
        {
            audioMixer = value;
            if (audioMixer != null)
            {
                SetMixerValuesFromSettings();
            }
        }
    }*/
    public float MasterVolume
    {
        get => masterVolume/* / 100f*/;
        set
        {
            //masterVolume = Mathf.Clamp(value, 0f, 100f);
            masterVolume = Mathf.Clamp01(value);
            //SetVolumeInMixer(masterVolume, "MasterVolume");
        }
    }
    
    public FullScreenMode FullScreenMode
    {
        get => fullScreenMode;
        set
        {
            fullScreenMode = value;
            Screen.SetResolution(resolution.width, resolution.height, fullScreenMode, resolution.refreshRateRatio);
            OnScreenChangedEvent?.Invoke();
        }
    }
    public Resolution Resolution
    {
        get => resolution;
        set
        {
            resolution = value;
            Screen.SetResolution(resolution.width, resolution.height, fullScreenMode, resolution.refreshRateRatio);
            screenResolution = new ScreenResolution(resolution);
            OnScreenChangedEvent?.Invoke();
        }
    }

    public UnityEvent OnScreenChangedEvent;

    /*public void LoadAudioMixer()
    {
        audioMixer = Resources.Load<AudioMixer>("Audio/GlobalMixer");
        SetMixerValuesFromSettings();
    }
    public void SetMixerValuesFromSettings()
    {
        SetVolumeInMixer(masterVolume, "MasterVolume");
        //SetVolumeInMixer(musicVolume, "MusicVolume");
        //SetVolumeInMixer(soundVolume, "SoundVolume");
    }
    public void SetVolumeInMixer(float volume, string volumeNameInMixer)
    {
        audioMixer = Resources.Load<AudioMixer>("Audio/GlobalMixer");
        if (audioMixer)
        {
            float newVolume = volume / 100f * 40f - 40f;
            audioMixer.SetFloat(volumeNameInMixer, newVolume);
            if (newVolume <= -39.5f) audioMixer.SetFloat(volumeNameInMixer, -80f);
        }
        else
        {
            Debug.LogError("AudioMixer is still null after attempt to reload.");
        }
    }*/
    public void LoadResolution()
    {
        resolution = screenResolution.ToResolution();
        Screen.SetResolution(resolution.width, resolution.height, fullScreenMode, resolution.refreshRateRatio);
    }

    public static GameSettings Get() => Resources.Load<GameSettings>("ScriptableObject/GameSettings");
}

[System.Serializable]
public class ScreenResolution
{
    public int Width;
    public int Height;
    public ScreenRefreshRate RefreshRate;

    public ScreenResolution(Resolution resolution)
    {
        Width = resolution.width;
        Height = resolution.height;
        RefreshRate = new ScreenRefreshRate(resolution.refreshRateRatio);
    }

    public Resolution ToResolution()
    {
        return new Resolution
        {
            width = Width,
            height = Height,
            refreshRateRatio = RefreshRate.ToRefreshRate()
        };
    }
}
[System.Serializable]
public class ScreenRefreshRate
{
    public long Numerator;
    public long Denominator;

    public ScreenRefreshRate(RefreshRate refreshRate)
    {
        Numerator = refreshRate.numerator;
        Denominator = refreshRate.denominator;
    }

    public RefreshRate ToRefreshRate()
    {
        return new RefreshRate
        {
            numerator = (uint)Numerator,
            denominator = (uint)Denominator
        };
    }
}

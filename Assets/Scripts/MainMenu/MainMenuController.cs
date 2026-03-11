using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Missions")]
    [SerializeField] private GameObject m_missionsPanel;
    [SerializeField] private MissionsSO m_missionsSO;
    [SerializeField] private RectTransform m_missionsListContainer;
    [SerializeField] private GameObject _missionPrefab;
    [SerializeField] private List<MissionPrefabData> _missionPrefabsDatas;
    [SerializeField] private string _missionPrefix;

    [Header("Settings")]
    [SerializeField] private GameObject m_settingsPanel;
    [SerializeField] private Slider m_masterAudioSlider;
    [SerializeField] private Toggle m_fullscreenToggle;
    [SerializeField] private TMP_Dropdown m_resolutionDropdown;
    [SerializeField] private GameSettings _gameSettings;

    private List<Resolution> _availableResolutions = new();
    private bool _ignoreUiCallbacks;

    private void Awake()
    {
        if (_gameSettings == null)
            _gameSettings = GameSettings.Get();
        _gameSettings.Initialize();
    }
    private void Start()
    {
        LoadMissions();
        InitializeSettingsUI();
        LoadSettings();
    }

    public void LoadMissions()
    {
        _missionPrefabsDatas.ForEach(m =>
        {
            if (m != null)
                Destroy(m.gameObject);
        });

        _missionPrefabsDatas.Clear();

        foreach (var mission in m_missionsSO.missions)
        {
            GameObject missionGO = Instantiate(_missionPrefab, m_missionsListContainer);
            MissionPrefabData data = missionGO.GetComponent<MissionPrefabData>();
            data.SetMission(mission.Id);
            data.SetText($"{_missionPrefix}{mission.Id}");
            _missionPrefabsDatas.Add(data);
        }
    }

    #region Settings
    public void LoadSettings()
    {
        _ignoreUiCallbacks = true;

        m_masterAudioSlider.minValue = 0f;
        m_masterAudioSlider.maxValue = 1f;
        m_masterAudioSlider.value = _gameSettings.MasterVolume;

        m_fullscreenToggle.isOn = _gameSettings.FullScreenMode == FullScreenMode.FullScreenWindow;

        m_resolutionDropdown.ClearOptions();
        _availableResolutions = Screen.resolutions.ToList();

        List<string> options = new();
        int currentResolutionIndex = 0;

        for (int i = 0; i < _availableResolutions.Count; i++)
        {
            Resolution resolution = _availableResolutions[i];
            string option =
                $"{resolution.width}x{resolution.height} " +
                $"({resolution.refreshRateRatio.value:F0} Hz)";
            options.Add(option);

            bool sameWidth = resolution.width == _gameSettings.Resolution.width;
            bool sameHeight = resolution.height == _gameSettings.Resolution.height;
            bool sameRefresh =
                resolution.refreshRateRatio.numerator == _gameSettings.Resolution.refreshRateRatio.numerator &&
                resolution.refreshRateRatio.denominator == _gameSettings.Resolution.refreshRateRatio.denominator;

            if (sameWidth && sameHeight && sameRefresh)
                currentResolutionIndex = i;
        }

        m_resolutionDropdown.AddOptions(options);
        m_resolutionDropdown.value = currentResolutionIndex;
        m_resolutionDropdown.RefreshShownValue();

        _ignoreUiCallbacks = false;
    }
    private void InitializeSettingsUI()
    {
        m_masterAudioSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        m_fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenToggleChanged);
        m_resolutionDropdown.onValueChanged.RemoveListener(OnResolutionDropdownChanged);

        m_masterAudioSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        m_fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggleChanged);
        m_resolutionDropdown.onValueChanged.AddListener(OnResolutionDropdownChanged);
    }
    private void OnMasterVolumeChanged(float value)
    {
        if (_ignoreUiCallbacks)
            return;

        _gameSettings.MasterVolume = value;
        _gameSettings.SaveSettings();
    }
    private void OnFullscreenToggleChanged(bool isOn)
    {
        if (_ignoreUiCallbacks)
            return;

        _gameSettings.FullScreenMode = isOn
            ? FullScreenMode.FullScreenWindow
            : FullScreenMode.Windowed;

        _gameSettings.SaveSettings();
    }
    private void OnResolutionDropdownChanged(int index)
    {
        if (_ignoreUiCallbacks)
            return;

        if (index < 0 || index >= _availableResolutions.Count)
            return;

        _gameSettings.Resolution = _availableResolutions[index];
        _gameSettings.SaveSettings();
    }
    #endregion

    public void Quit() => Application.Quit();

    public void ShowMissionsPanel()
    {
        m_missionsPanel.SetActive(true);
        m_settingsPanel.SetActive(false);
    }
    public void ShowSettingsPanel()
    {
        m_missionsPanel.SetActive(false);
        m_settingsPanel.SetActive(true);
    }
}
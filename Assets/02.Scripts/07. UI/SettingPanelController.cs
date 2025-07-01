using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingPanelController : MonoBehaviour
{
    [Header("설정 UI 요소")]
    [SerializeField] private Button backButton;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Toggle fullScreenToggle;
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    public static event Action OnBackButtonClicked;
    public static event Action<float> OnMasterVolumeChanged;
    public static event Action<float> OnBGMVolumeChanged;
    public static event Action<float> OnSFXVolumeChanged;
    public static event Action<bool> OnFullscreenChanged;

    private void Awake()
    {
        SetupSettingsEvents();
        LoadSettings();
    }

    private void OnEnable()
    {
        RefreshSettings();
    }

    /// <summary>
    /// UI이벤트 설정
    /// </summary>
    private void SetupSettingsEvents()
    {
        if(backButton != null)
            backButton.onClick.AddListener(OnBackClicked);
        
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeValueChanged);
        
        if (bgmVolumeSlider != null)
            bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeValueChanged);
            
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeValueChanged);

        if (fullScreenToggle != null)
            fullScreenToggle.onValueChanged.AddListener(OnFullscreenToggleChanged);
        
        if (resolutionDropdown != null)
        {
            SetupResolutionDropdown();
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        }
    }

    /// <summary>
    /// 저장된 설정 값 로드
    /// </summary>
    private void LoadSettings()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1.0f);

        if (bgmVolumeSlider != null)
            bgmVolumeSlider.value = PlayerPrefs.GetFloat("BGMVolume", 0.8f);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1.0f);

        if (fullScreenToggle != null)
            fullScreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        
    }

    /// <summary>
    /// 설정값 새로고침
    /// </summary>
    private void RefreshSettings()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.value = AudioListener.volume;

        if (fullScreenToggle != null)
            fullScreenToggle.isOn = Screen.fullScreen;
    }

    private void SetupResolutionDropdown()
    {
        if (resolutionDropdown == null) return;
        
        resolutionDropdown.ClearOptions();

        var options = new List<string>();
        Resolution[] resolutions = Screen.resolutions;
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
                currentResolutionIndex = i;
        }
        
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    #region 이벤트 핸들러

    private void OnBackClicked()
    {
        //PlayButtonSound();
        SaveSettings();
        OnBackButtonClicked?.Invoke();
    }

    private void OnMasterVolumeValueChanged(float value)
    {
        AudioListener.volume = value;
        OnMasterVolumeChanged?.Invoke(value);
        SaveSetting("MasterVolume", value);
    }
    
    private void OnBGMVolumeValueChanged(float value)
    {
        OnBGMVolumeChanged?.Invoke(value);
        SaveSetting("BGMVolume", value);
    }

    private void OnSFXVolumeValueChanged(float value)
    {
        OnSFXVolumeChanged?.Invoke(value);
        SaveSetting("SFXVolume", value);
    }

    private void OnFullscreenToggleChanged(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        OnFullscreenChanged?.Invoke(isFullscreen);
        SaveSetting("Fullscreen", isFullscreen ? 1 : 0);
    }
    
    private void OnResolutionChanged(int resolutionIndex)
    {
        Resolution resolution = Screen.resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        
        SaveSetting("ResolutionWidth", resolution.width);
        SaveSetting("ResolutionHeight", resolution.height);
    }

    #endregion

    #region 설정 저장/로드

    private void SaveSetting(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();
    }

    private void SaveSetting(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 모든 설정 저장
    /// </summary>
    private void SaveSettings()
    {
        if (masterVolumeSlider != null)
            PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);
            
        if (bgmVolumeSlider != null)
            PlayerPrefs.SetFloat("BGMVolume", bgmVolumeSlider.value);
            
        if (sfxVolumeSlider != null)
            PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);

        if (fullScreenToggle != null)
            PlayerPrefs.SetInt("Fullscreen", fullScreenToggle.isOn ? 1 : 0);
            
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 설정 초기화
    /// </summary>
    public void ResetToDefault()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.value = 1.0f;
            
        if (bgmVolumeSlider != null)
            bgmVolumeSlider.value = 0.8f;
            
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = 1.0f;

        if (fullScreenToggle != null)
            fullScreenToggle.isOn = true;

        SaveSettings();
    }

    #endregion


    public void ClosePanel()
    {
        SaveSettings();
        gameObject.SetActive(false);
    }
}

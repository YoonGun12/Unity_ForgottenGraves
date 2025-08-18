using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("메인메뉴 UI")] 
    [SerializeField] private GameObject mainMenuPanel;

    [Header("메뉴 버튼")] 
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("설정 메뉴")] 
    [SerializeField] private GameObject settingsPanelPrefab;
    private GameObject settingPanel;
    private SettingPanelController settingsController;

    [Header("게임 정보")] 
    [SerializeField] private TextMeshProUGUI gameVersionText;
    
    [Header("확인 창")] 
    [SerializeField] private GameObject confirmDialog;
    [SerializeField] private TextMeshProUGUI confirmText;
    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;

    [Header("페이드 효과")] 
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1.0f;

    private SaveManager saveManager;
    private bool hasSaveData = false;

    private void Start()
    {
        InitializeMainMenu();
    }

    private void OnEnable()
    {
        SettingPanelController.OnBackButtonClicked += OnBackFromSettings;
        SettingPanelController.OnMasterVolumeChanged += OnMasterVolumeChanged;
        SettingPanelController.OnBGMVolumeChanged += OnBGMVolumeChanged;
        SettingPanelController.OnSFXVolumeChanged += OnSFXVolumeChanged;
        SettingPanelController.OnFullscreenChanged += OnFullscreenChanged;
    }
    
    private void OnDisable()
    {
        SettingPanelController.OnBackButtonClicked -= OnBackFromSettings;
        SettingPanelController.OnMasterVolumeChanged -= OnMasterVolumeChanged;
        SettingPanelController.OnBGMVolumeChanged -= OnBGMVolumeChanged;
        SettingPanelController.OnSFXVolumeChanged -= OnSFXVolumeChanged;
        SettingPanelController.OnFullscreenChanged -= OnFullscreenChanged;
    }

    private void InitializeMainMenu()
    {
        //CheckSaveData();
        SetupUI();
        SetupButtonEvents();
        DisplayGameInfo();
        PlayMenuBGM();
        StartCoroutine(FadeIn());
    }

    private void SetupUI()
    {
        ShowMainMenu();

        if (continueButton != null)
        {
            continueButton.interactable = hasSaveData;

            if (!hasSaveData)
            {
                var colors = continueButton.colors;
                colors.normalColor = Color.gray;
                colors.disabledColor = Color.gray;
                continueButton.colors = colors;
            }
        }
        if(confirmDialog != null)
            confirmDialog.SetActive(false);
    }

    private void SetupButtonEvents()
    {
        if(newGameButton != null)
            newGameButton.onClick.AddListener(OnNewGameClicked);
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
        if (confirmYesButton != null)
            confirmYesButton.onClick.AddListener(OnConfirmYes);
        if (confirmNoButton != null)
            confirmNoButton.onClick.AddListener(OnConfirmNo);
        
    }

    private void CreateSettingPanel()
    {
        if (settingPanel == null && settingsPanelPrefab != null)
        {
            settingPanel = Instantiate(settingsPanelPrefab, transform);
            settingPanel.SetActive(false);

            settingsController = settingPanel.GetComponent<SettingPanelController>();
        }
    }
    
    private void DisplayGameInfo()
    {
        // 게임 버전 표시
        if (gameVersionText != null)
        {
            gameVersionText.text = $"v{Application.version}";
        }
    }

    #region 버튼 이벤트 핸들러

    /// <summary>
    /// 새 게임 시작
    /// </summary>
    private void OnNewGameClicked()
    {
        if (hasSaveData)
        {
            ShowConfirmDialog("새 게임을 시작하면 기존 저장 데이터가 삭제됩니다.\n계속하시겠습니까?", 
                () => StartNewGame());
        }
        else
        {
            StartNewGame();
        }
    }
    
    /// <summary>
    /// 게임 계속하기
    /// </summary>
    private void OnContinueClicked()
    {
        if (!hasSaveData) return;
        StartCoroutine(LoadGameWithFade("Cemetery_Hub"));
    }
    
    /// <summary>
    /// 설정 메뉴 열기
    /// </summary>
    private void OnSettingsClicked()
    {
        // 설정 패널이 없으면 생성
        if (settingPanel == null)
        {
            CreateSettingPanel();
        }
        
        ShowSettingsMenu();
    }
    
    /// <summary>
    /// 게임 종료
    /// </summary>
    private void OnQuitClicked()
    {
        ShowConfirmDialog("게임을 종료하시겠습니까?", () => QuitGame());
    }
    
    /// <summary>
    /// 확인 다이얼로그 - 예
    /// </summary>
    private void OnConfirmYes()
    {
        confirmAction?.Invoke();
        HideConfirmDialog();
    }
    
    /// <summary>
    /// 확인 다이얼로그 - 아니오
    /// </summary>
    private void OnConfirmNo()
    {
        HideConfirmDialog();
    }

    #endregion
    
    #region 설정 패널 이벤트 핸들러

    /// <summary>
    /// 설정에서 뒤로가기 (설정 패널에서 호출)
    /// </summary>
    private void OnBackFromSettings()
    {
        ShowMainMenu();
    }

    /// <summary>
    /// 마스터 볼륨 변경 이벤트
    /// </summary>
    private void OnMasterVolumeChanged(float value)
    {
        Debug.Log($"마스터 볼륨 변경: {value}");
    }

    /// <summary>
    /// BGM 볼륨 변경 이벤트
    /// </summary>
    private void OnBGMVolumeChanged(float value)
    {
        Debug.Log($"BGM 볼륨 변경: {value}");
    }

    /// <summary>
    /// SFX 볼륨 변경 이벤트
    /// </summary>
    private void OnSFXVolumeChanged(float value)
    {
        Debug.Log($"SFX 볼륨 변경: {value}");
    }

    /// <summary>
    /// 전체화면 설정 변경 이벤트
    /// </summary>
    private void OnFullscreenChanged(bool isFullscreen)
    {
        Debug.Log($"전체화면 모드: {isFullscreen}");
    }

    #endregion
    
    #region UI 표시/숨기기
    
    /// <summary>
    /// 메인 메뉴 표시
    /// </summary>
    private void ShowMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (settingPanel != null) settingPanel.SetActive(false);
    }
    
    /// <summary>
    /// 설정 메뉴 표시
    /// </summary>
    private void ShowSettingsMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingPanel != null) settingPanel.SetActive(true);
    }
    
    /// <summary>
    /// 확인 다이얼로그 표시
    /// </summary>
    private System.Action confirmAction;
    private void ShowConfirmDialog(string message, System.Action onConfirm)
    {
        if (confirmDialog == null) return;
        
        confirmAction = onConfirm;
        
        if (confirmText != null)
            confirmText.text = message;
            
        confirmDialog.SetActive(true);
    }
    
    /// <summary>
    /// 확인 다이얼로그 숨기기
    /// </summary>
    private void HideConfirmDialog()
    {
        if (confirmDialog != null)
            confirmDialog.SetActive(false);
            
        confirmAction = null;
    }
    
    #endregion
    
    #region 게임 시작/로드
    
    /// <summary>
    /// 새 게임 시작
    /// </summary>
    private void StartNewGame()
    {
        // 기존 저장 데이터 삭제
        if (hasSaveData)
        {
            saveManager.DeleteSaveData();
        }
        
        // 프롤로그 씬으로 이동
        StartCoroutine(LoadGameWithFade("Prologue3"));
    }
    
    /// <summary>
    /// 페이드 효과와 함께 씬 로드
    /// </summary>
    /// <param name="sceneName">로드할 씬 이름</param>
    /// <returns></returns>
    private IEnumerator LoadGameWithFade(string sceneName)
    {
        yield return StartCoroutine(FadeOut());
        SceneManager.LoadScene(sceneName);
    }
    
    /// <summary>
    /// 게임 종료
    /// </summary>
    private void QuitGame()
    {
        Debug.Log("게임을 종료합니다.");
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
    
    #endregion
    
    #region 오디오 및 효과
    
    /// <summary>
    /// 메뉴 BGM 재생
    /// </summary>
    private void PlayMenuBGM()
    {
        /*if (menuAudioSource != null && menuBGM != null)
        {
            menuAudioSource.clip = menuBGM;
            menuAudioSource.loop = true;
            menuAudioSource.Play();
        }*/
    }
    
    /// <summary>
    /// 페이드 인 효과
    /// </summary>
    /// <returns></returns>
    private IEnumerator FadeIn()
    {
        if (fadeImage == null) yield break;
        
        Color color = fadeImage.color;
        color.a = 1f;
        fadeImage.color = color;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }
        
        color.a = 0f;
        fadeImage.color = color;
    }
    
    /// <summary>
    /// 페이드 아웃 효과
    /// </summary>
    /// <returns></returns>
    private IEnumerator FadeOut()
    {
        if (fadeImage == null) yield break;
        
        Color color = fadeImage.color;
        color.a = 0f;
        fadeImage.color = color;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }
        
        color.a = 1f;
        fadeImage.color = color;
    }
    
    #endregion
    
    /// <summary>
    /// ESC 키 처리
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (confirmDialog != null && confirmDialog.activeInHierarchy)
            {
                OnConfirmNo();
            }
            else if (settingPanel != null && settingPanel.activeInHierarchy)
            {
                OnBackFromSettings();
            }
        }
    }
}

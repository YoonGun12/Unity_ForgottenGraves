using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("게임 상태")] 
    public Enums.GameState currentGameState = Enums.GameState.MainMenu;         //현재 게임 상태
    public Enums.SceneType currentScene = Enums.SceneType.MainMenu;              //현재 씬 타입

    [Header("진행 추적")] 
    public bool[] tombstoneCompleted = new bool[5];                             //완료한 묘비
    public int completedTombstoneCount = 0;                                          //완료한 묘비 에피소드 수
    
    [Header("현재 에피소드")]
    public Enums.TombstoneType currentEpisode = Enums.TombstoneType.None;       //현재 에피소드
    public Enums.EpisodePhase currentPhase = Enums.EpisodePhase.None;           //현재 에피소드 내 페이즈

    [Header("레퍼런스")] 
    public DialogueManager dialogueManager;
    public AudioManager audioManager;
    public SaveManager saveManager;
    public ProgressTracker progressTracker;
    public SceneTransitionManager sceneTransitionManager;

    [Header("게임 이벤트")] 
    public Action<Enums.TombstoneType> OnTombstoneStarted;                      //묘비 에피소드 시작 이벤트
    public Action<Enums.TombstoneType> OnTombstoneCompleted;                    //묘비 에피소드 완료 이벤트
    public Action<Enums.EpisodePhase> OnPhaseChanged;                           //페이즈 전환시 이벤트
    public Action<Enums.SceneType> OnSceneChanged;                              //씬 변경시 이벤트
    public Action OnGameCompleted;                                              //게임 완료시 이벤트

    private void Awake()
    {
        //싱글톤
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartCoroutine(WaitForSceneLoad());
    }

    private IEnumerator WaitForSceneLoad()
    {
        yield return new WaitForEndOfFrame();

        //현재 씬에 따른 게임 상태 설정
        string sceneName = SceneManager.GetActiveScene().name;
        currentScene = GetSceneTypeFromName(sceneName);

        SetGameStateFromScene();
        OnSceneChanged?.Invoke(currentScene);
    }

    /// <summary>
    /// 현재 씬에 따라 게임 상태를 결정
    /// </summary>
    private void SetGameStateFromScene()
    {
        switch (currentScene)
        {
            //메인메뉴
            case Enums.SceneType.MainMenu:
                currentGameState = Enums.GameState.MainMenu;
                break;
            //프롤로그
            case Enums.SceneType.Prologue1:
            case Enums.SceneType.Prologue2:
            case Enums.SceneType.Prologue3:
                currentGameState = Enums.GameState.Prologue;
                break;
            //메인 게임 허브
            case Enums.SceneType.Cemetery_Hub:
                currentGameState = Enums.GameState.Playing;
                CheckGameProgress();
                break;
            //묘비 에피소드
            case Enums.SceneType.Edgar_Episode:
            case Enums.SceneType.Matilda_Episode:
            case Enums.SceneType.Toby_Episode:
            case Enums.SceneType.Violet_Episode:
            case Enums.SceneType.Ria_Episode:
                currentGameState = Enums.GameState.Episode;
                SetCurrentEpisodeFromScene();
                break;
            //묘비 미니게임
            case Enums.SceneType.Edgar_Minigame:
            case Enums.SceneType.Matilda_Minigame:
            case Enums.SceneType.Toby_Minigame:
            case Enums.SceneType.Violet_Minigame:
            case Enums.SceneType.Ria_Minigame:
                currentGameState = Enums.GameState.MiniGame;
                SetCurrentEpisodeFromScene();
                break;
                
            case Enums.SceneType.Epilogue:
                currentGameState = Enums.GameState.Ending;
                break;
            
            default:
                currentGameState = Enums.GameState.Loading;
                break;
        }
    }

    /// <summary>
    /// 씬 이름으로부터 SceneType을 가져오는 메서드
    /// </summary>
    /// <param name="sceneName"></param>
    /// <returns></returns>
    private Enums.SceneType GetSceneTypeFromName(string sceneName)
    {
        if (Enum.TryParse(sceneName, out Enums.SceneType sceneType))
        {
            return sceneType;
        }
        
        // 특별한 경우들 처리
        switch (sceneName)
        {
            case "Forgotten Graves": return Enums.SceneType.Cemetery_Hub;
            default: return Enums.SceneType.MainMenu;
        }
    }
    
    /// <summary>
    /// 현재 씬으로부터 에피소드 타입을 설정
    /// </summary>
    private void SetCurrentEpisodeFromScene()
    {
        switch (currentScene)
        {
            case Enums.SceneType.Edgar_Episode:
            case Enums.SceneType.Edgar_Minigame:
                currentEpisode = Enums.TombstoneType.Edgar;
                break;
                
            case Enums.SceneType.Matilda_Episode:
            case Enums.SceneType.Matilda_Minigame:
                currentEpisode = Enums.TombstoneType.Matilda;
                break;
                
            case Enums.SceneType.Toby_Episode:
            case Enums.SceneType.Toby_Minigame:
                currentEpisode = Enums.TombstoneType.Toby;
                break;
                
            case Enums.SceneType.Violet_Episode:
            case Enums.SceneType.Violet_Minigame:
                currentEpisode = Enums.TombstoneType.Violet;
                break;
                
            case Enums.SceneType.Ria_Episode:
            case Enums.SceneType.Ria_Minigame:
                currentEpisode = Enums.TombstoneType.Ria;
                break;
            
            default:
                currentEpisode = Enums.TombstoneType.None;
                break;
        }
    }

    private void InitializeGame()
    {
        for (int i = 0; i < tombstoneCompleted.Length; i++)
        {
            tombstoneCompleted[i] = false;
        }

        completedTombstoneCount = 0;
        currentEpisode = Enums.TombstoneType.None;
        currentPhase = Enums.EpisodePhase.None;

        FindManagers();
    }

    private void FindManagers()
    {
        if (dialogueManager == null)
            dialogueManager = FindObjectOfType<DialogueManager>();
        
        if (audioManager == null)
            audioManager = FindObjectOfType<AudioManager>();
            
        if (saveManager == null)
            saveManager = FindObjectOfType<SaveManager>();
            
        if (progressTracker == null)
            progressTracker = FindObjectOfType<ProgressTracker>();

        if (sceneTransitionManager == null)
            sceneTransitionManager = FindObjectOfType<SceneTransitionManager>();
    }

    #region 묘비 에피소드 관리

    /// <summary>
    /// 묘비 에피소드 시작
    /// </summary>
    /// <param name="tombstone"></param>
    public void StartTombstoneEpisode(Enums.TombstoneType tombstone)
    {
        if (tombstone == Enums.TombstoneType.None) return;
        if(tombstoneCompleted[(int)tombstone]) return;
        
        currentEpisode = tombstone;
        currentPhase = Enums.EpisodePhase.PreDialogue;
        currentGameState = Enums.GameState.Episode;
        
        OnTombstoneStarted?.Invoke(tombstone);
        OnPhaseChanged?.Invoke(currentPhase);
    }

    /// <summary>
    /// 에피소드 페이즈 변경
    /// </summary>
    /// <param name="newPhase"></param>
    public void ChangeEpisodePhase(Enums.EpisodePhase newPhase)
    {
        if (currentPhase == newPhase) return;
        
        currentPhase = newPhase;
        OnPhaseChanged?.Invoke(currentPhase);
    }

    /// <summary>
    /// 미니게임 완료
    /// </summary>
    public void CompleteMiniGame()
    {
        if (currentEpisode == Enums.TombstoneType.None) return;
        ChangeEpisodePhase(Enums.EpisodePhase.PostDialogue);
    }

    /// <summary>
    /// 묘비 에피소드 완료
    /// </summary>
    public void CompleteTombstoneEpisode()
    {
        if (currentEpisode == Enums.TombstoneType.None) return;

        tombstoneCompleted[(int)currentEpisode] = true;
        completedTombstoneCount++;

        Enums.TombstoneType completedTombStone = currentEpisode;

        currentEpisode = Enums.TombstoneType.None;
        currentPhase = Enums.EpisodePhase.None;
        currentGameState = Enums.GameState.Playing;
        
        OnTombstoneCompleted?.Invoke(completedTombStone);

        if (completedTombstoneCount >= 5)
        {
            CompleteGame();
        }

        SaveGame();
    }

    #endregion

    #region 게임 흐름 관리

    /// <summary>
    /// 게임 진행 상황 확인
    /// </summary>
    private void CheckGameProgress()
    {
        LoadGame();

        if (progressTracker != null)
            progressTracker.UpdateProgress(completedTombstoneCount, tombstoneCompleted);
    }

    /// <summary>
    /// 게임 완료
    /// </summary>
    private void CompleteGame()
    {
        currentGameState = Enums.GameState.Ending;
        OnGameCompleted?.Invoke();
        StartCoroutine(LoadEndingScene());
    }

    /// <summary>
    /// 엔딩 씬 로드
    /// </summary>
    /// <returns></returns>
    private IEnumerator LoadEndingScene()
    {
        yield return new WaitForSeconds(2f);
        LoadScene("Epilogue");
    }

    /// <summary>
    /// 특정 묘비의 완료 상태 확인
    /// </summary>
    /// <param name="tombstone"></param>
    /// <returns></returns>
    public bool IsTombStoneCompleted(Enums.TombstoneType tombstone)
    {
        return tombstoneCompleted[(int)tombstone];
    }

    /// <summary>
    /// 현재 에피소드 진행중인지 확인
    /// </summary>
    /// <returns></returns>
    public bool IsInEpisode()
    {
        return currentGameState == Enums.GameState.Episode || currentGameState == Enums.GameState.MiniGame;
    }

    /// <summary>
    /// 모든 에피소드 완료 여부 확인
    /// </summary>
    /// <returns></returns>
    public bool IsAllEpisodesCompleted()
    {
        return completedTombstoneCount >= 5;
    }
    
    #endregion

    #region 세이브 로드

    public void SaveGame()
    {
        if (saveManager != null)
        {
            saveManager.SaveProgress(completedTombstoneCount, tombstoneCompleted);
        }
    }

    public void LoadGame()
    {
        if (saveManager != null)
        {
            var saveData = saveManager.LoadProgress();
            if (saveData != null)
            {
                completedTombstoneCount = saveData.completedTombstones;
                tombstoneCompleted = saveData.tombstoneCompleted;
            }
        }
    }

    #endregion

    #region 씬관리

    public void LoadScene(string sceneName)
    {
        currentGameState = Enums.GameState.Loading;
        SceneManager.LoadScene(sceneName);
    }

    public void RestartGame()
    {
        for (int i = 0; i < tombstoneCompleted.Length; i++)
        {
            tombstoneCompleted[i] = false;
        }

        completedTombstoneCount = 0;
        currentEpisode = Enums.TombstoneType.None;
        currentPhase = Enums.EpisodePhase.None;
        
        SaveGame();
        LoadScene("Intro");
    }

    public void QuitGame()
    {
        SaveGame();
        Application.Quit();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveGame();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveGame();
        }
    }

    #endregion
}


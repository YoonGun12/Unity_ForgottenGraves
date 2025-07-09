using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 1. DialogueDatabase에서 대화 데이터를 가져와서 실제 대화 진행
/// 2. 대화 순서 제어
/// 3. DialogueUI와 연동하여 화면에 대화 표시
/// 4. 대화 관련 이벤트 발생 및 처리
/// 5. 자동진행, 타이핑 효과 등 대화 옵션 관리
/// </summary>
public class DialogueManager : MonoBehaviour
{
    //싱글톤
    public static DialogueManager Instance { get; private set; }

    [Header("UI 시스템")] 
    [SerializeField] private DialogueUI dialogueUI;

    [Header("대화설정")] 
    [SerializeField] private Enums.DialogueMode currentMode = Enums.DialogueMode.Normal;
    [SerializeField] private bool autoAdvance = false;
    [SerializeField] private float autoAdvanceDelay = 2f;

    private Enums.DialogueState currentState = Enums.DialogueState.Inactive;
    private List<DialogueData> currentDialogueList;
    private int currentDialogueIndex = 0;
    private string currentSetName = "";
    
    public static event Action<DialogueData> OnDialogueStarted;
    public static event Action<DialogueData> OnDialogueChanged;
    public static event Action<string> OnDialogueCompleted;
    public static event Action<string> OnEventFlagTriggered;

    private Coroutine autoAdvanceCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// DialogueManager초기화
    /// </summary>
    private void Initialize()
    {
        // DialogueUI가 없으면 찾기
        if (dialogueUI == null)
        {
            StartCoroutine(FindDialogueUICoroutine());
        }

        // 이벤트 구독
        SetupEventSubscriptions();
    }

    /// <summary>
    /// DialogueUI를 찾는 코루틴
    /// </summary>
    private IEnumerator FindDialogueUICoroutine()
    {
        int attempts = 0;
        const int maxAttempts = 50;

        while (dialogueUI == null && attempts < maxAttempts)
        {
            dialogueUI = FindObjectOfType<DialogueUI>();
            if (dialogueUI != null) break;
            
            attempts++;
            yield return new WaitForSeconds(0.1f);
        }
    }

    /// <summary>
    /// 이벤트 구독 설정
    /// </summary>
    private void SetupEventSubscriptions()
    {
        // 기존 구독 해제 (중복 방지)
        DialogueUI.OnNextButtonClicked -= OnNextDialogue;
        DialogueUI.OnTextTypingCompleted -= OnTypingCompleted;
        
        // 새로 구독
        DialogueUI.OnNextButtonClicked += OnNextDialogue;
        DialogueUI.OnTextTypingCompleted += OnTypingCompleted;
    }

    private void OnDestroy()
    {
        DialogueUI.OnNextButtonClicked -= OnNextDialogue;
        DialogueUI.OnTextTypingCompleted -= OnTypingCompleted;
    }

    /// <summary>
    /// 씬 로드시 이벤트 재구독
    /// </summary>
    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // 새 씬에서 DialogueUI 찾기
        if (dialogueUI == null)
        {
            StartCoroutine(FindDialogueUICoroutine());
        }
        
        // 이벤트 재구독
        SetupEventSubscriptions();
    }

    /// <summary>
    /// 특정 대화 세트 전체를 시작
    /// </summary>
    public void StartDialogueSet(string setName, Enums.DialogueMode mode = Enums.DialogueMode.Normal)
    {
        if (DialogueDatabase.Instance == null) return;
        
        var dialogueSet = DialogueDatabase.Instance.GetDialogueSet(setName);
        if (dialogueSet == null) return;
        
        StartDialogue(dialogueSet.dialogues, setName, mode);
    }

    /// <summary>
    /// 특정 대화 세트의 일부 범위만 시작
    /// </summary>
    public void StartDialogueRange(string setName, int startIndex, int count, Enums.DialogueMode mode = Enums.DialogueMode.Normal)
    {
        if (DialogueDatabase.Instance == null) return;
        
        var dialogueList = DialogueDatabase.Instance.GetDialogueRange(setName, startIndex, count);
        if (dialogueList == null || dialogueList.Count == 0) return;
        
        StartDialogue(dialogueList, setName, mode);
    }
    
    /// <summary>
    /// CSV파일 배열의 인덱스를 사용하여 대화 범위 시작
    /// </summary>
    public void StartDialogueRangeByIndex(int csvIndex, int startIndex, int count, Enums.DialogueMode mode = Enums.DialogueMode.Normal)
    {
        if (DialogueDatabase.Instance == null) return;
    
        var dialogueList = DialogueDatabase.Instance.GetDialogueRangeByIndex(csvIndex, startIndex, count);
        if (dialogueList == null || dialogueList.Count == 0) return;
    
        StartDialogue(dialogueList, $"CSV[{csvIndex}]", mode);
    }

    /// <summary>
    /// 실제 대화를 시작하는 핵심 메서드
    /// </summary>
    public void StartDialogue(List<DialogueData> dialogueList, string setName = "", Enums.DialogueMode mode = Enums.DialogueMode.Normal)
    {
        if (dialogueList == null || dialogueList.Count == 0) return;
        
        // DialogueUI가 없으면 다시 찾기
        if (dialogueUI == null)
        {
            dialogueUI = FindObjectOfType<DialogueUI>();
            if (dialogueUI == null) return;
        }

        StopCurrentDialogue();

        currentDialogueList = dialogueList;
        currentDialogueIndex = 0;
        currentSetName = setName;
        currentMode = mode;
        currentState = Enums.DialogueState.Active;
        
        // 자동진행 강제 비활성화 (수동 진행)
        autoAdvance = false;

        if (dialogueUI != null)
        {
            dialogueUI.OpenDialogue();
        }
        
        OnDialogueStarted?.Invoke(GetCurrentDialogue());
        ShowCurrentDialogue();
    }

    /// <summary>
    /// 현재 진행중인 대화를 중지
    /// </summary>
    public void StopCurrentDialogue()
    {
        if (currentState == Enums.DialogueState.Inactive) return;
        
        currentState = Enums.DialogueState.Inactive;
        
        if (autoAdvanceCoroutine != null)
        {
            StopCoroutine(autoAdvanceCoroutine);
            autoAdvanceCoroutine = null;
        }
        
        if(dialogueUI != null)
            dialogueUI.CloseDialogue();

        currentDialogueList = null;
        currentDialogueIndex = 0;
    }

    /// <summary>
    /// 다음 대화로 넘어가기
    /// </summary>
    public void NextDialogue()
    {
        // WaitingForInput 상태도 허용
        if (currentState != Enums.DialogueState.Active && currentState != Enums.DialogueState.WaitingForInput) 
        {
            return;
        }

        if (dialogueUI != null && dialogueUI.IsTyping)
        {
            dialogueUI.CompleteTyping();
            return;
        }

        currentDialogueIndex++;

        if (currentDialogueIndex < currentDialogueList.Count)
        {
            ShowCurrentDialogue();
        }
        else
        {
            CompleteDialogue();
        }
    }

    /// <summary>
    /// 현재 대화가 활성상태인지 확인
    /// </summary>
    public bool IsDialogueActive()
    {
        return currentState != Enums.DialogueState.Inactive;
    }

    /// <summary>
    /// 자동진행 설정
    /// </summary>
    public void SetAutoAdvance(bool enabled, float delay = 2f)
    {
        autoAdvance = enabled;
        autoAdvanceDelay = delay;
    }

    /// <summary>
    /// 현재 대화를 화면에 표시
    /// </summary>
    private void ShowCurrentDialogue()
    {
        if (currentDialogueList == null || currentDialogueIndex >= currentDialogueList.Count) return;
        
        var currentDialogue = GetCurrentDialogue();
        if (currentDialogue == null) return;

        if (dialogueUI != null)
        {
            dialogueUI.ShowDialogue(currentDialogue);
        }

        ProcessEventFlag(currentDialogue.eventFlag);
        OnDialogueChanged?.Invoke(currentDialogue);

        if (autoAdvance && currentMode == Enums.DialogueMode.CutScene)
        {
            if(autoAdvanceCoroutine != null)
                StopCoroutine(autoAdvanceCoroutine);

            autoAdvanceCoroutine = StartCoroutine(AutoAdvanceCoroutine());
        }
    }

    /// <summary>
    /// 현재 표시되어야 할 대화 데이터 반환
    /// </summary>
    private DialogueData GetCurrentDialogue()
    {
        if (currentDialogueList != null && currentDialogueIndex < currentDialogueList.Count)
            return currentDialogueList[currentDialogueIndex];
        return null;
    }

    /// <summary>
    /// 모든 대화가 완료되었을 때 처리
    /// </summary>
    private void CompleteDialogue()
    {
        string completedSetName = currentSetName;
        StopCurrentDialogue();
        OnDialogueCompleted?.Invoke(completedSetName);
    }

    /// <summary>
    /// 대화의 이벤트 플래그 처리
    /// </summary>
    private void ProcessEventFlag(string eventFlag)
    {
        if (string.IsNullOrEmpty(eventFlag)) return;
        
        OnEventFlagTriggered?.Invoke(eventFlag);

        switch (eventFlag.ToLower())
        {
            case "scene_transition":
                break;
            case "save_game":
                break;
            case "play_sound":
                break;
        }
    }

    private IEnumerator AutoAdvanceCoroutine()
    {
        yield return new WaitForSeconds(autoAdvanceDelay);

        if (currentState == Enums.DialogueState.Active && autoAdvance)
        {
            NextDialogue();
        }
    }

    private void OnNextDialogue()
    {
        NextDialogue();
    }

    private void OnTypingCompleted()
    {
        currentState = Enums.DialogueState.WaitingForInput;
    }
}
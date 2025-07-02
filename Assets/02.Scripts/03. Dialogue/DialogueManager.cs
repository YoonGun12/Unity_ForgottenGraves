using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
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
    
    private void Initialize()
    {
        // DialogueUI가 없으면 찾기
        if (dialogueUI == null)
            dialogueUI = FindObjectOfType<DialogueUI>();

        // 이벤트 구독
        DialogueUI.OnNextButtonClicked += OnNextDialogue;
        DialogueUI.OnTextTypingCompleted += OnTypingCompleted;
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        DialogueUI.OnNextButtonClicked -= OnNextDialogue;
        DialogueUI.OnTextTypingCompleted -= OnTypingCompleted;
    }

    public void StartDialogueSet(string setName, Enums.DialogueMode mode = Enums.DialogueMode.Normal)
    {
        if (DialogueDatabase.Instance == null) return;
        var dialogueSet = DialogueDatabase.Instance.GetDialogueSet(setName);
        if (dialogueSet == null) return;
        StartDialogue(dialogueSet.dialogues, setName, mode);
    }

    public void StartDialogueRange(string setName, int startIndex, int count,
        Enums.DialogueMode mode = Enums.DialogueMode.Normal)
    {
        if (DialogueDatabase.Instance == null) return;
        var dialogueList = DialogueDatabase.Instance.GetDialogueRange(setName, startIndex, count);
        if (dialogueList == null || dialogueList.Count == 0) return;
        StartDialogue(dialogueList, setName, mode);
    }

    public void StartDialogue(List<DialogueData> dialogueList, string setName = "",
        Enums.DialogueMode mode = Enums.DialogueMode.Normal)
    {
        if (dialogueList == null || dialogueList.Count == 0) return;

        StopCurrentDialogue();

        currentDialogueList = dialogueList;
        currentDialogueIndex = 0;
        currentSetName = setName;
        currentMode = mode;
        currentState = Enums.DialogueState.Active;
        
        if(dialogueUI != null)
            dialogueUI.OpenDialogue();
        OnDialogueStarted?.Invoke(GetCurrentDialogue());
    }

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

    public void NextDialogue()
    {
        if (currentState != Enums.DialogueState.Active) return;

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

    public bool IsDialogueActive()
    {
        return currentState != Enums.DialogueState.Inactive;
    }

    public void SetAutoAdvance(bool enabled, float delay = 2f)
    {
        autoAdvance = enabled;
        autoAdvanceDelay = delay;
    }

    private void ShowCurrentDialogue()
    {
        if (currentDialogueList == null || currentDialogueIndex >= currentDialogueList.Count) return;
        var currentDialogue = GetCurrentDialogue();
        if(dialogueUI != null)
            dialogueUI.ShowDialogue(currentDialogue);

        ProcessEventFlag(currentDialogue.eventFlag);
        
        OnDialogueChanged?.Invoke(currentDialogue);

        if (autoAdvance && currentMode == Enums.DialogueMode.CutScene)
        {
            if(autoAdvanceCoroutine != null)
                StopCoroutine(autoAdvanceCoroutine);

            autoAdvanceCoroutine = StartCoroutine(AutoAdvanceCoroutine());
        }
    }

    private DialogueData GetCurrentDialogue()
    {
        if (currentDialogueList != null && currentDialogueIndex < currentDialogueList.Count)
            return currentDialogueList[currentDialogueIndex];
        return null;
    }

    private void CompleteDialogue()
    {
        string completedSetName = currentSetName;
        
        StopCurrentDialogue();
        
        OnDialogueCompleted?.Invoke(completedSetName);
    }

    private void ProcessEventFlag(string eventFlag)
    {
        if (string.IsNullOrEmpty(eventFlag)) return;
        OnEventFlagTriggered?.Invoke(eventFlag);

        switch (eventFlag.ToLower())
        {
            case "scene_transition" :
                break;
            case "save_game" :
                break;
            case "play_sound" :
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

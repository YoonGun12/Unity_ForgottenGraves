using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 1. 대화창, 텍스트, 캐릭터 초상화 등 UI 요소를 표시
/// 2. 타이핑 효과로 텍스트를 한글자씩 보여주기
/// 3. 대화창 열기/닫기 애니메이션
/// 4. 사용자 입력 처리
/// 5. DialogueManager와 이벤트로 소통
/// </summary>
public class DialogueUI : MonoBehaviour
{
    [Header("UI 요소들")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image characterPortrait;
    [SerializeField] private Button nextButton;
    [SerializeField] private GameObject continueIndicator; 

    [Header("타이핑 설정")]
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private bool enableTypingSound = true;

    [Header("애니메이션 설정")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.3f;

    // 상태 관리
    private bool isTyping = false;
    private bool isActive = false;
    private Coroutine typingCoroutine;
    private string currentFullText = "";

    // 이벤트
    public static event Action OnDialogueUIOpened;
    public static event Action OnDialogueUIClosed;
    public static event Action OnTextTypingCompleted;
    public static event Action OnNextButtonClicked;
    
    private void Awake()
    {
        SetupUI();
    }

    /// <summary>
    /// UI 초기 설정
    /// </summary>
    private void SetupUI()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
            
        if(nextButton != null)
            nextButton.onClick.AddListener(OnNextButtonClick);
            
        if(continueIndicator != null)
            continueIndicator.SetActive(false);

        isActive = false;
        isTyping = false;
    }

    /// <summary>
    /// 대화창을 열기
    /// </summary>
    public void OpenDialogue()
    {
        if (isActive) return;
        
        if (dialoguePanel == null) return;
        
        isActive = true;
        dialoguePanel.SetActive(true);
        StartCoroutine(FadeInPanel());
        OnDialogueUIOpened?.Invoke();
    }

    /// <summary>
    /// 대화창 닫기
    /// </summary>
    public void CloseDialogue()
    {
        if (!isActive) return;
        StartCoroutine(CloseDialogueCoroutine());
    }

    /// <summary>
    /// 특정 대화 데이터를 화면에 표시
    /// </summary>
    public void ShowDialogue(DialogueData dialogueData)
    {
        if (dialogueData == null) return;

        if (!isActive)
        {
            OpenDialogue();
        }

        SetSpeakerName(dialogueData.speaker);
        SetCharacterPortrait(dialogueData.speaker, dialogueData.portraitIndex);
        StartTyping(dialogueData.text);
    }

    /// <summary>
    /// 현재 진행중인 타이핑 효과를 즉시 완료
    /// </summary>
    public void CompleteTyping()
    {
        if (isTyping && typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            ShowCompleteText();
        }
    }

    public bool IsTyping => isTyping;
    public bool IsActive => isActive;

    /// <summary>
    /// 이름을 UI에 설정
    /// </summary>
    private void SetSpeakerName(string speakerName)
    {
        if (speakerNameText != null)
        {
            speakerNameText.text = speakerName;
            speakerNameText.gameObject.SetActive(!string.IsNullOrEmpty(speakerName));
        }
    }

    /// <summary>
    /// 캐릭터 초상화를 UI에 설정
    /// </summary>
    private void SetCharacterPortrait(string characterName, int portraitIndex)
    {
        if (characterPortrait == null) return;
        
        Sprite portrait = DialogueDatabase.Instance?.GetCharacterPortrait(characterName, portraitIndex);
        if (portrait != null)
        {
            characterPortrait.sprite = portrait;
            characterPortrait.color = Color.white;
            characterPortrait.gameObject.SetActive(true);
        }
        else
        {
            characterPortrait.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 타이핑 효과
    /// </summary>
    private void StartTyping(string text)
    {
        currentFullText = text;
        if(typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(text));
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;

        if (dialogueText != null)
            dialogueText.text = "";
        if(continueIndicator != null)
            continueIndicator.SetActive(false);

        for (int i = 0; i <= text.Length; i++)
        {
            if (dialogueText != null)
                dialogueText.text = text.Substring(0, i);

            yield return new WaitForSeconds(typingSpeed);
        }

        ShowCompleteText();
    }

    private void ShowCompleteText()
    {
        isTyping = false;
        if (dialogueText != null)
            dialogueText.text = currentFullText;
        if(continueIndicator != null)
            continueIndicator.SetActive(true);
        
        OnTextTypingCompleted?.Invoke();
    }

    private IEnumerator FadeInPanel()
    {
        if(dialoguePanel == null) yield break;

        CanvasGroup canvasGroup = dialoguePanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = dialoguePanel.AddComponent<CanvasGroup>();

        float elapsedTime = 0f;
        canvasGroup.alpha = 0f;

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private IEnumerator CloseDialogueCoroutine()
    {
        isActive = false;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            isTyping = false;
        }

        yield return StartCoroutine(FadeOutPanel());
        
        if(dialoguePanel != null)
            dialoguePanel.SetActive(false);
            
        OnDialogueUIClosed?.Invoke();
    }

    private IEnumerator FadeOutPanel()
    {
        if(dialoguePanel == null) yield break;

        CanvasGroup canvasGroup = dialoguePanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null) yield break;

        float elapsedTime = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeOutDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }

    private void OnNextButtonClick()
    {
        OnNextButtonClicked?.Invoke();
    }

    public void SetTypingSpeed(float speed)
    {
        typingSpeed = Mathf.Max(0.01f, speed);
    }
    
    public void SetTypingSoundEnabled(bool enabled)
    {
        enableTypingSound = enabled;
    }
    
    private void Update()
    {
        if (isActive && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0)))
        {
            if (isTyping)
            {
                CompleteTyping();
            }
            else
            {
                OnNextButtonClick();
            }
        }
    }
}
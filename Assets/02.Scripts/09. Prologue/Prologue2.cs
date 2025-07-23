using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Prologue2 : MonoBehaviour
{
    [Header("캐릭터")] 
    [SerializeField] private Transform lyle;
    [SerializeField] private Transform granpa;
    [SerializeField] private Animator lyleAnimator;
    [SerializeField] private Animator granpaAnimator;

    [Header("씬")] 
    [SerializeField] private string nextSceneName = "Prologue3";
    [SerializeField] private float sceneTransitionDelay = 2f;

    [Header("이동")] 
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private Transform[] waypoints; // 라일의 이동 지점들

    [Header("특별 오브젝트")]
    [SerializeField] private GameObject soupPot;           // 스프 냄비
    [SerializeField] private GameObject granpaWatch;       // 할아버지 시계
    //[SerializeField] private Light2D roomLight;             // 방 조명

    [Header("CSV 파일")]
    [SerializeField] private string prologueCSVName = "Prologue - Prologue2";
    [SerializeField] private int prologueCSVIndex = 1;
    
    [Header("페이드 효과")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 3f;
   
    
    private bool isPlaying = false;
    private int currentSequence = 0;

    private void Start()
    {
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            Color color = fadeImage.color;
            color.a = 1f;
            fadeImage.color = color;
        }
        
        InitializeScene();
        StartCoroutine(WaitForDatabaseAndStart());
    }

    /// <summary>
    /// 씬 초기 설정
    /// </summary>
    private void InitializeScene()
    {
        // 할아버지 시계 비활성화
        if (granpaWatch != null)
            granpaWatch.SetActive(false);
            
        // 배경음악 설정
        
    }

    private IEnumerator WaitForDatabaseAndStart()
    {
        // DialogueDatabase 인스턴스 생성 대기
        yield return new WaitUntil(() => DialogueDatabase.Instance != null);
        
        // 데이터베이스 로딩 완료 대기
        yield return new WaitUntil(() => DialogueDatabase.Instance.IsLoaded);
        
        // DialogueManager 인스턴스 대기
        yield return new WaitUntil(() => DialogueManager.Instance != null);
        
        // DialogueUI 확인 및 설정
        yield return StartCoroutine(CheckAndSetupDialogueUI());
        
        // 이벤트 구독
        DialogueManager.OnDialogueCompleted += OnDialogueCompleted;
        DialogueManager.OnEventFlagTriggered += OnEventFlagTriggered;

        StartPrologueSequence();
    }

    private IEnumerator CheckAndSetupDialogueUI()
    {
        DialogueUI dialogueUI = FindObjectOfType<DialogueUI>();

        if (dialogueUI == null)
        {
            Debug.LogError("DialogueUI를 찾을 수 없습니다. 프롤로그2 씬에 DialogueUI가 있는지 확인하세요.");
            yield break;
        }
        
        if (DialogueManager.Instance != null)
        {
            yield return new WaitForEndOfFrame();
        }
    }
    
    private void OnDestroy()
    {
        DialogueManager.OnDialogueCompleted -= OnDialogueCompleted;
        DialogueManager.OnEventFlagTriggered -= OnEventFlagTriggered;
    }

    private void StartPrologueSequence()
    {
        if (isPlaying) return;

        isPlaying = true;
        currentSequence = 0;

        StartCoroutine(PlayPrologueSequence());
    }

    private IEnumerator PlayPrologueSequence()
    {
        yield return StartCoroutine(Sequence0_Introduction());
        yield return StartCoroutine(Sequence1_CookingSoup());
        yield return StartCoroutine(Sequence2_LastConversation());
        yield return StartCoroutine(Sequence3_FarewellMoment());
        yield return StartCoroutine(Sequence4_AloneWithWatch());
        yield return StartCoroutine(Sequence5_Conclusion());
    }

    #region 시퀀스

    /// <summary>
    /// 시퀀스 0: 시작 - 시간이 흐른 후 상황
    /// </summary>
    private IEnumerator Sequence0_Introduction()
    {
        currentSequence = 0;
        
        // 배경음악 시작

        yield return StartCoroutine(SetCharacterDirection(lyleAnimator, "left"));
        
        // 시작 대화 (ID 0-2) 내래이션
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogueRangeByIndex(prologueCSVIndex, 0, 2, Enums.DialogueMode.CutScene);
            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive());
        yield return new WaitForSeconds(1f);
    }

    /// <summary>
    /// 시퀀스 1: 라일이 스프를 끓이는 장면
    /// </summary>
    private IEnumerator Sequence1_CookingSoup()
    {
        currentSequence = 1;
        
        // 라일을 요리 공간으로 이동
        if (waypoints.Length > 0)
        {
            yield return StartCoroutine(MoveCharacterToPoint(lyle, waypoints[0], lyleAnimator));
        }
        
        yield return StartCoroutine(SetCharacterDirection(lyleAnimator, "up"));
        
        // 요리 소리 재생
        
        // 요리 관련 대화 중 할아버지가 부름
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogueRangeByIndex(prologueCSVIndex, 2, 2, Enums.DialogueMode.CutScene);
            yield return new WaitForSeconds(0.5f);
            yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive());

            yield return new WaitForSeconds(1f);
        }

        yield return StartCoroutine(SetCharacterDirection(lyleAnimator, "right"));
        
        //할아버지에게 대답
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogueRangeByIndex(prologueCSVIndex, 4, 3, Enums.DialogueMode.CutScene);
            yield return new WaitForSeconds(0.5f);
            yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive());
        }
        
        yield return new WaitForSeconds(1f);
    }

    /// <summary>
    /// 시퀀스 2: 할아버지와의 마지막 대화
    /// </summary>
    private IEnumerator Sequence2_LastConversation()
    {
        currentSequence = 2;
        
        // 라일을 할아버지 침대 옆으로 이동
        if (waypoints.Length > 1)
        {
            yield return StartCoroutine(MoveCharacterToPoint(lyle, waypoints[1], lyleAnimator));
        }
        
        // 할아버지를 바라보도록 방향 설정
        yield return StartCoroutine(SetCharacterDirection(lyleAnimator, "right"));
        
        // 마지막 대화 
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogueRangeByIndex(prologueCSVIndex, 7, 7, Enums.DialogueMode.CutScene);
            yield return new WaitForSeconds(0.5f);
            yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive());
        }
        
        yield return new WaitForSeconds(2f);
    }

    /// <summary>
    /// 시퀀스 3: 이별의 순간
    /// </summary>
    private IEnumerator Sequence3_FarewellMoment()
    {
        currentSequence = 3;

        StartCoroutine( FadeOut());
        
        // 할아버지 캐릭터 서서히 투명하게 (임종 표현)
        if (granpa != null)
        {
            yield return StartCoroutine(FadeOutCharacter(granpa, 2f));
        }
        
        lyle.position = (Vector2)granpa.position + new Vector2(-0.45f, 0);
        StartCoroutine(SetCharacterDirection(lyleAnimator, "down"));
        
        granpaWatch.SetActive(true);
        soupPot.SetActive(false);
        
        yield return StartCoroutine(FadeIn());
        
        
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogueRangeByIndex(prologueCSVIndex, 14, 3, Enums.DialogueMode.CutScene);
            yield return new WaitForSeconds(0.5f);
            yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive());
        }
        yield return new WaitForSeconds(2f);
    }

    /// <summary>
    /// 시퀀스 4: 할아버지 시계와 함께 혼자 남겨진 라일
    /// </summary>
    private IEnumerator Sequence4_AloneWithWatch()
    {
        currentSequence = 4;
        
        // 라일을 시계 앞으로 이동
        if (waypoints.Length > 2)
        {
            yield return StartCoroutine(MoveCharacterToPoint(lyle, waypoints[2], lyleAnimator));
        }
        
        // 시계를 바라보는 방향으로 설정
        yield return StartCoroutine(SetCharacterDirection(lyleAnimator, "up"));
        
        // 혼자 남겨진 라일의 심정 
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogueRangeByIndex(prologueCSVIndex, 17, 1, Enums.DialogueMode.CutScene);
            yield return new WaitForSeconds(0.5f);
            yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive());
        }
        
        yield return new WaitForSeconds(3f);
    }

    /// <summary>
    /// 시퀀스 5: 마무리 및 다음 씬으로 전환
    /// </summary>
    private IEnumerator Sequence5_Conclusion()
    {
        currentSequence = 5;
        
        yield return new WaitForSeconds(2f);
        
        // 페이드 아웃
        yield return StartCoroutine(FadeOut());
        
        yield return new WaitForSeconds(sceneTransitionDelay);
        
        SceneManager.LoadScene(nextSceneName);
    }

    #endregion

    #region 캐릭터 이동 및 애니메이션

    /// <summary>
    /// 캐릭터를 특정 지점으로 이동
    /// </summary>
    private IEnumerator MoveCharacterToPoint(Transform character, Transform targetPoint, Animator animator)
    {
        if(character == null || targetPoint == null) yield break;

        Vector2 startPos = character.position;
        Vector2 targetPos = targetPoint.position;
        
        // 수평 이동
        if (Mathf.Abs(startPos.x - targetPos.x) > 0.1f)
        {
            yield return StartCoroutine(MoveHorizontal(character, targetPos.x, animator));
        }
        
        // 수직 이동
        if (Mathf.Abs(character.position.y - targetPos.y) > 0.1f)
        {
            yield return StartCoroutine(MoveVertical(character, targetPos.y, animator));
        }
        
        character.position = targetPoint.position;
        
        if (animator != null)
        {
            animator.SetBool("isChange", false); 
            animator.SetInteger("h", 0);
            animator.SetInteger("v", 0);
        }
    }
    
    private IEnumerator MoveHorizontal(Transform character, float targetX, Animator animator)
    {
        Vector2 direction = Vector2.right * Mathf.Sign(targetX - character.position.x);
        
        if (animator != null)
        {
            animator.SetInteger("h", (int)direction.x);
            animator.SetInteger("v", 0);
            animator.SetBool("isChange", true);
        }
        
        while (Mathf.Abs(character.position.x - targetX) > 0.1f)
        {
            Vector2 newPos = character.position;
            newPos.x = Mathf.MoveTowards(newPos.x, targetX, moveSpeed * Time.deltaTime);
            character.position = newPos;
            yield return null;
        }
        
        Vector2 correctedPos = character.position;
        correctedPos.x = targetX;
        character.position = correctedPos;
    }
    
    private IEnumerator MoveVertical(Transform character, float targetY, Animator animator)
    {
        Vector2 direction = Vector2.up * Mathf.Sign(targetY - character.position.y);
        
        if (animator != null)
        {
            animator.SetInteger("h", 0);
            animator.SetInteger("v", (int)direction.y);
            animator.SetBool("isChange", true);
        }
        
        while (Mathf.Abs(character.position.y - targetY) > 0.1f)
        {
            Vector2 newPos = character.position;
            newPos.y = Mathf.MoveTowards(newPos.y, targetY, moveSpeed * Time.deltaTime);
            character.position = newPos;
            yield return null;
        }
        
        Vector2 correctedPos = character.position;
        correctedPos.y = targetY;
        character.position = correctedPos;
    }
    
    /// <summary>
    /// 캐릭터 방향 설정
    /// </summary>
    private IEnumerator SetCharacterDirection(Animator animator, string direction)
    {
        if (animator == null) yield break;

        switch (direction.ToLower())
        {
            case "left":
                animator.SetInteger("h", -1);
                animator.SetInteger("v", 0);
                break;
            case "right":
                animator.SetInteger("h", 1);
                animator.SetInteger("v", 0);
                break;
            case "up":
                animator.SetInteger("h", 0);
                animator.SetInteger("v", 1);
                break;
            case "down":
                animator.SetInteger("h", 0);
                animator.SetInteger("v", -1);
                break;
            default:
                yield break;
        }

        animator.SetBool("isChange", true);
        yield return new WaitForSeconds(0.1f);
        
        animator.SetInteger("h", 0);
        animator.SetInteger("v", 0);
    }

    #endregion

    #region 특별 효과

    /// <summary>
    /// 조명을 서서히 어둡게
    /// </summary>
    private IEnumerator DimLight(Light light, float startIntensity, float endIntensity, float duration)
    {
        if (light == null) yield break;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            light.intensity = Mathf.Lerp(startIntensity, endIntensity, elapsedTime / duration);
            yield return null;
        }
        
        light.intensity = endIntensity;
    }

    /// <summary>
    /// 캐릭터를 서서히 투명하게 (임종 표현)
    /// </summary>
    private IEnumerator FadeOutCharacter(Transform character, float duration)
    {
        if (character == null) yield break;
        
        SpriteRenderer spriteRenderer = character.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) yield break;
        
        Color startColor = spriteRenderer.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
        
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            spriteRenderer.color = Color.Lerp(startColor, endColor, elapsedTime / duration);
            yield return null;
        }
        
        spriteRenderer.color = endColor;
        character.gameObject.SetActive(false);
    }

    #endregion

    #region 이벤트 처리

    private void OnDialogueCompleted(string setName)
    {
        //Debug.Log($"대화 완료: {setName}");
    }

    private void OnEventFlagTriggered(string eventFlag)
    {
        switch (eventFlag.ToLower())
        {
            case "scene_start":
                StartCoroutine(FadeIn());
                break;
            
            case "scene_end":
                StartCoroutine(FadeOut());
                break;
            
            case "show_watch":
                if (granpaWatch != null)
                    granpaWatch.SetActive(true);
                break;
        }
    }
    
    #endregion
    
    #region 페이드 효과

    private IEnumerator FadeIn()
    {
        if (fadeImage == null) yield break;

        fadeImage.gameObject.SetActive(true);
        Color color = fadeImage.color;
    
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
        fadeImage.gameObject.SetActive(false);
    }

    private IEnumerator FadeOut()
    {
        if (fadeImage == null) yield break;

        fadeImage.gameObject.SetActive(true);
        Color color = fadeImage.color;
    
        float elapsedTime = 0f;
        color.a = 0f;
        fadeImage.color = color;
    
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
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Prologue1 : MonoBehaviour
{
    [Header("캐릭터")] 
    [SerializeField] private Transform lyle;
    [SerializeField] private Transform granpa;
    [SerializeField] private Animator lyleAnimator;
    [SerializeField] private Animator granpaAnimator;

    [Header("씬")] 
    [SerializeField] private string nextSceneName = "Prologue2";
    [SerializeField] private float sceneTransitionDelay = 2f;

    [Header("이동")] 
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private Transform[] waypoints;

    [Header("CSV 파일")]
    [SerializeField] private string prologueCSVName = "Prologue - Prologue1";
    [SerializeField] private int prologueCSVIndex = 0;
    
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
        StartCoroutine(WaitForDatabaseAndStart());
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
            Debug.LogError("DialogueUI를 찾을 수 없습니다. 프롤로그1 씬에 DialogueUI가 있는지 확인하세요.");
            yield break;
        }
        
        // DialogueManager 초기화 대기
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
        yield return StartCoroutine(Sequence1_MoveToGraveyard());
        yield return StartCoroutine(Sequence2_DerekTombstone());
        yield return StartCoroutine(Sequence3_Conclusion());
    }

    #region 시퀀스

    private IEnumerator Sequence0_Introduction()
    {
        currentSequence = 0;
        
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogueRangeByIndex(prologueCSVIndex, 0, 5, Enums.DialogueMode.CutScene);
            yield return new WaitForSeconds(0.5f);
        }

        // 대화 완료 대기
        yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive());
        yield return new WaitForSeconds(1f);
    }

    private IEnumerator Sequence1_MoveToGraveyard()
    {
        currentSequence = 1;
        
        if (waypoints.Length > 0)
        {
            StartCoroutine(MoveCharacterToPoint(granpa, waypoints[0], granpaAnimator));
            yield return StartCoroutine(MoveCharacterToPoint(lyle, waypoints[0], lyleAnimator));
            
            yield return StartCoroutine(MoveCharacterToPoint(lyle, waypoints[1], lyleAnimator));
        }

        StartCoroutine(SetCharacterDirection(lyleAnimator, "up")); 

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogueRangeByIndex(prologueCSVIndex, 5, 5, Enums.DialogueMode.CutScene);
            yield return new WaitForSeconds(0.5f);
            yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive());
        }
    }

    private IEnumerator Sequence2_DerekTombstone()
    {
        currentSequence = 2;
        
        if (waypoints.Length > 5) // waypoint 6개 이상 필요
        {
            Coroutine lyleMove1 = StartCoroutine(MoveCharacterToPoint(lyle, waypoints[2], lyleAnimator));
            Coroutine granpaMove1 = StartCoroutine(MoveCharacterToPoint(granpa, waypoints[3], granpaAnimator));
    
            yield return lyleMove1;   
            yield return granpaMove1; 
    
            yield return StartCoroutine(MoveCharacterToPoint(lyle, waypoints[4], lyleAnimator));
            yield return StartCoroutine(MoveCharacterToPoint(granpa, waypoints[5], granpaAnimator));
        }
        
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogueRangeByIndex(prologueCSVIndex, 10, 6, Enums.DialogueMode.CutScene);
            yield return new WaitForSeconds(0.5f);
            yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive());
        }
    }

    private IEnumerator Sequence3_Conclusion()
    {
        currentSequence = 3;
        
        yield return new WaitForSeconds(0.5f);
        yield return new WaitForSeconds(sceneTransitionDelay);
        
        SceneManager.LoadScene(nextSceneName);
    }

    #endregion

    #region 캐릭터 이동

    /// <summary>
    /// 격자 기반 이동: 대각선 금지, 상하좌우로만 이동
    /// </summary>
    private IEnumerator MoveCharacterToPoint(Transform character, Transform targetPoint, Animator animator)
    {
        if(character == null || targetPoint == null) yield break;

        Vector2 startPos = character.position;
        Vector2 targetPos = targetPoint.position;
        
        // 1단계: 수평 이동 (좌우)
        if (Mathf.Abs(startPos.x - targetPos.x) > 0.1f)
        {
            yield return StartCoroutine(MoveHorizontal(character, targetPos.x, animator));
        }
        
        // 2단계: 수직 이동 (상하)
        if (Mathf.Abs(character.position.y - targetPos.y) > 0.1f)
        {
            yield return StartCoroutine(MoveVertical(character, targetPos.y, animator));
        }
        
        // 최종 위치 보정
        character.position = targetPoint.position;
        
        // 애니메이션 정리
        if (animator != null)
        {
            animator.SetBool("isChange", false); 
            animator.SetInteger("h", 0);
            animator.SetInteger("v", 0);
        }
    }
    
    /// <summary>
    /// 수평 이동 (좌우)
    /// </summary>
    private IEnumerator MoveHorizontal(Transform character, float targetX, Animator animator)
    {
        Vector2 direction = Vector2.right * Mathf.Sign(targetX - character.position.x);
        
        // 애니메이션 설정
        if (animator != null)
        {
            animator.SetInteger("h", (int)direction.x);
            animator.SetInteger("v", 0);
            animator.SetBool("isChange", true);
        }
        
        // 수평 이동
        while (Mathf.Abs(character.position.x - targetX) > 0.1f)
        {
            Vector2 newPos = character.position;
            newPos.x = Mathf.MoveTowards(newPos.x, targetX, moveSpeed * Time.deltaTime);
            character.position = newPos;
            yield return null;
        }
        
        // X 좌표 보정
        Vector2 correctedPos = character.position;
        correctedPos.x = targetX;
        character.position = correctedPos;
    }
    
    /// <summary>
    /// 수직 이동 (상하)
    /// </summary>
    private IEnumerator MoveVertical(Transform character, float targetY, Animator animator)
    {
        Vector2 direction = Vector2.up * Mathf.Sign(targetY - character.position.y);
        
        // 애니메이션 설정
        if (animator != null)
        {
            animator.SetInteger("h", 0);
            animator.SetInteger("v", (int)direction.y);
            animator.SetBool("isChange", true);
        }
        
        // 수직 이동
        while (Mathf.Abs(character.position.y - targetY) > 0.1f)
        {
            Vector2 newPos = character.position;
            newPos.y = Mathf.MoveTowards(newPos.y, targetY, moveSpeed * Time.deltaTime);
            character.position = newPos;
            yield return null;
        }
        
        // Y 좌표 보정
        Vector2 correctedPos = character.position;
        correctedPos.y = targetY;
        character.position = correctedPos;
    }
    
    /// <summary>
    /// 캐릭터의 보는 방향 설정
    /// </summary>
    /// <param name="animator">대상 애니메이터</param>
    /// <param name="direction">보는 방향 (Left, Right, Up, Down)</param>
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

        animator.SetBool("isChange", true);  // Walk 상태로 전환
    
        yield return new WaitForSeconds(0.1f); // 애니메이션 전환 대기
    
        animator.SetInteger("h", 0);  // ✅ 이게 핵심!
        animator.SetInteger("v", 0);  // ✅ 이게 핵심!
    }

    #endregion

    private void OnDialogueCompleted(string setName)
    {
        // 필요시 처리
    }

    private void OnEventFlagTriggered(string eventFlag)
    {
        switch (eventFlag.ToLower())
        {
            case "scene_start":
                StartCoroutine(FadeIn());
                break;
            
            case "scene_end":
                StartCoroutine(WaitForUserInputThenFadeOut());
                break;
        }
    }
    
    #region 페이드 효과

    /// <summary>
    /// 페이드 인 (검은화면 → 투명)
    /// </summary>
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

    /// <summary>
    /// 페이드 아웃 (투명 → 검은화면)
    /// </summary>
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
    
    private IEnumerator WaitForUserInput()
    {
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Space) || 
                Input.GetKeyDown(KeyCode.Return) || 
                Input.GetMouseButtonDown(0))
            {
                break;
            }
            yield return null;
        }
    }
    
    // 사용자 입력 대기 후 페이드아웃 실행
    private IEnumerator WaitForUserInputThenFadeOut()
    {
        // 대화가 완전히 끝날 때까지 대기 (타이핑 완료 등)
        yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive());
    
        // 사용자 입력 대기
        yield return StartCoroutine(WaitForUserInput());
    
        // 사용자 입력 후 페이드아웃 실행
        yield return StartCoroutine(FadeOut());
    }
    
}
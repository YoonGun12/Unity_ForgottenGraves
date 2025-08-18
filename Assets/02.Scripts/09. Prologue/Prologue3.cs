using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Prologue3 : MonoBehaviour
{
    [Header("캐릭터")] 
    [SerializeField] private Transform lyle;
    [SerializeField] private Transform ria;
    [SerializeField] private Animator lyleAnimator;
    [SerializeField] private Animator riaAnimator;

    [Header("씬")] 
    [SerializeField] private string nextSceneName = "Cemetery_Hub";
    [SerializeField] private float sceneTransitionDelay = 2f;

    [Header("이동")] 
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private Transform[] waypoints; // 이동 지점들

    [Header("특별 오브젝트")]
    [SerializeField] private GameObject granpaWatch;       // 할아버지 시계

    [Header("CSV 파일")]
    [SerializeField] private string prologueCSVName = "Prologue - Prologue3";
    [SerializeField] private int prologueCSVIndex = 2;
    
    [Header("페이드 효과")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 3f;

    [Header("리아 등장 효과")]
    [SerializeField] private float riaFadeInDuration = 2f;
   
    private bool isPlaying = false;
    private int currentSequence = 0;
    private Ria riaScript;

    private void Start()
    {
        InitializeScene();
        StartCoroutine(WaitForDatabaseAndStart());
    }

    /// <summary>
    /// 씬 초기 설정
    /// </summary>
    private void InitializeScene()
    {
        //페이드 이미지 초기화
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            Color color = fadeImage.color;
            color.a = 1f;
            fadeImage.color = color;
        }
        
        // 할아버지 시계 활성화 (라일이 바라보고 있음)
        if (granpaWatch != null)
            granpaWatch.SetActive(true);
            
        
        // 리아 처음에는 비활성화
        if (ria != null)
        {
            ria.gameObject.SetActive(false);
            riaScript = ria.GetComponent<Ria>();
        }

        if (lyleAnimator != null)
        {
            StartCoroutine(SetCharacterDirection(lyleAnimator, "up"));
        }
            
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
            Debug.LogError("DialogueUI를 찾을 수 없습니다. 프롤로그3 씬에 DialogueUI가 있는지 확인하세요.");
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
        yield return StartCoroutine(Sequence0_LyleAlone());
        yield return StartCoroutine(Sequence1_RiaEntersAndApproaches()); // 리아 등장 + 이동 + 첫 대화
        yield return StartCoroutine(Sequence2_MainConversation()); // 본격적인 대화
        yield return StartCoroutine(Sequence3_LeavingTogether()); // 함께 나가기
        yield return StartCoroutine(Sequence4_Conclusion()); // 마무리
    }

    #region 시퀀스

    /// <summary>
    /// 시퀀스 0: 라일 혼자 있는 모습
    /// </summary>
    private IEnumerator Sequence0_LyleAlone()
    {
        currentSequence = 0;
    
        // 라일이 시계를 바라보는 방향으로 설정
        yield return StartCoroutine(SetCharacterDirection(lyleAnimator, "up"));
    
        // 페이드 인
        yield return StartCoroutine(FadeIn());
    
        // 첫 번째 내래이션 (라일이 여전히 집에서 지내고 있다는 내용)
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogueRangeByIndex(prologueCSVIndex, 0, 1, Enums.DialogueMode.CutScene);
            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive());
        yield return new WaitForSeconds(0.5f); // 잠시 대기
    }

    /// <summary>
    /// 시퀀스 1: 리아 등장, 이동, 첫 대화 (수정됨)
    /// </summary>
    private IEnumerator Sequence1_RiaEntersAndApproaches()
    {
        currentSequence = 1;
        
        // 리아 등장 
        if (ria != null && waypoints.Length > 5)
        {
            ria.gameObject.SetActive(true);
            ria.position = waypoints[5].position;
            
            if (riaScript != null)
            {
                // ✅ 리아를 완전히 일반 캐릭터 모드로 설정
                riaScript.SetNormalCharacterMode(true);
                riaScript.SetGhostEffectActive(false);
                riaScript.SetVisible(true);
                riaScript.SetFloatingSettings(0f, 0f);
                riaScript.SetFlickeringEnabled(false);
            }
            
            yield return StartCoroutine(SetCharacterDirection(riaAnimator, "up"));
        }
        
        yield return new WaitForSeconds(0.5f); 
        
        // ✅ 각 이동마다 명확하게 설정
        if (waypoints.Length > 0)
        {
            yield return StartCoroutine(MoveCharacterToPoint(ria, waypoints[0], riaAnimator));
            yield return new WaitForSeconds(0.2f);
        }
        
        if (waypoints.Length > 1)
        {
            yield return StartCoroutine(MoveCharacterToPoint(ria, waypoints[1], riaAnimator));
            yield return new WaitForSeconds(0.2f);
        }
        
        if (waypoints.Length > 2)
        {
            yield return StartCoroutine(MoveCharacterToPoint(ria, waypoints[2], riaAnimator));
            yield return new WaitForSeconds(0.2f);
        }
        
        // 대화 진행...
        yield return new WaitForSeconds(1f); 
        
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.SetAutoAdvance(false, 0f);
        }
        
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogueRangeByIndex(prologueCSVIndex, 1, 1, Enums.DialogueMode.Normal);
            yield return new WaitForSeconds(0.5f);
            yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive());
        }
        yield return new WaitForSeconds(1f); 
        
        // 라일이 뒤를 돌아봄
        yield return StartCoroutine(SetCharacterDirection(lyleAnimator, "down"));
        yield return new WaitForSeconds(0.5f);
        
        // 후속 대화들...
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogueRangeByIndex(prologueCSVIndex, 2, 1, Enums.DialogueMode.Normal);
            yield return new WaitForSeconds(0.5f);
            yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive());
        }
        yield return new WaitForSeconds(1f);
        
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogueRangeByIndex(prologueCSVIndex, 3, 1, Enums.DialogueMode.Normal);
            yield return new WaitForSeconds(0.5f);
            yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive());
        }
        
        yield return new WaitForSeconds(1f); 
        
        // 라일이 침대로 도망
        if (waypoints.Length > 3)
        {
            yield return StartCoroutine(MoveCharacterToPoint(lyle, waypoints[3], lyleAnimator));
            
            if (waypoints.Length > 4)
            {
                yield return StartCoroutine(MoveCharacterToPoint(ria, waypoints[4], riaAnimator));
            }
        }
        
        yield return new WaitForSeconds(0.5f);
        
        // 방향 설정 
        yield return StartCoroutine(SetCharacterDirection(lyleAnimator, "up"));
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(SetCharacterDirection(riaAnimator, "right"));
        yield return new WaitForSeconds(0.5f);
        
        // 마지막 대화
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogueRangeByIndex(prologueCSVIndex, 4, 1, Enums.DialogueMode.Normal);
            yield return new WaitForSeconds(0.5f);
            yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive());
        }
        
        yield return new WaitForSeconds(1f);
    }

    /// <summary>
    /// 시퀀스 2: 본격적인 대화 (기존 Sequence3 + Sequence4 통합)
    /// </summary>
    private IEnumerator Sequence2_MainConversation()
    {
        currentSequence = 2;
    
        // 라일이 왼쪽을 바라봄 (리아와 대화하기 위해)
        yield return StartCoroutine(SetCharacterDirection(lyleAnimator, "left"));
    
        // <<<<<<< 수정: 본격적인 대화 시작 (인덱스 5부터)
        // 리아의 자기소개와 격려 대화
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogueRangeByIndex(prologueCSVIndex, 5, 15, Enums.DialogueMode.CutScene); // 인덱스 5부터 15개 정도
            yield return new WaitForSeconds(0.5f);
            yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive());
        }
        
        yield return new WaitForSeconds(0.5f);
    
        // 시계 이야기가 나오면 리아가 위쪽을 바라봄 (시계 방향)
        yield return StartCoroutine(SetCharacterDirection(riaAnimator, "up"));
        
    
        // 시계 관련 대화
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogueRangeByIndex(prologueCSVIndex, 20, 3, Enums.DialogueMode.CutScene);
            yield return new WaitForSeconds(0.5f);
            yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive());
        }
        
        yield return new WaitForSeconds(0.5f);
    
        // 대화 후 리아가 다시 라일 쪽을 바라봄
        yield return StartCoroutine(SetCharacterDirection(riaAnimator, "right"));
    
        yield return new WaitForSeconds(1f);
    }

    /// <summary>
    /// 시퀀스 3: 함께 밖으로 나가기 (리아 이동 설정 강화)
    /// </summary>
    private IEnumerator Sequence3_LeavingTogether()
    {
        currentSequence = 3;
        
        // 마지막 대화
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogueRangeByIndex(prologueCSVIndex, 23, 2, Enums.DialogueMode.CutScene);
            yield return new WaitForSeconds(0.5f);
            yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive());
        }
        
        // ✅ 연속 이동을 위한 리아 초기 설정
        if (riaScript != null)
        {
            riaScript.SetNormalCharacterMode(true);
            riaScript.SetGhostEffectActive(false);
            riaScript.SetFloatingSettings(0f, 0f);
            riaScript.SetFlickeringEnabled(false);
        }
        
        // 새로운 경로: WP2 → WP1 → WP0 → WP5 순으로 이동
        if (waypoints.Length > 5)
        {
            // 1단계: WP2로 이동 (리아는 이미 WP4에 있고, 라일만 이동)
            Coroutine lyleToWP2 = StartCoroutine(MoveCharacterToPoint(lyle, waypoints[2], lyleAnimator));
            yield return lyleToWP2;
            yield return new WaitForSeconds(0.3f);
            
            // 2단계: 둘이 함께 WP1으로 이동
            granpaWatch.SetActive(false);
            
            // ✅ 리아 이동 전 상태 재확인
            if (riaScript != null)
            {
                riaScript.SetNormalCharacterMode(true);
                riaScript.SetMoving(true);
            }
            
            Coroutine riaToWP1 = StartCoroutine(MoveCharacterToPoint(ria, waypoints[1], riaAnimator));
            Coroutine lyleToWP1 = StartCoroutine(MoveCharacterToPoint(lyle, waypoints[1], lyleAnimator));
            yield return riaToWP1;
            yield return lyleToWP1;
            yield return new WaitForSeconds(0.3f);
            
            // 3단계: 둘이 함께 WP0으로 이동
            
            // ✅ 리아 이동 전 상태 재확인
            if (riaScript != null)
            {
                riaScript.SetNormalCharacterMode(true);
                riaScript.SetMoving(true);
            }
            
            Coroutine riaToWP0 = StartCoroutine(MoveCharacterToPoint(ria, waypoints[0], riaAnimator));
            Coroutine lyleToWP0 = StartCoroutine(MoveCharacterToPoint(lyle, waypoints[0], lyleAnimator));
            yield return riaToWP0;
            yield return lyleToWP0;
            yield return new WaitForSeconds(0.3f);
            
            // 4단계: 마지막으로 WP5(문)로 이동
            
            // ✅ 리아 이동 전 상태 재확인
            if (riaScript != null)
            {
                riaScript.SetNormalCharacterMode(true);
                riaScript.SetMoving(true);
            }
            
            Coroutine riaToWP5 = StartCoroutine(MoveCharacterToPoint(ria, waypoints[5], riaAnimator));
            Coroutine lyleToWP5 = StartCoroutine(MoveCharacterToPoint(lyle, waypoints[5], lyleAnimator));
            yield return riaToWP5;
            yield return lyleToWP5;
        }
        
        // 문을 향해 아래쪽을 바라봄
        yield return StartCoroutine(SetCharacterDirection(lyleAnimator, "down"));
        yield return StartCoroutine(SetCharacterDirection(riaAnimator, "down"));
        
        yield return new WaitForSeconds(1f);
    }

    /// <summary>
    /// 시퀀스 4: 마무리 및 다음 씬으로 전환 (기존 Sequence6)
    /// </summary>
    private IEnumerator Sequence4_Conclusion()
    {
        currentSequence = 4;
    
        yield return new WaitForSeconds(1f);
    
        // 페이드 아웃
        yield return StartCoroutine(FadeOut());
    
        yield return new WaitForSeconds(sceneTransitionDelay);
    
        // 다음 씬 로드
        SceneManager.LoadScene(nextSceneName);
    }

    

    #endregion

    #region 캐릭터 이동 및 애니메이션

    /// <summary>
    /// 캐릭터를 특정 지점으로 이동 (리아용 특별 처리 및 디버깅 포함)
    /// </summary>
    private IEnumerator MoveCharacterToPoint(Transform character, Transform targetPoint, Animator animator)
    {
        if(character == null || targetPoint == null) yield break;

        string characterName = character == ria ? "리아" : "라일";

        // ✅ 리아인 경우 이동 모드 활성화
        bool isRiaCharacter = character == ria;
        if (isRiaCharacter && riaScript != null)
        {
            riaScript.SetNormalCharacterMode(true);
            riaScript.SetMoving(true);
            riaScript.SetGhostEffectActive(false);
        }

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

        // ✅ 리아인 경우 이동 모드 비활성화 및 위치 업데이트
        if (isRiaCharacter && riaScript != null)
        {
            riaScript.SetMoving(false);
            riaScript.SetOriginalPosition(targetPoint.position);
        }
        
    }

    private IEnumerator MoveHorizontal(Transform character, float targetX, Animator animator)
    {
        string characterName = character == ria ? "리아" : "라일";
        Vector2 direction = Vector2.right * Mathf.Sign(targetX - character.position.x);
        
        if (animator != null)
        {
            animator.SetInteger("h", (int)direction.x);
            animator.SetInteger("v", 0);
            animator.SetBool("isChange", true);
            
            // 애니메이션 상태 확인
            yield return new WaitForEndOfFrame();
        }
        
        float startTime = Time.time;
        while (Mathf.Abs(character.position.x - targetX) > 0.1f)
        {
            Vector2 newPos = character.position;
            float oldX = newPos.x;
            newPos.x = Mathf.MoveTowards(newPos.x, targetX, moveSpeed * Time.deltaTime);
            character.position = newPos;
            
            // 5초마다 진행 상황 로그
            if (Time.time - startTime > 1f)
            {
                startTime = Time.time;
            }
            
            yield return null;
        }
        
        Vector2 correctedPos = character.position;
        correctedPos.x = targetX;
        character.position = correctedPos;
    }

    private IEnumerator MoveVertical(Transform character, float targetY, Animator animator)
    {
        string characterName = character == ria ? "리아" : "라일";
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
            float oldY = newPos.y;
            newPos.y = Mathf.MoveTowards(newPos.y, targetY, moveSpeed * Time.deltaTime);
            character.position = newPos;
            
            yield return null;
        }
        
        Vector2 correctedPos = character.position;
        correctedPos.y = targetY;
        character.position = correctedPos;
        
    }

    /// <summary>
    /// 캐릭터 방향 설정 (리아용 특별 처리 및 디버깅 포함)
    /// </summary>
    private IEnumerator SetCharacterDirection(Animator animator, string direction)
    {
        if (animator == null) yield break;

        string characterName = animator == riaAnimator ? "리아" : "라일";

        // ✅ 리아인 경우 잠시 일반 캐릭터 모드로 전환
        bool isRiaAnimator = animator == riaAnimator;
        if (isRiaAnimator && riaScript != null)
        {
            riaScript.SetNormalCharacterMode(true);
        }

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
    /// 캐릭터를 서서히 나타나게 (리아의 등장 효과)
    /// </summary>
    private IEnumerator FadeInCharacter(Transform character, float duration)
    {
        if (character == null) yield break;
        
        SpriteRenderer spriteRenderer = character.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) yield break;
        
        Color startColor = spriteRenderer.color;
        startColor.a = 0f;
        spriteRenderer.color = startColor;
        
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 1f);
        
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            spriteRenderer.color = Color.Lerp(startColor, endColor, elapsedTime / duration);
            yield return null;
        }
        
        spriteRenderer.color = endColor;
    }

    /// <summary>
    /// 캐릭터를 서서히 투명하게
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
            
            case "show_ria":
                if (ria != null && !ria.gameObject.activeInHierarchy)
                {
                    ria.gameObject.SetActive(true);
                    StartCoroutine(FadeInCharacter(ria, riaFadeInDuration));
                }
                break;
                
            case "hide_lyle":
                // 라일이 숨는 애니메이션이나 효과
                if (lyleAnimator != null)
                {
                    StartCoroutine(SetCharacterDirection(lyleAnimator, "down"));
                }
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
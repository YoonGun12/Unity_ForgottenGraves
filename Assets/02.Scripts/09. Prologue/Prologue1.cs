using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    private bool isPlaying = false;
    private int currentSequence = 0;

    private void Start()
    {
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
        }

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
        
        if (waypoints.Length > 1)
        {
            StartCoroutine(MoveCharacterToPoint(granpa, waypoints[1], granpaAnimator));
            yield return StartCoroutine(MoveCharacterToPoint(lyle, waypoints[1], lyleAnimator));
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
        
        yield return new WaitForSeconds(2f);
        yield return new WaitForSeconds(sceneTransitionDelay);
        
        SceneManager.LoadScene(nextSceneName);
    }

    #endregion

    #region 캐릭터 이동

    private IEnumerator MoveCharacterToPoint(Transform character, Transform targetPoint, Animator animator)
    {
        if(character == null || targetPoint == null) yield break;

        if (animator != null)
        {
            Vector2 direction = (targetPoint.position - character.position).normalized;
            animator.SetFloat("h", direction.x);
            animator.SetFloat("v", direction.y);
            animator.SetBool("isMoving", true);
        }

        while (Vector2.Distance(character.position, targetPoint.position) > 0.1f)
        {
            character.position = Vector2.MoveTowards(character.position, targetPoint.position, moveSpeed * Time.deltaTime);
            yield return null;
        }

        character.position = targetPoint.position;

        if (animator != null)
        {
            animator.SetBool("isMoving", false);
        }
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
            case "look_around":
                break;
            case "scene_change":
                break;
        }
    }
}
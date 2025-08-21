using System.Collections;
using UnityEngine;

/// <summary>
/// 프롤로그 3 - 리아의 등장과 라일과의 첫 만남
/// </summary>
public class Prologue3 : Prologue
{
    [Header("특별 오브젝트")]
    [SerializeField] private GameObject granpaWatch;

    [Header("리아 등장 효과")]
    [SerializeField] private float riaFadeInDuration = 2f;

    private Ria riaScript;

    protected override void InitializeScene()
    {
        // 할아버지 시계 활성화 (라일이 바라보고 있음)
        if (granpaWatch != null)
            granpaWatch.SetActive(true);
            
        // 리아 처음에는 비활성화
        if (ria != null)
        {
            ria.gameObject.SetActive(false);
            riaScript = ria.GetComponent<Ria>();
        }

        // 라일 초기 방향 설정
        if (lyleAnimator != null)
        {
            StartCoroutine(SetCharacterDirection(lyleAnimator, "up"));
        }
    }

    protected override IEnumerator PlayPrologueSequence()
    {
        yield return StartCoroutine(Sequence0_LyleAlone());
        yield return StartCoroutine(Sequence1_RiaEntersAndApproaches());
        yield return StartCoroutine(Sequence2_MainConversation());
        yield return StartCoroutine(Sequence3_LeavingTogether());
        yield return StartCoroutine(Sequence4_Conclusion());
    }

    #region 시퀀스들

    private IEnumerator Sequence0_LyleAlone()
    {
        currentSequence = 0;
        yield return StartCoroutine(SetCharacterDirection(lyleAnimator, "up"));
        yield return StartCoroutine(FadeIn());
        yield return StartCoroutine(StartDialogueAndWait(0, 1));
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator Sequence1_RiaEntersAndApproaches()
    {
        currentSequence = 1;
        
        // 리아 등장
        if (ria != null && waypoints.Length > 5)
        {
            ria.gameObject.SetActive(true);
            ria.position = waypoints[5].position;
            
            SetupRiaEffects();
            yield return StartCoroutine(SetCharacterDirection(riaAnimator, "up"));
        }
        
        yield return new WaitForSeconds(0.5f);
        
        // 리아 이동 시퀀스
        yield return StartCoroutine(MoveRiaToTarget(0));
        yield return StartCoroutine(MoveRiaToTarget(1));
        yield return StartCoroutine(MoveRiaToTarget(2));
        
        yield return new WaitForSeconds(1f);
        
        // 대화 모드 설정
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.SetAutoAdvance(false, 0f);
        }
        
        // 첫 대화들
        yield return StartCoroutine(StartDialogueAndWait(1, 1, Enums.DialogueMode.Normal));
        yield return new WaitForSeconds(1f);
        
        // 라일이 뒤를 돌아봄
        yield return StartCoroutine(SetCharacterDirection(lyleAnimator, "down"));
        yield return new WaitForSeconds(0.5f);
        
        // 후속 대화들
        yield return StartCoroutine(StartDialogueAndWait(2, 1, Enums.DialogueMode.Normal));
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(StartDialogueAndWait(3, 1, Enums.DialogueMode.Normal));
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
        yield return StartCoroutine(StartDialogueAndWait(4, 1, Enums.DialogueMode.Normal));
        yield return new WaitForSeconds(1f);
    }

    private IEnumerator Sequence2_MainConversation()
    {
        currentSequence = 2;
        
        yield return StartCoroutine(SetCharacterDirection(lyleAnimator, "left"));
        
        // 리아와 본격 대화
        yield return StartCoroutine(StartDialogueAndWait(5, 15));
        yield return new WaitForSeconds(0.5f);
        
        // 시계 이야기가 나오면 리아가 위쪽을 바라봄
        yield return StartCoroutine(SetCharacterDirection(riaAnimator, "up"));
        
        // 시계 관련 대화
        yield return StartCoroutine(StartDialogueAndWait(20, 3));
        yield return new WaitForSeconds(0.5f);
        
        // 대화 후 리아가 다시 라일 쪽을 바라봄
        yield return StartCoroutine(SetCharacterDirection(riaAnimator, "right"));
        yield return new WaitForSeconds(1f);
    }

    private IEnumerator Sequence3_LeavingTogether()
    {
        currentSequence = 3;
        
        yield return StartCoroutine(StartDialogueAndWait(23, 2));
        
        // 리아 이동 설정
        if (riaScript != null)
        {
            riaScript.SetMoving(true);
        }
        
        // 함께 나가는 경로: WP2 → WP1 → WP0 → WP5
        if (waypoints.Length > 5)
        {
            // 1단계: WP2로 이동 (라일만)
            yield return StartCoroutine(MoveCharacterToPoint(lyle, waypoints[2], lyleAnimator));
            yield return new WaitForSeconds(0.3f);
            
            // 2단계: 둘이 함께 WP1으로 이동
            if (granpaWatch != null) granpaWatch.SetActive(false);
            
            var riaToWP1 = StartCoroutine(MoveCharacterToPoint(ria, waypoints[1], riaAnimator));
            var lyleToWP1 = StartCoroutine(MoveCharacterToPoint(lyle, waypoints[1], lyleAnimator));
            yield return riaToWP1;
            yield return lyleToWP1;
            yield return new WaitForSeconds(0.3f);
            
            // 3단계: 둘이 함께 WP0으로 이동
            var riaToWP0 = StartCoroutine(MoveCharacterToPoint(ria, waypoints[0], riaAnimator));
            var lyleToWP0 = StartCoroutine(MoveCharacterToPoint(lyle, waypoints[0], lyleAnimator));
            yield return riaToWP0;
            yield return lyleToWP0;
            yield return new WaitForSeconds(0.3f);
            
            // 4단계: 마지막으로 WP5(문)로 이동
            var riaToWP5 = StartCoroutine(MoveCharacterToPoint(ria, waypoints[5], riaAnimator));
            var lyleToWP5 = StartCoroutine(MoveCharacterToPoint(lyle, waypoints[5], lyleAnimator));
            yield return riaToWP5;
            yield return lyleToWP5;
        }

        if (riaScript != null)
        {
            riaScript.SetMoving(false);
        }
        
        // 문을 향해 아래쪽을 바라봄
        yield return StartCoroutine(SetCharacterDirection(lyleAnimator, "down"));
        yield return StartCoroutine(SetCharacterDirection(riaAnimator, "down"));
        yield return new WaitForSeconds(1f);
    }

    private IEnumerator Sequence4_Conclusion()
    {
        currentSequence = 4;
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(TransitionToNextScene());
    }

    #endregion

    #region 헬퍼 메서드

    private void SetupRiaEffects()
    {
        if (riaScript != null)
        {
            riaScript.SetGhostEffectActive(false);
            riaScript.SetFadeEffectEnabled(true);
            riaScript.SetFloatingEffectEnabled(true);
            riaScript.SetVisible(true);
            riaScript.SetFlickeringEnabled(false);
        }
    }

    private IEnumerator MoveRiaToTarget(int waypointIndex)
    {
        if (waypoints.Length > waypointIndex)
        {
            yield return StartCoroutine(MoveCharacterToPoint(ria, waypoints[waypointIndex], riaAnimator));
            yield return new WaitForSeconds(0.2f);
        }
    }

    #endregion

    protected override void HandleCustomEventFlag(string eventFlag)
    {
        switch (eventFlag.ToLower())
        {
            case "show_ria":
                if (ria != null && !ria.gameObject.activeInHierarchy)
                {
                    ria.gameObject.SetActive(true);
                    StartCoroutine(FadeInCharacter(ria, riaFadeInDuration));
                }
                break;
                
            case "hide_lyle":
                if (lyleAnimator != null)
                {
                    StartCoroutine(SetCharacterDirection(lyleAnimator, "down"));
                }
                break;
        }
    }
}
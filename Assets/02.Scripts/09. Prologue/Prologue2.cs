using System.Collections;
using UnityEngine;

/// <summary>
/// 프롤로그 2 - 할아버지의 임종과 이별
/// </summary>
public class Prologue2 : Prologue
{
    [Header("특별 오브젝트")]
    [SerializeField] private GameObject soupPot;
    [SerializeField] private GameObject granpaWatch;

    protected override void InitializeScene()
    {
        // 할아버지 시계 비활성화
        if (granpaWatch != null)
            granpaWatch.SetActive(false);
    }

    protected override IEnumerator PlayPrologueSequence()
    {
        yield return StartCoroutine(Sequence0_Introduction());
        yield return StartCoroutine(Sequence1_CookingSoup());
        yield return StartCoroutine(Sequence2_LastConversation());
        yield return StartCoroutine(Sequence3_FarewellMoment());
        yield return StartCoroutine(Sequence4_AloneWithWatch());
        yield return StartCoroutine(Sequence5_Conclusion());
    }

    #region 시퀀스들

    private IEnumerator Sequence0_Introduction()
    {
        currentSequence = 0;
        yield return StartCoroutine(SetCharacterDirection(lyleAnimator, "left"));
        yield return StartCoroutine(StartDialogueAndWait(0, 2));
        yield return new WaitForSeconds(1f);
    }

    private IEnumerator Sequence1_CookingSoup()
    {
        currentSequence = 1;
        
        // 라일을 요리 공간으로 이동
        if (waypoints.Length > 0)
        {
            yield return StartCoroutine(MoveCharacterToPoint(lyle, waypoints[0], lyleAnimator));
        }
        
        yield return StartCoroutine(SetCharacterDirection(lyleAnimator, "up"));
        
        // 요리 관련 대화
        yield return StartCoroutine(StartDialogueAndWait(2, 2));
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(SetCharacterDirection(lyleAnimator, "right"));
        yield return StartCoroutine(StartDialogueAndWait(4, 3));
        yield return new WaitForSeconds(1f);
    }

    private IEnumerator Sequence2_LastConversation()
    {
        currentSequence = 2;
        
        // 라일을 할아버지 침대 옆으로 이동
        if (waypoints.Length > 1)
        {
            yield return StartCoroutine(MoveCharacterToPoint(lyle, waypoints[1], lyleAnimator));
        }
        
        yield return StartCoroutine(SetCharacterDirection(lyleAnimator, "right"));
        yield return StartCoroutine(StartDialogueAndWait(7, 7));
        yield return new WaitForSeconds(2f);
    }

    private IEnumerator Sequence3_FarewellMoment()
    {
        currentSequence = 3;

        // 페이드 아웃과 함께 할아버지 사라짐 연출
        StartCoroutine(FadeOut());
        
        if (granpa != null)
        {
            yield return StartCoroutine(FadeOutCharacter(granpa, 2f));
        }
        
        // 라일 위치 조정 및 오브젝트 변경
        lyle.position = (Vector2)granpa.position + new Vector2(-0.45f, 0);
        yield return StartCoroutine(SetCharacterDirection(lyleAnimator, "down"));
        
        if (granpaWatch != null) granpaWatch.SetActive(true);
        if (soupPot != null) soupPot.SetActive(false);
        
        yield return StartCoroutine(FadeIn());
        yield return StartCoroutine(StartDialogueAndWait(14, 3));
        yield return new WaitForSeconds(2f);
    }

    private IEnumerator Sequence4_AloneWithWatch()
    {
        currentSequence = 4;
        
        // 라일을 시계 앞으로 이동
        if (waypoints.Length > 2)
        {
            yield return StartCoroutine(MoveCharacterToPoint(lyle, waypoints[2], lyleAnimator));
        }
        
        yield return StartCoroutine(SetCharacterDirection(lyleAnimator, "up"));
        yield return StartCoroutine(StartDialogueAndWait(17, 1));
        yield return new WaitForSeconds(1f);
    }

    private IEnumerator Sequence5_Conclusion()
    {
        currentSequence = 5;
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(TransitionToNextScene());
    }

    #endregion

    protected override void HandleCustomEventFlag(string eventFlag)
    {
        switch (eventFlag.ToLower())
        {
            case "show_watch":
                if (granpaWatch != null)
                    granpaWatch.SetActive(true);
                break;
        }
    }
}
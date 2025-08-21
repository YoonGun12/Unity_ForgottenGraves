using System.Collections;
using UnityEngine;

/// <summary>
/// 프롤로그 1 - 라일과 할아버지가 묘지를 둘러보는 씬
/// </summary>
public class Prologue1 : Prologue
{
    protected override void InitializeScene()
    {
        // Prologue1 특별한 초기화가 필요하면 여기에 작성
        // 현재는 특별한 초기화 없음
    }

    protected override IEnumerator PlayPrologueSequence()
    {
        yield return StartCoroutine(Sequence0_Introduction());
        yield return StartCoroutine(Sequence1_MoveToGraveyard());
        yield return StartCoroutine(Sequence2_DerekTombstone());
        yield return StartCoroutine(Sequence3_Conclusion());
    }

    #region 시퀀스들

    private IEnumerator Sequence0_Introduction()
    {
        currentSequence = 0;
        yield return StartCoroutine(StartDialogueAndWait(0, 5));
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

        yield return StartCoroutine(SetCharacterDirection(lyleAnimator, "up"));
        yield return StartCoroutine(StartDialogueAndWait(5, 5));
    }

    private IEnumerator Sequence2_DerekTombstone()
    {
        currentSequence = 2;
        
        if (waypoints.Length > 5)
        {
            Coroutine lyleMove1 = StartCoroutine(MoveCharacterToPoint(lyle, waypoints[2], lyleAnimator));
            Coroutine granpaMove1 = StartCoroutine(MoveCharacterToPoint(granpa, waypoints[3], granpaAnimator));
    
            yield return lyleMove1;   
            yield return granpaMove1; 
    
            yield return StartCoroutine(MoveCharacterToPoint(lyle, waypoints[4], lyleAnimator));
            yield return StartCoroutine(MoveCharacterToPoint(granpa, waypoints[5], granpaAnimator));
        }
        
        yield return StartCoroutine(StartDialogueAndWait(10, 6));
    }

    private IEnumerator Sequence3_Conclusion()
    {
        currentSequence = 3;
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(TransitionToNextScene());
    }

    #endregion
}
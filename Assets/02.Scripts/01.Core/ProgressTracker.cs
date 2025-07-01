using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressTracker : MonoBehaviour
{
    // 현재 진행도 정보
    private int currentCompletedCount = 0;
    private bool[] currentTombstoneStates = new bool[5];
    
    public Action<int> OnProgressChanged;                           // 진행도 변경 이벤트
    public Action<Enums.TombstoneType> OnTombstoneCompleted;        // 묘비 완료 이벤트
    public Action OnAllCompleted; 
    
    public void UpdateProgress(int completedCount, bool[] tombstoneStates)
    {
        if (tombstoneStates == null || tombstoneStates.Length != 5) return;
        
        int previousCount = currentCompletedCount;
        currentCompletedCount = completedCount;
        
        // 묘비 상태 업데이트
        for (int i = 0; i < tombstoneStates.Length; i++)
        {
            bool wasCompleted = currentTombstoneStates[i];
            currentTombstoneStates[i] = tombstoneStates[i];
            
            // 새로 완료된 묘비가 있으면 이벤트 발생
            if (!wasCompleted && tombstoneStates[i])
            {
                OnTombstoneCompleted?.Invoke((Enums.TombstoneType)i);
                Debug.Log($"{(Enums.TombstoneType)i} 에피소드가 완료되었습니다!");
            }
        }
        
        // 진행도 변경 이벤트
        if (previousCount != currentCompletedCount)
        {
            OnProgressChanged?.Invoke(currentCompletedCount);
        }
        
        // 모든 에피소드 완료 체크
        if (currentCompletedCount >= 5)
        {
            OnAllCompleted?.Invoke();
            Debug.Log("모든 에피소드를 완료했습니다!");
        }
        
    }
}

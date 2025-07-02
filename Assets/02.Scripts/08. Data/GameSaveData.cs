using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSaveData
{
    [Header("진행도 정보")] 
    public int completedTombstones = 0;
    public bool[] tombstoneCompleted = new bool[5];
    
    [Header("저장 정보")]
    public long saveTime;
    public string gameVersion = "1.0.0";
    
    /*[Header("게임 설정 (확장용)")]
    public float masterVolume = 1.0f;
    public float bgmVolume = 0.8f;
    public float sfxVolume = 1.0f; 
    public bool isFullscreen = true; */

    public GameSaveData()
    {
        completedTombstones = 0;
        tombstoneCompleted = new bool[5];
        for (int i = 0; i < tombstoneCompleted.Length; i++)
        {
            tombstoneCompleted[i] = false;
        }

        saveTime = System.DateTime.Now.ToBinary();
        gameVersion = Application.version;
    }
    
    /// <summary>
    /// 진행도만 설정하는 생성자
    /// </summary>
    /// <param name="completedCount"></param>
    /// <param name="tombstoneStates"></param>
    public GameSaveData(int completedCount, bool[] tombstoneStates)
    {
        completedTombstones = completedCount;
        tombstoneCompleted = new bool[5];
        
        if (tombstoneStates != null && tombstoneStates.Length >= 5)
        {
            for (int i = 0; i < 5; i++)
            {
                tombstoneCompleted[i] = tombstoneStates[i];
            }
        }
        
        saveTime = System.DateTime.Now.ToBinary();
        gameVersion = Application.version;
    }
    
     /// <summary>
    /// 데이터 유효성 검사
    /// </summary>
    /// <returns></returns>
    public bool IsValid()
    {
        // 기본 검사
        if (tombstoneCompleted == null || tombstoneCompleted.Length != 5)
            return false;
            
        if (completedTombstones < 0 || completedTombstones > 5)
            return false;
            
        // 완료 수와 실제 완료된 에피소드 수 일치 검사
        int actualCompleted = 0;
        for (int i = 0; i < tombstoneCompleted.Length; i++)
        {
            if (tombstoneCompleted[i])
                actualCompleted++;
        }
        
        return actualCompleted == completedTombstones;
    }
    
    /// <summary>
    /// 특정 묘비의 완료 상태 확인
    /// </summary>
    /// <param name="tombstoneType"></param>
    /// <returns></returns>
    public bool IsTombstoneCompleted(Enums.TombstoneType tombstoneType)
    {
        if (tombstoneType == Enums.TombstoneType.None)
            return false;
            
        int index = (int)tombstoneType;
        if (index >= 0 && index < tombstoneCompleted.Length)
            return tombstoneCompleted[index];
            
        return false;
    }
    
    /// <summary>
    /// 묘비 완료 상태 설정
    /// </summary>
    /// <param name="tombstoneType"></param>
    /// <param name="completed"></param>
    public void SetTombstoneCompleted(Enums.TombstoneType tombstoneType, bool completed)
    {
        if (tombstoneType == Enums.TombstoneType.None)
            return;
            
        int index = (int)tombstoneType;
        if (index >= 0 && index < tombstoneCompleted.Length)
        {
            bool wasCompleted = tombstoneCompleted[index];
            tombstoneCompleted[index] = completed;
            
            // 완료 수 업데이트
            if (completed && !wasCompleted)
            {
                completedTombstones++;
            }
            else if (!completed && wasCompleted)
            {
                completedTombstones--;
            }
            
            // 범위 체크
            completedTombstones = Mathf.Clamp(completedTombstones, 0, 5);
        }
    }
    
    /// <summary>
    /// 진행률 반환 (0.0 ~ 1.0)
    /// </summary>
    /// <returns></returns>
    public float GetProgressPercentage()
    {
        return (float)completedTombstones / 5.0f;
    }
    
    /// <summary>
    /// 저장 시간을 DateTime으로 반환
    /// </summary>
    /// <returns></returns>
    public System.DateTime GetSaveDateTime()
    {
        return System.DateTime.FromBinary(saveTime);
    }
    
    /// <summary>
    /// 디버그용 정보 출력
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"SaveData - 진행도: {completedTombstones}/5 ({GetProgressPercentage():P1}), " +
               $"저장시간: {GetSaveDateTime():yyyy-MM-dd HH:mm:ss}, " +
               $"버전: {gameVersion}";
    }
}

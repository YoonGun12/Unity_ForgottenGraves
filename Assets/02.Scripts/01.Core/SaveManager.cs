using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    [Header("저장 설정")] 
    [SerializeField] private string saveFileName = "ForgottenGraves_Save";

    /// <summary>
    /// 게임 진행도 저장
    /// </summary>
    /// <param name="completeCount">완료한 에피소드 수</param>
    /// <param name="tombstoneStates">각 묘비별 완료 상태</param>
    public void SaveProgress(int completeCount, bool[] tombstoneStates)
    {
        try
        {
            GameSaveData saveData = new GameSaveData
            {
                completedTombstones = completeCount,
                tombstoneCompleted = new bool[tombstoneStates.Length]
            };

            for (int i = 0; i < tombstoneStates.Length; i++)
            {
                saveData.tombstoneCompleted[i] = tombstoneStates[i];
            }

            saveData.saveTime = System.DateTime.Now.ToBinary();

            string jsonData = JsonUtility.ToJson(saveData, true);
            PlayerPrefs.SetString(saveFileName, jsonData);
            PlayerPrefs.Save();
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"저장 중 오류 발생 {e.Message}");
        }
    }
    
     /// <summary>
    /// 게임 진행도 로드
    /// </summary>
    /// <returns>저장된 게임 데이터 (없으면 null)</returns>
    public GameSaveData LoadProgress()
    {
        try
        {
            if (PlayerPrefs.HasKey(saveFileName))
            {
                string jsonData = PlayerPrefs.GetString(saveFileName);
                GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(jsonData);
                
                // 데이터 유효성 검사
                if (saveData != null && IsValidSaveData(saveData))
                {
                    System.DateTime saveTime = System.DateTime.FromBinary(saveData.saveTime);
                    Debug.Log($"게임 진행도를 로드했습니다. 저장 시간: {saveTime}, 완료 에피소드: {saveData.completedTombstones}/5");
                    return saveData;
                }
                else
                {
                    Debug.LogWarning("저장 데이터가 손상되었습니다.");
                    return null;
                }
            }
            else
            {
                Debug.Log("저장된 게임 데이터가 없습니다.");
                return null;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"로드 중 오류 발생: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// 저장 데이터 유효성 검사
    /// </summary>
    /// <param name="saveData"></param>
    /// <returns></returns>
    private bool IsValidSaveData(GameSaveData saveData)
    {
        // 기본 검사
        if (saveData.tombstoneCompleted == null || saveData.tombstoneCompleted.Length != 5)
            return false;
            
        if (saveData.completedTombstones < 0 || saveData.completedTombstones > 5)
            return false;
            
        // 완료 수와 배열 상태 일치 검사
        int actualCompleted = 0;
        for (int i = 0; i < saveData.tombstoneCompleted.Length; i++)
        {
            if (saveData.tombstoneCompleted[i])
                actualCompleted++;
        }
        
        return actualCompleted == saveData.completedTombstones;
    }
    
    /// <summary>
    /// 저장 데이터 삭제
    /// </summary>
    public void DeleteSaveData()
    {
        if (PlayerPrefs.HasKey(saveFileName))
        {
            PlayerPrefs.DeleteKey(saveFileName);
            PlayerPrefs.Save();
            Debug.Log("저장 데이터를 삭제했습니다.");
        }
        else
        {
            Debug.Log("삭제할 저장 데이터가 없습니다.");
        }
    }
    
    /// <summary>
    /// 저장 데이터 존재 여부 확인
    /// </summary>
    /// <returns></returns>
    public bool HasSaveData()
    {
        return PlayerPrefs.HasKey(saveFileName);
    }
    
    /// <summary>
    /// 저장 시간 가져오기
    /// </summary>
    /// <returns></returns>
    public System.DateTime? GetSaveTime()
    {
        GameSaveData saveData = LoadProgress();
        if (saveData != null)
        {
            return System.DateTime.FromBinary(saveData.saveTime);
        }
        return null;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 1. CSV 파일에서 대화 데이터를 읽어와 파싱
/// 2. 캐릭터 초상화 이미지 관리
/// 3. 대화 데이터를 메모리에 저장하고 다른 클래스에서 조회할 수 있도록 제공
/// 4. 싱글톤 패턴으로 전역접근 가능
/// </summary>
public class DialogueDatabase : MonoBehaviour
{
    //싱글톤
    public static DialogueDatabase Instance { get; private set; }

    [Header("CSV 파일")] 
    [SerializeField] private TextAsset[] csvFiles;

    [Header("캐릭터 초상화")] 
    [SerializeField] private CharacterPortraitData[] characterPortraits;

    private Dictionary<string, DialogueSet> dialogueSets = new Dictionary<string, DialogueSet>(); //파싱된 대화데이터를 저장, (CSV파일 이름) >> (해당 CSV에서 파싱된 모든 대화 데이터를 포함하는 DialogueSet)
    private Dictionary<string, CharacterPortraitData>   
        portraitDatabase = new Dictionary<string, CharacterPortraitData>(); //캐릭터별 초상화 데이터를 저장, (캐릭터 이름) >> (해당 캐릭터의 모든 초상화 이미지를 포함하는 CharacterPortraitData)

    public bool IsLoaded { get; private set; } = false; //데이터베이스 로딩이 완료되었는지 확인하는 플래그. 다른 스크립트에서 데이터베이스 사용 전에 이 값을 확인

    public static event Action OnDatabaseLoaded; //데이터베이스 로드 완료시 발생하는 이벤트, 다른 시스템이 이 이벤트를 구독하면 로딩완료를 감지 가능

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            StartCoroutine(LoadDatabase());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 데이터베이스를 비동기적으로 로드하는 코루틴
    /// 초상화 데이터와 CSV파일들을 순차적으로 로드
    /// </summary>
    /// <returns></returns>
    private IEnumerator LoadDatabase()
    {
        LoadPortraitDatabase(); //초상화 데이터베이스 로드
        
        //CSV파일을 순차적으로 파싱
        foreach (var csvFile in csvFiles)
        {
            if (csvFile != null)
            {
                ParseCSVFile(csvFile);
                yield return null;
            }
        }

        //로딩 완료 처리
        IsLoaded = true;
        
        //로딩 완료 이벤트 발생
        OnDatabaseLoaded?.Invoke();
    }

    /// <summary>
    /// 캐릭터 초상화 데이터베이스를 메모리에 로드
    /// 인스펙터에 설정한 초상화 배열을 딕셔너리로 변환하여 빠른 접근 가능
    /// </summary>
    private void LoadPortraitDatabase()
    {
        portraitDatabase.Clear();

        foreach (var portraitData in characterPortraits)
        {
            if (!string.IsNullOrEmpty(portraitData.characterName))
            {
                portraitDatabase[portraitData.characterName] = portraitData;
            }
        }
    }

    /// <summary>
    /// 단일 CSV 파일을 파싱하여 대화 데이터로 변환
    /// ID, Speaker, DialogueText, PortraitIndex, EventFlag
    /// </summary>
    /// <param name="csvFile">파싱할 CSV 파일</param>
    private void ParseCSVFile(TextAsset csvFile)
    {
        try
        {
            //줄 단위로 분할
            string[] lines = csvFile.text.Split('\n');
            if (lines.Length <= 1) return;

            //파일 이름을 키로 하여 대화세트 생성
            string fileName = csvFile.name;
            DialogueSet dialogueSet = new DialogueSet(fileName);

            //첫째줄 건너뛰기
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if(string.IsNullOrEmpty(line)) continue; //빈 줄 건너뛰기

                //각 줄을 대화 데이터로 파싱
                DialogueData dialogue = ParseCSVLine(line);
                if (dialogue != null)
                {
                    dialogueSet.dialogues.Add(dialogue);
                }
            }

            //유효 대화가 있는 경우에만 딕셔너리에 추가
            if (dialogueSet.dialogues.Count > 0)
            {
                dialogueSets[fileName] = dialogueSet;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"파싱오류 {csvFile.name} {e.Message}");
        }
    }

    /// <summary>
    /// CSV의 한 줄을 파싱하여 DialogueData 객체로 변환
    /// </summary>
    /// <param name="line">파싱할 CSV 한 줄</param>
    /// <returns></returns>
    private DialogueData ParseCSVLine(string line)
    {
        try
        {
            //CSV 한 줄을 쉼표로 구분
            string[] values = SplitCSVLine(line);

            //최소 5개 필드가 있어야 유효
            if (values.Length < 5) return null;

            int id = int.Parse(values[0]);
            string speaker = values[1].Replace("\"", "");
            string text = values[2].Replace("\"", "");
            
            int portraitIndex = 0;
            if (!string.IsNullOrEmpty(values[3]) && values[3] != "\"\"")
            {
                if (float.TryParse(values[3], out float floatvalue))
                {
                    portraitIndex = (int)floatvalue;
                }
            }

            string eventFlag = values[4].Replace("\"", "");
            return new DialogueData(id, speaker, text, portraitIndex, eventFlag);
        }
        catch (Exception e)
        {
            Debug.LogError($"CSV라인 파싱 실패 {line} {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// CSV 라인을 필드 단위로 분할(따옴표 안의 쉼표는 무시)
    /// </summary>
    /// <param name="line">분할할 CSV라인</param>
    /// <returns></returns>
    private string[] SplitCSVLine(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false; //현재 따옴표 안에 있는지 여부
        string currentField = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(currentField);
                currentField = "";
            }
            else
            {
                currentField += c;
            }
        }
        
        result.Add(currentField);
        return result.ToArray();
    }
    
    #region 공개 메서드

    /// <summary>
    /// 파일 이름으로 전체 대화 세트 가져오기
    /// </summary>
    /// <param name="setName">CSV 파일 이름</param>
    /// <returns>해당하는 DialogueSet</returns>
    public DialogueSet GetDialogueSet(string setName)
    {
        return dialogueSets.ContainsKey(setName) ? dialogueSets[setName] : null;
    }

    /// <summary>
    /// 특정 범위의 대화만 가져오기
    /// </summary>
    /// <param name="setName">CSV파일 이름</param>
    /// <param name="startIndex">시작 인덱스</param>
    /// <param name="count">가져올 대화 개수</param>
    /// <returns>지정된 범위의 대화 리스트</returns>
    public List<DialogueData> GetDialogueRange(string setName, int startIndex, int count)
    {
        var dialogueSet = GetDialogueSet(setName);
        if (dialogueSet == null) return null;
        
        if (startIndex < 0 || startIndex >= dialogueSet.dialogues.Count) return null;
        
        int actualCount = Mathf.Min(count, dialogueSet.dialogues.Count - startIndex);
        return dialogueSet.dialogues.GetRange(startIndex, actualCount);
    }

    /// <summary>
    /// 특정 ID를 가진 대화 하나만 가져오기
    /// </summary>
    /// <param name="setName">CSV파일 이름</param>
    /// <param name="id">찾을 대화 ID</param>
    /// <returns>해당 ID의 DialogueData</returns>
    public DialogueData GetDialogueById(string setName, int id)
    {
        var dialogueSet = GetDialogueSet(setName);
        return dialogueSet?.dialogues.FirstOrDefault(d => d.id == id);
    }

    /// <summary>
    /// 캐릭터의 특정 초상화 이미지 가져오기
    /// </summary>
    /// <param name="characterName">캐릭터이름</param>
    /// <param name="portraitIndex">초상화 인덱스</param>
    /// <returns>해당하는 Sprite이미지</returns>
    public Sprite GetCharacterPortrait(string characterName, int portraitIndex)
    {
        if (portraitDatabase.ContainsKey(characterName))
        {
            // 캐릭터별 인덱스 오프셋 계산
            int actualIndex = GetActualPortraitIndex(characterName, portraitIndex);
            return portraitDatabase[characterName].GetPortrait(actualIndex);
        }
        return null;
    }

    /// <summary>
    /// 캐릭터별 실제 초상화 배열 인덱스 계산
    /// </summary>
    /// <param name="characterName">캐릭터 이름</param>
    /// <param name="csvIndex">CSV 파일의 원본 인덱스</param>
    /// <returns>실제 배열 인덱스</returns>
    private int GetActualPortraitIndex(string characterName, int csvIndex)
    {
        // 캐릭터별 CSV 인덱스 오프셋 정의
        switch (characterName.ToLower())
        {
            case "라일":
                return csvIndex - 1;

            case "할아버지":
                return csvIndex - 11;

            case "나래이션":
                return 0;

            case "리아":
                return csvIndex - 21;

            default:
                // 기본값: 오프셋 없음
                return csvIndex;
        }
    }

    /// <summary>
    /// 현재 로드된 대화 세트의 이름 목록 반환(디버그용)
    /// </summary>
    /// <returns>로드된 대화 세트 이름 배열</returns>
    public string[] GetLoadedDialogueSets()
    {
        return dialogueSets.Keys.ToArray();
    }
    
    
    /// <summary>
    /// 인덱스와 범위를 모두 사용하여 특정 대화 가져오기
    /// </summary>
    public List<DialogueData> GetDialogueRangeByIndex(int csvIndex, int startIndex, int count)
    {
        var dialogueSet = GetDialogueSetByIndex(csvIndex);
        if (dialogueSet == null) return null;

        if (startIndex < 0 || startIndex >= dialogueSet.dialogues.Count) return null;

        int actualCount = Mathf.Min(count, dialogueSet.dialogues.Count - startIndex);
        return dialogueSet.dialogues.GetRange(startIndex, actualCount);
    }

    /// <summary>
    /// CSV파일 배열의 인덱스로 대화 세트 가져오기
    /// </summary>
    public DialogueSet GetDialogueSetByIndex(int index)
    {
        if (csvFiles == null || index < 0 || index >= csvFiles.Length) 
            return null;

        string fileName = csvFiles[index].name;
        return dialogueSets.ContainsKey(fileName) ? dialogueSets[fileName] : null;
    }



    #endregion
}

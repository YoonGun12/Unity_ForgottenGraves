using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DialogueDatabase : MonoBehaviour
{
    public static DialogueDatabase Instance { get; private set; }

    [Header("CSV 파일")] 
    [SerializeField] private TextAsset[] csvFiles;

    [Header("캐릭터 초상화")] 
    [SerializeField] private CharacterPortraitData[] characterPortraits;

    private Dictionary<string, DialogueSet> dialogueSets = new Dictionary<string, DialogueSet>();
    private Dictionary<string, CharacterPortraitData>
        portraitDatabase = new Dictionary<string, CharacterPortraitData>();

    public bool IsLoaded { get; private set; } = false;

    public static event Action OnDatabaseLoaded;

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

    private IEnumerator LoadDatabase()
    {
        LoadPortraitDatabase();
        foreach (var csvFile in csvFiles)
        {
            if (csvFile != null)
            {
                ParseCSVFile(csvFile);
                yield return null;
            }
        }

        IsLoaded = true;
        OnDatabaseLoaded?.Invoke();
    }

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

    private void ParseCSVFile(TextAsset csvFile)
    {
        try
        {
            string[] lines = csvFile.text.Split('\n');
            if (lines.Length <= 1) return;

            string fileName = csvFile.name;
            DialogueSet dialogueSet = new DialogueSet(fileName);

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if(string.IsNullOrEmpty(line)) continue;

                DialogueData dialogue = ParseCSVLine(line);
                if (dialogue != null)
                {
                    dialogueSet.dialogues.Add(dialogue);
                }
            }

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

    private DialogueData ParseCSVLine(string line)
    {
        try
        {
            string[] values = SplitCSVLine(line);

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

    private string[] SplitCSVLine(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
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
    /// 대화 세트 가져오기
    /// </summary>
    public DialogueSet GetDialogueSet(string setName)
    {
        return dialogueSets.ContainsKey(setName) ? dialogueSets[setName] : null;
    }

    /// <summary>
    /// 특정 범위의 대화 가져오기
    /// </summary>
    public List<DialogueData> GetDialogueRange(string setName, int startIndex, int count)
    {
        var dialogueSet = GetDialogueSet(setName);
        if (dialogueSet == null) return null;
        
        if (startIndex < 0 || startIndex >= dialogueSet.dialogues.Count) return null;
        
        int actualCount = Mathf.Min(count, dialogueSet.dialogues.Count - startIndex);
        return dialogueSet.dialogues.GetRange(startIndex, actualCount);
    }

    /// <summary>
    /// ID로 특정 대화 가져오기
    /// </summary>
    public DialogueData GetDialogueById(string setName, int id)
    {
        var dialogueSet = GetDialogueSet(setName);
        return dialogueSet?.dialogues.FirstOrDefault(d => d.id == id);
    }

    /// <summary>
    /// 캐릭터 초상화 가져오기
    /// </summary>
    public Sprite GetCharacterPortrait(string characterName, int portraitIndex)
    {
        if (portraitDatabase.ContainsKey(characterName))
        {
            return portraitDatabase[characterName].GetPortrait(portraitIndex);
        }
        return null;
    }

    /// <summary>
    /// 로드된 대화 세트 목록
    /// </summary>
    public string[] GetLoadedDialogueSets()
    {
        return dialogueSets.Keys.ToArray();
    }

    #endregion
}

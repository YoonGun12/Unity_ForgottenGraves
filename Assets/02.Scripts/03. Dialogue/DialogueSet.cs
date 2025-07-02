using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueData
{
    public int id;
    public string speaker;
    public string text;
    public int portraitIndex;
    public string eventFlag;

    public DialogueData(int id, string speaker, string text, int portraitIndex, string eventFlag)
    {
        this.id = id;
        this.speaker = speaker;
        this.text = text;
        this.portraitIndex = portraitIndex;
        this.eventFlag = eventFlag;
    }
}

[System.Serializable]
public class DialogueSet : MonoBehaviour
{
    public string setName;
    public List<DialogueData> dialogues;

    public DialogueSet(string name)
    {
        setName = name;
        dialogues = new List<DialogueData>();
    }
}

[System.Serializable]
public class CharacterPortraitData
{
    public string characterName;
    public Sprite[] portraits;

    public Sprite GetPortrait(int index)
    {
        if (portraits != null && index >= 0 && index < portraits.Length)
            return portraits[index];
        return null;
    }
}

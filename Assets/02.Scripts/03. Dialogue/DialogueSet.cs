using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개별 대화 하나의 모든 정보를 담는 데이터 클래스
/// CSV파일의 한 줄에 해당하는 데이터를 저장
/// </summary>
[System.Serializable]
public class DialogueData
{
    public int id;              //대화 순서
    public string speaker;      //대화하는 캐릭터 이름
    public string text;         //실제 대화 내용 텍스트
    public int portraitIndex;   //캐릭터 초상화 이미지 인덱스
    public string eventFlag;    //특별한 이벤트를 발생시키는 플래그

    //DialogueData 생성자
    public DialogueData(int id, string speaker, string text, int portraitIndex, string eventFlag)
    {
        this.id = id;
        this.speaker = speaker;
        this.text = text;
        this.portraitIndex = portraitIndex;
        this.eventFlag = eventFlag;
    }
}

/// <summary>
/// 하나의 CSV 파일에서 읽어온 모든 대화들을 담는 컨테이너 클래스
/// </summary>
[System.Serializable]
public class DialogueSet 
{
    public string setName;                  //대화 세트의 이름, CSV파일 이름과 동일
    public List<DialogueData> dialogues;    //해당 세트에 포함된 모든 대화 데이터들의 리스트

    public DialogueSet(string name)
    {
        setName = name;
        dialogues = new List<DialogueData>();
    }
}


/// <summary>
/// 특정 캐릭터의 모든 초상화 이미지들을 관리하는 데이터 클래스
/// </summary>
[System.Serializable]
public class CharacterPortraitData
{
    public string characterName;        //캐릭터 이름
    public Sprite[] portraits;          //해당 캐릭터의 모든 초상화 이미지

    public Sprite GetPortrait(int index)
    {
        if (portraits != null && index >= 0 && index < portraits.Length)
            return portraits[index];
        return null;
    }
}

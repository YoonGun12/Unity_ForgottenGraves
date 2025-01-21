using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("Dialogue UI")]
    public GameObject dialogueBox;
    public TextMeshProUGUI dialougeText;
    public Image portraitImg;
    public Sprite[] portraitArr;

    [Header("Dialogue Data")]
    private Dictionary<int, string[]> talkData = new Dictionary<int, string[]>();
    private Dictionary<int, Sprite> portraitData = new Dictionary<int, Sprite>();
    private GameObject scanObject;
    private int talkIndex;
    public bool isInteraction;
    
    private void Awake()
    {
        GenerateData();
    }

    private void GenerateData()
    {
        talkData.Add(2000, new string[] { "꼬마 묘지기 친구!:1", "내 이름은 리아야.\n보다시피 유령이지.. 흐흐:1" });
        talkData.Add(100, new string[] { "핸즈필드 공동묘지", "묘지 입구 ->" });

        // 초상화 데이터 추가 (0: Lyle, 1: Ria)
        for (int i = 0; i < portraitArr.Length; i++)
        {
            portraitData.Add(2000 + i, portraitArr[i]);
        }
    }

    public void Interaction(GameObject scanObj)
    {
        var objData = scanObj.GetComponent<InteractableObjData>();

        if (!isInteraction)
            StartDialogue(objData.id, objData.isNPC);
        else
            NextDialogue(objData.id, objData.isNPC);

        dialogueBox.SetActive(isInteraction);
    }

    private void StartDialogue(int id, bool isNPC)
    {
        talkIndex = 0;
        ShowDialogue(id, isNPC);
        isInteraction = true;
    }

    private void NextDialogue(int id, bool isNPC)
    {
        talkIndex++;
        if (!ShowDialogue(id, isNPC))
            EndDialogue();
    }

    private bool ShowDialogue(int id, bool isNPC)
    {
        string currentTalk = GetTalk(id, talkIndex);
        if (currentTalk == null) return false;

        string[] talkParts = currentTalk.Split(":");
        dialougeText.text = talkParts[0];

        if (isNPC && talkParts.Length > 1 && int.TryParse(talkParts[1], out int portraitIndex))
        {
            portraitImg.sprite = GetPortrait(id, portraitIndex);
            portraitImg.color = new Color(1, 1, 1, 1);
        }
        else
        {
            portraitImg.color = new Color(1, 1, 1, 0);
        }

        return true;
    }

    private void EndDialogue()
    {
        isInteraction = false;
        dialogueBox.SetActive(false);
        talkIndex = 0;
    }

    public string GetTalk(int id, int talkIndex)
    {
        return talkData.ContainsKey(id) && talkIndex < talkData[id].Length ? talkData[id][talkIndex] : null;
    }

    public Sprite GetPortrait(int id, int portraitIndex)
    {
        return portraitData.ContainsKey(id + portraitIndex) ? portraitData[id + portraitIndex] : null;
    }
}

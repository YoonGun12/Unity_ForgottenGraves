using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroManager : MonoBehaviour
{
    [Header("Intro UI")] 
    [SerializeField] private GameObject introPanel;
    [SerializeField] private TextMeshProUGUI introText;
    [SerializeField] private Image characterPortrait;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button nextButton;

    [Header("Character Portraits")] 
    [SerializeField] private Sprite[] lylePortraits;
    [SerializeField] private Sprite[] grandfatherPortraits;
    [SerializeField] private Sprite[] riaPortraits;

    [SerializeField] private Image fadeImage;

    private Dialogue[] currentDialogues;
    private int currentPrologueIndex = 0;
    private int currentDialogueIndex = 0;
    private bool isTyping = false;
    private bool canProceed = true;

    private string[] prologueFiles = { "Prologue1", "Prologue2", "Prologue3" };

    private void Start()
    {
        StartCoroutine(StartIntroSequence());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            if (isTyping)
            {
                StopAllCoroutines();
                ShowCompleteText();
            }
            else if (canProceed)
            {
                NextDialogue();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SkipIntro();
        }
    }

    private IEnumerator StartIntroSequence()
    {
        yield return new WaitUntil(() => DatabaseManager.isFinish);

        introPanel.SetActive(true);

        LoadPrologueDialogue(0);
    }

    private void LoadPrologueDialogue(int prologueIndex)
    {
        currentPrologueIndex = prologueIndex;
        currentDialogueIndex = 0;

        switch (prologueIndex)
        {
            case 0:
                break;
            case 1:
                StartCoroutine(FadeTransition());
                break;
            case 2:
                StartCoroutine(FadeTransition());
                break;
        }

        currentDialogues = DatabaseManager.instance.GetDialogue(1, GetPrologueLength(prologueIndex));
        ShowCurrentDialogue();
    }

    private int GetPrologueLength(int prologueIndex)
    {
        switch (prologueIndex)
        {
            case 0: return 8;
            case 1: return 11;
            case 2: return 19;
            default: return 0;
        }
    }

    private void ShowCurrentDialogue()
    {
        if (currentDialogueIndex >= currentDialogues.Length)
        {
            NextPrologue();
            return;
        }

        Dialogue dialogue = currentDialogues[currentDialogueIndex];

        SetCharacterPortrait(dialogue.name);
        StartCoroutine(TypeText(dialogue.contexts[0]));
    }

    private void SetCharacterPortrait(string characterName)
    {
        Sprite portrait = null;

        switch (characterName)
        {
            case "라일":
                portrait = GetLylePortrait();
                break;
            case "할아버지":
                portrait = GetGrandfatherPortrait();
                break;
            case "리아":
                portrait = GetRiaPortrait();
                break;
            
        }

        if (portrait != null)
        {
            characterPortrait.sprite = portrait;
            characterPortrait.color = Color.white;
        }
        else
        {
            characterPortrait.color = Color.clear;
        }
    }

    private Sprite GetLylePortrait()
    {
        if (currentPrologueIndex == 1) return lylePortraits[4];
        if (currentPrologueIndex == 2 && currentDialogueIndex <= 5) return lylePortraits[3];
        if (currentPrologueIndex == 2 && currentDialogueIndex >= 10) return lylePortraits[5];
        return lylePortraits[0];
    }

    private Sprite GetGrandfatherPortrait()
    {
        return grandfatherPortraits[currentPrologueIndex == 1 ? 0 : 1];
    }

    private Sprite GetRiaPortrait()
    {
        if (currentDialogueIndex < 5) return riaPortraits[1];
        if (currentDialogueIndex <= 10) return riaPortraits[2];
        return riaPortraits[0];
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        canProceed = false;
        introText.text = "";

        string[] parts = text.Split(':');
        string displayText = parts.Length > 1 ? parts[0] : text;

        for (int i = 0; i < displayText.Length; i++)
        {
            introText.text += displayText;
            yield return new WaitForSeconds(0.05f);
        }

        isTyping = false;
        canProceed = true;
    }

    private void ShowCompleteText()
    {
        if (currentDialogueIndex < currentDialogues.Length)
        {
            string text = currentDialogues[currentDialogueIndex].contexts[0];
            string[] parts = text.Split(':');
            string displayText = parts.Length > 1 ? parts[0] : text;
            introText.text = displayText;
        }

        isTyping = false;
        canProceed = true;
    }

    private void NextDialogue()
    {
        currentDialogueIndex++;
        ShowCurrentDialogue();
    }

    private void NextPrologue()
    {
        if (currentPrologueIndex < 2)
        {
            LoadPrologueDialogue(currentPrologueIndex + 1);
        }
        else
        {
            StartCoroutine(FinishIntro());
        }
    }

    private IEnumerator FinishIntro()
    {
        yield return StartCoroutine(FadeOut());

        SceneManager.LoadScene("Forgatten Graves");
    }

    private IEnumerator FadeTransition()
    {
        yield return StartCoroutine(FadeOut());
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(FadeIn());
    }
    private IEnumerator FadeOut()
    {
        float duration = 1f;
        Color color = fadeImage.color;
        
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            color.a = Mathf.Lerp(0, 1, t / duration);
            fadeImage.color = color;
            yield return null;
        }
        
        color.a = 1;
        fadeImage.color = color;
    }
    
    private IEnumerator FadeIn()
    {
        float duration = 1f;
        Color color = fadeImage.color;
        
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            color.a = Mathf.Lerp(1, 0, t / duration);
            fadeImage.color = color;
            yield return null;
        }
        
        color.a = 0;
        fadeImage.color = color;
    }
    
    public void SkipIntro()
    {
        StartCoroutine(FinishIntro());
    }
    
    public void OnSkipButtonClick()
    {
        SkipIntro();
    }
    
    public void OnNextButtonClick()
    {
        if (canProceed && !isTyping)
        {
            NextDialogue();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 열거형 관리 클래스
/// </summary>
public class Enums : MonoBehaviour
{
    /// <summary>
    /// 게임 상태
    /// </summary>
    public enum GameState
    {
        Loading,
        MainMenu,
        Prologue,
        Playing,
        Episode,
        MiniGame,
        Dialogue,
        Ending,
        Paused
    }

    /// <summary>
    /// 묘비 캐릭터 타입
    /// </summary>
    public enum TombstoneType
    {
        Edgar = 0,
        Matilda = 1,
        Toby =  2,
        Violet = 3,
        Ria = 4,
        None = -1
    }

    /// <summary>
    /// 에피소드 진행단계
    /// </summary>
    public enum EpisodePhase
    {
        None,
        PreDialogue,
        Story,
        StoryDialogue,
        PreMiniGame,
        MiniGame,
        MiniGameDialogue,
        PostDialogue,
        Completed
    }

    /// <summary>
    /// 씬 타입
    /// </summary>
    public enum SceneType
    {
        GameManager,
        MainMenu,
        
        Prologue1,
        Prologue2,
        Prologue3,
        
        Cemetery_Hub,
        
        Edgar_Episode,
        Matilda_Episode,
        Toby_Episode,
        Violet_Episode,
        Ria_Episode,
    
        Edgar_Minigame,
        Matilda_Minigame,
        Toby_Minigame,
        Violet_Minigame,
        Ria_Minigame,
    
        Epilogue
    }
    
    /// <summary>
    /// 캐릭터 타입
    /// </summary>
    public enum CharacterType
    {
        Lyle,           
        Ria,            
        Grandfather,    
        Edgar,          
        Matilda,        
        Toby,           
        Violet,         
        Narrator        
    }
    
    /// <summary>
    /// 미니게임 상태
    /// </summary>
    public enum MiniGameState
    {
        NotStarted,     
        Playing,        
        Completed,      
        Failed         
    }
    
    /// <summary>
    /// 대화 표시 모드
    /// </summary>
    public enum DialogueMode
    {
        Normal,         // 일반 대화
        Narration,      // 내레이션
        Thought,        // 생각/독백
        Flashback       // 회상
    }
    
    

}

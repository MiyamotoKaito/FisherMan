using UnityEngine;

/// <summary>
/// ゲーム全体の進行を管理するクラス
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private TypingManager _typingManager;
    private FishingManager _fishingManager;
    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}
public enum GameStates
{
    Title,
    Playing,
    Pause
}

public enum PlayerStates
{
    Idle,
    Walk,
    Fishing,
    Typing
}

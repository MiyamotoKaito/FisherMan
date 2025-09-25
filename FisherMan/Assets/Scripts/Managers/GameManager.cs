using UnityEngine;

/// <summary>
/// ゲーム全体の進行を管理するクラス
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private TypingManager _typingManager;
    private FishingManager _fishingManager;
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

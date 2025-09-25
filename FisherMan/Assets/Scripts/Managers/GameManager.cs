using UnityEngine;

/// <summary>
/// ゲーム全体の進行を管理するクラス
/// </summary>
public class GameManager : MonoBehaviour
{
    
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

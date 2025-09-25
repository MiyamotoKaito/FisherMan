using UnityEngine;

public class PlayerBase : MonoBehaviour
{
    protected InputBuffer _inputBuffer;
    /// <summary>現在のプレイヤーの状態
    /// 初期はIdle
    /// </summary>
    protected PlayerStates _currentState = PlayerStates.Idle;
    public PlayerStates CurrentState => _currentState;
    /// <summary>
    /// 入力アクションを呼ぶ
    /// </summary>
    protected void BaseAwake()
    {
        _inputBuffer = new InputBuffer();
        _inputBuffer.Enable();
    }
    /// <summary>
    /// 入力を無効化する
    /// </summary>
    protected void BaseDisable()
    {
        _inputBuffer.Disable();
    }
}
public enum PlayerStates
{
    Idle,
    Move,
    Fishing,
    Typing
}
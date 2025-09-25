using System.Transactions;
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
    /// <summary>
    /// 現在のステートを引数1のステートに切り替える
    /// </summary>
    /// <param name="newState"></param>
    public void ChangeState(PlayerStates newState)
    {
        //もし現在のステートと同じだったら何もしない
        if(CurrentState == newState) return;

        Debug.Log($"{_currentState}状態から{newState}状態に変更");
        _currentState = newState;
    }
}
public enum PlayerStates
{
    Idle,
    Move,
    Fishing,
    Typing
}
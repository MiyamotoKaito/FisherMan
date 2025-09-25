using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : PlayerBase
{
    [SerializeField, Header("プレイヤーの歩くスピード")]
    private float _speed;
    private Vector2 _currentMove;
    private Rigidbody _rb;


    private void Awake()
    {
        base.BaseAwake();
        _rb = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {

    }
    private void OnEnable()
    {
        _inputBuffer.Player.Move.performed += OnInputWalk;
    }
    private void OnDisable()
    {
        base.BaseDisable();
    }
    /// <summary>
    /// 移動を管理する
    /// </summary>
    /// <param name="context"></param>
    private void OnInputWalk(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _currentMove = context.ReadValue<Vector2>();
        }
        else if (context.canceled)
        {
            _currentMove = Vector2.zero;
        }
    }
    /// <summary>
    /// 実際にプレイヤーを動かす関数
    /// </summary>
    private void PlayerWalk()
    {

    }
    /// <summary>
    /// アニメーションを管理する
    /// </summary>
    /// <param name="state"></param>
    protected override void OnStateEnter(PlayerStates state)
    {
        switch (state)
        {
            case PlayerStates.Idle:
                //ここにアニメーションのフラグ
                break;
            case PlayerStates.Walk:
                //ここにアニメーションのフラグ
                break;
            case PlayerStates.Fishing:
                //ここにアニメーションのフラグ
                break;
            case PlayerStates.Typing:
                //ここにアニメーションのフラグ
                break;
        }
    }
}

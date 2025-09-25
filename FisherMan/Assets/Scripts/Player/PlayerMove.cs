using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : PlayerBase
{
    [SerializeField, Header("プレイヤーの歩くスピード")]
    private float _speed;
    private Vector2 _currentMove;
    private Rigidbody _rb;
    private Camera _camera;


    private void Awake()
    {
        base.BaseAwake();
        _rb = GetComponent<Rigidbody>();
        _camera = FindFirstObjectByType<Camera>();
    }
    private void FixedUpdate()
    {
        PlayerWalk();
    }
    private void OnEnable()
    {
        _inputBuffer.Player.Move.performed += OnInputWalk;
        _inputBuffer.Player.Move.canceled += OnInputWalk;
    }
    private void OnDisable()
    {
        _inputBuffer.Player.Move.performed -= OnInputWalk;
        _inputBuffer.Player.Move.canceled -= OnInputWalk;
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
        Vector3 orientation = transform.forward * _currentMove.y + _camera.transform.right * _currentMove.x;
        Vector3 currentVelocity = orientation.normalized * _speed;
        _rb.linearVelocity = new Vector3(currentVelocity.x, _rb.linearVelocity.y, currentVelocity.z);
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

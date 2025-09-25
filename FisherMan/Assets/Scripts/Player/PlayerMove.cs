using UnityEngine;

public class PlayerMove : PlayerBase
{
    [SerializeField, Header("プレイヤーの歩くスピード")]
    private float _speed;

    private Rigidbody _rb;


    private void Awake()
    {
        base.BaseAwake();
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
            case PlayerStates.Move:
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

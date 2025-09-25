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
}

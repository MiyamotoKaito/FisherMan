using UnityEngine;

[CreateAssetMenu(fileName = "FishData", menuName = "EntityDatas/FishData")]
public class FishData : ScriptableObject
{
    /// <summary>魚の名前</summary>
    [SerializeField, Header("魚の名前")]
    private string _fishName;
    /// <summary>魚のHP</summary>
    [SerializeField, Header("魚のHP")]
    private int _fishHp;
    /// <summary>魚の売値</summary>
    [SerializeField, Header("魚の売値")]
    private int _fishPrice;
    /// <summary>魚の魚影のサイズ</summary>
    [SerializeField, Header("魚の魚影のサイズ")]
    private int _fishShadowSize;
    /// <summary>問題の難易度</summary>
    [SerializeField, Header("問題の難易度")]
    private int _fishLevel;
    public string FishName => _fishName;
    public int FishHp => _fishHp;
    public int FishPrice => _fishPrice;
    public int FishShadowSize => _fishShadowSize;
    public int FishLevel => _fishLevel;
}

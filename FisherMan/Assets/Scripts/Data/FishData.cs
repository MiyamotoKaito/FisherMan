using UnityEngine;

/// <summary>
/// 魚のデータを定義するクラス
/// </summary>
[CreateAssetMenu(fileName = "FishData", menuName = "Data/FishData")]
public class FishData : ScriptableObject
{
    /// <summary>魚の名前</summary>
    [SerializeField, Header("魚の名前")]
    private string _fishName;
    /// <summary>魚のHP</summary>
    [SerializeField, Header("魚のHP"), Range(1, 100)]
    private int _fishHp;
    /// <summary>魚の売値</summary>
    [SerializeField, Header("魚の売値"), Range(1, 10000)]
    private int _fishPrice;
    /// <summary>魚の魚影のサイズ</summary>
    [SerializeField, Header("魚の魚影のサイズ"),Range(1,5)]
    private int _fishShadowSize;
    /// <summary>問題の難易度</summary>
    [SerializeField, Header("問題の難易度"),Range(1,5)]
    private int _fishLevel;
    public string FishName => _fishName;
    public int FishHp => _fishHp;
    public int FishPrice => _fishPrice;
    public int FishShadowSize => _fishShadowSize;
    public int FishLevel => _fishLevel;
}
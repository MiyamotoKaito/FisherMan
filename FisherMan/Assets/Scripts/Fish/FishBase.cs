using UnityEngine;

public class FishBase : MonoBehaviour
{
    private string _name;
    private int _hp;
    private int _price;
    private int _shadowSize;
    private int _level;
    private int _timer;

    private void OnEnable()
    {
        SetParameter();
    }
    /// <summary>
    /// 釣り竿を投げ入れたら魚に値を設定する
    /// </summary>
    /// <param name="fishData"></param>
    private void SetParameter(FishData fishData)
    {
        _name = fishData.name;
        _hp = fishData.FishHp;
        _price = fishData.FishPrice;
        _shadowSize = fishData.FishShadowSize;
        _level = fishData.FishLevel;
        _timer = fishData.FishTimer;
    }
}

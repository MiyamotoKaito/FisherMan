using UnityEngine;

public class FishBase : MonoBehaviour
{
    private string _name;
    private int _hp;
    private int _price;
    private int _shadowSize;
    private int _level;
    private int _timer;

    public string Name => _name;
    public int HP
    {
        get { return _hp; }
        set { _hp = value; }
    }
    public int Price => _price;
    public int ShadowSize => _shadowSize;
    public int Level => _level;
    public int Timer
    {
        get { return _timer; }
        set { _timer = value; }
    }

    private void OnEnable()
    {
        SetParameter();
    }
    /// <summary>
    /// 釣り竿を投げ入れたら魚に値を設定する
    /// </summary>
    /// <param name="fishData"></param>
    public void SetParameter(FishData fishData)
    {
        _name = fishData.FishName;
        _hp = fishData.FishHp;
        _price = fishData.FishPrice;
        _shadowSize = fishData.FishShadowSize;
        _level = fishData.FishLevel;
        _timer = fishData.FishingTimer;
    }

}

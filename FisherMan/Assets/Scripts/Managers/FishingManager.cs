using System;
using UnityEngine;

public class FishingManager : MonoBehaviour
{
    public static FishingManager Instance;
    /// <summary>魚が掛かった時のイベント</summary>
    public event Action<FishData> OnFallBait;
    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    /// <summary>
    /// 魚が餌に掛かった時に呼び出す
    /// </summary>
    /// <param name="fishData"></param>
    private void FallBait(FishData fishData)
    {
        OnFallBait.Invoke(fishData);
    }
}

using System;
using UnityEngine;

public class FishingManager : MonoBehaviour
{
    public static FishingManager Instance;
    /// <summary>魚が掛かった時のイベント</summary>
    public event Action<FishBase> OnFallBait;
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
    /// <param name="fishBase"></param>
    private void FallBait(FishBase fishBase)
    {
        OnFallBait.Invoke(fishBase);
    }
}

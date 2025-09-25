using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// タイピングの管理を行うクラス
/// CSVから読み込み、レベルごとにランダムな単語を出題する
/// </summary>
public class TypingManager : MonoBehaviour
{
    public static TypingManager Instance;
    /// <summary>
    /// レベルごとの単語リスト
    /// key:魚のレベル, value:単語のリスト
    /// </summary>
    private Dictionary<int, List<string>> _words = new Dictionary<int, List<string>>();

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
        LoadWords();
    }
    /// <summary>
    /// CSVから単語を読み込む
    /// </summary>
    private void LoadWords()
    {

    }


}

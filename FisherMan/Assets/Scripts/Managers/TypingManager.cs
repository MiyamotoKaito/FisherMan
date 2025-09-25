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
    /// <summary>お題に出されているワード</summary>
    private string _targetWord;
    /// <summary>入力中の文字位置</summary>
    private int _currentIndex;

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
        TextAsset csv = Resources.Load<TextAsset>("Theme");
        if (csv == null)
        {
            Debug.LogError("Theme.csvが見つかりませんResourcesの中にThemeファイルを置け");
            return;
        }
        //一行ごとの文字配列に分割
        string[] lines = csv.text.Split('\n');
        //最初の文字列を扱わないためのフラグ
        bool isFirstLine = true;
    }
}

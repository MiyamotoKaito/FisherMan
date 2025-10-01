using System;
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
    private Dictionary<int, List<WordPair>> _wordPairs = new Dictionary<int, List<WordPair>();
    /// <summary>お題に出されているワード</summary>
    private string _targetWord;
    /// <summary>入力中の文字位置</summary>
    private int _currentIndex;
    /// <summary>餌に掛かっている魚のデータ</summary>
    private FishBase _currentFish;
    /// <summary>タイピングが成功したときのイベント</summary>
    public event Action<FishBase, bool> OnTypingCompleted;

    /// <summary>
    /// 単語に対応したローマ字のペア
    /// </summary>
    [System.Serializable]
    public class WordPair
    {
        private string _word;//表示用
        private string _romaji;//入力用
        public string Word => _word;
        public string Romaji => _romaji;
    }
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

        foreach (string line in lines)
        {
            //何も入っていないまたは空白の文字だったらスキップ
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] parts = line.Split(',');
            //最初の列だったらスキップ
            if (isFirstLine)
            {
                isFirstLine = false;
                continue;
            }

            //最初の列が数値ではなかったらスキップ
            if (!int.TryParse(parts[0], out int level)) continue;

            //辞書の中に同じレベルの数値が無かったら辞書に新しく追加する
            if (!_wordPairs.ContainsKey(level))
            {
                _wordPairs[level] = new List<WordPair>();
            }

            //2列目以降の単語追加
            //word1,romaji1,word2,romaji2...の形式で読み込む
            for (int i = 1; i < parts.Length; i+= 2)
            {
                //空白を取り除く
                string displayWord = parts[i].Trim();
                string InputWord = parts[i+1].Trim();

                //文字が入っていたら辞書に追加
                if (!string.IsNullOrWhiteSpace(InputWord))
                {
                    _wordPairs[level].Add(new WordPair(displayWord, InputWord));
                }
            }
        }
        Debug.Log("csvの読み込み完了");
    }
    /// <summary>
    /// 指定レベルの単語をランダムに取得
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    private string GetRandomWord(int level)
    {
        //指定されたレベルに単語があるかチェックして、存在しない場合空文字を返す
        if (!_wordPairs.ContainsKey(level) || _wordPairs[level].Count == 0)
        {
            //空文字を返す
            return string.Empty;
        }

        //ランダムに列の中の単語を返す
        int index = UnityEngine.Random.Range(0, _wordPairs[level].Count);
        return _wordPairs[level][index];
    }
    /// <summary>
    /// 出題開始
    /// </summary>
    /// <param name="word"></param>
    /// <param name="fisbase"></param>
    private void StartTyping(string word, FishBase fisbase)
    {
        _currentFish = fisbase;
        _currentIndex = 0;
        _targetWord = GetRandomWord(fisbase.Level);
    }
    /// <summary>
    /// タイピング
    /// </summary>
    /// <param name="input"></param>
    private void InputChar(char input)
    {
        if (_currentFish == null || string.IsNullOrEmpty(_targetWord))
        {
            return;
        }

        //インプットされた文字が正しいかどうか
        if (input == _targetWord[_currentIndex])
        {
            _currentIndex++;
            //文字列を打ち終えたらダメージを与える
            Attack();
        }
        //文字が違かったらタイマーを減らす
        else
        {
            _currentFish.Timer -= 1;
            if (_currentFish.Timer <= 0)
            {
                OnTypingCompleted?.Invoke(_currentFish, false);
                Reset();
            }
        }
    }
    /// <summary>
    /// 魚にダメージを与える
    /// </summary>
    private void Attack()
    {
        if (_currentIndex >= _targetWord.Length)
        {
            _currentFish.HP -= 10;
            //釣り成功
            if (_currentFish.HP <= 0)
            {
                Reset();
                OnTypingCompleted?.Invoke(_currentFish, true);
            }
            else
            {
                //HPが残っていたら新しい単語を出して釣り続行
                _currentIndex = 0;
                _targetWord = GetRandomWord(_currentFish.Level);
            }
        }
    }
    private void Reset()
    {
        _currentFish = null;
        _currentIndex = 0;
        _targetWord = string.Empty;
    }
}

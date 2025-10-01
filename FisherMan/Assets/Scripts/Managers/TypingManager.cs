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
    /// レベルごとの単語辞書
    /// key:魚のレベル, value:単語のリスト
    /// </summary>
    private Dictionary<int, List<WordPair>> _wordPairs = new Dictionary<int, List<WordPair>>();
    /// <summary>お題の複数入力パターン辞書</summary>
    private Dictionary<string, List<string>> _otherPatterns = new Dictionary<string, List<string>>();
    /// <summary>お題に出されているワード(表示用)</summary>
    private string _targetWord;
    /// <summary>現在のローマ字配列</summary>
    private List<char> _romajiChars = new List<char>();
    /// <summary>入力中の文字位置</summary>
    private int _currentIndex;
    /// <summary>複数入力候補（例: "fu"と"hu"）</summary>
    private List<string> _romajiOtherPatterns = new List<string>();

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
    /// <summary>タイプ済み文字列を取得</summary>
    private string GetTypedText()
    {
        //ローマ字配列が空ならば空文字を返す
        if (_romajiChars.Count == 0) return string.Empty;
        string text = string.Empty;

        //0番目から現在の入力位置(_romanIndex)まで繰り返す
        for (int i = 0; i < _romajiChars.Count && i < _currentIndex; i++)
        {
            //@(終端記号)に到達したら終了
            if (_romajiChars[i] == '@') break;
            // 文字を連結
            text += _romajiChars[i];
        }
        return text;
    }
    /// <summary>未タイプ文字列を取得</summary>
    private string GetUnTypedText()
    {
        //ローマ字配列が空ならば空文字を返す
        if (_romajiChars.Count == 0) return string.Empty;
        string text = string.Empty;

        //現在の入力位置(_romanIndex)から配列の末尾まで繰り返す
        for (int i = _currentIndex; i < _romajiChars.Count; i++)
        {
            //@(終端記号)に到達したら終了
            if (_romajiChars[i] == '@') break;

            //文字を連結
            text += _romajiChars[i];
        }
        return text;
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
            for (int i = 1; i < parts.Length; i += 2)
            {
                //空白を取り除く
                string displayWord = parts[i].Trim();
                string InputWord = parts[i + 1].Trim();

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
    /// <summary>
    /// ローマ字の別解パターンを初期化
    /// </summary>
    private void InitializeOtherPatterns()
    {
        _otherPatterns = new Dictionary<string, List<string>>
        {
            //あ行
            { "a", new List<string> { "a" } },
            { "i", new List<string> { "i", "yi" } },
            { "u", new List<string> { "u", "wu", "whu" } },
            { "e", new List<string> { "e" } },
            { "o", new List<string> { "o" } },

            //か行
            {"ka", new List<string>{"ka","ca" } },
            {"ki", new List<string>{"ki" } },
            {"ku", new List<string>{"ku","cu", "qu" } },
            {"ke", new List<string>{"ke", } },
            {"ko", new List<string>{"ko","co" } },

            //さ行
            {"sa", new List<string>{"sa" } },
            {"si", new List<string>{"si", "shi", "ci" } },
            {"su", new List<string>{"su" } },
            {"se", new List<string>{"se", "ce" } },
            {"so", new List<string>{"so" } },

            //た行
            {"ta", new List<string>{"ta" } },
            {"ti", new List<string>{"ti", "chi" } },
            {"tu", new List<string>{"tu", "tsu" } },
            {"te", new List<string>{"te" } },
            {"to", new List<string>{"to" } },

            //な行
            {"na", new List<string>{"na" } },
            {"ni", new List<string>{"ni" } },
            {"nu", new List<string>{"nu" } },
            {"ne", new List<string>{"ne" } },
            {"no", new List<string>{"no" } },

            //は行
            {"ha", new List<string>{"ha" } },
            {"hi", new List<string>{"hi" } },
            {"hu", new List<string>{"hu","fu" } },
            {"he", new List<string>{"he" } },
            {"ho", new List<string>{"ho" } },

            //ま行
            {"ma", new List<string>{"ma" } },
            {"mi", new List<string>{"mi" } },
            {"mu", new List<string>{"mu" } },
            {"me", new List<string>{"me" } },
            {"mo", new List<string>{"mo" } },

            //や行
            {"ya", new List<string>{"ya" } },
            {"yu", new List<string>{"yu" } },
            {"yo", new List<string>{"yo" } },

            //ら行
            {"ra", new List<string>{"ra" } },
            {"ri", new List<string>{"ri" } },
            {"ru", new List<string>{"ru" } },
            {"re", new List<string>{"re" } },
            {"ro", new List<string>{"ro" } },

            //わ行
            {"wa", new List<string>{"wa" } },
            {"wo", new List<string>{"wo" } },
            {"nn", new List<string>{"nn","n" } },

            //が行
            {"ga", new List<string>{"ga" } },
            {"gi", new List<string>{"gi" } },
            {"gu", new List<string>{"gu" } },
            {"ge", new List<string>{"ge" } },
            {"go", new List<string>{"go" } },

            //ざ行
            {"za", new List<string>{"za" } },
            {"zi", new List<string>{"zi","ji" } },
            {"zu", new List<string>{"zu" } },
            {"ze", new List<string>{"ze" } },
            {"zo", new List<string>{"zo" } },

            //だ行
            {"da", new List<string>{"da" } },
            {"di", new List<string>{"di" } },
            {"du", new List<string>{"du" } },
            {"de", new List<string>{"de" } },
            {"do", new List<string>{"do" } },

            //ば行
            {"ba", new List<string>{"ba" } },
            {"bi", new List<string>{"bi" } },
            {"bu", new List<string>{"bu" } },
            {"be", new List<string>{"be" } },
            {"bo", new List<string>{"bo" } },

            //ぱ行
            {"pa", new List<string>{"pa" } },
            {"pi", new List<string>{"pi" } },
            {"pu", new List<string>{"pu" } },
            {"pe", new List<string>{"pe" } },
            {"po", new List<string>{"po" } },

            //きゃ行など
            {"kya", new List<string>{"kya" } },
            {"kyu", new List<string>{"kyu" } },
            {"kyo", new List<string>{"kyo" } },
            {"sya", new List<string>{"sya", "sha" } },
            {"syu", new List<string>{"syu", "shu" } },
            {"syo", new List<string>{"syo","sho" } },
            {"tya", new List<string>{"tya","cha", "cya" } },
            {"tyu", new List<string>{"tyu", "chu", "cyu" } },
            {"tyo", new List<string>{"tyo", "cho", "cyo" } },
            {"nya", new List<string>{"nya" } },
            {"nyu", new List<string>{"nyu" } },
            {"nyo", new List<string>{"nyo" } },
            {"hya", new List<string>{"hya" } },
            {"hyu", new List<string>{"hyu" } },
            {"hyo", new List<string>{"hyo" } },
            {"mya", new List<string>{"mya" } },
            {"myu", new List<string>{"myu" } },
            {"myo", new List<string>{"myo" } },
            {"rya", new List<string>{"rya" } },
            {"ryu", new List<string>{"ryu" } },
            {"ryo", new List<string>{"ryo" } },
            {"gya", new List<string>{"gya" } },
            {"gyu", new List<string>{"gyu" } },
            {"gyo", new List<string>{"gyo" } },
            {"zya", new List<string>{"zya", "ja", "jya" } },
            {"zyu", new List<string>{"zyu", "ju", "jyu" } },
            {"zyo", new List<string>{"zyo", "jo", "jyo" } },
            {"bya", new List<string>{"bya" } },
            {"byu", new List<string>{"byu" } },
            {"byo", new List<string>{"byo" } },
            {"pya", new List<string>{"pya" } },
            {"pyu", new List<string>{"pyu" } },
            {"pyo", new List<string>{"pyo" } },

            //小文字
            {"la", new List<string>{"la", "xa" } },
            {"li", new List<string>{"li", "xi","lyi", "lxi" } },
            {"lu", new List<string>{"lu", "xu" } },
            {"le", new List<string>{"le", "xe", "lye", "lxe" } },
            {"lo", new List<string>{"lo", "xo" } },
            {"ltu", new List<string>{"ltu", "xtu", "ltsu" } },
            {"lya", new List<string>{"lya", "xya" } },
            {"lyu", new List<string>{"lyu", "xyu" } },
            {"lyo", new List<string>{"lyo", "xyo" } },
            {"lwa", new List<string>{"lwa","xwa" } },

            //っから始まる
        　　{"kka", new List<string>{"kka", "cca" } },
            {"kki", new List<string>{"kki" } },
            {"kku", new List<string>{"kke", "ccu" } },
            {"kke", new List<string>{"kke" } },
            {"kko", new List<string>{"kko", "cco"} },
            {"ssa", new List<string>{"ssa" } },
            {"ssi", new List<string>{"ssi","cci" } },
            {"ssu", new List<string>{"ssu" } },
            {"sse", new List<string>{"sse", "cce" } },
            {"sso", new List<string>{"sso" } },
            {"tta", new List<string>{"tta" } },
            {"tti", new List<string>{"tti" } },
            {"ttu", new List<string>{"ttu" } },
            {"tte", new List<string>{"tte" } },
            {"tto", new List<string>{"tto" } },
            {"nna", new List<string>{"nna" } },
            {"nni", new List<string>{"nni" } },
            {"nnu", new List<string>{"nnu" } },
            {"nne", new List<string>{"nne" } },
            {"nno", new List<string>{"nno" } },
            {"hha", new List<string>{"hha" } },
            {"hhi", new List<string>{"hhi" } },
            {"hhu", new List<string>{"hhu" } },
            {"hhe", new List<string>{"hhe" } },
            {"hho", new List<string>{"hho" } },
            {"mma", new List<string>{"mma" } },
            {"mmi", new List<string>{"mmi" } },
            {"mmu", new List<string>{"mmu" } },
            {"mme", new List<string>{"mme" } },
            {"mmo", new List<string>{"mmo" } },
            {"yya", new List<string>{"yya" } },
            {"yyu", new List<string>{"yyu" } },
            {"yyo", new List<string>{"yyo" } },
            {"rra", new List<string>{"rra" } },
            {"rri", new List<string>{"rri" } },
            {"rru", new List<string>{"rru" } },
            {"rre", new List<string>{"rre" } },
            {"rro", new List<string>{"rro" } },
            {"wwa", new List<string>{"wwa" } },
            {"wwo", new List<string>{"wwo" } },
            {"gga", new List<string>{"gga" } },
            {"ggi", new List<string>{"ggi" } },
            {"ggu", new List<string>{"ggu" } },
            {"gge", new List<string>{"gge" } },
            {"ggo", new List<string>{"ggo" } },
            {"zza", new List<string>{"zza" } },
            {"zzi", new List<string>{"zzi", "jji" } },
            {"zzu", new List<string>{"zzu" } },
            {"zze", new List<string>{"zze" } },
            {"zzo", new List<string>{"zzo" } },
            {"bba", new List<string>{"bba" } },
            {"bbi", new List<string>{"bbi" } },
            {"bbu", new List<string>{"bbu" } },
            {"bbe", new List<string>{"bbe" } },
            {"bbo", new List<string>{"bbo" } },
            {"ppa", new List<string>{"ppa" } },
            {"ppi", new List<string>{"ppi" } },
            {"ppu", new List<string>{"ppu" } },
            {"ppe", new List<string>{"ppe" } },
            {"ppo", new List<string>{"ppo" } },

            //記号
            {"-", new List<string>{"-" } },
            {",", new List<string>{"," } },
            {"!", new List<string>{"!" } },
            {"?", new List<string>{"?" } },
            {"", new List<string>{"" } },
        };
    }
}
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
    private List<int> _candidateIndices = new List<int>();
    /// <summary>複数入力候補（例: "fu"と"hu"）</summary>
    private List<string> _romajiOtherPatterns = new List<string>();

    /// <summary>餌に掛かっている魚のデータ</summary>
    private FishBase _currentFish;
    private int _mitakeTimer = 1;
    /// <summary>タイピングが成功したときのイベント</summary>
    public event Action<FishBase, bool> OnTypingCompleted;

    // UIManager用プロパティ
    public string CurrentDisplayWord => _targetWord;
    public FishBase CurrentFish => _currentFish;
    /// <summary>
    /// 単語に対応したローマ字のペア
    /// </summary>
    [System.Serializable]
    public class WordPair
    {
        private string _word;//表示用
        private string _romaji;//入力用
        public string Word
        {
            get { return _word; }
            set { _word = value; }
        }
        public string Romaji
        {
            get { return _romaji; }
            set { _romaji = value; }
        }
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
        InitializeOtherPatterns();
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
    /// ローマ字文字列から複数の入力候補を生成
    /// </summary>
    /// <param name="romaji"></param>
    /// <returns></returns>
    private List<string> RomajiCandidatesGenerater(string romaji)
    {
        // 最初の候補として元のローマ字を小文字化して追加
        List<string> candidates = new List<string> { romaji.ToLower() };

        // ローマ字を分解して別解を探す
        int i = 0;// 現在見ている文字位置
        List<List<string>> allVariants = new List<List<string>>();// 各部分の別解リスト

        // ローマ字文字列を先頭から1文字ずつ処理
        while (i < romaji.Length)
        {
            bool found = false;// この位置で辞書の一致が見つかったかのフラグ

            // 優先順位①　3文字一致を探す
            if (i + 2 < romaji.Length)
            {
                //現在の位置から3文字を取得
                string three = romaji.Substring(i, 3).ToLower();

                //辞書に一致している文字がないかチェック
                if (_otherPatterns.ContainsKey(three))
                {
                    //見つかったらその文字の別解リストを追加
                    allVariants.Add(_otherPatterns[three]);
                    i += 3;//3文字進める
                    found = true;//見つかったフラグを立てる
                }
            }
            // 優先順位②　2文字一致を探す
            if (i + 1 < romaji.Length && !found)
            {
                //現在の位置から2文字を取得
                string two = romaji.Substring(i, 2).ToLower();

                //辞書に一致している文字がないかチェック
                if (_otherPatterns.ContainsKey(two))
                {
                    allVariants.Add(_otherPatterns[two]);
                    i += 2;
                    found = true;
                }
            }
            // 優先順位③　1文字一致
            if (!found)//3文字も2文字も見つからなかった場合
            {
                //現在の位置から1文字を取得
                string one = romaji.Substring(i, 1).ToLower();

                //辞書に一致している文字がないかチェック
                if (_otherPatterns.ContainsKey(one))
                {
                    allVariants.Add(_otherPatterns[one]);
                    found = true;
                }
                else
                {
                    //辞書にない文字はそのまま1文字として追加
                    allVariants.Add(new List<string> { one });
                }
                i++;//1文字分進める
            }
        }
        //分解した各部分の別解を組み合わせて、全パターンを追加
        candidates = CombinationGenerator(allVariants);

        //生成された全パターンを返す
        return candidates;
    }
    /// <summary>
    ///　文字の組み合わせを生成
    /// </summary>
    /// <param name="variants"></param>
    /// <returns></returns>
    private List<string> CombinationGenerator(List<List<string>> variants)
    {
        // 空リストの場合は空文字のリストを返す
        if (variants.Count == 0) return new List<string> { "" };

        // 結果を格納するリスト（初期は空文字で開始）
        List<string> result = new List<string> { "" };

        // 各部分の別解リストを順番に処理
        foreach (var variantList in variants)
        {
            // 新しい結果を格納する一時リスト
            List<string> newResult = new List<string>();

            // 現在の結果の各文字列を取得
            foreach (var current in result)
            {
                foreach (var variant in variantList)
                {
                    // 現在の文字列と別解を連結して追加
                    newResult.Add(current + variant);
                }
            }
            result = newResult;
        }
        return result;
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
            for (int i = 1; i < parts.Length - 1; i += 2)
            {
                //空白を取り除く
                string displayWord = parts[i].Trim();
                string InputWord = parts[i + 1].Trim();

                //文字が入っていたら辞書に追加
                if (!string.IsNullOrWhiteSpace(InputWord) && !string.IsNullOrEmpty(displayWord))
                {
                    WordPair wordPair = new WordPair();
                    wordPair.Word = displayWord;
                    wordPair.Romaji = InputWord;
                    _wordPairs[level].Add(wordPair);
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
    private WordPair GetRandomWord(int level)
    {
        //指定されたレベルに単語があるかチェックして、存在しない場合空文字を返す
        if (!_wordPairs.ContainsKey(level) || _wordPairs[level].Count == 0)
        {
            //空文字を返す
            return null;
        }

        //ランダムに列の中の単語を返す
        int index = UnityEngine.Random.Range(0, _wordPairs[level].Count);
        return _wordPairs[level][index];
    }
    /// <summary>
    /// 出題開始
    /// </summary>
    /// <param name="fisbase"></param>
    public void StartTyping(FishBase fishBase)
    {
        if (fishBase == null)
        {
            Debug.Log("魚がまだ餌にかかってない");
            return;
        }

        _currentFish = fishBase;
        InitializeWord();
    }
    /// <summary>
    /// 単語の初期化
    /// </summary>
    private void InitializeWord()
    {
        if (_currentFish == null) return;

        WordPair wordPair = GetRandomWord(_currentFish.Level);
        if (wordPair == null)
        {
            Debug.Log($"レベル{_currentFish.Level}の単語が取得できませんでした");
            Reset();
            return;
        }

        _targetWord = wordPair.Word;

        //CSVに書かれたローマ字を分解し複数の候補を生成
        _romajiOtherPatterns = RomajiCandidatesGenerater(wordPair.Romaji);

        _romajiChars.Clear();
        _currentIndex = 0;

        string firstCandidate = _romajiOtherPatterns[0];
        foreach (char c in firstCandidate)
        {
            _romajiChars.Add(c);
        }
        _romajiChars.Add('@');

        //各候補の位置を初期化
        _candidateIndices.Clear();
        foreach (var candidate in _romajiOtherPatterns)
        {
            _candidateIndices.Add(0);
        }
    }
    /// <summary>
    /// キー入力処理
    /// </summary>
    private void OnGUI()
    {
        if (Event.current.type == EventType.KeyDown)
        {
            char inputChar = GetCharFromKeyCode(Event.current.keyCode);
            if (inputChar != '\0')
            {
                InputChar(inputChar);
            }
        }
    }
    /// <summary>
    /// タイピング
    /// </summary>
    /// <param name="input"></param>
    private void InputChar(char inputChar)
    {
        inputChar = char.ToLower(inputChar);

        bool isCorrect = false;
        int matchedCandidateIndex = -1;

        for (int i = 0; i < _romajiOtherPatterns.Count; i++)
        {
            string candidate = _romajiOtherPatterns[i];
            int currentIndex = _candidateIndices[i];

            if (currentIndex < candidate.Length && candidate[currentIndex] == inputChar)
            {
                isCorrect = true;
                matchedCandidateIndex = i;
                break;
            }
        }
        if (isCorrect)
        {
            _currentIndex++;
            _candidateIndices[matchedCandidateIndex]++;

            for (int i = _romajiOtherPatterns.Count - 1; i >= 0; i--)
            {
                string candidate = _romajiOtherPatterns[i];
                int currentIndex = _candidateIndices[i];

                if (currentIndex >= candidate.Length || candidate[currentIndex - 1] != inputChar)
                {
                    _romajiOtherPatterns.RemoveAt(i);
                    _candidateIndices.RemoveAt(i);
                }
            }
            bool complete = false;
            for (int i = 0; i < _romajiOtherPatterns.Count; i++)
            {
                if (_candidateIndices[i] >= _romajiOtherPatterns[i].Length)
                {
                    complete = true;
                    break;
                }
            }
            if (complete)
            {
                Attack();
            }
        }
        else
        {
            bool escaped = false;
            _currentFish.Timer -= 1;
            if (_currentFish.Timer <= 0)
            {
                escaped = true;
            }

            if (escaped)
            {
                OnTypingCompleted?.Invoke(_currentFish, false);
            }
        }
    }
    /// <summary>
    /// 魚にダメージを与える
    /// </summary>
    private void Attack()
    {
        bool isCaught = false;
        _currentFish.HP -= 10;
        if (_currentFish.HP <= 0)
        {
            isCaught = true;
        }
        //釣り成功
        if (isCaught)
        {
            Reset();
            OnTypingCompleted?.Invoke(_currentFish, true);
        }
        //HPが残っていたら新しい単語を出して釣り続行
        else
        {
            _currentIndex = 0;
            InitializeWord();
        }
    }
    private void Reset()
    {
        _currentFish = null;
        _currentIndex = 0;
        _targetWord = string.Empty;
    }
    /// <summary>
    /// KeyCodeを文字に変換
    /// </summary>
    private char GetCharFromKeyCode(KeyCode keyCode)
    {
        switch (keyCode)
        {
            case KeyCode.A: return 'a';
            case KeyCode.B: return 'b';
            case KeyCode.C: return 'c';
            case KeyCode.D: return 'd';
            case KeyCode.E: return 'e';
            case KeyCode.F: return 'f';
            case KeyCode.G: return 'g';
            case KeyCode.H: return 'h';
            case KeyCode.I: return 'i';
            case KeyCode.J: return 'j';
            case KeyCode.K: return 'k';
            case KeyCode.L: return 'l';
            case KeyCode.N: return 'n';
            case KeyCode.M: return 'm';
            case KeyCode.O: return 'o';
            case KeyCode.P: return 'p';
            case KeyCode.Q: return 'q';
            case KeyCode.R: return 'r';
            case KeyCode.S: return 's';
            case KeyCode.T: return 't';
            case KeyCode.U: return 'u';
            case KeyCode.V: return 'v';
            case KeyCode.W: return 'w';
            case KeyCode.X: return 'x';
            case KeyCode.Y: return 'y';
            case KeyCode.Z: return 'z';
            case KeyCode.Question: return '?';
            case KeyCode.Exclaim: return '!';
            case KeyCode.Minus: return '-';
            case KeyCode.Equals: return '=';
            default: return '\0';
        }
    }
    /// <summary>
    /// ローマ字の別解パターンを辞書に格納
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
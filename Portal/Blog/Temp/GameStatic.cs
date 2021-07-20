using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 窗口类型
/// </summary>
public enum Panel
{
    MainMenu,
    PersonCenter,                //个人中心窗口
    Promotion,                   //推广赚钱窗口
    WashCode,                    //洗码窗口
    CustomerService,             //客服窗口
    Safe,                        //保险箱窗口
    Withdraw,                    //提现窗口
    Recharge,                    //充值窗口
}

/// <summary>
/// 布局类型
/// </summary>
public enum LayoutMode
{
    Normal,
    QiPai,
    DianZi,
    ZhenRen,
}

/// <summary>
/// 游戏状态
/// </summary>
public enum GameState
{
    Init,
    Loading,
    Maintenance,
    LowVersion,
    Run
}

public static class GameStatic
{

    #region Variable

    public static GameState crtState = GameState.Init;

    /// <summary>
    /// 字体用的的金色
    /// </summary>
    public static readonly Color GoldColor = new Color(239 / (float)255, 208 / (float)255, 162 / (float)255);

    /// <summary>
    /// uncoding编码空格
    /// </summary>
    public static readonly string no_breaking_space = "\u00A0";

    /// <summary>
    /// 游戏是否正在开启中
    /// </summary>
    public static bool IsGameOpening = false;

    public static bool OpenEffect = false;


    [HideInInspector]
    public static int ScreenWidth;

    [HideInInspector]
    public static int ScreenHeight;

    public static GameObject MainCanvas;

    public static Camera MainCamera;

    /// <summary>
    /// 平台类型
    /// </summary>
#if UNITY_WEBGL
    public static readonly string SourceType="0";
#elif UNITY_ANDROID
    public static readonly string SourceType = "1";
#elif UNITY_IOS
    public static readonly string SourceType = "2";
#else
    public static readonly string SourceType = "0";
#endif

    public static readonly string Name = "完美棋牌";
    public static readonly string WebsocketUrl = @"...";
    public static readonly string HttpPostUrl = @"http://10.10.72.63:80/";

    /// <summary>
    /// APPInfo
    /// "id":1,"url":"https://www.baidu.com/","logo":"http://10.10.72.27:8081/version/157199940743382668.png","state":1,"isGraphVerifi":1,"isRealName":1,"isPhone":1,"isSmsCode":1,"promoteUrl":"","onlineServiceUrl":"","version":"","lua":""
    /// </summary>
    public static JsonData appInfo;

    #endregion

    #region Action

    /// <summary>
    /// 主要面板初始化的时候
    /// </summary>
    public static Action OnOpenMainScene;

    /// <summary>
    /// 登录的时候
    /// </summary>
    public static Action OnLogin;

    /// <summary>
    /// 登出的时候
    /// </summary>
    public static Action OnLogout;

    /// <summary>
    /// 初始化加载完毕的时候
    /// </summary>
    public static Action OnInitComplete;

    /// <summary>
    /// 切换面板的时候
    /// </summary>
    public static Action<Panel> OnChangePanel;

    /// <summary>
    /// 需要设置保险箱钱数的时候
    /// </summary>
    public static Action<string, string> OnSetSafeMoneyText;

    /// <summary>
    /// 下分的时候
    /// </summary>
    public static Action OnDownScore;

    #endregion

    #region Tool Method
    /// <summary>
    /// 初始化scrollRect对象的位置
    /// </summary>
    /// <param name="rt"></param>
    public static void InitScrollRectTarget(RectTransform rt)
    {
        Vector2 temp = rt.anchoredPosition;
        temp.x = 0;
        temp.y = 0;
        rt.anchoredPosition = temp;
    }

    /// <summary>
    /// 时间戳格式化
    /// </summary>
    /// <param name="originDateString"></param>
    /// <returns></returns>
    public static string DateFormat(string originDateString, bool hasSecend = false)
    {
        if (hasSecend == true)
        {
            return DateTime.Parse(originDateString).ToString("yyyy-MM-dd HH:mm:ss");
        }
        return DateTime.Parse(originDateString).ToString("yyyy-MM-dd HH:mm");
    }

    /// <summary>
    /// 检测jsondata是否为空
    /// </summary>
    /// <param name="jsondata">检测的JsonData</param>
    /// <returns></returns>
    public static string CheckNull(JsonData jsondata)
    {
        string str = string.Empty;

        if (jsondata != null)
        {
            str = jsondata.ToString();
        }

        return str;
    }

    /// <summary>
    /// 版本比较
    /// </summary>
    /// <param name="version1"></param>
    /// <param name="version2"></param>
    /// <returns></returns>
    public static int VersionComparison(string version1, string version2)
    {
        string[] version1String = version1.Split(new char[] { '.' });
        string[] version2String = version2.Split(new char[] { '.' });

        for (int i = 0; i < version1String.Length; i++)
        {
            if (version1String[i].CompareTo(version2String[i]) != 0)
            {
                return version1String[i].CompareTo(version2String[i]);
            }
        }

        return 0;
    }

    /// <summary>
    /// 复制字符串到剪切板
    /// </summary>
    /// <param name="copyTarget">复制的字符串</param>
    public static void CopyString(string copyString)
    {
        //GUIUtility.systemCopyBuffer = copyString;
        UniClipboard.SetText(copyString);
        TipsManager._Instance.OpenSuccessLable("复制成功");
    }

    /// <summary>
    /// base64转Sprite
    /// </summary>
    /// <param name="imgComponent"></param>
    /// <param name="base64"></param>
    public static void Base64ToImg(Image imgComponent, string base64)
    {
        byte[] bytes = Convert.FromBase64String(base64);
        Texture2D tex2D = new Texture2D(100, 100);
        tex2D.LoadImage(bytes);
        Sprite s = Sprite.Create(tex2D, new Rect(0, 0, tex2D.width, tex2D.height), new Vector2(0.5f, 0.5f));
        imgComponent.sprite = s;
        imgComponent.preserveAspect = true;
        Resources.UnloadUnusedAssets();
    }

    /// <summary>
    /// 退出应用
    /// </summary>
    public static void QuitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// 延迟一帧执行
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public static IEnumerator DelayDoIt(Action action)
    {
        yield return null;
        action?.Invoke();
    }
    #endregion

    #region LoopScroll
    public static void ChangeLoopScroll(LoopHorizontalScrollRect _target, JsonData _gameInfo, bool _isPlatform, bool _isSecond, LayoutMode mode)
    {
        crtMode = mode;
        isSearch = false;
        target = _target;
        isSecond = _isSecond;
        isPlatform = _isPlatform;

        JsonData list;
        if (isSecond == true)
        {
            secondLevelInfo = _gameInfo;
            list = _gameInfo["games"];
        }
        else
        {
            firstLevelInfo = _gameInfo;
            list = _gameInfo["list"];
        }

        SetCount(list.Count);
    }

    public static void SearchGame(string str)
    {
        if (str.Equals(string.Empty))
        {
            isSearch = false;
            SetCount(crtList.Count);
        }
        else
        {

            sreachInfo = JsonMapper.ToObject(crtGameInfo.ToJson());
            sreachInfo["games"].Clear();

            for (int i = 0; i < crtList.Count; i++)
            {
                if (crtList[i]["gameName"].ToString().Contains(str) == true)
                {
                    sreachInfo["games"].Add(crtList[i]);
                }
            }
            isSearch = true;
            SetCount(sreachInfo["games"].Count);
        }
    }

    public static void SetCount(int rawCount)
    {

        switch (crtMode)
        {
            case LayoutMode.Normal:
                rawCount = rawCount / 2 + rawCount % 2;
                break;
            case LayoutMode.QiPai:
                break;
            case LayoutMode.DianZi:
                rawCount = rawCount / 2 + rawCount % 2;
                break;
            case LayoutMode.ZhenRen:
                rawCount = 1 + (rawCount - 1) / 2 + (rawCount - 1) % 2;
                break;
            default:
                break;
        }
        target.totalCount = rawCount;
        target.RefillCells();
    }

    public static void ChangeState(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.Init:
                SceneManager.LoadScene("Init Scene");
                break;
            case GameState.Loading:
                SceneManager.LoadScene("Loading Scene");
                break;
            case GameState.Maintenance:
                SceneManager.LoadScene("Loading Scene");
                break;
            case GameState.LowVersion:
                SceneManager.LoadScene("Loading Scene");
                break;
            case GameState.Run:
                SceneManager.LoadScene("Main Scene");
                break;
            default:
                Debugger.Log("GameStatic判断了未知的枚举");
                break;
        }
        crtState = gameState;
    }

    public static JsonData crtGameInfo
    {
        get
        {
            if (isSearch == true)
            {
                return sreachInfo;
            }
            else if (isSecond == true)
            {
                return secondLevelInfo;
            }
            else
            {
                return firstLevelInfo;
            }
        }
    }

    public static JsonData crtList
    {
        get
        {
            if (isSearch == true)
            {
                return sreachInfo["games"];
            }
            else if (isSecond == true)
            {
                return secondLevelInfo["games"];
            }
            else
            {
                return firstLevelInfo["list"];
            }
        }
    }

    public static LoopHorizontalScrollRect target { get; private set; }

    public static void ReturnFirstLevel()
    {
        isSecond = false;
        isPlatform = true;
    }

    private static JsonData firstLevelInfo;
    private static JsonData secondLevelInfo;
    private static JsonData sreachInfo;
    private static bool isSecond;
    private static bool isSearch;

    public static LayoutMode crtMode { get; private set; }
    public static bool isPlatform { get; private set; }
    #endregion

}

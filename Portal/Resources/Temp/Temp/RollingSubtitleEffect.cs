using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RollingSubtitleEffect : MonoBehaviour
{
    public static RollingSubtitleEffect _Instance;

    #region public method
    /// <summary>
    /// 添加消息
    /// </summary>
    /// <param name="message"></param>
    public void AddMessage(string message)
    {
        _Messages.Enqueue(message);
    }

    /// <summary>
    /// 设置特别消息
    /// </summary>
    /// <param name="message"></param>
    public void SetSpecialMessage(string message)
    {
        _SpecialMessage = message;
    }

    public void SetList(List<string> ls)
    {
        _RawMessage = ls;
        _pos = 0;
        ResetState();
    }
    #endregion

    private List<string> _RawMessage;

    private int _pos;

    private Queue<string> _Messages;                        //待显示消息队列

    private Transform _Mask;                                //显示消息的mask

    private RectTransform[] _Texts = new RectTransform[2];  //文本RectTransform数组 

    private string _SpecialMessage = string.Empty;          //特殊消息 当不为空字符串时只显示特殊消息，当其为空时正常显示

    private float _MaskWidth = 0;                           //mask的宽度

    private bool _Text1Arrival;                             //文本1是否到达左侧状态量
    private bool _Text2Arrival;                             //文本2是否到达左侧状态量

    private int _FrameCounter = 0;                          //帧计数器

    private void Awake()
    {
        _Instance = this;
        _Messages = new Queue<string>();
    }

    private void Start()
    {
        _Mask = transform;
        _Texts[0] = _Mask.GetChild(0).GetComponent<RectTransform>();
        _Texts[1] = _Mask.GetChild(1).GetComponent<RectTransform>();

        _MaskWidth = _Mask.GetComponent<RectTransform>().rect.width;
    }

    private void Update()
    {
        //每一帧往右移动十分之一
        _Texts[0].anchoredPosition = new Vector2(_Texts[0].anchoredPosition.x - _MaskWidth * 0.1f * Time.deltaTime, 0);
        _Texts[1].anchoredPosition = new Vector2(_Texts[1].anchoredPosition.x - _MaskWidth * 0.1f * Time.deltaTime, 0);

        _FrameCounter++;

        //每五帧执行一次检测
        if (_FrameCounter % 5 == 0)
        {
            if (!_Text1Arrival)
            {
                if (_Texts[0].anchoredPosition.x - _Texts[0].rect.width * 0.5f < -1)
                {
                    if (_Texts[0].rect.width < _MaskWidth)
                    {
                        StartCoroutine(SetText(1));
                    }
                    else
                    {
                        if (_Texts[0].anchoredPosition.x + _Texts[0].rect.width * 0.5f < _MaskWidth * 0.9f)
                        {
                            StartCoroutine(SetText(1));
                        }
                    }
                }
            }
            if (!_Text2Arrival)
            {
                if (_Texts[1].anchoredPosition.x - _Texts[1].rect.width * 0.5f < -1)
                {
                    if (_Texts[1].rect.width < _MaskWidth)
                    {
                        StartCoroutine(SetText(0));
                    }
                    else
                    {
                        if (_Texts[1].anchoredPosition.x + _Texts[1].rect.width * 0.5f < _MaskWidth * 0.9f)
                        {
                            StartCoroutine(SetText(0));
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 重设文本协程
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private IEnumerator SetText()
    {

        StartCoroutine(SetText(0));

        StartCoroutine(SetText(1));

        yield return null;

        _Texts[0].SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, _Texts[0].rect.width * 0.5f, 0);
        _Texts[1].SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, _Texts[1].rect.width * 0.5f, 0);
        yield return null;
        _Texts[0].anchoredPosition = new Vector2(_Texts[0].anchoredPosition.x + _MaskWidth, 0);
        _Texts[1].anchoredPosition = new Vector2(_Texts[1].anchoredPosition.x + _Texts[0].rect.width + 2 * _MaskWidth, 0);

    }

    /// <summary>
    /// 设置文本协程
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private IEnumerator SetText(int index)
    {
        if (index == 1)
        {
            _Text1Arrival = true;

            _Text2Arrival = false;

        }
        else
        {
            _Text2Arrival = true;

            _Text1Arrival = false;
        }

        string text = GameStatic.Name+"，有你精彩！";

        if (_SpecialMessage != string.Empty)
        {
            text = _SpecialMessage;
        }
        else if (_RawMessage != null)
        {
            text = _RawMessage[_pos];
            if (++_pos >= _RawMessage.Count)
            {
                _pos = 0;
            }
        }
        else
        {
            if (_Messages.Count != 0)
            {
                text = _Messages.Dequeue();
            }
        }

        _Texts[index].anchoredPosition = new Vector2(_Texts[index].GetComponent<Text>().fontSize * text.Length, Screen.height);

        _Texts[index].GetComponent<Text>().text = text;

        yield return null;

        _Texts[index].anchoredPosition = new Vector2(_Texts[index].rect.width * 0.5f + _MaskWidth, 0);
    }

    /// <summary>
    /// 重置状态
    /// </summary>
    private void ResetState()
    {
        _SpecialMessage = string.Empty;
        _Messages.Clear();

        _Text1Arrival = false;
        _Text2Arrival = false;

        StartCoroutine(SetText());
    }
}

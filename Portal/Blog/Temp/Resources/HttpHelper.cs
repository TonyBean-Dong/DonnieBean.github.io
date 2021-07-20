using System;
using System.Text;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Networking;
using LitJson;
using UnityEngine;
using System.IO;

public static class HttpHelper
{
    /// <summary>
    /// post请求协程
    /// </summary>
    /// <param name="url"></param>
    /// <param name="dss"></param>
    /// <param name="header"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public static IEnumerator Post(string url, Dictionary<string, string> dss, Dictionary<string, string> header, Action<UnityWebRequest> callback)
    {
        TipsManager._Instance.StartLoading();
        if (dss == null)
        {
            Debug.LogError("Post请求内容为空");
        }

        UnityWebRequest unityWebRequest = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
        unityWebRequest.timeout = 8;

        //当请求的url中部含有sso时，发送加密的数据，否则发送未加密的数据
        if (url.Substring(GameStatic.HttpPostUrl.Length, 3).Equals("sso"))
        {
            JsonData jd = new JsonData();
            foreach (var item in dss)
            {
                jd[item.Key] = item.Value;
            }
            JsonData jd2 = new JsonData();
            jd2["data"] = DataEncryptDecrypt.EncryptDecrypt.dataEncrypt(jd.ToJson());

            unityWebRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jd2.ToJson()));
        }
        else
        {
            WWWForm form = new WWWForm();
            foreach (var item in dss)
            {
                form.AddField(item.Key, item.Value);
            }
            unityWebRequest = UnityWebRequest.Post(url, form);
        }

        if (header != null)
        {
            foreach (var tmp in header)
            {
                //当url中部不含有sso时不需要添加内容类型的请求头
                if (!url.Substring(GameStatic.HttpPostUrl.Length, 3).Equals("sso"))
                {
                    if (tmp.Key == "Content-Type")
                    {
                        continue;
                    }
                }
                unityWebRequest.SetRequestHeader(tmp.Key, tmp.Value);
            }
        }

        unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
        unityWebRequest.useHttpContinue = false;                            //当其值为true且后端未设置100-continue的回调时，会无法得到返回的回调

        yield return unityWebRequest.SendWebRequest();

        TipsManager._Instance.StopLoading();

        if (unityWebRequest.isHttpError || unityWebRequest.isNetworkError)
        {
            Debug.LogError(url + "请求出错" + unityWebRequest.error + "|" + unityWebRequest.downloadHandler.text);

            callback(null);

#if TEST
           GameObject go = new GameObject("TESTONLYIMAGE");
            go.transform.SetParent(CanvasManager._Instance._Canvas,false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 0);
            rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 0);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;

            go.AddComponent<UnityEngine.UI.Image>();
            UnityEngine.UI.Button button = go.AddComponent<UnityEngine.UI.Button>();
            button.onClick.AddListener(() => UnityEngine.Object.Destroy(go));

            GameObject go2 = new GameObject("TESTONLYTEXT");
            go2.transform.SetParent(go.transform, false);
            RectTransform rt2 = go2.AddComponent<RectTransform>();
            rt2.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 0);
            rt2.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 0);
            rt2.anchorMin = Vector2.zero;
            rt2.anchorMax = Vector2.one;

            UnityEngine.UI.Text text = go2.AddComponent<UnityEngine.UI.Text>();
            text.text = url + "请求出错" + unityWebRequest.error + "|" + unityWebRequest.downloadHandler.text;
            text.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
            text.color = Color.black;
            text.resizeTextForBestFit = true;
#endif
        }
        else
        {
            try
            {
                JsonData jd = JsonMapper.ToObject(unityWebRequest.downloadHandler.text); //字符串转Json
                if (((IDictionary)jd).Contains("code"))
                {
                    switch (JsonMapper.ToObject(unityWebRequest.downloadHandler.text)["code"].ToString())
                    {
                        case "-1":
                            LoadingManager._Instance.Maintenance();
                            break;
                        case "-2":
                            GameStatic.OnLogout?.Invoke();
                            break;
                        default:
                            callback(unityWebRequest);
                            unityWebRequest.Dispose();
                            break;
                    }
                }
                else
                {
                    callback(unityWebRequest);
                    unityWebRequest.Dispose();
                }
            }
            catch (Exception)
            {

            }
        }
    }
    public static IEnumerator GetText(string url, Action<string> actionResult)
    {
        UnityWebRequest unityWebRequest = new UnityWebRequest(url);
        unityWebRequest.downloadHandler = new DownloadHandlerBuffer();

        yield return unityWebRequest.SendWebRequest();

        if (unityWebRequest.isNetworkError || unityWebRequest.isHttpError)
        {
            Debug.LogError(url + "请求文本出错" + unityWebRequest.error + "|" + unityWebRequest.downloadHandler.text);
        }
        else
        {
            actionResult?.Invoke(unityWebRequest.downloadHandler.text);
            unityWebRequest.Dispose();
        }
    }



    /// <summary>
    /// 请求图片协程
    /// </summary>
    /// <param name="url">图片地址,like 'https://image.demo.test.doulang9.com/logo.png'</param>
    /// <param name="action">请求发起后处理回调结果的委托,处理请求结果的图片</param>
    /// <returns></returns>
    public static IEnumerator GetTexture(string url, Action<Texture2D> actionResult)
    {
        UnityWebRequest unityWebRequest = new UnityWebRequest(url);
        unityWebRequest.timeout = 300;
        DownloadHandlerTexture downloadTexture = new DownloadHandlerTexture(true);
        unityWebRequest.downloadHandler = downloadTexture;

        yield return unityWebRequest.SendWebRequest();

        if (unityWebRequest.isNetworkError || unityWebRequest.isHttpError)
        {
            Debug.LogError(url + "请求图片出错" + unityWebRequest.error + "|" + unityWebRequest.downloadHandler.text);
        }
        else
        {
            actionResult?.Invoke(downloadTexture.texture);
            unityWebRequest.Dispose();
        }
    }
}

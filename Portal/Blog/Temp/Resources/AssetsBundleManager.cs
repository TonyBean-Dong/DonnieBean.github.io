using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class AssetsBundleManager : MonoBehaviour
{
    public static AssetsBundleManager _Instance;

    private Dictionary<string, AssetBundleInfo> ABIS;

    public static string[] crtGameListTags;

    private void Awake()
    {

        ABIS = new Dictionary<string, AssetBundleInfo>();

        crtGameListTags = new string[0];

        _Instance = this;
    }

    bool isLoading=true;

    void Start()
    {
        StartCoroutine(LoadAssetBundle());
    }

    private void Update()
    {
        if (isLoading==true)
        {
            LoadingManager._Instance._TargetProgress = crtProgress;
        }
    }

    private IEnumerator LoadAssetBundle()
    {
        bool needWai = true;
        HttpManager._Instance.GetAssetBundleInfo((jsonData) =>
        {
            needWai = false;
            InitABIS(jsonData);
        });

        yield return new WaitUntil(() => needWai == false);

        foreach (var item in ABIS)
        {
            WWW www = WWW.LoadFromCacheOrDownload(item.Value.url, item.Value.version);
            StartCoroutine(ShowProgress(www));
            yield return www;
            item.Value.assetBundle = www.assetBundle;

            if (item.Value.assetBundle!=null)
            {
                if (item.Key != "local")
                {
                    item.Value.assetBundle.Unload(true);
                }
            }
#if TEST
            else
            {
                Debug.Log(item.Key + " is null,url:" + item.Value.url);
            }
#endif

            LoadingManager._Instance.ResetProgressBar();
        }



        GameStatic.OnLocalAssetBundleLoadComplete();
        isLoading = false;
    }

    private float crtProgress;

    IEnumerator ShowProgress(WWW www)
    {
        crtProgress = 0;
        while (www.progress < 1)
        {
            crtProgress = www.progress;
            yield return null;
        }
        crtProgress = 1;
    }

    public void InitABIS(JsonData jsonData)
    {
        for (int i = 0; i < jsonData["result"].Count; i++)
        {
            ABIS.Add(jsonData["result"][i]["note"].ToString(),new AssetBundleInfo(jsonData["result"][i]["imgFileAddress"].ToString(),int.Parse( jsonData["result"][i]["version"].ToString())));
        }
    }

    public Sprite GetLocalSprite(string name)
    {
        if (ABIS.ContainsKey("local")==false|| ABIS["local"].assetBundle == null)
        {
            return null;
        }
        Sprite sprite = ABIS["local"].assetBundle.LoadAsset<Sprite>(name);
        return sprite;
    }

    public IEnumerator GetResource<T>(string tag, string name, Action<T> action) where T : UnityEngine.Object
    {
        if (tag==null|| ABIS.ContainsKey(tag)==false|| ABIS[tag] == null|| ABIS[tag].assetBundle == null)
        {
            action(null);
            yield break;
        }
        AssetBundleRequest abr= ABIS[tag].assetBundle.LoadAssetAsync<T>(name);
        yield return new WaitUntil(()=> abr.isDone==true) ;

        T res = abr.asset as T;
        action(res);
    }

    public void StartLoadAssetBundle(string tag)
    {
        StartCoroutine(LoadAssetBundle(tag));
    }

    public IEnumerator StartLoadGameListResources(string[] tagsName)
    {

        if (tagsName.Equals(crtGameListTags)==true)
        {
            yield break;
        }

        DisposeGameListResources(crtGameListTags);

        crtGameListTags = tagsName;

        for (int i = 0; i < tagsName.Length; i++)
        {
           yield return StartCoroutine(LoadAssetBundle(tagsName[i]));
        }
    }


    private IEnumerator LoadAssetBundle(string tag)
    {
        if (ABIS.ContainsKey(tag)==false)
        {
            yield break;
        }

        if (ABIS[tag].assetBundle!=null)
        {
            yield break;
        }
       
        WWW www = WWW.LoadFromCacheOrDownload(ABIS[tag].url, ABIS[tag].version);
        yield return www;
        ABIS[tag].assetBundle = www.assetBundle;
    }

    public void DisposeAssetBundle(string tag,bool clearAll)
    {
        if (ABIS.ContainsKey(tag)==false)
        {
            return;
        }

        if (ABIS[tag].assetBundle != null)
        {
            ABIS[tag].assetBundle.Unload(clearAll);
        }
    }

    private void DisposeGameListResources(string[] tagNames)
    {
        for (int i = 0; i < tagNames.Length; i++)
        {
            DisposeAssetBundle(tagNames[i], false);
        }
       
    }

    class AssetBundleInfo
    {
        public string url;
        public int version;
        public AssetBundle assetBundle;

        public AssetBundleInfo(string _url, int _version)
        {
            url = _url;
            version = _version;
        }
    }
}
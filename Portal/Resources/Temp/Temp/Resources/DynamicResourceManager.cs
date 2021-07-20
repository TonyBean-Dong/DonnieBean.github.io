using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DynamicResourceManager : MonoBehaviour
{
    public static DynamicResourceManager _Instance;

    void Awake()
    {
        _Instance = this;
    }

    public void StartSetTexture(Image target, string url, bool preserveAspect = true)
    {
        StartCoroutine(SetTexture(target, url, preserveAspect));
    }

    public void StartSetTexture(Image[] targets, string url, bool preserveAspect = true)
    {
        foreach (var item in targets)
        {
            StartCoroutine(SetTexture(item, url, preserveAspect));
        }
    }

    private IEnumerator SetTexture(Image target, string url, bool preserveAspect)
    {
        var values = url.Split(new char[] { '/' });
        string name = values[values.Length - 1];

        Sprite sprite;

        if ((sprite = AssetsBundleManager._Instance.GetLocalSprite(name)) != null)
        {

        }
        else if ((sprite = LocalFileManager._Instance.GetSprite(name)) != null)
        {

        }
        else
        {
            bool isOk = false;
            bool isDone = false;

            while (!isOk)
            {
                HttpManager._Instance.StartGetTexture(url, (texture) =>
                {
                    if (texture != null)
                    {
                        LocalFileManager._Instance.SaveTexture2DToPNG(texture, name);

                        sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                        isOk = true;
                    }

                    isDone = true;

                });

                while (!isDone)
                {
                    yield return new WaitForSeconds(0.2f);
                }

                isDone = false;

                yield return new WaitForSeconds(2f);
            }
        }

        target.sprite = sprite;

        target.preserveAspect = preserveAspect;
    }

    public void SetTextureWithTag(Image target, string url, string tag, bool preserveAspect = true)
    {
        var values = url.Split(new char[] { '/' });
        string name = values[values.Length - 1];

        StartCoroutine(AssetsBundleManager._Instance.GetResource<Sprite>(tag, name, (res) =>
        {
            Sprite sprite;
            if (res != null)
            {
                sprite = res;
            }
            else if ((sprite = LocalFileManager._Instance.GetSprite(name)) != null)
            {

            }

            if (sprite != null)
            {

                target.sprite = sprite;

                target.preserveAspect = preserveAspect;
            }
            else
            {
                StartCoroutine(SetTextureByUrl(target, url, name, preserveAspect));
            }
        }));
    }

    private IEnumerator SetTextureByUrl(Image target, string url, string name, bool preserveAspect)
    {
        bool isOk = false;
        bool isDone = false;

        while (!isOk)
        {
            HttpManager._Instance.StartGetTexture(url, (texture) =>
            {
                if (texture != null)
                {
                    LocalFileManager._Instance.SaveTexture2DToPNG(texture, name);

                    target.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));


                    target.preserveAspect = preserveAspect;

                    isOk = true;
                }

                isDone = true;

            });

            while (!isDone)
            {
                yield return new WaitForSeconds(0.2f);
            }

            isDone = false;
            yield return new WaitForSeconds(2f);
        }
    }
}

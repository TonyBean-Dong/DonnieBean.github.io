using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using static NativeGallery;

public class LocalFileManager : MonoBehaviour
{
    public static LocalFileManager _Instance;

    public GameData _GameData;

    private string _PersistentPath;

    private string _ArchivePathName = @"save";

    private string _ArchiveFileName = @"savedata.json";

    private string _ImagesPathName = @"images";

    private List<string> imagesName;

    void Awake()
    {
        GameStatic.OnLogin += SaveArchive;
        GameStatic.OnLogout += SaveArchive;

        _PersistentPath = Application.persistentDataPath;
        _ArchivePathName = Path.Combine(_PersistentPath, _ArchivePathName);
        _ArchiveFileName = Path.Combine(_ArchivePathName, _ArchiveFileName);
        _ImagesPathName = Path.Combine(_PersistentPath, _ImagesPathName);

        ReadArchive();
        imagesName = GetImagesName();

        _Instance = this;
    }

    void OnApplicationPause()
    {
        SaveArchive();
    }

    void OnApplicationQuit()
    {
        SaveArchive();
    }

    void OnDestroy()
    {
        SaveArchive();
    }

    private IEnumerator TakeScreenshotAndSave()
    {
        yield return new WaitForEndOfFrame();

        Texture2D ss = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        ss.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        ss.Apply();

        NativeGallery.SaveImageToGallery(ss,GameStatic.Name,"截屏"+ GameStatic.DateFormat(DateTime.Now.ToString(),true) + ".png", (error) =>
        {
            if (error != null)
            {
                TipsManager._Instance.OpenWarningBox(error);
            }
        });;

        Destroy(ss);
    }


    public void SaveImageToGallery()
    {
        StartCoroutine(TakeScreenshotAndSave());
    }

    private void ReadArchive()
    {
        if (File.Exists(_ArchiveFileName) == false)
        {
            _GameData = new GameData();
            return;
        }

        try
        {
            string s = File.ReadAllText(_ArchiveFileName, Encoding.UTF8);
            _GameData = JsonMapper.ToObject<GameData>(s); ;
        }
        catch (Exception)
        {
            _GameData = new GameData();
            File.Delete(_ArchiveFileName);
        }

        AudioSourceManager._Instance.SetVolume((float)_GameData._BGMVolume, (float)_GameData._SEVolume, (float)_GameData._CSVolume);
    }

    public void SaveArchive()
    {
        if (Directory.Exists(_ArchivePathName) == false)
        {
            Directory.CreateDirectory(_ArchivePathName);
        }

        string jsonData = JsonMapper.ToJson(_GameData);

        File.WriteAllText(_ArchiveFileName, jsonData, Encoding.UTF8);
    }

#if UNITY_EDITOR
    public string GetLua()
    {
        string path = Path.Combine(Application.dataPath, "Editor", "HotUpdate.lua.txt");
        if (File.Exists(path) == false)
        {
            return string.Empty;
        }
        return File.ReadAllText(path, Encoding.UTF8);
    }
#endif

    public List<string> GetImagesName()
    {
        if (Directory.Exists(_ImagesPathName) == false)
        {
            Directory.CreateDirectory(_ImagesPathName);
        }

        List<string> names = new List<string>();
        foreach (var item in new DirectoryInfo(_ImagesPathName).GetFiles())
        {
            names.Add(item.Name);
        }

        //Test(names.ToArray());

        return names;
    }

    public void DeleteAllImages()
    {
        DirectoryInfo di = new DirectoryInfo(_ImagesPathName);

        foreach (var item in di.GetFiles())
        {
            File.Delete(item.FullName);
        }
    }

    public Sprite GetSprite(string name)
    {
        if (imagesName.Exists(s => s == name) == false)
        {
            return null;
        }

        string filePath = Path.Combine(_ImagesPathName, name);
        //创建文件读取流
        FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        fileStream.Seek(0, SeekOrigin.Begin);
        //创建文件长度缓冲区
        byte[] bytes = new byte[fileStream.Length];
        //读取文件
        fileStream.Read(bytes, 0, (int)fileStream.Length);
        //释放文件读取流
        fileStream.Close();
        fileStream.Dispose();
        //创建Texture
        Texture2D texture = new Texture2D(800, 640, TextureFormat.BGRA32, false);
        texture.LoadImage(bytes);
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    //将Texture2D保存成一张png图片
    public void SaveTexture2DToPNG(Texture2D texture, string pngName)
    {
        imagesName.Add(pngName);

        byte[] bytes = texture.EncodeToPNG();

        FileStream file = File.Open(Path.Combine(_ImagesPathName, pngName), FileMode.Create);
        BinaryWriter writer = new BinaryWriter(file);
        writer.Write(bytes);
        file.Close();
    }

    public class GameData
    {
        public double _BGMVolume;
        public double _SEVolume;
        public double _CSVolume;
        public bool _RememberToken;
        public bool _IsLogin;
        public string _Token;
        public string _Account;

        public GameData()
        {
            _BGMVolume = 1;
            _SEVolume = 1;
            _CSVolume = 1;
            _RememberToken = false;
            _IsLogin = false;
            _Token = "";
            _Account = "";
        }
    }

}

# Unity在编辑器中可视化配置数据  
当想要快速的配置一些数据，比如怪物Hp,Atk之类的数据时，在编辑器中进行可视化的配置有两种思路。  
* 第一种是使用Unity的ScriptableObject，直接在Inspector中修改数据，这样做的好处是方便，写个一个类之后直接创建配置就好了，坏处在于无法通过文本编辑器进行修改，且在有后台的项目中后台无法直接使用。  
* 另一种就是使用有一定规则的文本来进行数据保存，虽然可以自己编写一套规则来处理数据，但是使用已有的通用规则显然更加方便，比如使用Json。  

## 使用ScriptableObject的方法  
首先编写保存数据的类。 
```c#
[System.Serializable]
public class MonsterData
{
    public string Name;
    public float Hp;
    public float Atk;
    public float Def;

    public override string ToString()
    {
        return $"名字是{Name}，Hp为{(int)Hp},Atk为{(int)Atk},Def为{((int)Def)}";
    }
}
```
ToString用于之后的调用来进行Debug,[System.Serializable]特性来标识这个类代表其可序列化，这样子Unity才会对其进行序列化。

随便编写使用这个类的ScriptableObject对象类。 
```c#
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterDataObject", menuName = "ScriptableObjects/MonsterDataObject")]
public class MonsterDataObject : ScriptableObject
{
    public MonsterData monsterData;
}
```

然后通过在Project窗口内右键，在菜单中选中Create-ScriptableObjects-MonsterDataObject来生成一个对象。(子路径就是类的特性中的menuName，默认文件名就是类的特性中的fileName）
![生成对象.png](https://i.loli.net/2021/08/02/ixgG1webDkHSa4o.png)  

创建两个对象，分别命名为Monster1和Monster2。
点击对象就可以直接在Inspector窗口中修改数据，如下图。  
![直接修改ScriptableObject.png](https://i.loli.net/2021/08/02/akWmgSZ2IvbseGu.png)

修改完数据后就是使用方法，编写一个使用这些数据的类。  
```c#   
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptableObjectTemplate : MonoBehaviour
{
    [SerializeField] private MonsterDataObject moster1;
    [SerializeField] private MonsterDataObject moster2;

    private void Awake()
    {
        Debug.Log(moster1.ToString());
        Debug.Log(moster2.ToString());
    }
}
```
把这个脚本挂载到场景里，并使用拖拽往组件中放入创建的两个对象之后运行场景就可以从Console窗口中看到Debug输出了。  
## 使用Json的方法  
在Unity中使用的Json库一般有三种，Unity自带的JsonUtility，Litjson以及Newtonsoft.Json,这三者的兼容性和库大小依次递增，在实际使用中，一个怎样都不会出问题的库往往更加易用，所以本文使用Newtonsoft.Json，本文只会使用序列化和反序列化的方法，每个库会有这两个方法，只是兼容性不同。（比如是否支持Dictionary）  
数据类复用之前的MonsterData类，这里就不用再编写了。  
首先在Assets目录下创建StreamingAssets文件夹，这个文件夹是Unity的特殊文件夹，特点是这个文件夹内的内容在项目发布后会原封不动的复制出去（Assets内的文件在项目发布后一般而言会被Unity压缩并打包，无法直接访问），如果想要在发布后仍然可以通过文本编辑器修改内容，就需要把数据放到这个文件夹里面。  
一条数据就用一个文件保存固然可以，但是可以直接使用数据的链表来简化操作。  
为了使编辑可视化，编写如下工具类并放入Editor文件夹中。Editor文件夹也是Unity的特殊类，这个文件夹里的内容只会在编辑器下生效，在发布时会完全忽略这些文件夹。  
```c# 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using Newtonsoft.Json;

public class DataEditor : EditorWindow
{
    private static List<MonsterData> monsterDatas;

    private static MonsterData crtMonsterData;

    [MenuItem("Tools/数据配置", false)]
    private static void ShowWindow()
    {
        monsterDatas = null;
        string path = Path.Combine(Application.streamingAssetsPath, "MonsterDatas.json");
        if (File.Exists(path))
        {
            string txt = File.ReadAllText(path);
            try
            {
                monsterDatas = JsonConvert.DeserializeObject<List<MonsterData>>(txt);
            }
            catch (System.Exception)
            {
                Debug.LogWarning("反序列化失败");
            }
        }
        if (monsterDatas == null)
        {
            monsterDatas = new List<MonsterData>();
        }

        GetWindow(typeof(DataEditor));
    }


    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        DrawLeft();
        DrawRight();
        EditorGUILayout.EndHorizontal();

        DrawBottom();
    }

    private void DrawLeft()
    {
        EditorGUILayout.BeginVertical();

        EditorGUILayout.LabelField("当前对象", GUILayout.Width(250));
        if (crtMonsterData==null)
        {
            EditorGUILayout.LabelField("当前未选择对象",GUILayout.Width(100));
        }
        else
        {
            GUILayout.Label("Name", GUILayout.Width(50));
            crtMonsterData.Name = EditorGUILayout.TextField(crtMonsterData.Name, GUILayout.Width(200));
            GUILayout.Label("Hp", GUILayout.Width(50));
            crtMonsterData.Hp = float.Parse( EditorGUILayout.TextField(crtMonsterData.Hp.ToString(), GUILayout.Width(200)));
            GUILayout.Label("Atk", GUILayout.Width(50));
            crtMonsterData.Atk = float.Parse(EditorGUILayout.TextField(crtMonsterData.Atk.ToString(), GUILayout.Width(200)));
            GUILayout.Label("Def", GUILayout.Width(50));
            crtMonsterData.Def = float.Parse(EditorGUILayout.TextField(crtMonsterData.Def.ToString(), GUILayout.Width(200)));
        }
        if (GUILayout.Button("删除数据", GUILayout.Width(100)))
        {
            monsterDatas.Remove(crtMonsterData);
            crtMonsterData = null;
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawRight()
    {
        EditorGUILayout.BeginVertical();

        EditorGUILayout.LabelField("对象列表", GUILayout.Width(350));
        foreach (var item in monsterDatas)
        {
            if (GUILayout.Button(item.Name, GUILayout.Width(300)))
            {
                crtMonsterData = item;
            }
        }
        if (GUILayout.Button("新增数据", GUILayout.Width(100)))
        {
            monsterDatas.Add(new MonsterData() { Name="新敌人"});
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawBottom()
    {
        if (GUILayout.Button("保存数据"))
        {
            string path = Path.Combine(Application.streamingAssetsPath, "MonsterDatas.json");
            FileStream fs=new FileStream(path,FileMode.OpenOrCreate);
            byte[] b= Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(monsterDatas));
            fs.Write(b,0,b.Length);
            fs.Close();
        }
    }
}
```
随后便可以直接打开编辑窗口进行编辑了。  
![数据配置.png](https://i.loli.net/2021/08/02/CgRHEbxFfQA6IWw.png)  
![编辑窗口.png](https://i.loli.net/2021/08/02/YbLWPIyM729JB4l.png)  
随后便是使用，和之前一样编写一个使用的脚本。  
```c#
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
public class JsonTemplate : MonoBehaviour
{
    [SerializeField]private string filePath= "MonsterDatas.json";

    private List<MonsterData> monsterDatas;
    
    private void Awake()
    {
        string path = Path.Combine(Application.streamingAssetsPath, filePath);
        monsterDatas = JsonConvert.DeserializeObject<List<MonsterData>>(File.ReadAllText(path));

        foreach (var item in monsterDatas)
        {
            Debug.Log(item.ToString());
        }
    }
}

```

之后在场景中挂载并运行，就可以在Console窗口中查看Debug输出了。  
本文项目地址:https://github.com/DonnieBean/VisualConfiguration
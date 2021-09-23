# Unity有多个Console窗口时的Debug跳转   

在Unity中使用自定义的Debug辅助类对Debug进行封装时，会遇到一个问题，就是双击Console窗口内的Debug信息时，不会像使用默认Debug一样跳转Debug的初始位置，而是跳转至辅助类Debug的地方，虽然也可以通过点击窗口中的脚本链接跳转到正确的脚本中，但是没有双击信息跳转来的方便。  

解决方案有两种，第一种是使用dll封装辅助类，双击跳转是不会跳转到dll中的内容的，这样就可以进行正确的跳转，但是这种方法由于需要额外创建类库项目，而且无法直接的看到源码，在需要进行修改时非常不方便，需要再次打包dll，所以我并没有使用这种方法。  
另一种就是编写Editor工具类，通过读取Console窗口中的文本，来进行正确的跳转，我使用的就是这种方法。  

## 原始代码  
于是我便去网络上找了一个看起来最好用的代码，复制粘贴后稍微优化就是我的了，代码长这样。  
```c#
public class DebugJump
{
    public static string className = nameof(Debugger)+".cs";

    [UnityEditor.Callbacks.OnOpenAsset(0)]
    private static bool OnOpenAsset(int instanceID, int line)
    {
        string stackTrace = GetStackTrace();
        if (!string.IsNullOrEmpty(stackTrace) && stackTrace.Contains(className))
        {
            Match matches = Regex.Match(stackTrace, @"\(at (.+)\)", RegexOptions.IgnoreCase);
            string pathline = "";
            while (matches.Success)
            {
                pathline = matches.Groups[1].Value;
                if (!pathline.Contains(className))
                {
                    int splitIndex = pathline.LastIndexOf(":");
                    string path = pathline.Substring(0, splitIndex);
                    line = System.Convert.ToInt32(pathline.Substring(splitIndex + 1));
                    string fullPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("Assets"));
                    fullPath = fullPath + path;
                    UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(fullPath.Replace('/', '\\'), line);
                    break;
                }
                matches = matches.NextMatch();
            }
            return true;
        }
        return false;
    }
    private static string GetStackTrace()
    {
        var ConsoleWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.ConsoleWindow");
        var fieldInfo = ConsoleWindowType.GetField("ms_ConsoleWindow", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        var consoleWindowInstatnce = fieldInfo.GetValue(null);
        if (consoleWindowInstatnce != null)
        {
            if ((object)EditorWindow.focusedWindow == consoleWindowInstatnce)
            {
                fieldInfo = ConsoleWindowType.GetField("m_ActiveText", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                string activeText = fieldInfo.GetValue(consoleWindowInstatnce).ToString();
                return activeText;
            }
        }
        return null;
    }
}
```
其中Debugger是辅助类的类名。  
主要方法使用了 ***[UnityEditor.Callbacks.OnOpenAsset(0)]*** 属性，Unity会检测项目中所有使用了这个属性的方法，用括号中的数字进行排序,方法的返回值必须为bool，形参表为两个int，在用户双击了Console窗口中的信息后（暂不清楚有没有其他情况会调用这些方法）便会依次调用这些方法，使用默认调用的资源id和行数传参（使用辅助类时就会是辅助类的id和debug的行数），直到有一个方法返回true，便会中断之后的调用，假如所有的方法都返回false，便会使用默认的跳转。  

Unity并没有公开获取Console窗口的数据，所有是使用反射获取到的私有静态对象来获取Console窗口的文本，就是下图红框中的内容。  
![Console窗口的文本.png](https://i.loli.net/2021/07/21/hQIWgFimcNbwX5z.png)  

获取到了之后使用正则匹配出所有脚本路径（括号包起来的at+路径字符串），遍历到第一个不包含辅助类字符串的路径，使用 ***UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal*** 进行跳转。  

## 兼容多窗口  
一开始使用是没有啥问题的，都能够正常的进行跳转，直到后来我在使用新的Unity版本后改变了编辑器的布局，使用了两个Console窗口，就出现了仍然跳转到辅助类的情况。  

先Debug ***GetStackTrace*** 返回的数值，发现返回的是空字符串。

然后对 ***GetStackTrace*** 中所有通过反射获取到的字段进行了一番Debug，结果却是全都有值，随后便发现
```c#
   if ((object)EditorWindow.focusedWindow == consoleWindowInstatnce)
```
这个判断返回的是false。  
由此猜测，应当是反射获取到的Console窗口是不是当前聚焦的Console窗口。  
由于不知道ConsoleWindow的源码，也就不知道获取什么字段才能获取到其他Console窗口，于是便只能从能获取到的窗口入手，修改后的 ***GetStackTrace*** 方法代码如下。  
```c#
private static string GetStackTrace()
{
    var ConsoleWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.ConsoleWindow"); 
    if (ConsoleWindowType!=null&&EditorWindow.focusedWindow.titleContent.text=="Console")
    {
        var activeTextField = ConsoleWindowType.GetField("m_ActiveText", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);   
        string activeText = activeTextField.GetValue(EditorWindow.focusedWindow).ToString();    
        return activeText;
    }   
    return null;
}
```
通过 ***EditorWindow.focusedWindow.titleContent.text*** 直接判断当前聚焦窗口的标题文本是否是Console，是的话就直接用反射取出文本  
此时就可以在有多个Console窗口时正确的跳转了。  

## 参考资料  
最开始在网上找到的方法的原文章没有记下来，现在搜索也找不到了，一开始的方法和那篇文章里的差不多。  
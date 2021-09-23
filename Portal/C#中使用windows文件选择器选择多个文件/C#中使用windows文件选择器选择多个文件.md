# C#中使用windows文件选择器选择多个文件

1. 调用windows的文件选择窗口需要使用Comdlg32.h的方法,详见[GetOpenFileNameA]，这个方法需要传递一个包含文件选择所需所有信息的类，详见[OPENFILENAMEA]，以下是这个类的定义：  
``` C# 
using System;
using System.Runtime.InteropServices;


[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public class OpenFileName
{
    public int structSize = 0;
    public IntPtr dlgOwner = IntPtr.Zero;
    public IntPtr instance = IntPtr.Zero;
    public String filter = null;
    public String customFilter = null;
    public int maxCustFilter = 0;
    public int filterIndex = 0;
    public IntPtr file;
    public int maxFile = 0;
    public String fileTitle = null;
    public int maxFileTitle = 0;
    public String initialDir = null;
    public String title = null;
    public int flags = 0;
    public short fileOffset = 0;
    public short fileExtension = 0;
    public String defExt = null;
    public IntPtr custData = IntPtr.Zero;
    public IntPtr hook = IntPtr.Zero;
    public String templateName = null;
    public IntPtr reservedPtr = IntPtr.Zero;
    public int reservedInt = 0;
    public int flagsEx = 0;
}
```
需要注意的是，file字段并不是用String类型存储，这个字段用于存储用户选择的文件名，当只选择一个文件时，这个字段返回选择文件的全路径，当选择多个文件时，会返回文件夹路径以及选择的多个文件名组成的字符串，如果是使用的资源管理器风格，会以 __NULL__ 分割，如果使用的是旧式对话框风格时，会以 __空格__ 分割。  
一般而言，会使用资源管理器风格。（旧式风格我尝试过，界面和操作较为反人类）  
使用IntPtr存储file字段的原因就在于此，因为String类型以 __NULL__ 结尾，选中多个文件时，String类型的file字段只能获取到文件夹路径。

2. 使用 __DllImport__ 属性调用系统函数  

``` C#
using System.Runtime.InteropServices;

public class LocalDialog
{
    //链接指定系统函数       打开文件对话框
    [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);
}
```

3. 使用方法  

``` C#
    public List<string> FileSelector()
    {
        List<string> fileFullNames = new List<string>();

        OpenFileName openFileName = new OpenFileName();
        openFileName.structSize = Marshal.SizeOf(openFileName);
        openFileName.filter = "模型文件(*.fbx,*.obj)\0*.fbx;*.obj\0";
        openFileName.fileTitle = new string(new char[64]);
        openFileName.maxFileTitle = openFileName.fileTitle.Length;
        openFileName.initialDir = Application.streamingAssetsPath.Replace('/', '\\');//默认路径
        openFileName.title = "选择fbx文件";
        openFileName.flags = 0x00000004 | 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008 | 0x00000200;

        // Create buffer for file names
        string fileNames = new String(new char[2048]);
        openFileName.file = Marshal.StringToBSTR(fileNames);
        openFileName.maxFile = fileNames.Length;


        if (LocalDialog.GetOpenFileName(openFileName))
        {
            List<string> selectedFilesList = new List<string>();

            long pointer = (long)openFileName.file;
            string file = Marshal.PtrToStringAuto(openFileName.file);

            while (file.Length > 0)
            {
                selectedFilesList.Add(file);

                pointer += file.Length * 2 + 2;
                openFileName.file = (IntPtr)pointer;
                file = Marshal.PtrToStringAuto(openFileName.file);
            }

            if (selectedFilesList.Count == 1)
            {
                fileFullNames = selectedFilesList;
            }
            else
            {
                string[] selectedFiles = new string[selectedFilesList.Count - 1];

                for (int i = 0; i < selectedFiles.Length; i++)
                {
                    selectedFiles[i] = selectedFilesList[0] + "\\" + selectedFilesList[i + 1];
                }
                fileFullNames = new List<string>(selectedFiles);
            }
        }

        if (fileFullNames.Count > 0)
        {
           return fileFullNames;
        }
        else
        {
            return null;
        }
    }
```
在flag的设置中关键的几个为：  
* 0x00000200：允许多选
* 0x00080000：使用资源管理器风格  
其余选项详见[OPENFILENAMEA]中关于flag字段的枚举介绍  

参考资料：  
[Unity中调用Windows窗口选择文件](https://blog.csdn.net/qq_31841403/article/details/90368213)

[GetOpenFileNameA function (commdlg.h)](https://docs.microsoft.com/en-us/windows/win32/api/commdlg/nf-commdlg-getopenfilenamea)

[OPENFILENAMEA structure (commdlg.h)](https://docs.microsoft.com/en-us/windows/win32/api/commdlg/ns-commdlg-openfilenamea)

[GetOpenFileName for multiple files](https://social.msdn.microsoft.com/Forums/en-US/2f4dd95e-5c7b-4f48-adfc-44956b350f38/getopenfilename-for-multiple-files?forum=csharpgeneral)

[GetOpenFileNameA]:https://docs.microsoft.com/en-us/windows/win32/api/commdlg/nf-commdlg-getopenfilenamea

[OPENFILENAMEA]:https://docs.microsoft.com/en-us/windows/win32/api/commdlg/ns-commdlg-openfilenamea
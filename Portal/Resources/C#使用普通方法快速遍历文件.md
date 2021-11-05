# C#使用普通方法快速遍历文件

起因是想制作一个给文件贴标签用来管理的系统，首先遇到的问题就是快速的获取到所有想管理到的文件，首先想到的就是EveryThing的实现方式，使用直接访问USNJournal日志，但是在网上搜到的C#实现方式一运行不是访问被拒绝([How do we access MFT through C#]),就是各种超出索引([使用MFT Scanner遍巡USN Journal])。猜测是因为时代久远底层出现变化导致原本的方法需要进行一些改动才能使用，由于这方面知识实在不足，就放弃了这种实现方法。  

## 基础方法
随后便写了如下方法进行获取
``` C#
private List<FileInfo> GetFilesInfo(string dirPath)
{
    List<FileInfo> info = new List<FileInfo>();
    if (!Directory.Exists(dirPath))
    {
        message = dirPath + " is not a correct folder path!";
        return null;
    }
    Queue<string> dirq = new Queue<string>();
    dirq.Enqueue(dirPath);
    while (dirq.Count > 0)
    {
        string path = dirq.Dequeue();
        try
        {
            string[] filesPath = Directory.GetFiles(path);
            foreach (var item in filesPath)
            {
                FileInfo fi = new FileInfo(item);
                if ((fi.Attributes & FileAttributes.System) != FileAttributes.System)
                {
                    message = fi.FullName;
                    info.Add(fi);
                }
            }
            string[] directorysPath = Directory.GetDirectories(path);
            foreach (var item in directorysPath)
            {
                DirectoryInfo di = new DirectoryInfo(item);
                if ((di.Attributes & FileAttributes.System) != FileAttributes.System)
                {
                    dirq.Enqueue(item);
                }
            }
        }
        catch (Exception e)
        {
            message=e.Message + "\n" + e.Data;
            return null;
        }
    }
    return info;
}
```
其中
* message为string类型变量，用于展示在UI界面（表示正在运行）
* if ((di.Attributes & FileAttributes.System) != FileAttributes.System)这个判断是用来回避系统文件  

结果  
688461个文件，用时：152706ms

## 优化
搜索两分钟用时还是太长了，然后便在网上搜索快速查询的方法，这是优化后的代码
``` C#
private void GetFilesInfo(DirectoryInfo path)
{
    try
    {
        var files = path.EnumerateFiles();
        foreach (var item in files)
        {
            if ((item.Attributes & FileAttributes.System) != FileAttributes.System)
            {
                message = item.FullName;
                fis.Add(item);
            }
        }
        var dirs = path.EnumerateDirectories();

        Parallel.ForEach(dirs, dir =>
        {
            if ((dir.Attributes & FileAttributes.System) != FileAttributes.System)
            {
                Interlocked.Increment(ref count);
                GetFilesInfo(dir);
            }
        });

        Interlocked.Decrement(ref count);
    }
    catch (Exception e)
    {
        message=e.Message + "\n" + e.Data;
        error=true;
    }
}
```
其中  
* 使用EnumerateFiles和EnumerateDirectories来获取文件和文件夹，不在获取时得到全部信息（之前的的方法应该还有new的消耗，因为返回的是数组）
* 使用 Parallel.ForEach 来多线程使用递归方法
* fis类型为 ConcurrentBag<FileInfo>，就是一个多线程安全的链表
* count类型为int,用于记录还有多少文件夹没有搜索完成，搜索前为1（根节点），用于判断是否搜索完成,Interlocked.Increment为线程安全的++，Interlocked.Decrement为线程安全的--
* error为bool类型，用于判断是否出现错误  

结果  
用时：34175ms，缩减了四分之三的时间

## 结论  
考虑到Everything扫描同样的磁盘也需要十秒左右，优化后的时间相对可以接受，能够正常使用了
## 补充  
第二天测试时发现首次运行搜索同样的磁盘竟然需要五分钟，但第二次运行只需要30秒，打包发布出来后又只需要16秒（原本是在unity编辑器内运行的），应该是内存命中率导致的，时间参考意义不大
## 补充2
重启电脑后直接运行，在零内存命中的情况下需要十一分钟，这就不能接受了，于是还是去之前的问答寻找MFT的解决方案，然后找到了这个dll:[NtfsReader .NET Library]，下载后里面有使用的例子，同样的磁盘扫描只需要8秒，扫描完成后可以通过某个文件夹路径获取到路径下的所有节点的集合，节点用接口INode表示，INode代表一个文件或者文件夹，有着需要用到的一些参数。

[Fastest way searching specific files]"https://codereview.stackexchange.com/questions/74156/fastest-way-searching-specific-files"

[How do we access MFT through C#]:https://stackoverflow.com/questions/21661798/how-do-we-access-mft-through-c-sharp
[使用MFT Scanner遍巡USN Journal]:https://dotblogs.com.tw/larrynung/2012/10/26/79041
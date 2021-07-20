# C#控制台中获取字符串宽度

如果你在写控制台应用，有的时候需要输出中英文，同时还要保证文字能够对齐。 这个时候，就需要计算一个字符串需要占用几个长度的字符。  
通常情况下，英文数字占用1个单位宽度，而汉字需要2个单位宽度。 接下来的任务就是怎么判断某个字符是中文还是英文了。  
不考虑制表符的情况  
考虑不同的字符集，对于存储所需要的Byte数量的不同，不难发现 GBK 字符集中，中文需要两个字节，英文需要一个字节。 因此可以使用下面的代码来求出字符所占的显示长度。  
System.Text.Encoding.GetEncoding("GBK").GetByteCount(str);  
考虑制表符的情况  
如果我们的字符串中包含 '\t' ，这时候就要单独处理了，制表符产生的显示长度取决于之前的字符串显示到哪里了。 下面的代码应该可以解决这个问题。  

``` C#
using System.Text;
public static class StringExtension
{
    public static int DisplayLength(this string str)
    {
        int lengthCount = 0;
        var splits = str.ToCharArray();
        for (int i = 0; i < splits.Length; i++)
        {
            if (splits[i] == '\t')
            {
                lengthCount += 8 - lengthCount % 8;
            }
            else
            {
                lengthCount += Encoding.GetEncoding("GBK").GetByteCount(splits[i].ToString());
            }
        }
        return lengthCount;
    }
```

# C#控制台密码隐码输入

``` C#
    /// <summary>
    /// 输入密码
    /// </summary>
    /// <returns>返回密码字符串</returns>
    static string Input_Password()
    {
        ConsoleKeyInfo cki;
        string password = "";
        while (true)
        {
            cki = Console.ReadKey(true);
            if (cki.Key == ConsoleKey.Enter)
            {
                break;
            }
            if (cki.Key == ConsoleKey.Backspace)
            {
                if (password.Length > 0)
                {
                    password = password.Substring(0, password.Length - 1);
                    Console.Write("\b \b");
                }
            }
            else
            {
                password += cki.KeyChar.ToString();
                Console.Write("*");
            }
        }
        Console.WriteLine();
        return password;
    }
```

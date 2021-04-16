# C#性能测试  

测试方法:  

``` C#
    public static double GetRunTime(Action action)
    {
        long[] times = new long[100];
        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

        for (int count = 0; count < 100; count++)
        {
            watch.Restart();

            action?.Invoke();

            watch.Stop();
            times[count] = watch.Elapsed.Ticks;
        }

        return times.Average();
    }
```

测试时保证一次只运行一个方法，防止方法间互相影响

## for循环测试  

测试环境：  
vs2017,.NET Framework 4.7.1

1. 测试在循环内初始换int和在循环前初始化int的性能差异

测试用的方法：  

``` C#
    //方法A
    for (int indexA = 0; indexA < 100000000; indexA++)
    {
        int a = indexA;
    }

    //方法B
    int b;
    for (int indexB = 0; indexB < 100000000; indexB++)
    {
        b = indexB;
    }
```

测试结果：  
方法A平均时长：2000975.15 Tick  
方法B平均时长：2004687.01 Tick  
差异基本忽略不计。  

编译出来的IL代码：

``` IL
    //方法A的il
    .method assembly hidebysig instance void
            '<Main>b__0_0'() cil managed
    {
      // 代码大小       26 (0x1a)
      .maxstack  2
      .locals init ([0] int32 indexA,
               [1] int32 a,
               [2] bool V_2)
      IL_0000:  nop
      IL_0001:  ldc.i4.0
      IL_0002:  stloc.0
      IL_0003:  br.s       IL_000d

      IL_0005:  nop
      IL_0006:  ldloc.0
      IL_0007:  stloc.1
      IL_0008:  nop
      IL_0009:  ldloc.0
      IL_000a:  ldc.i4.1
      IL_000b:  add
      IL_000c:  stloc.0
      IL_000d:  ldloc.0
      IL_000e:  ldc.i4     0x5f5e100
      IL_0013:  clt
      IL_0015:  stloc.2
      IL_0016:  ldloc.2
      IL_0017:  brtrue.s   IL_0005

      IL_0019:  ret
    } // end of method '<>c'::'<Main>b__0_0'

    //方法B的il
    .method assembly hidebysig instance void
            '<Main>b__0_1'() cil managed
    {
      // 代码大小       26 (0x1a)
      .maxstack  2
      .locals init ([0] int32 b,
               [1] int32 indexB,
               [2] bool V_2)
      IL_0000:  nop
      IL_0001:  ldc.i4.0
      IL_0002:  stloc.1
      IL_0003:  br.s       IL_000d

      IL_0005:  nop
      IL_0006:  ldloc.1
      IL_0007:  stloc.0
      IL_0008:  nop
      IL_0009:  ldloc.1
      IL_000a:  ldc.i4.1
      IL_000b:  add
      IL_000c:  stloc.1
      IL_000d:  ldloc.1
      IL_000e:  ldc.i4     0x5f5e100
      IL_0013:  clt
      IL_0015:  stloc.2
      IL_0016:  ldloc.2
      IL_0017:  brtrue.s   IL_0005

      IL_0019:  ret
    } // end of method '<>c'::'<Main>b__0_1'
```

两个方法的IL代码基本相同  

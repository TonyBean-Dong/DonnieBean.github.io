# Unity中使用shader制作消融特效  

软件环境：Unity2018.1.1f1  

游戏中的敌人死亡时，直接删除或者隐藏实在过于突兀，一般都会以一些特效进行铺垫，常见的简单特效有烟雾和消融，本篇文章写的就是如何制作在Unity中使用的消融特效。  

为了制作合理的消融效果，需要一张用于判断消融顺序的贴图，只用到rgba其中的某一个值，这里假设使用的是一张单独贴图的r值。为了达到平滑的效果，贴图的生成需要用到柏林噪声。

[Unity柏林噪音官方文档] : "https://docs.unity3d.com/ScriptReference/Mathf.PerlinNoise.html"  
[Unity中使用柏林噪声生成地图] : "https://blog.csdn.net/u010019717/article/details/72673225"  

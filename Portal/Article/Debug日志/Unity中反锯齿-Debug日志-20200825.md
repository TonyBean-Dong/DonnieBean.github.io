# Unity中反锯齿

环境：Unity2018.1.1f1;Visual Studio 2017
问题：在Edit-ProjectSetting-Quality中设置反锯齿后，仍然会有明显的锯齿
解决：由于主摄像机的RenderingPath设为了延迟（Deferred）,MSAA无法正常工作,导致主摄像机渲染的对象没有进行反锯齿处理。将主摄像机的RenderingPath设置为提前（forward）后解决。  

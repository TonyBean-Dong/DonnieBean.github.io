# 设置mesh时随机报错  

* 环境：Unity2018.1.1f1,vs2017  

* 问题：代码进行了mesh的顶点和三角形操作，在执行到如下代码时  

    ``` C#
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uv);
        mesh.SetNormals(normals);
        mesh.SetTriangles(triangles, 0);
    ```

    会随机报错，Mesh.vertices is too small. The supplied vertex array has less vertices than are referenced by the triangles array.  
    理论上  

    ``` C#
        mesh.SetTriangles(triangles, 0);
    ```

    会设置正确的三角形链表,不应当会报这种错误

* 尝试：Google报错信息，找到解决方案[array out of range while updating mesh variables](https://answers.unity.com/questions/334706/array-out-of-range-while-updating-mesh-variables.html)  

* 解决：在这段代码前添加

``` C#
    mesh.Clear();
```  

* 分析：猜测有可能是因为Unity里面mesh的渲染是另一个线程执行的，所以会出现随机报错的情况，当设置完顶点且没有设置完三角形时，cpu切换到渲染线程的话，就会出现报错。使用Clear语句后三角形链表清空，这时即使遇到之前那种中途切换线程的情况也能保证没有错误。  

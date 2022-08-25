# Unity实现五子棋  

之前一直想弄懂Alpha-Beta剪枝算法，看了许多网站，仍然是似懂非懂，最终决定还是实现一款五子棋游戏来帮助自己弄清楚，同时还能整理相关的知识。  
本文记录了使用unity实现五子棋的过程，写的比较详细，应该能对unity的新手有所帮助。  
文末有github的项目地址。  

## 创建unity项目

本文使用的unity版本为 *Unity 2020.3.9f1c1 Personal*， 为unity2020TLS版本，使用UnityHub创建新的3D项目***Gobang***。

## 构建描述棋盘的类  

首先使用一个类描述棋盘， 新的项目内只有一个 ***Scenes*** 文件夹， 先在根目录创建一个 ***Scripts*** 文件夹， 在其中创建 ***GobangBoard.cs*** 文件。  
```c#
using System. Collections. Generic; 
using System; 

public class GobangBoard
{

}

```
使用CellType枚举用于描述格子的状态，在类的外面创建，因为其他的地方也会需要用到这个枚举。 
```c#
public enum CellType
{
    Empty，
    White，
    Black，
    Void，
}
```

其中Empty代表没有棋子的空格子，White和Black分别代表白棋和黑棋，Void代表棋盘外的非法格子。  
创建基本字段和构造函数
```c#
public int Width { get; private set; }
public int Height { get; private set; }
private CellType[] board; 

public GobangBoard() : this(15， 15) { }
public GobangBoard(int width， int height)
{

    this.Width = width;
    this.Height = height;
    board = new CellType[width * height];

}

```
标准的五子棋棋盘是十五道的，但为了让游戏变得更加有趣，假如自定义棋盘的功能，无参的构造函数就使用标准的十五道。棋盘的宽度和高度可以在外部直接访问，但是棋格数组不行，因为数组用起来还是不太方便，需要对其封装一下。
```c#
public CellType this[int x， int y]
{
    get
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return CellType.Void;
        }
        return board[x * Width + y];
    }
    set
    {
        board[x * Width + y] = value;
    }
}
```

现在可以就使用x和y坐标获取对应格子的状态了。  

由于棋格数组被私有化了，之前的公共属性只能获取数组内的内容，为了能够从类外面获取数组的长度，需要创建公共的属性来获取。
```c#
public int CellCount
{

    get
    {
        return board.Length;
    }

}

```

棋盘应当可以判断是否有一方获胜了，所以创建判断获胜的方法  
```c#
public bool CheckWin(bool isWhite)
{
    CellType checkType = isWhite ? CellType.White : CellType.Black;
    for (int x = 0; x < Width; x++)
    {
        for (int y = 0; y < Height; y++)
        {
            if (this[x, y] == checkType)
            {
                if (this[x + 1, y] == checkType && this[x + 2, y] ==checkType && this[x + 3, y] == checkType && this[x + 4,y] == checkType)//右
                {
                    return true;
                }
                if (this[x, y + 1] == checkType && this[x, y + 2] ==checkType && this[x, y + 3] == checkType && this[x, y +4] == checkType)//上
                {
                    return true;
                }
                if (this[x + 1, y + 1] == checkType && this[x + 2, y +2] == checkType && this[x + 3, y + 3] == checkType &&this[x + 4, y + 4] == checkType)//右上
                {
                    return true;
                }
                if (this[x + 1, y - 1] == checkType && this[x + 2, y -2] == checkType && this[x + 3, y - 3] == checkType &&this[x + 4, y - 4] == checkType)//右下
                {
                    return true;
                }
            }
        }
    }
    return false;
}
```

## 创建棋盘和棋子

想要展现整个棋盘至少需要单个棋格的贴图，使用GIMP创建一张方形图片，填充自带的木头纹理之一，再把周围一圈像素用黑色填充，结果图如下：  

![chessGrid.png](https://i.loli.net/2021/07/13/DsdkG2gc9RWB1nK.png)

  
创建***Textures***文件夹将这张图片放入。  
然后创建***Materials***文件夹并在其中创建一个材质球，命名为***chessBoard***，使用默认的shader就行了，将其***MainMaps***下的***Albedo***属性的贴图使用之前创建的图片，将其***MainMaps***下的***Tilling***属性的X和Y修改为14，因为15道有十四格格子所以是14。  

![棋格的材质球设置.png](https://i.loli.net/2021/07/13/E1MU4NIbPvnSJQ5.png)

  
将项目中默认的***SampleScene***场景重命名为***MainScene***，并在其中添加一个 3D Object/Plane ，命名为***Gobang Board***，对其使用之前创建的材质球，将其位移归零，大小设置为(2.8，1，2.8)，这个时候就有了一个能够使用的棋盘了，这个棋盘的每个格子大小为unity场景中的2米。  

![棋盘1.png](https://i.loli.net/2021/07/13/lIEd6UgmyJTfeac.png)

  
为了使棋盘更加美观，再使用GIMP创建黑点和底板的图片。  
选择正圆然后填充黑色，随后反选并使选择区域变透明：  

![blackPoint.png](https://i.loli.net/2021/07/13/tbd4gK69hrOxLsC.png)

  
创建一张较大的图片并填充另一种自带的木头纹理：

![bottomPlate.png](https://i.loli.net/2021/07/13/eE1FS3hNPvlsHf8.png)

  
将这两张图片放入***Textures***文件夹中并创建对应的相同名字的材质球对其进行引用，底板的材质球不需要更改材质球的任何其他设置，黑点的材质球需要修改其渲染模式为Cutout。  

![黑点材质球配置.png](https://i.loli.net/2021/07/13/DbsqCz28a3mKU1t.png)

  
在之前的棋盘对象***Gobang Board***下创建类型为3D Object/Plane的子物体***Bottom Plate***，位移设置为(0, -0.01, 0)，大小设置为(1.1, 0, 1.1)，使用底板的贴图。  

在场景根节点下创建空节点***Black Points Parent***，位移为(0, 0.01, 0)，大小为(1, 1, 1)，并在这个空节点下创建五个类型为3D Object/Plane的子物体，分别命名为Black Point 1到Black Point 5, 这五个节点大小都为（0.05，0，0.05），位移分别为(0, 0, 0)、(8, 0, 8)、(8, 0, -8)、(-8, 0, -8)、(-8, 0, 8)。  
最终的场景节点如下图：  

![创建完棋盘的场景节点.png](https://i.loli.net/2021/07/13/pbEkq7nDOs3Nvah.png)

  
效果如下：  

![完全的棋盘.png](https://i.loli.net/2021/07/13/iNgWnCshaAu3TOm.png)

  

本着方便的原则，直接使用unity自带的圆柱体制作棋子。  
创建两个空节点，分别命名为***White Chess***和***Black Chess***, 分别在其中创建一个类型为 3D/Cylinder 的子物体，子物体的位移都设置为(0, 0.3, 0)，大小都设置为
(2, 0.3, 2)，将其默认的碰撞器组件删除防止干扰射线检测，创建两个材质球***whiteChess***和***blackChess***, 材质球的***MainMaps***的***Albedo***属性分别设置为白色和黑色。将这两个材质球分别用于对应的圆柱体。  
创建 ***Prefabs*** 文件夹，将 ***White Chess*** 和 ***Black Chess*** 拖入其中，此时棋子的预制体就做好了。  

然后制作用于显示鼠标位置的棋子虚影，在***Hierarchy***窗口中分别右键场景中残留的两个棋子，选择Prefab/Unpack来解除其与预制体的关联。  
创建两个材质球，设置 ***RenderingMode*** 为 ***Transparent*** ，分别设置 ***MainMaps*** 的 ***Albedo*** 属性为白色和黑色，此时需要设置颜色的 ***Alpha*** 参数为128。  

![虚影的材质球设置.png](https://i.loli.net/2021/07/13/2ybIgAQc4ZvUXs6.png)

  
将这两个材质球分别用于场景中对应的棋子。  
将场景中的 ***White Chess*** 和 ***Black Chess*** 节点重命名为 ***White Phantom*** 和 ***Black Phantom*** ，此时棋子虚影就做好了。  

随后制作用于显示上一步走哪里的标识物，创建空节点 ***Marker*** ，在其中创建类型为 ***3D Object/Cube*** 的子物体，子物体的位移设置为(0, 2, 0)，大小设置为(0.5, 0.5, 0.5), 移除子物体的 ***Box Collider*** 组件，并将 ***Mesh Renderer*** 组件的 ***Lighting/CastShadows*** 属性设置为 ***off*** 。  

![MeshRenderer的设置.png](https://i.loli.net/2021/07/13/sjV9YqUKZ7ehDS4.png)

  
创建材质球 ***Marker***，将其 ***MainMaps*** 的 ***Albedo*** 属性设置为红色，并将这个材质球使用于刚刚创建的Cube。  

## 构建管理类

由于整个项目的规模很小，使用单个管理类进行管理足够了，创建文件 ***GobangManager.cs*** ，使用单例模式来使得这个类能够全局获取。  
```c#
using UnityEngine; 

public class GobangManager : MonoBehaviour
{ 

    public static GobangManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            return;
        }

        Instance = this;
    }
    

}

```
先创建五个GameObject字段用于追踪场景中的对应物体  
```c#
public GameObject blackChessPrefab;
public GameObject whiteChessPrefab;
public GameObject blackChessPhantom;
public GameObject whiteChessPhantom;
public GameObject marker;
```

创建棋盘属性来存储当前棋盘的状态；下棋次数字段来判断棋盘是否已经下满了；是否是玩家1的回合的字段用于判断当前是哪方的回合。  
```c#
public GobangBoard CrtBoard { get; private set; }
public int chessCount { get; private set; }
private bool player1Turn; 

```
创建开始游戏的方法并在Start方法中对其进行调用  
```c#
private void Start()
{
    StartGame(15,15);
}

public void StartGame(int width, int height)
{
    chessCount = 0;
    CrtBoard = new GobangBoard(width ,height);
    player1Turn=true;
    marker.SetActive(false);
}
```

创建下棋的方法，在下棋时同时使逻辑上的棋盘和场景中的棋盘改变  
```c#
public void SetPiece(int x, int y)
{

    GameObject newPiece;
    if (player1Turn)
    {
        newPiece = Instantiate(whiteChessPrefab);
    }
    else
    {
        newPiece = Instantiate(blackChessPrefab);
    }
    Vector3 targetPos = new Vector3(x * 2 - (CrtBoard.Width - 1), 0, y* 2 - (CrtBoard.Height - 1)); 
    newPiece.transform.position = targetPos;
    marker.transform.position = targetPos;
    marker.SetActive(true);
    chessCount++;

 

    CrtBoard[x, y] = player1Turn ? CellType.White : CellType.Black;
    if (CrtBoard.CheckWin(player1Turn))
    {
        Debug.Log(player1Turn ? "Player1 Win!" : "Player2 Win!");
        return;
    }
    if (chessCount == CrtBoard.CellCount)
    {
        Debug.Log("Board is full!");
        return;
    }
    player1Turn = !player1Turn;

}

```
为了实现玩家在棋盘上下棋，需要先实现三个方法，分别是获取最近的正确的棋位的方法、使用世界坐标判断对应棋位是否为空的方法，以及使用世界坐标来下棋的方法。  
使用鼠标进行操作的情况，获取到的鼠标射线位置一般不可能正好是一个棋位的位置，所以需要进行一些计算获取到最近的棋位的位置，方法如下：  
```c#
private float GetNearInt(float rawFloat, int magnification = 1, int offset = 0)
{
    return Mathf.Round((rawFloat + offset) / magnification) *magnification - offset;
}
```

magnification参数为放大率，本项目中应当为2，即棋格的大小，offset参数为任意一个正确的结果值，本项目中，当棋道为奇数时可以为0，当棋道数量为偶数时可以为1。  
随后是使用世界坐标判断对应棋位是否为空的方法，此时的坐标应当是已经正确的棋位的坐标  
```c#
public bool CheckEmpty(Vector3 pos)
{

    int x = (Mathf.RoundToInt(pos.x) + CrtBoard.Width - 1) / 2;
    int z = (Mathf.RoundToInt(pos.z) + CrtBoard.Height - 1) / 2;
    return CrtBoard[x, z] == CellType.Empty;

}

```
最后是使用世界坐标下棋的方法，只需要算出对应的棋位，然后调用之前写好的方法就可以了。  
```c#
public void SetPiece(Vector3 pos)
{
    int x = (Mathf.RoundToInt(pos.x) + CrtBoard.Width - 1) / 2;
    int z = (Mathf.RoundToInt(pos.z) + CrtBoard.Height - 1) / 2;
    SetPiece(x, z);
}
```

最后在Update中写入使用射线检测下棋的方法：  
```c#
private void Update()
{

    RaycastHit raycastHit;
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    Physics.Raycast(ray, out raycastHit, 100);
    if (raycastHit.transform != null)
    {
        Vector3 pos = new Vector3(GetNearInt(raycastHit.point.x, 2,(CrtBoard.Width & 1) == 0 ? 1 : 0), 0 , GetNearInt(raycastHit.pointz, 2, (CrtBoard.Height & 1) == 0 ? 1 : 0));
        if (Input.GetMouseButtonDown(0))
        {
            if (CheckEmpty(pos))
            {
                blackChessPhantom.SetActive(false);
                whiteChessPhantom.SetActive(false);
                SetPiece(pos);
            }
        }
        else
        {
            if (CheckEmpty(pos))
            {
                if (player1Turn)
                {
                    whiteChessPhantom.SetActive(true);
                    whiteChessPhantom.transform.position = pos;
                }
                else
                {
                    blackChessPhantom.SetActive(true);
                    blackChessPhantom.transform.position = pos;
                }
            }
            else
            {
                blackChessPhantom.SetActive(false);
                whiteChessPhantom.SetActive(false);
            }
        }
    }
    else
    {
        blackChessPhantom.SetActive(false);
        whiteChessPhantom.SetActive(false);
    }

}

```
此时回到unity的场景中，随便找个对象挂载 ***GobangManager*** 脚本，将五个参数进行赋值，两个Prefab参数选择Project窗口中对应的预制体，另外三个参数选择场景中对应的物体。将主摄像机摆到一个合适的位置，运行场景，此时就可以使用鼠标进行双方都为人类的游戏了。  

![无AI对战.png](https://i.loli.net/2021/07/13/LXIQNTPb27nfpMw.png)

  

## 摄像机的控制以及不同大小的棋盘

目前摄像机是无法操控的，但我并不打算做成传统棋类游戏的固定视角，那样太无聊了，所以使用unity官方例子的自由相机脚本( ***SimpleCameraController*** )对其进行控制，代码太长就不在此赘述。

将这个脚本挂载在主摄像机上，就可以通过adsw和鼠标右键控制摄像机了。  

另一个问题是不同大小的棋盘适应问题，这时当我们改变GobangManager脚本中的Start方法，创建一个不是15*15的棋盘时，虽然逻辑上和棋位都没有问题，但是棋盘并没有随之改变。  
为了对其进行控制，在GobangManager中增加三个公共字段，分别为棋盘对象，黑点父物体对象，棋盘材质：    
```c#
    public GameObject board;
    public GameObject blackPointsParent;
    public Material boardMaterial;
```

修改StartGame方法，使在开始游戏确定棋盘大小时能过一并修改场景中的棋盘大小，当棋盘大小不为15道时，直接隐藏黑点，因为我也不知道非正规期盼的黑点应当摆在哪里  
```c#
public void StartGame(int width, int height)
{

    board.transform.localScale = new Vector3((width - 1) * 0.2f, 1, (height - 1) * 0.2f);
    boardMaterial.SetTextureScale("_MainTex", new Vector2(width - 1, height - 1));
    blackPointsParent.SetActive(width == 15 && height == 15);

    //...

}

```
在场景中将新增的三个字段进行赋值，之后便能适应不同大小的棋盘了，下图是一个14*18的棋盘  

![14X18的棋盘.png](https://i.loli.net/2021/07/13/XFZlhYN7TA9GE8b.png)

  

## AI下棋  

目前已经实现了基本的下棋功能，接下来就是AI的实现了，为了判断应当下在哪里，需要对下棋后的棋盘情况进行判断，通过一个函数评估出棋盘的对自己的有利度，使用一个float来表示。AI应当选择一个能获得最高有利度的地方下棋。首先实现评估函数。  

### 评估函数  

在GobangBoard类中创建两个类型为Dictionary<int[],int>的字典，分别对连续五格和连续六格的情况进行评分：  
```c#
static readonly Dictionary<int[], int> ScoreTable5 = new Dictionary<int[], int>
{
    {new int[] { 0, 1, 1, 0, 0 },5 },
    {new int[] { 0, 0, 1, 1, 0 },5 },
    {new int[] { 1, 1, 0, 1, 0 },20 },
    {new int[] { 0, 1, 0, 1, 1 },20 },
    {new int[] { 0, 0, 1, 1, 1 },50 },
    {new int[] { 1, 1, 1, 0, 0 },50 },
    {new int[] { 0, 1, 1, 1, 0 },100 },
    {new int[] { 1, 1, 1, 0, 1 },300 },
    {new int[] { 1, 1, 0, 1, 1 },300 },
    {new int[] { 1, 0, 1, 1, 1 },300 },
    {new int[] { 1, 1, 1, 1, 0 },500 },
    {new int[] { 0, 1, 1, 1, 1 },500 },
    {new int[] { 1, 1, 1, 1, 1 },99999999 },
};
static readonly Dictionary<int[], int> ScoreTable6 = new Dictionary<int[], int>
{
    {new int[]{ 0, 1, 0, 1, 1, 0 },500 },
    {new int[]{ 0, 1, 1, 0, 1, 0 },500 },
    {new int[]{ 0, 1, 1, 1, 1, 0 },5000 },
};
```

其中1代表自己的棋子，0代表空格
例如当棋盘中只有一个自己空头四连时，判断出的有利度为6000分，因为同时符合{1, 1, 1, 1, 0}、{0, 1, 1, 1, 1}、{0, 1, 1, 1, 1, 0}三种情况。
只对连续格子中不包含对手棋子的情况进行判断，当连续的格子中包含对手格子时，必然无法凑成五连。   

随后编写获取分数的方法，传入数组评估其分值，此时需要using System. Linq  
```c#
using System. Linq; 

//...

private int GetScore5(int[] input)
{

    foreach (var item in ScoreTable5)
    {
        if (Enumerable.SequenceEqual(input, item.Key))
        {
            return item.Value;
        }
    }
    return 0;

}
private int GetScore6(int[] input)
{

    foreach (var item in ScoreTable6)
    {
        if (Enumerable.SequenceEqual(input, item.Key))
        {
            return item.Value;
        }
    }
    return 0;

}

```

随后是评估函数以及为了缩短代码的子方法  
```c#
public void Judgment5(CellType crtCellType, int index, ref int[] array1, ref int[] array2, ref bool flag1, ref bool flag2, ref CellType cellType)
{
    switch (crtCellType)
    {
        case CellType.White:
            if (cellType == CellType.Black)
            {
                flag1 = false;
                flag2 = false;
                cellType = CellType.Void;
            }
            else if (cellType != CellType.Void)
            {
                flag1 = true;
                flag2 = true;
                cellType = CellType.White;
                array1[index] = 1;
                array2[index] = 1;
            }
            break;
        case CellType.Black:
            if (cellType == CellType.White)
            {
                flag1 = false;
                flag2 = false;
                cellType = CellType.Void;
            }
            else if (cellType != CellType.Void)
            {
                flag1 = true;
                flag2 = true;
                cellType = CellType.Black;
                array1[index] = 1;
                array2[index] = 1;
            }
            break;
        case CellType.Empty:
            array1[index] = 0;
            array2[index] = 0;
            break;
        default:
            flag1 = false;
            flag2 = false;
            cellType = CellType.Void;
            break;
    }
}

public void Judgment6(CellType crtCellType, ref int[] array, ref bool flag, CellType cellType)
{
    switch (crtCellType)
    {
        case CellType.White:
            if (cellType == CellType.Black)
            {
                flag = false;
            }
            else if (cellType != CellType.Void)
            {
                flag = true;
                array[5] = 1;
            }
            break;
        case CellType.Black:
            if (cellType == CellType.White)
            {
                flag = false;
            }
            else if (cellType != CellType.Void)
            {
                flag = true;
                array[5] = 1;
            }
            break;
        case CellType.Empty:
            array[5] = 0;
            break;
        default:
            flag = false;
            break;
    }
}

public float Evaluation(bool isWhite)
{
    float totalPower = 0;
    for (int x = 0; x < Width; x++)
    {
        for (int y = 0; y < Height; y++)
        {
            int[] rightDir = new int[5];
            int[] rightDir2 = new int[6];
            int[] upDir = new int[5];
            int[] upDir2 = new int[6];
            int[] rightUpDir = new int[5];
            int[] rightUpDir2 = new int[6];
            int[] rightDownDir = new int[5];
            int[] rightDownDir2 = new int[6];
            bool rightFlag = false;
            bool rightFlag2 = false;
            bool upFlag = false;
            bool upFlag2 = false;
            bool rightUpFlag = false;
            bool rightUpFlag2 = false;
            bool rightDownFlag = false;
            bool rightDownFlag2 = false;
            CellType rightCellType = CellType.Empty;
            CellType upCellType = CellType.Empty;
            CellType rightUpCellType = CellType.Empty;
            CellType rightDownCellType = CellType.Empty;
            for (int i = 0; i < 5; i++)
            {
                CellType rct = this[x + i, y];
                CellType uct = this[x, y + i];
                CellType ruct = this[x + i, y + i];
                CellType rdct = this[x + i, y - i];
                Judgment5(rct, i, ref rightDir, ref rightDir2, ref rightFlag, ref rightFlag2, ref rightCellType);
                Judgment5(uct, i, ref upDir, ref upDir2, ref upFlag, ref upFlag2, ref upCellType);
                Judgment5(ruct, i, ref rightUpDir, ref rightUpDir2, ref rightUpFlag, ref rightUpFlag2, ref rightUpCellType);
                Judgment5(rdct, i, ref rightDownDir, ref rightDownDir2, ref rightDownFlag, ref rightDownFlag2, ref rightDownCellType);
            }
            CellType rct6 = this[x + 5, y];
            CellType dct6 = this[x, y + 5];
            CellType ruct6 = this[x + 5, y + 5];
            CellType rdct6 = this[x + 5, y - 5];
            Judgment6(rct6, ref rightDir2,ref rightFlag2,  rightCellType);
            Judgment6(dct6, ref upDir2, ref upFlag2,  upCellType);
            Judgment6(ruct6, ref rightUpDir2, ref rightUpFlag2, rightUpCellType);
            Judgment6(rdct6, ref rightDownDir2, ref rightDownFlag2, rightDownCellType);
           
            if (rightFlag)
            {
                totalPower += (rightCellType == CellType.White ? 1 : -1) * GetScore5(rightDir);
            }
            if (rightFlag2)
            {
                totalPower += (rightCellType == CellType.White ? 1 : -1) * GetScore6(rightDir2);
            }
            if (upFlag)
            {
                totalPower += (upCellType == CellType.White ? 1 : -1) * GetScore5(upDir);
            }
            if (upFlag2)
            {
                totalPower += (upCellType == CellType.White ? 1 : -1) * GetScore6(upDir2);
            }
            if (rightUpFlag)
            {
                totalPower += (rightUpCellType == CellType.White ? 1 : -1) * GetScore5(rightUpDir);
            }
            if (rightUpFlag2)
            {
                totalPower += (rightUpCellType == CellType.White ? 1 : -1) * GetScore6(rightUpDir2);
            }
            if (rightDownFlag)
            {
                totalPower += (rightDownCellType == CellType.White ? 1 : -1) * GetScore5(rightDownDir);
            }
            if (rightDownFlag2)
            {
                totalPower += (rightDownCellType == CellType.White ? 1 : -1) * GetScore6(rightDownDir2);
            }
        }
    }
    if (!isWhite)
    {
        totalPower = -totalPower;
    }
    return totalPower;
}
```

评估函数传入需要判断的是否是玩家1，前面的得分汇总的是对玩家1的有利分，当不是玩家1时，需要在返回前对其进行取反。   
评估的过程就是遍历每一个棋格，对右(x+)，上(y+)，右上(x+, y+)，右下(x+, y-)四个方向进行取值和评分，当取到棋盘外的棋格时，会返回Void。   
对应的cellType代表连续棋子的颜色，当为Void时代表取到了棋格外或者同时有两种颜色的棋子。  
当连续的五个或六个位置棋子数少于一个，或有两种不同的棋子，或位置取到棋盘外时，对应的flag会为false，否则就会为true，为true时就会进行评分。

### AI类  

新建一个AI类来编写思考的方法，创建 ***GobangAI.cs***文件：
```c#
using System; 

public class GobangAI
{

}

```

首先先编写一个简单的判断方法，遍历所有非空的棋格，下在走了之后价值最高的棋格，编写这个方法需要两个前置方法  
* 判断周围某个位置周围一周是否有其他棋子的方法，下在远离其他棋子的地方通常没有意义，不考虑这种情况能减少很多判断  
* GobangBoard的克隆方法，为了判断走了之后的棋盘的分值，需要临时创建一个独立的棋盘副本  

在 ***GobangBoard*** 中，添加两个方法  
```c#
public GobangBoard Clone()
{
    GobangBoard newGobangBoard = new GobangBoard();
    newGobangBoard.board = board.Clone() as CellType[];
    return newGobangBoard;
}

public bool HasNear(int x, int y)
{
    return (this[x + 1, y] == CellType.Black || this[x + 1, y] == CellType.White
         || this[x - 1, y] == CellType.Black || this[x - 1, y] == CellType.White
         || this[x, y + 1] == CellType.Black || this[x, y + 1] == CellType.White
         || this[x, y - 1] == CellType.Black || this[x, y - 1] == CellType.White
         || this[x + 1, y + 1] == CellType.Black || this[x + 1, y + 1] == CellType.White
         || this[x + 1, y - 1] == CellType.Black || this[x + 1, y - 1] == CellType.White
         || this[x - 1, y + 1] == CellType.Black || this[x - 1, y + 1] == CellType.White
         || this[x - 1, y - 1] == CellType.Black || this[x - 1, y - 1] == CellType.White);
}
```

然后再GobangAI中，添加思考的方法，当棋盘中没有棋子时，没有必要浪费时间计算，直接下在棋盘中间就可以了。
```c#
public void Thinking(bool isWhite)
{

    if (GobangManager.Instance.chessCount==0)
    {
        GobangManager.Instance.SetPiece(manager.CrtBoard.Width / 2, manager.CrtBoard.Height / 2);
        return;
    }
    CellType selfType = isWhite ? CellType.White : CellType.Black;
    GobangBoard crtBoard = GobangManager.Instance.CrtBoard;
    float best = int.MinValue;
    int bestX = 0;
    int bestY = 0;
    for (int x = 0; x < crtBoard.Width; x++)
    {
        for (int y = 0; y < crtBoard.Height; y++)
        {
            if (crtBoard[x,y]==CellType.Empty&&crtBoard.HasNear(x,y))
            {
                GobangBoard nextBoard = crtBoard.Clone();
                nextBoard[x, y] = selfType;
                float val = nextBoard.Evaluation(isWhite);
                if (val > best)
                {
                    bestX = x;
                    bestY = y;
                }
                best = Math.Max(best, val);
            }
        }
    }
    GobangManager.Instance.SetPiece(bestX,bestY);

}

```

### 使用AI类

写好了AI类后,随后便是使游戏逻辑能够使用AI，接下来编写GobangManager，先往其中添加五个私有字段
```c#
private bool player1IsAI;
private bool player2IsAI;
private bool humanCanControl;
private bool autoAITurn;
private bool waitControlAI;
```

其中：  
* player1IsAI、player2IsAI：用于记录玩家一或者玩家二是否是AI
* humanCanControl：加入AI后，鼠标就不能无限制的进行下棋了，这个字段用于判断是否能够使用鼠标下棋  
* autoAITurn：当只有一边是AI时，可以使AI自动在轮到时下棋，但是当两方都是AI时，自动则会自动计算到结束，此时应当对AI进行手动控制
* waitControlAI：手动控制AI的标记量  

首先从开始游戏方法入手，修改 ***StartGame*** 方法和调用此方法的 ***Start*** 方法
```c# 
private void Start()
{

    StartGame(15,15, true, false);

}
public void StartGame(int width, int height, bool player1IsAI, bool player2IsAI)
{
    board.transform.localScale = new Vector3((width - 1) * 0.2f, 1, (height - 1) * 0.2f);
    boardMaterial.SetTextureScale("_MainTex", new Vector2(width - 1, height - 1));
    blackPointsParent.SetActive(width == 15 && height == 15);

    chessCount = 0;
    waitControlAI = false;
    this.player1IsAI = player1IsAI;
    this.player2IsAI = player2IsAI;
    CrtBoard = new GobangBoard(width, height);
    marker.SetActive(false);
    player1Turn = true;
    autoAITurn = player1IsAI ^ player2IsAI;
    humanCanControl = !player1IsAI;

    if (player1IsAI)
    {
        if(autoAITurn)
        {
            new GobangAI().Thinking(true);
        }
        else
        {
            waitControlAI=true;
        }
    }

}

```
然后是在下棋后对AI进行判断，在 ***SetPiece*** 方法尾部追加判断的代码就可以了  
```c#
public void SetPiece(int x, int y)
{
    //...

    if (player1Turn)
    {
        if (player1IsAI)
        {
            humanCanControl = false;
            if (autoAITurn)
            {
                new GobangAI().Thinking(true);
            }
            else
            {
                waitControlAI = true;
            }
        }
        else
        {
            humanCanControl = true;
        }
    }
    else
    {
        if (player2IsAI)
        {
            humanCanControl = false;
            if (autoAITurn)
            {
                new GobangAI().Thinking(false);
            }
            else
            {
                waitControlAI = true;
            }
        }
        else
        {
            humanCanControl = true;
        }
    }
}
```

最后则是对Update进行修改，对鼠标操作进行限制，同时使用空格键对AI进行手动控制  
```c#
private void Update()
{

    if (humanCanControl)
    {
        //... 之前Update里的代码
    }
    if (waitControlAI)
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            waitControlAI = false;
            if (player1Turn)
            {
                new GobangAI().Thinking(true);
            }
            else
            {
                new GobangAI().Thinking(false);
            }
        }
    }

}

```

此时运行场景，已经可以和AI下棋了，但此时AI智商仍然十分有限，大多数情况下只会在自己有利的地方下棋，完全没有考虑对手，偶尔则会因为自己无法凑成高分连续，下在负分最少的地方，来堵住对手的连续棋子。  

![低智商的AI.png](https://i.loli.net/2021/07/14/xSKL5ROsyo69NWv.png)

### 提高AI的智商  

为了提高AI的智商，一个简单的思路就是多思考几步，但并不是取多步之后对自己最有利的结果下棋，因为对手并不会这样下棋来帮助自己，所以需要假定对手一定会按照对我方最不利的方法来下棋，在这个前期下，来算出对自己最有利的结果，这种算法就是MiniMax算法。

首先先将GobangAI对象化，在创建对象时初始化基础信息，可以方便之后的计算,在 ***GobangAI***中添加如下字段和构造函数。  
```c#
private GobangManager manager;
private int depth;
private CellType selfType;
private CellType opponentType;

public GobangAI(int depth, bool isWhite)
{
    manager = GobangManager.Instance;
    if ((depth & 1) != 1)
    {
        depth++;
    }
    this.depth = depth;
    selfType = isWhite? CellType.White: CellType.Black;
    opponentType = isWhite ? CellType.Black : CellType.White;
}
```

新增MiniMax方法以及修改原本的Thinking方法, 在自己的回合取最大值，对手的回合取最小值，得出为了获得最好的结果应当下在那里。
```c#
public void Thinking()
{

    if (GobangManager.Instance.chessCount == 0)
    {
        GobangManager.Instance.SetPiece(manager.CrtBoard.Width / 2, manager.CrtBoard.Height / 2);
        return;
    }
    MiniMax(depth,  manager.CrtBoard, true);

}

private float MiniMax(int depth, GobangBoard crtBoard, bool selfTurn)
{

    if (depth == 0)
        return crtBoard.Evaluation(selfType==CellType.White);
    if (selfTurn)
    {
        float best = float.MinValue;
        int bestX=0;
        int bestY=0;
        for (int x = 0; x < crtBoard.Width; x++)
        {
            for (int y = 0; y < crtBoard.Height; y++)
            {
                if (crtBoard[x,y] == CellType.Empty && crtBoard.HasNear(x, y))
                {
                    GobangBoard nextBoard = crtBoard.Clone();
                    nextBoard[x, y] = selfType;
                    float val = MiniMax(depth - 1, nextBoard, false);
                    if (val > best)
                    {
                        bestX = x;
                        bestY = y;
                    }
                    best = Math.Max(best, val);
                }
            }
        }
        if (depth == this.depth)
        {
            manager.SetPiece(bestX,bestY);
        }
        return best;
    }
    else
    {
        float best = float.MaxValue;
        for (int x = 0; x < crtBoard.Width; x++)
        {
            for (int y = 0; y < crtBoard.Height; y++)
            {
                if (crtBoard[x, y] == CellType.Empty && crtBoard.HasNear(x, y))
                {
                    GobangBoard nextBoard = crtBoard.Clone();
                    nextBoard[x, y] = opponentType;
                    float val = MiniMax(depth - 1, nextBoard, true);
                    best = Math.Min(best, val);
                }
            }
        }
        return best;
    }

}

```

随后修改 ***GobangManager***类来使用AI对象。

首先增加两个字段保存AI对象：
```c#
private GobangAI AI1;
private GobangAI AI2;
```

修改StartGame方法来初始化AI对象：
```c#
public void StartGame(int width, int height, bool player1IsAI, bool player2IsAI)
{

    board.transform.localScale = new Vector3((width - 1) * 0.2f, 1, (height - 1) * 0.2f);
    boardMaterial.SetTextureScale("_MainTex", new Vector2(width - 1, height - 1));
    blackPointsParent.SetActive(width == 15 && height == 15);

    chessCount = 0; 
    waitControlAI = false; 
    this.player1IsAI = player1IsAI; 
    AI1 = new GobangAI(3, true); //新增
    this.player2IsAI = player2IsAI; 
    AI2 = new GobangAI(3, false); //新增
    CrtBoard = new GobangBoard(width, height); 
    marker. SetActive(false); 
    player1Turn = true; 
    autoAITurn = player1IsAI ^ player2IsAI; 
    humanCanControl = !player1IsAI; 
    if (player1IsAI)
    {
        if (autoAITurn)
        {
            AI1. Thinking(); //修改
        }
        else
        {
            waitControlAI=true;
        }
    }

}

```
然后将SetPiece和Update中报错的地方替换成对应的内容：
```c#
AI1.Thinking();     //player1Turn为true时

AI2.Thinking();     //player1Turn为false时
```

此时就可以通过调整层数来调整AI的智商了，当层数为1时就会和之前的一致。
但是目前为止，AI每思考一次，就会消耗大量的时间，只是思考三层时间就已经时无法正常游玩了。

## AI性能优化

目前虽然有了能够使用的AI，但是却存在着严重的性能问题，必须要对其进行优化。  

### 使用AlphaBeta剪枝优化MiniMax算法  

在进行判断的过程中，会遇到这种情况：
* 在已经判断过的分支中，已知我方至少能够获得的结果评分为n，在之后的判断中，当我方确定走了某一步之后，对方在这一步的基础上走的第二步的某一个分支能够使结果评分比n还低，则此时应当抛弃第一步的所有剩余子节点，因为对手可以选择这个分支使得无论如何都无法获得比之前更好的情况了。  
* 在已经判断过的分支中，已知我方最好能够获得的结果评分为n，在之后的判断中，当对方确定走了某一步之后，我方在这一步的基础上走的第二步的某一个分支能够使结果评分比n还高，则此时应当抛弃第一步的所有剩余子节点，因为我方可以选择这个分支使得评分比之前更好，而对手并不希望如此。
在这两种情况下可以删减不需要判断的分支，最终结果会大大减少最终的节点评估函数调用的次数。这种优化算法就是AlphaBeta剪枝。  

Alpha 是可能解的最大下限，也就是至少能够获得的评分，我方的步骤中会不断试图提高这个值。
Beta 是可能解的最小上限，也就是最好能够获得的评分，对方的步骤中会不断试图降低这个值。
当Beta<=Alpha时，则代表遇到之前的那两种情况之一。

以此为基础，修改 ***MiniMax*** 方法和 ***Thinking*** 方法, 初始的alpha为极小值，beta为极大值。  
```c# 
public void Thinking()
{

    //。。。
    MiniMax(depth, manager.CrtBoard, true, float.MinValue, float.MaxValue);

}

private float MiniMax(int depth, GobangBoard crtBoard, bool selfTurn, float alpha, float beta)
{

    //。。。
                    best = Math.Max(best, val);
                    alpha = Math.Max(alpha, best);  //新增
                    if (beta <= alpha) break;       //新增
    //。。。
                    best = Math.Min(best, val);
                    beta = Math.Min(beta, best);    //新增
                    if (beta <= alpha) break;       //新增
    //。。。

}

```

此时运行场景，速度已经比之前快多了，但是仍然有明显延迟。

### 细节修改

算法已经很难优化了，接下来优化其他细节。  

#### 减少循环层数

目前是使用xy来遍历整个棋盘，这样子需要两层循环，使用单个索引遍历能够减少循环的层数。  
但此时缺少通过单个索引获取棋格状态的属性，使用单个索引判断邻近棋格是否存在棋子的方法，以及使用单个索引下棋的方法。  
在 ***GobangBoard*** 中添加如下属性和方法。
```c#
public CellType this[int index]
{
    get
    {
        if (index < 0 || index >= board.Length)
        {
            return CellType.Void;
        }
        return board[index];
    }
    set
    {
        board[index] = value;
    }
}
public bool HasNear(int index)
{
    return HasNear(GetX(index), GetY(index));
}
public int GetX(int pos)
{
    return pos / Width;
}
public int GetY(int pos)
{
    return pos % Width;
}
```

在 ***GobangManager*** 中添加如下方法  
```c#
public void SetPiece(int index)
{

    SetPiece(CrtBoard.GetX(index), CrtBoard.GetY(index));

}

```

修改 ***GobangAI*** 中的MiniMax方法使其使用单层循环  
```c#
private float MiniMax(int depth, GobangBoard crtBoard, bool selfTurn, float alpha, float beta)
{
    if (depth == 0)
        return crtBoard.Evaluation(selfType == CellType.White);
    if (selfTurn)
    {
        float best = float.MinValue;
        int bestIndex = 0;
        for (int i = 0; i < crtBoard.CellCount; i++)
        {
            if (crtBoard[i] == CellType.Empty && crtBoard.HasNear(i))
            {
                GobangBoard nextBoard = crtBoard.Clone();
                nextBoard[i] = selfType;
                float val = MiniMax(depth - 1, nextBoard, false, alpha, beta);
                if (val > best)
                {
                    bestIndex = i;
                }
                best = Math.Max(best, val);
                alpha = Math.Max(alpha, best);
                if (beta <= alpha)
                    break;
            }
        }
        if (depth == this.depth)
        {
            manager.SetPiece(bestIndex);
        }
        return best;
    }
    else
    {
        float best = float.MaxValue;
        for (int i = 0; i < crtBoard.CellCount; i++)
        {
            if (crtBoard[i] == CellType.Empty && crtBoard.HasNear(i))
            {
                if (crtBoard[i] == CellType.Empty && crtBoard.HasNear(i))
                {
                    GobangBoard nextBoard = crtBoard.Clone();
                    nextBoard[i] = opponentType;
                    float val = MiniMax(depth - 1, nextBoard, true, alpha, beta);
                    best = Math.Min(best, val);
                    beta = Math.Min(beta, best);
                    if (beta <= alpha)
                        break;
                }
            }
        }
        return best;
    }
}
```

#### 只评估有两个以上棋子的数组  

目前在进行评估时，只要有一个及以上的棋子就会遍历字典判断其得分，但是至少有两个棋子才会得分，所以应当减少这部分的评分。  
修改 ***GobangAI*** 的Judgment5和Judgment6方法，只有在之前已经至少一个棋子的情况下才会使flag置为true  
```c#
public void Judgment5(CellType crtCellType, int index, ref int[] array1, ref int[] array2, ref bool flag1, ref bool flag2, ref CellType cellType)
{

    switch (crtCellType)
    {
        case CellType.White:
            //...
            else if (cellType != CellType.Void)
            {
                if (cellType==CellType.White)
                {
                    flag1 = true;
                    flag2 = true;
                }
                cellType = CellType.White;
                array1[index] = 1;
                array2[index] = 1;
            }
            break;
        case CellType.Black:
           //...
            else if (cellType != CellType.Void)
            {
                if (cellType == CellType.Black)
                {
                    flag1 = true;
                    flag2 = true;
                }
                cellType = CellType.Black;
                array1[index] = 1;
                array2[index] = 1;
            }
            break;
        //...
    }

}
public void Judgment6(CellType crtCellType, ref int[] array, ref bool flag, CellType cellType)
{

    switch (crtCellType)
    {
        case CellType.White:
            //...
            else if (cellType != CellType.Void)
            {
                if (cellType == CellType.White)
                {
                    flag = true;
                }
                array[5] = 1;
            }
            break;
        case CellType.Black:
            //...
            else if (cellType != CellType.Void)
            {
                if (cellType == CellType.Black)
                {
                    flag = true;
                }
                array[5] = 1;
            }
            break;
        //...
    }

}

```

#### 复用int数组  

现在每次调用评估函数都会在每次循环中创建8个int数组，由于每次使用都会对其完全赋值一遍，所以可以在不进行初始化的情况下复用这些int数组  
将数组的初始化移动到类的内部  
```c#
int[] rightDir = new int[5];
int[] rightDir2 = new int[6];
int[] upDir = new int[5];
int[] upDir2 = new int[6];
int[] rightUpDir = new int[5];
int[] rightUpDir2 = new int[6];
int[] rightDownDir = new int[5];
int[] rightDownDir2 = new int[6];

public float Evaluation(bool isWhite)
{
    float totalPower = 0;
    for (int x = 0; x < Width; x++)
    {
        for (int y = 0; y < Height; y++)
        {
            //int[] rightDir = new int[5];
            //int[] rightDir2 = new int[6];
            //int[] upDir = new int[5];
            //int[] upDir2 = new int[6];
            //int[] rightUpDir = new int[5];
            //int[] rightUpDir2 = new int[6];
            //int[] rightDownDir = new int[5];
            //int[] rightDownDir2 = new int[6];
            //。。。
        }
    }
    //。。。
}
```

#### 优化分值字典及遍历

现在定义数组的分值是用的键为数组值为int的字典(Dictionary<int[], int>), 比较分值的方法则是使用了linq的比较集合方法，这两个东西都是性能较差的。  
把字典改为二重数组(int[][])，将分值并入数组的末尾，之后将通过索引来获取分值，修改 ***GobangBoard***中的字典。  
```c#
static readonly int[][] ScoreTable5 =new int[][]
{

    new int[] { 0, 1, 1, 0, 0 ,5 },
    new int[] { 0, 0, 1, 1, 0 ,5 },
    new int[] { 1, 1, 0, 1, 0 ,20 },
    new int[] { 0, 1, 0, 1, 1 ,20 },
    new int[] { 0, 0, 1, 1, 1 ,50 },
    new int[] { 1, 1, 1, 0, 0 ,50 },
    new int[] { 0, 1, 1, 1, 0 ,100 },
    new int[] { 1, 1, 1, 0, 1 ,300 },
    new int[] { 1, 1, 0, 1, 1 ,300 },
    new int[] { 1, 0, 1, 1, 1 ,300 },
    new int[] { 1, 1, 1, 1, 0 ,500 },
    new int[] { 0, 1, 1, 1, 1 ,500 },
    new int[] { 1, 1, 1, 1, 1 ,99999999 },

}; 
static readonly int[][] ScoreTable6 = new int[][]
{

    new int[]{ 0, 1, 0, 1, 1, 0 ,500 },
    new int[]{ 0, 1, 1, 0, 1, 0 ,500 },
    new int[]{ 0, 1, 1, 1, 1, 0 ,5000 },

}; 

```
新增比较数组的方法并使用。  
```c#
private int GetScore5(int[] input)
{
    foreach (var item in ScoreTable5)
    {
        if (CompareIntArray(input, item, 5))
        {
            return item[5];
        }
    }
    return 0;
}

private int GetScore6(int[] input)
{
    foreach (var item in ScoreTable6)
    {
        if (CompareIntArray(input, item, 6))
        {
            return item[6];
        }
    }
    return 0;
}

private bool CompareIntArray(int[] arr1, int[] arr2, int length)
{
    for (int i = 0; i < length; i++)
    {
        if (arr1[i] != arr2[i])
        {
            return false;
        }
    }
    return true;
}
```

#### 使用对象池来管理GobangBoard对象  

在AI进行判断的时候，需要创建许多的棋盘用来建立分支，这导致了大量的new的性能消耗，使用对象池来管理GobangBoard对象的获取可以节省一部分性能。  
创建 ***ObjectPool*** 工具类。  
```c#
using System; 
using System. Collections. Generic; 

public class ObjectPool<T> where T : class, new()
{

    private readonly Queue<T> objects;
    private readonly Action<T> resetAction;
    private readonly Action<T> initAction;
    private readonly Action<T> firstTimeInitAction;

    public ObjectPool(Action<T> resetAction = null, 
        Action<T> initAction = null,
        Action<T> firstTimeInitAction = null)
    {
        objects = new Queue<T>();
        this.resetAction = resetAction;
        this.initAction = initAction;
        this.firstTimeInitAction = firstTimeInitAction;
    }

    public T New()
    {
        if (objects.Count > 0)
        {
            T t = objects.Dequeue();
            initAction?.Invoke(t);
            return t;
        }
        else
        {
            T t = new T();
            firstTimeInitAction?.Invoke(t);
            initAction?.Invoke(t);
            return t;
        }
    }

    public void Store(T obj)
    {
        resetAction?.Invoke(obj);
        objects.Enqueue(obj);
    }

}

```
在 ***GobangBoard*** 中增加初始化数组的方法来使得能够在构造函数以外的地方修改棋格数组的大小。  
```c#
public void InitBoard(int width, int height)
{
    this.Width = width;
    this.Height = height;
    if (board.Length != width * height)
    {
        board = new CellType[width * height];
    }
}
```

在 ***GobangManager*** 中增加对象池的对象并在开始游戏时进行初始化
```c#
public ObjectPool<GobangBoard> boardPool; 

public void StartGame(int width, int height, bool player1IsAI, bool player2IsAI)
{

    //...

    //CrtBoard = new GobangBoard(width, height); 
    boardPool = new ObjectPool<GobangBoard>(null, null, (board)=> { board. InitBoard(width, height); }); 
    CrtBoard = boardPool. New(); 

    //...

}

```
修改GobangBoard的Clone方法，是之从对象池中获取新对象。  
```c#
public GobangBoard Clone()
{
    GobangBoard newGobangBoard = GobangManager.Instance.boardPool.New();
    board.CopyTo(newGobangBoard.board, 0);
    return newGobangBoard;
}
```

使GobangBoard实现IDisposable接口，使之在使用完毕时将自身放入对象池。  
```c#
using System; 
public class GobangBoard: IDisposable
{

    //...

    public void Dispose()
    {
        GobangManager. Instance.boardPool. Store(this); 
    }   

}

```

在GobangAI中获取棋盘副本的地方使用using关键字，控制其回收。
```c#

private float MiniMax(int depth, GobangBoard crtBoard, bool selfTurn, float alpha, float beta)
{

    //...
    if (crtBoard[i] == CellType. Empty && crtBoard. HasNear(i))
    {
        using GobangBoard nextBoard = crtBoard. Clone(); //修改
        nextBoard[i] = selfType; 
       //...
    }
    //...
    if (crtBoard[i] == CellType. Empty && crtBoard. HasNear(i))
    {
        using GobangBoard nextBoard = crtBoard. Clone(); //修改
        nextBoard[i] = opponentType; 
       //...
    }
    //...

}
```

修改GobangManager的StartGame方法手动控制棋盘的回收
```c#

    public void StartGame(int width, int height, bool player1IsAI, bool player2IsAI)
    {   
        //...
        CrtBoard?.Dispose();    //新增
        boardPool = new ObjectPool<GobangBoard>(null,null,(board)=> { board.InitBoard(width, height); });
        CrtBoard = boardPool.New();
        //...
    }

```

#### 使用线程

在调用必然需要消耗大量时间方法时，应当使用线程对其进行调用，这并不会解决性能问题，但是至少能够摆脱程序卡死的情况。  
修改 ***GobangAI***的 ***Thinking*** 方法，调用线程来执行MiniMax方法  
```c#
public void Thinking()
{

    if (GobangManager.Instance.chessCount == 0)
    {
        GobangManager.Instance.SetPiece(manager.CrtBoard.Width / 2, manager.CrtBoard.Height / 2);
        return;
    }
    Thread thread = new Thread(() => MiniMax(depth, manager.CrtBoard, true, float.MinValue, float.MaxValue));
    thread.Start();

}
```

但是unity并不支持在线程中调用Unity的方法(如：Object. Instantiate 实例化)，所以需要在线程中修改标记量，主线程中则会检测标记量，在主线程中调用对应的方法。  
在 ***GobangManager*** 中使用一个int属性来作为标记量，初始为-1，当其变为正数时代表需要在对应索引的位置下棋。在Update方法中对其进行检测。
```c#
public int IndexBuffer { get; set; } = -1; 

private void Update()
{

    if (IndexBuffer != -1)
    {
        SetPiece(IndexBuffer); 
        IndexBuffer = -1; 
    }

    //...

}

```
修改GobangAI中调用下棋方法的地方，将其改为修改标记量  
```c#
private float MiniMax(int depth, GobangBoard crtBoard, bool selfTurn, float alpha, float beta)
{

    //...
    if (depth == this.depth)
    {
        //manager. SetPiece(bestIndex); 
        manager. IndexBuffer=bestIndex; 
    }
    return best; 
    //...

}
```

现在运行场景，在思考三层的前提下，已经能够以相当快的速度思考了，而思考四层时虽然会较慢，但是由于是在线程中计算所以程序本身也不会卡顿。  

## 完善游戏  

现在仍然是通过Unity编辑器内的场景运行来控制游戏的运行，制作UI来控制游戏的流程。

### 重新开始 

现在无法在运行后重置棋盘，在Start离开时一次默认的对局是没问题的，暂时不需要对其进行改变。  
为了在重新开始后清除棋盘上的棋子，在开始游戏时创建一个GameObject，创建的棋子将放入其子节点，并在开始另一局游戏时将其销毁。  
```c#
private GameObject chessParent; 

public void StartGame(int width, int height, bool player1IsAI, bool player2IsAI)
{

    if (chessParent!=null)
    {
        Destroy(chessParent); 
    }
    chessParent = new GameObject(); 

    //...

}

public void SetPiece(int x, int y)
{

    GameObject newPiece; 
    if (player1Turn)
    {
        newPiece = Instantiate(whiteChessPrefab, chessParent.transform); //修改
    }
    else
    {
        newPiece = Instantiate(blackChessPrefab, chessParent.transform); //修改
    }

    //...

}

```

现在也无法通过传参改变AI的等级，修改 ***GobangManager*** 的 ***StartGame*** 方法使之能够传递AI等级的参数，同时修改其 ***Start*** 方法传递AI等级为3。  
```c#
private void Start()
{

    StartGame(15, 15, true, false,3,3); //修改

}
public void StartGame(int width, int height, bool player1IsAI, bool player2IsAI, int AI1Level, int AI2Level)  //修改
{

    //...
    this.player1IsAI = player1IsAI; 
    AI1 = new GobangAI(AI1Level, true); //修改
    this.player2IsAI = player2IsAI; 
    AI2 = new GobangAI(AI2Level, false); //修改
    //...

}
```

之后将通过用户界面控制AI的手动运行，将代码从Update中提取出来，同时增加用户托管AI下棋的功能。  
```c#
private void Update()
{    
    //...
    //if (waitControlAI)
    //{
    //    if (Input.GetKeyDown(KeyCode.Space))
    //    {
    //        waitControlAI = false;
    //        if (player1Turn)
    //        {
    //            AI1.Thinking();
    //        }
    //        else
    //        {
    //            AI2.Thinking();
    //        }
    //    }
    //}
}

public void ManualAI()
{
    if (waitControlAI)
    {
        waitControlAI = false;
        if (player1Turn)
        {
            AI1.Thinking();
        }
        else
        {
            AI2.Thinking();
        }
    }
    else if (humanCanControl)
    {
        humanCanControl = false;
        if (player1Turn)
        {
            AI1.Thinking();
        }
        else
        {
            AI2.Thinking();
        }
    }
}
```

### 用户界面

现在仍然是通过Unity编辑器内的场景运行来控制游戏的运行，现在制作能够控制流程的UI，不需要制作过于复杂的界面，制作一个能够编辑参数的开始游戏面板，以及少数几个按钮就可以了。  

首先创建控制UI逻辑的管理类 ***GobangUI*** 并创建需要的UI对象。  
```c#
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GobangUI : MonoBehaviour
{
    public GameObject window_StartGame;
    public Toggle tog_Player1IsAI;
    public Toggle tog_Player2IsAI;
    public Dropdown dpd_Width;
    public Dropdown dpd_Height;
    public Dropdown dpd_AI1Level;
    public Dropdown dpd_AI2Level;
    public Button btn_StartGame;
    public Button btn_CloseWindow;

    public Text txt_Thinking;

    public Button btn_OpenWindow;
    public Button btn_ManualAI;
}
```

创建两个数组用于表示Dropdown中的值。  
```c#
static readonly int[] Size = new int[] { 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };
static readonly int[] Level = new int[] { 1, 2, 3, 4, 5 };
```

创建相关的方法并在Awake时进行初始化。  
```c#
private void Awake()
{
    btn_StartGame.onClick.AddListener(StartGame);
    btn_ManualAI.onClick.AddListener(ManualAI);
    btn_OpenWindow.onClick.AddListener(OpenWindow);
    btn_CloseWindow.onClick.AddListener(CloseWindow);

    List<string> sizeStr = new List<string>();
    foreach (var item in Size)
    {
        sizeStr.Add(item.ToString());
    }
    dpd_Width.ClearOptions();
    dpd_Width.AddOptions(sizeStr);
    dpd_Width.value = 5;
    dpd_Height.ClearOptions();
    dpd_Height.AddOptions(sizeStr);
    dpd_Height.value = 5;
    
    List<string> levelStr = new List<string>();
    foreach (var item in Level)
    {
        levelStr.Add(item.ToString());
    }
    dpd_AI1Level.ClearOptions();
    dpd_AI1Level.AddOptions(levelStr);
    dpd_AI1Level.value = 2;
    dpd_AI2Level.ClearOptions();
    dpd_AI2Level.AddOptions(levelStr);
    dpd_AI2Level.value = 2;

    tog_Player1IsAI.isOn = false;
    
    HideTips();
    CloseWindow();
}

private void StartGame()
{
    GobangManager.Instance.StartGame(Size[dpd_Width.value], Size[dpd_Height.value],
        tog_Player1IsAI.isOn, tog_Player2IsAI.isOn,
        Level[dpd_AI1Level.value], Level[dpd_AI2Level.value]);
    CloseWindow();
}

private void ManualAI()
{
    GobangManager.Instance.ManualAI();
}

private void OpenWindow()
{
    window_StartGame.SetActive(true);
}

private void CloseWindow()
{
    window_StartGame.SetActive(false);
}

public void ShowTips(string msg)
{
    txt_Tips.gameObject.SetActive(true);
    txt_Tips.text = msg;
}

public void HideTips()
{
    txt_Tips.gameObject.SetActive(false);
}
```

随后就是在场景中创建对应的对象新建一个UI/Panel, 假如之前没有创建过UI的话，会自动新建一个Canvas和EventSystem。
删除Panel的Image组件，在其中创建一个UI/Image作为窗口，然后在窗口中创建三个UI/Text，一个作为标题，另外两个作为文本提示，创建两个UI/Toggle,选择是否两方为AI，创建四个UI/Dropdown，两个选择AI等级，两个选择棋盘尺寸，创建两个个UI/Button，其中一个作为开始游戏的按钮，一个作为关闭面板的按钮。效果如下：  

![开始游戏面板.png](https://i.loli.net/2021/07/15/Mor6xymAIR8nlC3.png)  

在Panel左下角创建一个UI/Text，用于AI思考时以及游戏结束时的提示。  

![左下角文本.png](https://i.loli.net/2021/07/15/pRoVUsnQPdatNCL.png)   

右下角创建两个UI/Button，其中一个作为开启开始游戏面板的按钮，另一个作为使用AI下棋的按钮，用于在两方都为AI时的手动控制，以及非AI时的托管下棋。  

![右下角按钮.png](https://i.loli.net/2021/07/15/SaCrNMeOh1DI2Fm.png)  

将GobangUI挂载在Panel上，对字段进行赋值。  

现在仍然缺少UI与流程的交互，现在回到 ***GobangManager*** 。
首先添加对GobangUI的引用，添加GobangUI的字段并在Unity编辑器中对其进行赋值，这样就能够访问GobangUI的公共方法了。
```c#
public GobangUI gobangUI;
```
需要在开始思考是显示AI思考中的提示并在思考完成后隐藏提示，首先创建一个公共的展示提示的方法。  
```c#
public void AIThinkingTips()
{
    gobangUI.ShowTips("AI思考中");
}
```
然后再 ***GobangAI*** 中对其进行调用。
```c#
public void Thinking()
{
    if (GobangManager.Instance.chessCount == 0)
    {
        GobangManager.Instance.SetPiece(manager.CrtBoard.Width / 2, manager.CrtBoard.Height / 2);
        return;
    }
    GobangManager.Instance.AIThinkingTips();    //新增
    Thread thread = new Thread(() => MiniMax(depth, manager.CrtBoard, true, float.MinValue, float.MaxValue));
    thread.Start();
}
```
最后在GobangManager的Update方法中对提示进行隐藏。  
```c#
private void Update()
{
    if (IndexBuffer != -1)
    {
        gobangUI.HideTips();        //新增
        SetPiece(IndexBuffer);
        IndexBuffer = -1;
    }
    //...
}
```
这样子AI思考的提示就完成了，接下来是结束游戏的提示，将SetPiece方法中的Debug.Log替换为gobangUI.ShowTips。  
```c#
public void SetPiece(int x, int y)
{
    //...
    if (CrtBoard.CheckWin(player1Turn))
    {
        //Debug.Log(player1Turn ? "Player1 Win!" : "Player2 Win!");
        gobangUI.ShowTips(player1Turn ? "Player1 Win!" : "Player2 Win!");
        return;
    }
    if (chessCount == CrtBoard.CellCount)
    {
        //Debug.Log("Board is full!");
        gobangUI.ShowTips("Board is full!");        
        return;
    }
    //...
}
```
此时这个项目已经可以重新开始一盘自定义的对局了。 
但是这时候有了一个新的问题用户点击UI时仍然会影响棋盘，使Update中的射线检测前提增加一个条件,需要using UnityEngine.EventSystems。

```c#
using UnityEngine.EventSystems;
private void Update()
{
    //...
    if (!EventSystem.current.IsPointerOverGameObject()&& humanCanControl)
    //...
}
```
这样鼠标放置在UI上时用户就无法影响棋盘了。  

## 重新开始的二次处理  
此时重新开始仍有一些问题，比如虚影会留下，左下角文本会显示之前的状态，在StartGame开始时进行处理
```c#
public void StartGame(int width, int height, bool player1IsAI, bool player2IsAI, int AI1Level, int AI2Level)
{
    gobangUI.ShowTips("");
    blackChessPhantom.gameObject.SetActive(false);
    whiteChessPhantom.gameObject.SetActive(false);
    //...
}
```
同时由于线程未作处理，可能会出现开始新的一局后上一局的线程下在新的一局的情况，将GobangAI中的thread变量提取至类的域中，并创建Abort方法中断线程。  
```c#
Thread thread;
public void Abort()
{
    thread?.Abort();
}
public void Thinking()
{
    if (GobangManager.Instance.chessCount == 0)
    {
        GobangManager.Instance.SetPiece(manager.CrtBoard.Width / 2, manager.CrtBoard.Height / 2);
        return;
    }
    GobangManager.Instance.AIThinkingTips();
    thread = new Thread(() => MiniMax(depth, manager.CrtBoard, true, float.MinValue, float.MaxValue));
    thread.Start();
}
```

在StartGame中对其进行调用。  

```c#
public void StartGame(int width, int height, bool player1IsAI, bool player2IsAI, int AI1Level, int AI2Level)
{
    //。。。
    AI1?.Abort();                           //新增
    AI1 = new GobangAI(AI1Level, true);
    this.player2IsAI = player2IsAI;
    AI2?.Abort();                           //新增
    AI2 = new GobangAI(AI2Level, false);
    //。。。
}
```
## AI的二次优化  

现在会有一种情况，当思考深度为3时，判断的时我方走两次对方走一次的情况，初始状态时我经有三连，但对方有四连了。在判断中，在第三步时我方完成五连的分值，会比在第一步阻碍对方五连的分数高，但是在第二步时对方已经完成五连，所以并没有意义。  
只是单纯增大敌方分值的权重可以防止这个问题，但是这样会导致AI变成完全的防守型人格，只有对方在思考步数以内不可能五连的情况下才会进攻。
另一个方法就是对每一步都进行评估，当某一方凑成五连时，不再判断之后的步骤，而是直接返回当前的分数。  

修改GobangBoard的数值表，将五连独立出来。  
```c#
static readonly int[] ConnectFive = new int[] { 1, 1, 1, 1, 1, 99999999 };

static readonly int[][] ScoreTable5 = new int[][]
{
    new int[] { 0, 1, 1, 0, 0 ,5 },
    new int[] { 0, 0, 1, 1, 0 ,5 },
    new int[] { 1, 1, 0, 1, 0 ,20 },
    new int[] { 0, 1, 0, 1, 1 ,20 },
    new int[] { 0, 0, 1, 1, 1 ,50 },
    new int[] { 1, 1, 1, 0, 0 ,50 },
    new int[] { 0, 1, 1, 1, 0 ,100 },
    new int[] { 1, 1, 1, 0, 1 ,300 },
    new int[] { 1, 1, 0, 1, 1 ,300 },
    new int[] { 1, 0, 1, 1, 1 ,300 },
    new int[] { 1, 1, 1, 1, 0 ,500 },
    new int[] { 0, 1, 1, 1, 1 ,500 },
};
```

修改GetScore5方法，使其能返回是否五连的信息。  
```c#
private int GetScore5(int[] input, ref bool haveConnectFive)
{
    if (CompareIntArray(input, ConnectFive, 5))
    {
        haveConnectFive = true;
        return ConnectFive[5];
    }
    
    foreach (var item in ScoreTable5)
    {
        if (CompareIntArray(input, item, 5))
        {
            return item[5];
        }
    }
    return 0;
}
```
修改评估函数的返回值，现在返回是否有五连，并使用out关键字返回分值。  
```c#
public bool Evaluation(bool isWhite, out float totalPower)
{
    totalPower = 0;
    bool  haveConnectFive = false;
    //...
            if (rightFlag)
            {
                totalPower += (rightCellType == CellType.White ? 1 : -1) * GetScore5(rightDir, ref haveConnectFive);
            }
            if (rightFlag2)
            {
                totalPower += (rightCellType == CellType.White ? 1 : -1) * GetScore6(rightDir2 );
            }
            if (upFlag)
            {
                totalPower += (upCellType == CellType.White ? 1 : -1) * GetScore5(upDir, ref haveConnectFive);
            }
            if (upFlag2)
            {
                totalPower += (upCellType == CellType.White ? 1 : -1) * GetScore6(upDir2);
            }
            if (rightUpFlag)
            {
                totalPower += (rightUpCellType == CellType.White ? 1 : -1) * GetScore5(rightUpDir, ref haveConnectFive);
            }
            if (rightUpFlag2)
            {
                totalPower += (rightUpCellType == CellType.White ? 1 : -1) * GetScore6(rightUpDir2);
            }
            if (rightDownFlag)
            {
                totalPower += (rightDownCellType == CellType.White ? 1 : -1) * GetScore5(rightDownDir, ref haveConnectFive);
            }
            if (rightDownFlag2)
            {
                totalPower += (rightDownCellType == CellType.White ? 1 : -1) * GetScore6(rightDownDir2);
            }
    //...
}

```
修改GobangAI中的方法，使其在中途可以中断，并根据层数增加权重。  
```c#
private float MiniMax(int depth, GobangBoard crtBoard, bool selfTurn, float alpha, float beta)
{
    if (depth == 0)
    {
        crtBoard.Evaluation(selfType == CellType.White, out float score);
        return score;
    }
    else
    {
        if (crtBoard.Evaluation(selfType == CellType.White, out float score))
        {
            return (2 + depth) * score;
        }
    }
    //...
}
```

本以为这样可以使AI变得正常，但测试后发现和之前一样，随后便意识到，由于思考三层时对手只走了一步，自己走两步导致的五连会比对手的四连分数更加高，仍然没有作用。  
能想到的解决方案就是在思考奇数步时将对手的分数升阶，即四连当作五连，但这样会使评估函数变得更加复杂，所以可以用更加简单的方法，使用四层思考深度。  
四层的思考时间有些过长了，应该还有优化的空间，暂时就写这么多，之后有机会再深入一下。  

[github项目地址]("https://github.com/DonnieBean/Gobang")

## 参考资料  
* 博弈树alpha-beta剪枝搜索的五子棋AI[https://www.jianshu.com/p/8376efe0782d]:五子棋的评估函数及AI的计算方法  
* Computer Science Game Trees[https://www.yosenspace.com/posts/computer-science-game-trees.html]:这篇文章里有一个可交互的Alpha-Beta剪枝示例，可以较为清晰的观察剪枝的过程  
* CS 161 Recitation Notes - Minimax with Alpha Beta Pruning[http://web.cs.ucla.edu/~rosen/161/notes/alphabeta.html]:这篇文章详细用文字介绍了一个剪枝的过程  
* Minimax Algorithm in Game Theory | Set 4 (Alpha-Beta Pruning)[https://www.geeksforgeeks.org/minimax-algorithm-in-game-theory-set-4-alpha-beta-pruning/]:有时候直接看到代码能更快的理解逻辑  

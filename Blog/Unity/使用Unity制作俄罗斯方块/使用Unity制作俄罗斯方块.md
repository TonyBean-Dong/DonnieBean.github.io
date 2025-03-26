# 使用Unity制作俄罗斯方块  
开发环境:Unity2021.3.2f1c1Personal,visualStudio2022，windows11系统，Unity设置平台windows  
学习C#期间曾通宵用控制台应用写了一个俄罗斯方块，后来发现一些小bug想修复但是我已经看不懂那堆代码了，索性直接删了用unity来做

## 基础规则  

传统俄罗斯方块的场地宽10格高20格，共有七种方块，根据形状可以用S、Z、J、L、I、O、T这7个字母来命名，规则是移动、旋转和摆放游戏自动生成并下降的各种方块，使之排列成完整的一行或多行并且消除得分。

## 基础场景配置与UI适配  

首先将摄像机的Projection属性设置为Orthographic，假如使用2D模板创建项目的话就是默认设置。  
![摄像机设置]()  
然后创建Canvas，将其CanvasScaler组件的UIScaleMode设置为ScaleWithScreenSize，ReferenceResoulution设置为1920*1080，Match设置为0.5，这种设置是在没有对设备单独UI适配的情况下一种较为通用的适配方法。  
![Canvas配置]()  
为了开发时更好的进行布局，需要在Game窗口中设置分辨率，如果没有1920*1080分辨率的话，可以点击下方加号进行添加。  
![分辨率设置]()  

为了确定好方块空间的大小，先填充10*20的空间后调整摄像机的位置，编写测试脚本动态生成。  
``` C#
    private void Fill()
    {
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 20; j++)
            {
                var go = Instantiate(whiteCube,transform);
                go.transform.position = new Vector3(i,j,0);
            }
        }
    }
```
其中whiteCube为unity默认创建出的cube做成的预制体，大小也是默认的大小。  

将运行时调用Fill方法的脚本挂载在场景中，运行后调整摄像机位置和其Camera组件中的Size属性，使生成的方块能够完整的落入屏幕中，我得出的大小为10.5。  
然后在Canvas中创建两个Image用于布局，分别置于左右，使其刚好留出中间能够完整看清方块的空间，留出一定空间用于之后的边框，左右区域用于之后的菜单。  
![标识区域]()  
但此时的布局只能适配1920*1080的分辨率，假如将Game窗口的分辨率设置为FreeAspect后随意改变Game窗口大小，就会发现由于3D的方块不会跟随屏幕改变大小但是UI会跟着改变布局导致方块会被UI挡住，所以需要使用脚本动态改变左右区域的大小。  
``` C#
    /// <summary>
    /// 动态改变宽度
    /// </summary>
    /// <param name="width">屏幕宽度：Screen.width</param>
    /// <param name="height">屏幕高度：Screen.height</param>
    private void ChangeWidth(int width, int height)
    {
        float logWidth = Mathf.Log(width / 1920f, 2);
        float logHeight = Mathf.Log(height / 1080f, 2);
        float logWeightedAverage = Mathf.Lerp(logWidth, logHeight, 0.5f);
        float power = Mathf.Pow(2, logWeightedAverage);
        float trueWidth = width / power;
        float trueHeight = height / power;
        float left = trueWidth - (trueHeight / 2);
        float newWidth = left / 2 - 20;
        GetComponent<RectTransform>().sizeDelta = new Vector2(newWidth, 0);
    }
```
这个方法只适配CanvasScaler组件中之前的设置，其中1920、1080、0.5皆是之前在CanvasScaler组件中设置的参数，如果时其他设置的话适配的方法不一样。这个方法在每一次屏幕宽高改变后都要调用一次，可以在update中进行检测并调用。  
将带有ChangeWidth方法的脚本挂载在左右区域后，需要修改这两个对象的锚点和中心点，左侧区域的锚点为0、0、0、1，中心点为0，0.5，右侧区域的锚点为1、0、1、1，中心点为1，0.5，目的是使其在改变大小后仍然靠边。  
现在在奇怪的分辨率下也能正常划分区域了。  
![自适应分辨率]()  
实际上即使这样适配也很难将一套Ui同时用在横屏和竖屏上，有横屏和竖屏需求时应该使用两套Ui动态使用，但这个项目是pc项目所以只适配横屏。  

## 小方块管理类  
俄罗斯方块的不同版本方块颜色各不相同，但总的来说能区分就行，本项目使用S：绿色、Z：红色、J：蓝色、L：橙色、I：青色、O：黄色、T：紫色。  
使用作图软件制作一张带边框的方形图片，因为这样才能在方块连在一起时进行区分。  
![Block]()  
然后创建七个不同的材质球，设置使用这张图片后分别使用不同的颜色，上面这张图片边缘有透明边框，所以使用Transparent模式效果较好。  
![材质球设置]()  
接下来编写代码，首先编写方块类型枚举  
``` C#
    public enum BlockType
    {
        Null,
        S,
        Z,
        J,
        L,
        I,
        O,
        T,
    }
```
然后编写小方块控制脚本，当其停靠后就不属于某一个大方块了，所以需要对每一个小方块单独控制，脚本挂载在方块预制体上后使用对象池管理，因为这属于常用资源,定义公开的对象池属性。
``` C# 
    public GameObjectPool pool { get; set; }    //所属对象池
```
不同对象池会有不一样的对象管理逻辑，我编写的对象池只会保存待使用的对象，正在使用的对象想要返回池中需要调用存储方法，具体的使用在后面细说。  
接下来是在从对象池取出后调用的方法：  
``` C#
    [SerializeField] private Material[] materials;  //材质球数组
    [SerializeField] private MeshRenderer render;   //渲染组件
    private Vector2Int pos; //当前位置

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="blockType"></param>
    public void Create(Vector2Int pos,BlockType blockType)
    {
        this.pos = pos;
        UpdatePos();
        render.material = materials[(int)blockType];
    }

    /// <summary>
    /// 更新实际位置
    /// </summary>
    private void UpdatePos()
    {
        transform.position = new Vector3(pos.x,pos.y,0) ;
    }
```
其中，materials序列化存储其中方块的材质球，在预制体界面拖动放入，创建后根据方块类型修改材质球。  
然后是改变位置的的方法：  
``` C#
    /// <summary>
    /// 改变位置
    /// </summary>
    /// <param name="pos"></param>
    public void ChangePos(Vector2Int pos)
    {
        this.pos = pos;
        UpdatePos();
    }
```
在游戏运行时，当方块停靠后，需要进行两个处理，首先将全满的行清除，然后将被清除的行上方的行下落，为此编写清除方法和下落方法：  
``` C#
    protected override void Awake()
    {
        base.Awake();

        Subscribe<int>("tetrisClearLine", Clear);
        Subscribe<int,int>("tetrisFallen", Fallen);
    }
    /// <summary>
    /// 清除某一行
    /// </summary>
    /// <param name="line"></param>
    private void Clear(int line)
    {
        if (gameObject.activeSelf&& line==pos.y)
        {
            pool.Store(gameObject);
        }
    }

    /// <summary>
    /// 某一行下落到另一行
    /// </summary>
    /// <param name="oldLine"></param>
    /// <param name="newLine"></param>
    private void Fallen(int oldLine,int newLine)
    {
        if (gameObject.activeSelf && oldLine == pos.y)
        {
            this.pos = new Vector2Int(pos.x, newLine);
            UpdatePos();
        }
    }
```
具体清除哪一行以及哪一行下落都是在管理类逻辑中判断，小方块控制类只需要执行即可。其中pool.Store方法就是对象池的存储方法。  
Subscribe方法是消息中心的注册监听方法，可以理解为收听了某个频道的电台广播，但是广播是否在找你需要自行判断，上面两个方法就是自己判断自己是否是需要改动的行。属于观察者设计模式。  
作为本文的替代，可以编写一个静态类：
``` C#
public static class TetrisDefine
{
    public static Action<int> tetrisClearLine;
    public static Action<int, int> tetrisFallen;
}

//将Subscribe<int>("tetrisClearLine", Clear);  改为TetrisDefine.tetrisClearLine+=Clear;
//将Subscribe<int,int>("tetrisFallen", Fallen);  改为TetrisDefine.tetrisFallen+=Fallen;
```
## 定义类  
接下来编写基础逻辑的定义类，为了对七种方块每种四个旋转状态的四个小块位置进行定义，在定义类中使用一个方法来获取
``` C#
/// <summary>
/// 俄罗斯方块定义
/// </summary>
public class TetrisDefine
{
    public static int baseWidth = 10;   //基础区域宽度
    public static int baseHeight = 20;  //基础区域高度
    public static int extraHeight = 24; //包含了生成区域的高度，最高的情况是生成了i，即20+4
    public static int blockNum = 4; //每个方块包含的小块数
    public static Vector2Int[] offsets; //小块偏移数组

    /// <summary>
    /// 静态构造函数,初始化小块偏移数组
    /// </summary>
    static TetrisDefine()
    {
        offsets = new Vector2Int[112];    //7种类型，每种四个旋转状态，每个状态四个小块，共7*4*4=112个变量
        int index = 0;
        //S
        offsets[index++] = new Vector2Int(-1, 0);
        offsets[index++] = new Vector2Int(0, 0);
        offsets[index++] = new Vector2Int(0, 1);
        offsets[index++] = new Vector2Int(1, 1);

        offsets[index++] = new Vector2Int(0, 2);
        offsets[index++] = new Vector2Int(0, 1);
        offsets[index++] = new Vector2Int(1, 1);
        offsets[index++] = new Vector2Int(1, 0);

        offsets[index++] = new Vector2Int(-1, 0);
        offsets[index++] = new Vector2Int(0, 0);
        offsets[index++] = new Vector2Int(0, 1);
        offsets[index++] = new Vector2Int(1, 1);

        offsets[index++] = new Vector2Int(0, 2);
        offsets[index++] = new Vector2Int(0, 1);
        offsets[index++] = new Vector2Int(1, 1);
        offsets[index++] = new Vector2Int(1, 0);
        
        //Z
        //......下略
    }

    /// <summary>
    /// 对每种方块的四种旋转状态定义其四个小块的位置
    /// </summary>
    /// <param name="blockType"></param>
    /// <param name="rotate">只能是0、1、2、3，分别代表初始状态和顺时针旋转了1、2、3次后的状态</param>
    /// <returns></returns>
    public static Vector2Int[] GetOffsets(BlockType blockType, int rotate)
    {
        Vector2Int[] result = new Vector2Int[4];
        int baseIndex = (int)blockType * 16 + rotate * 4;
        result[0] = offsets[baseIndex++];
        result[1] = offsets[baseIndex++];
        result[2] = offsets[baseIndex++];
        result[3] = offsets[baseIndex++];
        return result;
    }
}

```
为了格式更加易读，使用静态构造函数初始化偏移量。由于代码重复且长，上面只有类型S的初始化，略去了其他类型，其他类型格式一样，只有数值略有不同，保证能记录四个小块偏移位置的信息即可，GetOffsets方法就是用来获取小块偏移位置的方法  

## 游戏逻辑管理类  
有了小方块管理类，接下来就编写重点的游戏逻辑管理类。首先是对象池类  
``` C#
    /// <summary>
    /// GameObject对象池
    /// </summary>
    public class GameObjectPool 
    {
        private GameObject prefab;  //预制体
        private Queue<GameObject> queue;    //待使用的对象
        private Action<GameObject> resetAction; //返回池中后调用
        private Action<GameObject> initAction;  //取出时调用
        private Action<GameObject> firstInitAction; //首次生成时调用

        public GameObjectPool( GameObject prefab, Action<GameObject>
            ResetAction , Action<GameObject> InitAction = null, Action<GameObject> FirstInitAction = null)
        {
            this.prefab = prefab;
            queue = new Queue<GameObject>();
            resetAction = ResetAction;
            initAction = InitAction;
            firstInitAction = FirstInitAction;
        }

        /// <summary>
        /// 取出对象
        /// </summary>
        /// <returns></returns>
        public GameObject New()
        {
            if (queue.Count > 0)
            {
                GameObject t = queue.Dequeue();
                initAction?.Invoke(t);
                return t;
            }
            else
            {
                GameObject t = GameObject.Instantiate(prefab);
                firstInitAction?.Invoke(t);
                initAction?.Invoke(t);

                return t;
            }
        }

        /// <summary>
        /// 放回对象
        /// </summary>
        /// <param name="obj"></param>
        public void Store(GameObject obj)
        {
            if (queue.Contains(obj)==false)
            {
                resetAction(obj);
                queue.Enqueue(obj);
            }
        }
    }
```
然后是管理类中的使用：  
``` C#
    [SerializeField] private GameObject blockPrefab;    //方块预制体  
    private GameObjectPool blockPool;   //方块对象池
    protected override void Awake()
    {
        base.Awake();
        //......
        blockPool = new GameObjectPool(blockPrefab, (go) => go.SetActive(false), (go) => go.SetActive(true), (go) =>{ go.GetComponent<BlockController>().pool = blockPool;go.transform.SetParent(transform); }) ;
        //......
    }
```
使用逻辑很简单，取出时SetActive(true)，放回时SetActive(false)，首次取出时设置小方块管理类中的对象池属性，并将其transform节点至于管理类下。  
然后定义棋盘、当前控制对象数组以及一些基础变量：  
``` C#
    private bool[,] board; //只保存已经固定的方块
    private BlockController[] crtControl = new BlockController[4];   //当前控制的方块
    private int crtRotate; //当前控制方块的旋转状态
    private Vector2Int pointer;    //保存控制方块的指针位置，保证连续生成时的位置不会乱动
    private BlockType nextBlock;    //下一个方块类型
    private BlockType crtBlock;     //当前方块类型
    private bool isRunning; //是否正在运行
```
pointer是用来保存当前操作位置，当一个方块停靠后，如果强制从中间生成新方块，会使得操作非常不流畅，所以使用一个变量来记录上一次的位置，同时这个变量也是在获得到小块偏移后计算小块实际位置的基础位置。  
接着编写随机下一个方块的方法和生成新方块的方法，之所以有下一个方块变量，是为了之后制作下一个方块预览做准备。  
``` C# 
    /// <summary>
    /// 随机出下一个方块
    /// </summary>
    private void RandomNext()
    {
        nextBlock = (BlockType)Random.Range(0, 7);
    }   
    
    /// <summary>
    /// 生成新方块
    /// </summary>
    private void CreateNew()
    {
        crtRotate = 0;
        crtBlock = nextBlock;
        switch (crtBlock)
        {
            case BlockType.S:
                CorrectPointerPos(true, true);
                break;
            case BlockType.Z:
                CorrectPointerPos(true, true);
                break;
            case BlockType.J:
                CorrectPointerPos(true, false);
                break;
            case BlockType.L:
                CorrectPointerPos(false, true);
                break;
            case BlockType.I:
                CorrectPointerPos(false, false);
                break;
            case BlockType.O:
                CorrectPointerPos(false, true);
                break;
            case BlockType.T:
                CorrectPointerPos(true, true);
                break;
            default:
                Debug.LogError("下一个方块的类型错误");
                return;
        }

        var offsets = TetrisDefine.GetOffsets(crtBlock, crtRotate);
        for (int i = 0; i < TetrisDefine.blockNum; i++)
        {
            var go = blockPool.New();
            crtControl[i] = go.GetComponent<BlockController>();
            crtControl[i].Create(pointer + offsets[i], crtBlock);
        }

        RandomNext();
    }

    /// <summary>
    /// 纠正指针位置
    /// 比如，当上一个方块为I，在靠边时停靠，下一个方块为Z，如果指针位置不变的话方块会在边框外生成
    /// </summary>
    /// <param name="left">左侧是否需要留空</param>
    /// <param name="right">右侧是否需要留空</param>
    private void CorrectPointerPos(bool left, bool right)
    {
        int x = pointer.x;
        int y = 20;
        if (left && pointer.x == 0)
        {
            x = 1;
        }
        if (right && pointer.x == 9)
        {
            x = 8;
        }
        pointer = new Vector2Int(x, y);
    }
```
CorrectPointerPos方法有两个作用，一个是将指针移动至最上方，另一个则是防止新生成方块部分落在区域外。  
接下来编写移动当前控制对象的方法：  
``` C#
    /// <summary>
    /// 移动方块
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="checkParking">是否检查停靠</param>
    private void Move(Vector2Int offset,bool checkParking)
    {
        var targetPointer = pointer + offset ;

        var blockOffsets = TetrisDefine.GetOffsets(crtBlock, crtRotate);
        bool parking = false;
        for (int i = 0; i < TetrisDefine.blockNum; i++)
        {
            var targetPoint = blockOffsets[i] + targetPointer;
            if (CheckNewPos(targetPoint.x, targetPoint.y) == false)
            {
                parking = true;
                break;
            }
        }

        if (parking)
        {
            if (checkParking)
            {
                //停靠则使用当前位置进行处理，然后创建新对象
                for (int i = 0; i < TetrisDefine.blockNum; i++)
                {
                    var point = blockOffsets[i] + pointer;

                    board[point.x, point.y] = true;
                }
                ParkingCheck();
                CreateNew();
            }
        }
        else
        {
            //未停靠则正常移动
            for (int i = 0; i < TetrisDefine.blockNum; i++)
            {
                crtControl[i].ChangePos(blockOffsets[i] + targetPointer);
            }

            pointer = targetPointer;
        }
    }

    /// <summary>
    /// 检查新位置是否合法（是否超出边界或者已经有方块）
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private bool CheckNewPos(int x, int y)
    {
        if (x < 0 || x > 9 || y < 0)  //不检测上方
        {
            return false;
        }
        if (board[x, y] == true)
        {
            return false;
        }
        return true;
    }
```
其中传入参数offset代表位移，比如左移就是(-1,0)，checkParking参数代表是否检测停靠，游戏中自动下移时遇到不能继续移动的情况会将方块停靠，但是左右移动时就不会，通过参数来传递这次移动是否判断停靠。TetrisDefine就是用来定义的静态类。  
ParkingCheck方法负责停靠后检测是否有满行以及相关处理：  
``` C#
    /// <summary>
    /// 当前操作对象停靠后进行一次检测，消除满了的行数
    /// </summary>
    private void ParkingCheck()
    {
        bool[] full = new bool[TetrisDefine.baseHeight];
        
        int fullLineCount = 0;
        //检测满了的行数
        for (int y = 0; y < TetrisDefine.baseHeight; y++)
        {
            bool havaHole = false;
            for (int x = 0; x < TetrisDefine.baseWidth; x++)
            {
                if (board[x, y] == false)
                {
                    havaHole = true;
                    break;
                }
            }
            if (havaHole)
            {
                full[y] = false;
            }
            else
            {
                fullLineCount++;
                full[y] = true;
            }
        }

         if (fullLineCount>0)
        {
            //行下坠
            int baseLine = 0;
            for (int y = 0; y < TetrisDefine.baseHeight; y++)
            {
                if (full[y])
                {
                    for (int x = 0; x < TetrisDefine.baseWidth; x++)
                    {
                        board[x, y] = false;
                    }
                    Publish("tetrisClearLine", y);
                }
                else
                {
                    if (y != baseLine)
                    {
                        for (int x = 0; x < TetrisDefine.baseWidth; x++)
                        {
                            board[x, baseLine] = board[x, y];
                            board[x, y] = false;
                        }
                        Publish("tetrisFallen", y, baseLine);
                    }

                    baseLine++;
                }
            }
        }

        //检查区域外的位置，存在方块则意味着达成失败条件
        for (int y = TetrisDefine.baseHeight; y < TetrisDefine.extraHeight; y++)
        {
            for (int x = 0; x < TetrisDefine.baseWidth; x++)
            {
                if (board[x, y])
                {
                    GameOver();
                    return;
                }
            }
        }
    }

    /// <summary>
    /// 达成失败条件
    /// </summary>
    private void GameOver()
    {
        isRunning = false;
        StopAllCoroutines();

        Debug.Log("游戏结束");
    }
```
Publish方法和小方块控制类中的Subscribe方法相对应，可以理解为发布广播。  
同样的，作为替代，可以将Publish("tetrisClearLine", y);  改为TetrisDefine.tetrisClearLine?.Invoke(y);  
将Publish("tetrisFallen", y, baseLine);  改为TetrisDefine.tetrisFallen.Invoke(y,baseLine);  
移动运动后，接下来就是旋转运动：  
``` C#
    /// <summary>
    /// 旋转方块
    /// </summary>
    private void RotateBlock()
    {
        int targetRotate = crtRotate + 1;
        if (targetRotate > 3)
        {
            targetRotate = 0;
        }
        var newPoints = TetrisDefine.GetOffsets(crtBlock, targetRotate);

        for (int i = 0; i < TetrisDefine.blockNum; i++)
        {
            var newPoint = newPoints[i] + pointer;
            if (CheckNewPos(newPoint.x, newPoint.y) == false)
            {
                return;
            }
        }
        crtRotate = targetRotate;
        for (int i = 0; i < TetrisDefine.blockNum; i++)
        {
            crtControl[i].ChangePos(newPoints[i] + pointer);
        }
    }
```
因为旋转一定是玩家操作的，当旋转后的目标位置不合法时，则直接忽视这次操作。  
剩下的游戏逻辑就是自动下落：  
``` C# 
    private float dropInterval; //下落间隔
    /// <summary>
    /// 运行，负责不停的调用下坠方法
    /// </summary>
    /// <returns></returns>
    private IEnumerator Running()
    {
        float interval = dropInterval;
        WaitForSeconds wfs = new WaitForSeconds(interval);
        while (true)
        {
            if (interval != dropInterval)
            {
                interval = dropInterval;
                wfs = new WaitForSeconds(interval);
            }
            Move(new Vector2Int(0, -1), true);
            yield return wfs;
        }
    } 
    
    /// <summary>
    /// 当前操作对象下坠
    /// </summary>
    private void Drop(bool checkParking)
    {
        Move(new Vector2Int(0, -1), checkParking);
    }
```
下落间隔在运行后会随着进行动态更改，越变越快，所以每次下落都要检测下落间隔是否修改，修改则创建新的WaitForSeconds。  
接下来时玩家输入逻辑：  
``` C#
    [SerializeField] private float inputInterval = 0.1f;   //输入间隔
    private float inputTimer;   //输入计时器   

    private void Update()
    {
        if (isRunning)
        {
            inputTimer += Time.deltaTime;
            if (inputTimer > inputInterval)
            {
                if (Input.GetKey(KeyCode.S))
                {
                    inputTimer = 0;
                    Move(new Vector2Int(0, -1), false);
                }
                if (Input.GetKey(KeyCode.D))
                {
                    inputTimer = 0;
                    Move(new Vector2Int(1, 0), false);
                }
                if (Input.GetKey(KeyCode.A))
                {
                    inputTimer = 0;
                    Move(new Vector2Int(-1, 0), false);
                }
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                RotateBlock();
            }
        }
    }
```
左右平移和手动下落是会存在按着不动连续输入的情况，所以使用Input.GetKey获取输入并使用计时器防止输入过快，旋转操作则是属于精确操作，按一次触发一次。  
最后就是开始游戏的初始化方法了：  
``` C#
    /// <summary>
    /// 开始
    /// </summary>
    /// <param name="difficulty">难度</param>
    public void OnStart(int difficulty)
    {
        Debug.Log("start");
        if (isRunning)
        {
            return;
        }
        isRunning = true;
        if (difficulty < 0)
        {
            difficulty = 0;
        }
        else if (difficulty > 9)
        {
            difficulty = 9;
        }
        pointer = new Vector2Int(5, 21);
        dropInterval = 0.1f + 0.1f * (9 - difficulty);
        board = new bool[TetrisDefine.baseWidth, TetrisDefine.extraHeight];
        RandomNext();   //一开始需要随机出下一个方块类型用于第一次创建
        CreateNew();
        StartCoroutine(Running());
    }
```
这里使用了简单的难度逻辑，难度数值越高越简单，每级难度增加0.1秒的自动下坠间隔。  
此时在运行后调用OnStart方法，就可以开始游戏了。  
此时区域边缘不明显，随意使用方块搭建一个边框  
``` C#
    [SerializeField] private GameObject whiteCube;    //边框方块预制体，就是基础的Cube
    protected override void Awake()
    {
        base.Awake();
        //......
        Borders();
    }

    /// <summary>
    /// 创建边框
    /// </summary>
    private void Borders()
    {
        for (int i = -1; i < TetrisDefine.baseWidth+1; i++)
        {
            var go = Instantiate(whiteCube, transform);
            go.transform.position = new Vector3(i, -1, 0);
        }
        for (int i = 0; i < TetrisDefine.baseHeight; i++)
        {
            var go = Instantiate(whiteCube, transform);
            go.transform.position = new Vector3(-1, i, 0);
            go = Instantiate(whiteCube, transform);
            go.transform.position = new Vector3(TetrisDefine.baseWidth, i, 0);
        }
    }
```
上方不封顶，因为是从方块是从上方落下的。此时效果如图：  
![边框]()   

## UI界面和分数
首先编写预览下一个方块的代码

``` C#
    BlockController[] previews = new BlockController[TetrisDefine.blockNum];

    private void RandomNext()
    {
        //......
        ChangePreview();
    }

    
    private void ChangePreview()
    {
        foreach (var item in previews)
        {
            if (item!=null)
            {
                blockPool.Store(item.gameObject);
            }
        }
        var offsets = TetrisDefine.GetOffsets(nextBlock, 0);
        for (int i = 0; i < TetrisDefine.blockNum; i++)
        {
            var go = blockPool.New();
            previews[i] = go.GetComponent<BlockController>();
            previews[i].Create(new Vector2Int(13,13) + offsets[i], nextBlock);
        }
    }
```
13,13的位置就是原始区域的右边外面的上半部分
然后在运行后找出对应的位置并在UI上开一个洞，如下图  
![在UI中开洞]()   
随后处理分数，首先在游戏开始时将分数重置,并在消除行时增加分数  
``` C#
    private void OnStart(int difficulty)
    {
        Publish("scoreReset");
        //......
    }
    
    private void ParkingCheck()
     {
        //......
        if (fullLineCount>0)
        {
            Publish<int>("scoreAdd", (int)(Mathf.Pow(2, fullLineCount+1)*100)/(int)( dropInterval*10));
            //......
        }
        //......
    }
```
随后在负责处理UI的管理类中编写接收信号并处理的代码  
``` C#
    [SerializeField] private TextMeshProUGUI txt_score; 
    private int score;
    protected override void Awake()
    {
        base.Awake();
        Subscribe("scoreReset", ScoreReset);
        Subscribe<int>("scoreAdd", ScoreAdd);
    }

    private void ScoreReset()
    {
        score = 0;
        UpdateScoreText();
    }

    private void ScoreAdd(int score)
    {
        this.score += score;
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        txt_score.text= score.ToString();
    }
```
TextMeshProUGUI是TextMeshPro包中的组件，使用方法和UGUI中的Text组件一致。 
随后在界面中制作对应的UI并设置引用，则可以正常的显示分数了。
![有分数的界面]()  

## 总结
界面上目前只是实现了基础功能，且信息不全。unity的Random算法过于简单，仍然不够随机，需要使用更加高级的随机算法随机方块类型，本文就到这了，项目后续会继续优化，项目地址为https://github.com/DonnieBean/NonsensicalGame。  

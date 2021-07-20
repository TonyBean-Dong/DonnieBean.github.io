设置锚点
```
public RectTransform panelRectTransform;
 
 // Something like this.
 void Start()
 {
    panelRectTransform.anchorMin = new Vector2(1, 0);
    panelRectTransform.anchorMax = new Vector2(0, 1);
    panelRectTransform.pivot = new Vector2(0.5f, 0.5f);
 }
 
```
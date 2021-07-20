using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using XLua;
using System;

public class LuaManager : MonoBehaviour
{
    public static LuaManager _Instance;

    internal static LuaEnv luaEnv = new LuaEnv(); //all lua behaviour shared one luaenv only!
    internal static float lastGCTime = 0;
    internal const float GCInterval = 1;//1 second 

    private void Awake()
    {
        _Instance = this;
    }


    private void Update()
    {
        if (Time.time - lastGCTime > GCInterval)
        {
            luaEnv.Tick();
            lastGCTime = Time.time;
        }
    }

    public void CreateLua(string luaText)
    {
        GameObject gp = new GameObject("lua instance");
        LuaInstance li= gp.AddComponent<LuaInstance>();
        li.Init(luaText);
    }
}

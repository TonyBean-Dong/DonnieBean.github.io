using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

[LuaCallCSharp]
public class LuaInstance : MonoBehaviour
{

    private Action luaUpdate;
    private Action luaOnDestroy;

    private LuaTable scriptEnv;

    private LuaEnv luaEnv;


    public void Init(string luaScript)
    {
        luaEnv = LuaManager.luaEnv;
        scriptEnv = luaEnv.NewTable();

        // 为每个脚本设置一个独立的环境，可一定程度上防止脚本间全局变量、函数冲突
        LuaTable meta = luaEnv.NewTable();
        meta.Set("__index", luaEnv.Global);
        scriptEnv.SetMetaTable(meta);
        meta.Dispose();

        scriptEnv.Set("self", this);
        luaEnv.DoString(luaScript, "LuaTestScript", scriptEnv);

        Action luaAwake = scriptEnv.Get<Action>("awake");
        scriptEnv.Get("update", out luaUpdate);
        scriptEnv.Get("ondestroy", out luaOnDestroy);

        luaAwake?.Invoke();
    }

    void Update()
    {
        luaUpdate?.Invoke();
    }

    void OnDestroy()
    {
        luaOnDestroy?.Invoke();
        luaOnDestroy = null;
        luaUpdate = null;
        scriptEnv?.Dispose();
    }
}

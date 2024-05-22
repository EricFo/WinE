using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager
{
    /// <summary>
    /// 接口，只负责存在在字典中
    /// </summary>
    public interface IRegisterations
    {
        
    }

    /// <summary>
    /// 多个注册
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Registerations<T> : IRegisterations
    {
        /// <summary>
        /// 委托本身可以一对多注册
        /// </summary>
        public Action<T> OnReceives = obj => { };
    }
    
    /// <summary>
    /// 事件字典
    /// </summary>
    private static Dictionary<Type, IRegisterations> mTypeEventDic = new Dictionary<Type, IRegisterations>();

    
    /// <summary>
    /// 注册事件
    /// </summary>
    /// <param name="onReceive"></param>
    /// <typeparam name="T"></typeparam>
    public static void Register<T>(Action<T> onReceive)
    {
        var type = typeof(T);
        IRegisterations registerations = null;
        if (mTypeEventDic.TryGetValue(type,out registerations))
        {
            var reg = registerations as Registerations<T>;
            reg.OnReceives += onReceive;
        }
        else
        {
            var reg = new Registerations<T>();
            reg.OnReceives += onReceive;
            mTypeEventDic.Add(type,reg);
        }
    }

    /// <summary>
    /// 注销事件
    /// </summary>
    /// <param name="onReceive"></param>
    /// <typeparam name="T"></typeparam>
    public static void UnRegister<T>(Action<T> onReceive)
    {
        var type = typeof(T);
        IRegisterations registerations = null;
        if (mTypeEventDic.TryGetValue(type,out registerations))
        {
            var reg = registerations as Registerations<T>;
            reg.OnReceives -= onReceive;
        }
    }

    /// <summary>
    /// 发送事件
    /// </summary>
    /// <param name="t"></param>
    /// <typeparam name="T"></typeparam>
    public static void Send<T>(T t)
    {
        var type = typeof(T);
        IRegisterations registerations = null;
        if (mTypeEventDic.TryGetValue(type,out registerations))
        {
            var reg = registerations as Registerations<T>;
            reg.OnReceives(t);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Resources.Load
/// 나중에 AssetBundle로 변경할 예정
/// </summary>

public class ResourceManager
{
    public static Object Load(string path)
    {
        return Resources.Load(path);
    }
    
    public static GameObject LoadAndInstantiate(string path)
    {
        Object source = Load(path);
        if(source == null)
        {
            return null;
        }
        return GameObject.Instantiate(source) as GameObject;
    }
}

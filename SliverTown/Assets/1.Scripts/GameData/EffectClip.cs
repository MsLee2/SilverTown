using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 이펙트 프리팹과 경로, 타입 등의 속성 데이터
/// 프리팹 사전로딩 기능 - 풀링을 위한 기능
/// 이펙트 인스턴스 기능 - 풀링과 연계 가능
/// </summary>
public class EffectClip 
{
    public int realId = 0; //속성은 같지만 다른 클립들 구별

    public EffectType effectType = EffectType.NORMAL; //노멀타입
    public GameObject effectPrefab = null; //이펙트 프리팹
    public string effectName = string.Empty; //이펙트 이름
    public string effectPath = string.Empty; //이펙트 경로
    public string effectFullPath = string.Empty;

    public EffectClip() { }

    public void PreLoad()
    {
        this.effectFullPath = effectPath + effectName;
        if(effectFullPath != string.Empty && this.effectPrefab == null) //프리로딩 한번만
        {
            this.effectPrefab = ResourceManager.Load(effectFullPath) as GameObject;
        }
    }
    public void RealeaseEffect()
    {
        if (this.effectPrefab != null)
        {
            this.effectPrefab = null;
        }
    }

    /// <summary>
    /// 원하는 위치에 내가 원하는 이펙트 인스턴스 => Pos
    /// <returns></returns>
    public GameObject Instantiate(Vector3 Pos) //내가 원하는 포지션에 인스턴스
    {
        if(this.effectPrefab == null)
        {
            this.PreLoad();
        }
        if(this.effectPrefab != null)
        {
            GameObject effect = GameObject.Instantiate(effectPrefab, Pos, Quaternion.identity); 
            return effect;
        }
        return null;
    }


}

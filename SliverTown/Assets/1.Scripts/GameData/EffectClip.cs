using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// ����Ʈ �����հ� ���, Ÿ�� ���� �Ӽ� ������
/// ������ �����ε� ��� - Ǯ���� ���� ���
/// ����Ʈ �ν��Ͻ� ��� - Ǯ���� ���� ����
/// </summary>
public class EffectClip 
{
    public int realId = 0; //�Ӽ��� ������ �ٸ� Ŭ���� ����

    public EffectType effectType = EffectType.NORMAL; //���Ÿ��
    public GameObject effectPrefab = null; //����Ʈ ������
    public string effectName = string.Empty; //����Ʈ �̸�
    public string effectPath = string.Empty; //����Ʈ ���
    public string effectFullPath = string.Empty;

    public EffectClip() { }

    public void PreLoad()
    {
        this.effectFullPath = effectPath + effectName;
        if(effectFullPath != string.Empty && this.effectPrefab == null) //�����ε� �ѹ���
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
    /// ���ϴ� ��ġ�� ���� ���ϴ� ����Ʈ �ν��Ͻ� => Pos
    /// <returns></returns>
    public GameObject Instantiate(Vector3 Pos) //���� ���ϴ� �����ǿ� �ν��Ͻ�
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

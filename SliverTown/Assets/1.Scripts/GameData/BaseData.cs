using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �������� �⺻ Ŭ������
/// �������� �����͸� �����Ұ��ε� ���� �̸��� ����
/// �������� ������ �̸��� ��� ����Ʈ
/// </summary>


public class BaseData : ScriptableObject
{
    public const string dataDirectory = "/ResourcesData/Resources/Data/";
    public string[] names = null; //�̸� ����Ʈ �ʱ�ȭ

    public BaseData() { }
    
    public int GetDataCount()
    {
        int retValue = 0;

        if(this.names != null) //����Ʈ�� �о� ������
        {
            retValue = this.names.Length;
        }

        return retValue;
    }

    /// <summary>
    /// ���� ����ϱ� ���� �̸� ��� ���� �Լ�
    /// </summary>
    public string[] GetNameList(bool showID, string filterWord = "")
    {
        string[] retList = new string[0];

        if(this.names == null)
        {
            return retList;
        }

        retList = new string[this.names.Length];

        for(int i = 0; i < this.names.Length; i++)
        {
            if(filterWord != "")
            {
                if(names[i].ToLower().Contains(filterWord.ToLower()) == false)
                {
                    continue;
                }

            }
            if(showID)
            {
                retList[i] = i.ToString() + " : " + this.names[i];
            }
            else
            {
                retList[i] = this.names[i];
            }
        }

        return retList;
    }

    public virtual int AddData(string newName) //������ �߰�
    {
        return GetDataCount();
    }

    public virtual void ReMoveData(int index) //������ ����
    {

    }

    public virtual void Copy(int index) // ������ ����
    {

    }

}

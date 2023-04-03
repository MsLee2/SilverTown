using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 데이터의 기본 클래스임
/// 공통적인 데이터를 관리할거인데 현재 이름만 있음
/// 데이터의 개수와 이름의 목록 리스트
/// </summary>


public class BaseData : ScriptableObject
{
    public const string dataDirectory = "/ResourcesData/Resources/Data/";
    public string[] names = null; //이름 리스트 초기화

    public BaseData() { }
    
    public int GetDataCount()
    {
        int retValue = 0;

        if(this.names != null) //리스트를 읽어 왔으면
        {
            retValue = this.names.Length;
        }

        return retValue;
    }

    /// <summary>
    /// 툴에 출력하기 위한 이름 목록 생성 함수
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

    public virtual int AddData(string newName) //데이터 추가
    {
        return GetDataCount();
    }

    public virtual void ReMoveData(int index) //데이터 삭제
    {

    }

    public virtual void Copy(int index) // 데이터 복사
    {

    }

}

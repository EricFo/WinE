using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class ExtensionTools
{

    /// <summary>
    /// list随机排序
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    public static void ListSortRandom<T>(this List<T> list)
    {
        int randomIndex;
        for (int i = list.Count - 1; i >= 0; i--)
        {
            randomIndex = Random.Range(0, i);
            (list[randomIndex], list[i]) = (list[i], list[randomIndex]);
        }
    }

    /// <summary>
    /// 获取随机对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    public static T GetRandomItem<T>(this List<T> list)
    {
        T item = list[Random.Range(0, list.Count)];
        return item;
    }

    /// <summary>
    /// list<int>遍历输出为字符串
    /// </summary>
    /// <param name="ints"></param>
    /// <returns></returns>
    public static string ListIntToString(this List<int> ints)
    {
        string str = "";
        foreach (var item in ints)
        {
            str += item.ToString();
        }
        return str;
    }

    /// <summary>
    /// 分割整数
    /// </summary>
    /// <param name="value"></param>
    /// <param name="num0fParts">分割份数</param>
    /// <param name="factor">分割块公因数</param>
    /// <returns></returns>
    public static List<int> SplitValue(this int value, int num0fParts, int factor = 1)
    {
        value /= factor;
        List<int> result = new List<int>();
        for (int i = 0; i < num0fParts; i++)
        {
            result.Add(0);
        }
        if (value >= num0fParts)
        {
            for (int i = 0; i < num0fParts; i++)
            {
                result[i] += 1 * factor;
            }
            value -= num0fParts;
        }
        for (int i = 0; i < value; i++)
        {
            result[Random.Range(0, num0fParts)] += 1 * factor;
        }
        return result;
    }

    public static List<List<T>> SplitList<T>(this List<T> value,int numOfParts)
    {
        List<List<T>> result = new List<List<T>>(numOfParts);
        for (int i = 0; i < numOfParts; i++)
        {
            result.Add(new List<T>());
        }
        foreach (var item in value)
        {
            result[Random.Range(0,result.Count)].Add(item);
        }

        return result;
    }

    public static int ListIntSum(this List<int> value)
    {
        int sum = 0;
        foreach (var item in value)
        {
            sum += item;
        }

        return sum;
    }

}

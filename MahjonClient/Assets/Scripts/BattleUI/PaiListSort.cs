using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PaiCompare : IComparer<Pai>
{
    //ComparetTo:大于 1； 等于 0； 小于 -1；  
    public int Compare(Pai data1, Pai data2)
    {
        int result = data1.Status.CompareTo(data2.Status);
        if (result == 0)
        {
            result = data1.Id.CompareTo(data2.Id);
        }
        return result;
    }
}

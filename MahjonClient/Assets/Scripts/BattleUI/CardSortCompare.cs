//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;


//public class InHandCardSort : IComparer<Pai>
//{
//    public pb.CardInfo rightCard;
//    public int lackType;

//    public int Compare(Pai data1, Pai data2)
//    {
//        int result = 0;
//        if (rightCard != null)
//        {
//            if (data1.OID == rightCard.CardOid && data2.OID != rightCard.CardOid)
//            {
//                result = 1;
//            }
//            else if (data1.OID != rightCard.CardOid && data2.OID == rightCard.CardOid)
//            {
//                result = -1;
//            }
//        }
//        if (result == 0)
//        {
//            int type1 = Mathf.FloorToInt(data1.Id / 10) + 1;
//            int type2 = Mathf.FloorToInt(data2.Id / 10) + 1;
//            if (type1 != lackType && type2 == lackType)
//            {
//                result = -1;
//            }
//            else if (type1 == lackType && type2 != lackType)
//            {
//                result = 1;
//            }
//            else
//            {
//                result = data1.Id.CompareTo(data2.Id);
//            }
//        }
//        return result;
//    }
//}

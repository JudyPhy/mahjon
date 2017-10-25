using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardPos
{
    private static Vector3 InhandStartPos0 = new Vector3(-453, 80, 0);
    private static Vector3 InhandStartPos1 = new Vector3(-160, -155, 0);
    private static Vector3 InhandStartPos2 = new Vector3(228, -65, 0);
    private static Vector3 InhandStartPos3 = new Vector3(160, 210, 0);
    public static Vector3 InhandStartPos(int sideIndex)
    {
        switch (sideIndex)
        {
            case 0: return InhandStartPos0;
            case 1: return InhandStartPos1;
            case 2: return InhandStartPos2;
            case 3: return InhandStartPos3;
            default: return Vector3.zero;
        }
    }

    private static Vector3 InhandSpace0 = new Vector3(75, 0, 0);
    private static Vector3 InhandSpace1 = new Vector3(0, 27, 0);
    private static Vector3 InhandSpace2 = new Vector3(-38, 0, 0);
    private static Vector3 InhandSpace3 = new Vector3(0, -28, 0);
    public static Vector3 InhandSpace(int sideIndex)
    {
        switch (sideIndex)
        {
            case 0: return InhandSpace0;
            case 1: return InhandSpace1;
            case 2: return InhandSpace2;
            case 3: return InhandSpace3;
            default: return Vector3.zero;
        }
    }

    private static Vector3 PGStartPos0 = new Vector3(580, 80, 0);
    private static Vector3 PGStartPos1 = new Vector3(-160, 200, 0);
    private static Vector3 PGStartPos2 = new Vector3(-228, -65, 0);
    private static Vector3 PGStartPos3 = new Vector3(160, -210, 0);
    public static Vector3 PGStartPos(int sideIndex)
    {
        switch (sideIndex)
        {
            case 0: return PGStartPos0;
            case 1: return PGStartPos1;
            case 2: return PGStartPos2;
            case 3: return PGStartPos3;
            default: return Vector3.zero;
        }
    }

    private static Vector3 PPGGSpace0 = new Vector3(75, 0, 0);
    private static Vector3 PPGGSpace1 = new Vector3(0, 27, 0);
    private static Vector3 PPGGSpace2 = new Vector3(-38, 0, 0);
    private static Vector3 PPGGSpace3 = new Vector3(0, -28, 0);
    public static Vector3 PPGGSpace(int sideIndex)
    {
        switch (sideIndex)
        {
            case 0: return PGStartPos0;
            case 1: return PGStartPos1;
            case 2: return PGStartPos2;
            case 3: return PGStartPos3;
            default: return Vector3.zero;
        }
    }

    private static Vector3 ExchangeStartPos0 = new Vector3(65, 0, 0);
    private static Vector3 ExchangeStartPos1 = new Vector3(-215, -42, 0);
    private static Vector3 ExchangeStartPos2 = new Vector3(54, -135, 0);
    private static Vector3 ExchangeStartPos3 = new Vector3(215, 42, 0);
    public static Vector3 ExchangeStartPos(int sideIndex)
    {
        switch (sideIndex)
        {
            case 0: return ExchangeStartPos0;
            case 1: return ExchangeStartPos1;
            case 2: return ExchangeStartPos2;
            case 3: return ExchangeStartPos3;
            default: return Vector3.zero;
        }
    }

    private static Vector3 ExchangeStartSpace0 = new Vector3(65, 0, 0);
    private static Vector3 ExchangeStartSpace1 = new Vector3(0, 42, 0);
    private static Vector3 ExchangeStartSpace2 = new Vector3(-54, 0, 0);
    private static Vector3 ExchangeStartSpace3 = new Vector3(0, -42, 0);
    public static Vector3 ExchangeStartSpace(int sideIndex)
    {
        switch (sideIndex)
        {
            case 0: return ExchangeStartSpace0;
            case 1: return ExchangeStartSpace1;
            case 2: return ExchangeStartSpace2;
            case 3: return ExchangeStartSpace3;
            default: return Vector3.zero;
        }
    }

    private static Vector3 ExchangeEndPos0 = new Vector3(-37, 214, 0);
    private static Vector3 ExchangeEndPos1 = new Vector3(-285, -28, 0);
    private static Vector3 ExchangeEndPos2 = new Vector3(36, -200, 0);
    private static Vector3 ExchangeEndPos3 = new Vector3(285, 28, 0);
    public static Vector3 ExchangeEndPos(int sideIndex)
    {
        switch (sideIndex)
        {
            case 0: return ExchangeEndPos0;
            case 1: return ExchangeEndPos1;
            case 2: return ExchangeEndPos2;
            case 3: return ExchangeEndPos3;
            default: return Vector3.zero;
        }
    }

    private static Vector3 ExchangeEndSpace0 = new Vector3(37, 0, 0);
    private static Vector3 ExchangeEndSpace1 = new Vector3(0, 28, 0);
    private static Vector3 ExchangeEndSpace2 = new Vector3(-36, 0, 0);
    private static Vector3 ExchangeEndSpace3 = new Vector3(0, -28, 0);
    public static Vector3 ExchangeEndSpace(int sideIndex)
    {
        switch (sideIndex)
        {
            case 0: return ExchangeEndSpace0;
            case 1: return ExchangeEndSpace1;
            case 2: return ExchangeEndSpace2;
            case 3: return ExchangeEndSpace3;
            default: return Vector3.zero;
        }
    }

    private static Vector3 ExchangeUpOffset0 = new Vector3(0, 25, 0);
    private static Vector3 ExchangeUpOffset1 = new Vector3(0, 10, 0);
    private static Vector3 ExchangeUpOffset2 = new Vector3(0, 10, 0);
    private static Vector3 ExchangeUpOffset3 = new Vector3(0, 10, 0);
    public static Vector3 ExchangeUpOffset(int sideIndex)
    {
        switch (sideIndex)
        {
            case 0: return ExchangeUpOffset0;
            case 1: return ExchangeUpOffset1;
            case 2: return ExchangeUpOffset2;
            case 3: return ExchangeUpOffset3;
            default: return Vector3.zero;
        }
    }

    private static Vector3 DiscardStartPos0 = new Vector3(-195, 255, 0);
    private static Vector3 DiscardStartPos1 = new Vector3(-330, -150, 0);
    private static Vector3 DiscardStartPos2 = new Vector3(185, -260, 0);
    private static Vector3 DiscardStartPos3 = new Vector3(320, 180, 0);
    public static Vector3 DiscardStartPos(int sideIndex)
    {
        switch (sideIndex)
        {
            case 0: return DiscardStartPos0;
            case 1: return DiscardStartPos1;
            case 2: return DiscardStartPos2;
            case 3: return DiscardStartPos3;
            default: return Vector3.zero;
        }
    }

    private static Vector3 DiscardSpaceX0 = new Vector3(44, 0, 0);
    private static Vector3 DiscardSpaceX1 = new Vector3(0, 32, 0);
    private static Vector3 DiscardSpaceX2 = new Vector3(-37, 0, 0);
    private static Vector3 DiscardSpaceX3 = new Vector3(0, -32, 0);
    public static Vector3 DiscardSpaceX(int sideIndex)
    {
        switch (sideIndex)
        {
            case 0: return DiscardSpaceX0;
            case 1: return DiscardSpaceX1;
            case 2: return DiscardSpaceX2;
            case 3: return DiscardSpaceX3;
            default: return Vector3.zero;
        }
    }

    private static Vector3 DiscardSpaceY0 = new Vector3(0, -52, 0);
    private static Vector3 DiscardSpaceY1 = new Vector3(47, 0, 0);
    private static Vector3 DiscardSpaceY2 = new Vector3(0, 43, 0);
    private static Vector3 DiscardSpaceY3 = new Vector3(-47, 0, 0);
    public static Vector3 DiscardSpaceY(int sideIndex)
    {
        switch (sideIndex)
        {
            case 0: return DiscardSpaceY0;
            case 1: return DiscardSpaceY1;
            case 2: return DiscardSpaceY2;
            case 3: return DiscardSpaceY3;
            default: return Vector3.zero;
        }
    }

    private static Vector3 DiscardAniStartPos0 = new Vector3(0, 150, 0);
    private static Vector3 DiscardAniStartPos1 = new Vector3(-205, 0, 0);
    private static Vector3 DiscardAniStartPos2 = new Vector3(0, -100, 0);
    private static Vector3 DiscardAniStartPos3 = new Vector3(200, 0, 0);
    public static Vector3 DiscardAniStartPos(int sideIndex)
    {
        switch (sideIndex)
        {
            case 0: return DiscardAniStartPos0;
            case 1: return DiscardAniStartPos1;
            case 2: return DiscardAniStartPos2;
            case 3: return DiscardAniStartPos3;
            default: return Vector3.zero;
        }
    }

}

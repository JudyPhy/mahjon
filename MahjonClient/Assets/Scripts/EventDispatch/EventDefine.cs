using System;
using System.Collections.Generic;

public class EventDefine
{
    public static string UpdateRoomMember = "UpdateRoomMember";
    public static string UpdateRoleInRoom = "UpdateRoleInRoom";
    public static string PlayGamePrepareAni = "PlayGamePrepareAni";

    public static string ShowBtnExchangeCard = "UpdateBtnExchangeCard";
    public static string ReExchangeCard = "ReExchangeCard";
    public static string UpdateAllCardsAfterExhchange = "UpdateCardInfoAfterExchange";

    public static string SelectLack = "SelectLack";
    public static string EnsureLack = "EnsureLack";
    public static string ShowLackCard = "ShowLackCard";

    public static string TurnToPlayer = "TurnToPlayer";
    public static string ChooseDiscard = "ChooseDiscard";
    public static string UnSelectOtherDiscard = "UnSelectOtherDiscard";
    public static string EnsureDiscard = "EnsureDiscard";

    public static string ProcHPG = "ProcHPG"; 
    public static string EnsureProcHPG = "EnsureProcHPG";
    public static string UpdateSelfGangCard = "UpdateSelfGangCard";

    public static string BroadcastDiscard = "BroadcastDiscard";
    public static string BroadcastProc = "BroadcastProc";

    public static string UpdateAllCardsList = "UpdateAllCardsList";




    public static string RobotProc = "SomePlayerPG";
    public static string EnsureProcPG = "EnsureProcPG";
    public static string SelfEnsureProc = "SelfEnsureProc";
    public static string ReplacePlayerCards = "ReplacePlayerCards";
    public static string GameOver = "GameOver";
}
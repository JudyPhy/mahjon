//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from: mahjon.proto
namespace pb
{
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"CardInfo")]
  public partial class CardInfo : global::ProtoBuf.IExtensible
  {
    public CardInfo() {}
    
    private int _CardId;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"CardId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int CardId
    {
      get { return _CardId; }
      set { _CardId = value; }
    }
    private int _PlayerId;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"PlayerId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int PlayerId
    {
      get { return _PlayerId; }
      set { _PlayerId = value; }
    }
    private pb.CardStatus _Status;
    [global::ProtoBuf.ProtoMember(3, IsRequired = true, Name=@"Status", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public pb.CardStatus Status
    {
      get { return _Status; }
      set { _Status = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"RoleInfo")]
  public partial class RoleInfo : global::ProtoBuf.IExtensible
  {
    public RoleInfo() {}
    
<<<<<<< HEAD
    private int _oid;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"oid", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
=======
    private pb.BattleSide _side;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"side", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public pb.BattleSide side
    {
      get { return _side; }
      set { _side = value; }
    }
    private int _oid;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"oid", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
>>>>>>> 52ad724b41d9e08573258ee6687202786d75ae50
    public int oid
    {
      get { return _oid; }
      set { _oid = value; }
    }
    private string _nickName;
<<<<<<< HEAD
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"nickName", DataFormat = global::ProtoBuf.DataFormat.Default)]
=======
    [global::ProtoBuf.ProtoMember(3, IsRequired = true, Name=@"nickName", DataFormat = global::ProtoBuf.DataFormat.Default)]
>>>>>>> 52ad724b41d9e08573258ee6687202786d75ae50
    public string nickName
    {
      get { return _nickName; }
      set { _nickName = value; }
    }
    private string _headIcon;
<<<<<<< HEAD
    [global::ProtoBuf.ProtoMember(3, IsRequired = true, Name=@"headIcon", DataFormat = global::ProtoBuf.DataFormat.Default)]
=======
    [global::ProtoBuf.ProtoMember(4, IsRequired = true, Name=@"headIcon", DataFormat = global::ProtoBuf.DataFormat.Default)]
>>>>>>> 52ad724b41d9e08573258ee6687202786d75ae50
    public string headIcon
    {
      get { return _headIcon; }
      set { _headIcon = value; }
    }
    private int _lev;
<<<<<<< HEAD
    [global::ProtoBuf.ProtoMember(4, IsRequired = true, Name=@"lev", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
=======
    [global::ProtoBuf.ProtoMember(5, IsRequired = true, Name=@"lev", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
>>>>>>> 52ad724b41d9e08573258ee6687202786d75ae50
    public int lev
    {
      get { return _lev; }
      set { _lev = value; }
    }
    private bool _isOwner;
<<<<<<< HEAD
    [global::ProtoBuf.ProtoMember(5, IsRequired = true, Name=@"isOwner", DataFormat = global::ProtoBuf.DataFormat.Default)]
=======
    [global::ProtoBuf.ProtoMember(6, IsRequired = true, Name=@"isOwner", DataFormat = global::ProtoBuf.DataFormat.Default)]
>>>>>>> 52ad724b41d9e08573258ee6687202786d75ae50
    public bool isOwner
    {
      get { return _isOwner; }
      set { _isOwner = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"C2GSEnterGame")]
  public partial class C2GSEnterGame : global::ProtoBuf.IExtensible
  {
    public C2GSEnterGame() {}
    
    private int _playerId;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"playerId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int playerId
    {
      get { return _playerId; }
      set { _playerId = value; }
    }
    private pb.GameMode _mode;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"mode", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public pb.GameMode mode
    {
      get { return _mode; }
      set { _mode = value; }
    }
    private int _roomId = default(int);
    [global::ProtoBuf.ProtoMember(3, IsRequired = false, Name=@"roomId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int roomId
    {
      get { return _roomId; }
      set { _roomId = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"GS2CUpdateRoomInfo")]
  public partial class GS2CUpdateRoomInfo : global::ProtoBuf.IExtensible
  {
    public GS2CUpdateRoomInfo() {}
    
    private readonly global::System.Collections.Generic.List<pb.RoleInfo> _players = new global::System.Collections.Generic.List<pb.RoleInfo>();
    [global::ProtoBuf.ProtoMember(1, Name=@"players", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<pb.RoleInfo> players
    {
      get { return _players; }
    }
  
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"GS2CDealCard")]
  public partial class GS2CDealCard : global::ProtoBuf.IExtensible
  {
    public GS2CDealCard() {}
    
    private readonly global::System.Collections.Generic.List<pb.CardInfo> _CardList = new global::System.Collections.Generic.List<pb.CardInfo>();
    [global::ProtoBuf.ProtoMember(1, Name=@"CardList", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<pb.CardInfo> CardList
    {
      get { return _CardList; }
    }
  
    private pb.BattleSide _startSide;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"startSide", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public pb.BattleSide startSide
    {
      get { return _startSide; }
      set { _startSide = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
    [global::ProtoBuf.ProtoContract(Name=@"GameMode")]
    public enum GameMode
    {
            
      [global::ProtoBuf.ProtoEnum(Name=@"JoinRoom", Value=1)]
      JoinRoom = 1,
            
      [global::ProtoBuf.ProtoEnum(Name=@"CreateRoom", Value=2)]
      CreateRoom = 2,
            
      [global::ProtoBuf.ProtoEnum(Name=@"QuickEnter", Value=3)]
      QuickEnter = 3
    }
  
    [global::ProtoBuf.ProtoContract(Name=@"BattleSide")]
    public enum BattleSide
    {
            
      [global::ProtoBuf.ProtoEnum(Name=@"east", Value=1)]
      east = 1,
            
      [global::ProtoBuf.ProtoEnum(Name=@"south", Value=2)]
      south = 2,
            
      [global::ProtoBuf.ProtoEnum(Name=@"west", Value=3)]
      west = 3,
            
      [global::ProtoBuf.ProtoEnum(Name=@"north", Value=4)]
      north = 4
    }
  
    [global::ProtoBuf.ProtoContract(Name=@"CardStatus")]
    public enum CardStatus
    {
            
      [global::ProtoBuf.ProtoEnum(Name=@"noDeal", Value=1)]
      noDeal = 1,
            
      [global::ProtoBuf.ProtoEnum(Name=@"inHand", Value=2)]
      inHand = 2,
            
      [global::ProtoBuf.ProtoEnum(Name=@"bePeng", Value=3)]
      bePeng = 3,
            
      [global::ProtoBuf.ProtoEnum(Name=@"beGang", Value=4)]
      beGang = 4,
            
      [global::ProtoBuf.ProtoEnum(Name=@"dicard", Value=5)]
      dicard = 5
    }
  
}
// 所持個体の状態。instanceIdは個体ごと一意、characterIdは種類を指します。
using System;
[Serializable] public class OwnedCharacterData
{
    public string instanceId;
    public string characterId;
    public int level=1,experience,evolution;
}
[Serializable] public class PartyData
{
    public string[] memberIds=new string[3];
    public string leaderId;
}

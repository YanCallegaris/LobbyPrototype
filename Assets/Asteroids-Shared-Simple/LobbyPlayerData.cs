using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct LobbyPlayerData : INetworkStruct
{
    public NetworkBool isReady;
    public NetworkString<_32> PlayerName;
}

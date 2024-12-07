using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
[CreateAssetMenu(fileName = "RoomNodeType_", menuName = "Scriptable Objects/Dungeon/Room Node Type")]

public class RoomNodeTypeSO : ScriptableObject
{
    public string roomNodeTypeName;

    public bool displayInNodeGraphEditor = true;
    public bool isCorridor;
    public bool isCorridorNS;
    public bool isCorridorEW;
    public bool isEntrance;
    public bool isBossRoom;
    public bool isNone;
    
    #if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(roomNodeTypeName),roomNodeTypeName);
    }
    #endif
}

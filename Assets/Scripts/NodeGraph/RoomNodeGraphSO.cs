using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomNodeGraphSO", menuName = "Scriptable Objects/Dungeon/Room Node Graph")]
public class RoomNodeGraphSO : ScriptableObject
{
    [HideInInspector] public RoomNodeTypeListSO roomNodeTypeList;
    [HideInInspector] public List<RoomNodeSO> roomNodeList=new List<RoomNodeSO>();
    public readonly Dictionary<string,RoomNodeSO> RoomNodeDictionary = new Dictionary<string, RoomNodeSO>();

    private void Awake()
    {
        LoadRoomNodeDic();
    }

    private void LoadRoomNodeDic()
    {
        RoomNodeDictionary.Clear();
        foreach (RoomNodeSO node in roomNodeList)
        {
            RoomNodeDictionary[node.id] = node;
        }
    }

    public RoomNodeSO GetRoomNode(string id)
    {
        return RoomNodeDictionary.GetValueOrDefault(id);
    }

    #region Editor code
    #if UNITY_EDITOR

    [HideInInspector] public RoomNodeSO lineFromNode;
    [HideInInspector] public Vector2 linePosition;

    //Editor-only function that Unity calls when the script is loaded or a value changes in the Inspector.
    public void OnValidate()
    {
        LoadRoomNodeDic();
    }

    public void SetNodeToDrawConnectionLineFrom(RoomNodeSO lineFrom, Vector2 lineTo)
    {
        lineFromNode=lineFrom;
        linePosition=lineTo;
    }
#endif


    #endregion
}

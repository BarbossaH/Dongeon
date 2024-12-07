using System;
using System.Collections.Generic;
using Misc;
using UnityEditor;
using UnityEngine;

// [CreateAssetMenu(fileName = "RoomNodeSO", menuName = "Scriptable Objects/Dungeon/Room Node")]

public class RoomNodeSO : ScriptableObject
{
  [HideInInspector]public string id;
  [HideInInspector]public List<string> parentRoomNodeIdList = new List<string>();
  [HideInInspector]public List<string> childRoomIdList = new List<string>();
  [HideInInspector] public RoomNodeGraphSO roomNodeGraph;
  public RoomNodeTypeSO roomNodeType;
  [HideInInspector] public RoomNodeTypeListSO roomNodeTypeList;

  
  #if UNITY_EDITOR
  [HideInInspector] public Rect rect;
  [HideInInspector] public bool isLeftClickDragging;
  [HideInInspector] public bool isSelected;

  public void Initialise(Rect r, RoomNodeGraphSO graph, RoomNodeTypeSO type)
  {
   this.rect = r;
   this.id = Guid.NewGuid().ToString();
   this.name = "RoomNode";
   this.roomNodeGraph = graph;
   this.roomNodeType = type;

   roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
  }

  /// <summary>
  /// draw node with the node style
  /// </summary>
  /// <param name="style"></param>
  public void Draw(GUIStyle style)
  {
      GUILayout.BeginArea(rect, style);
      EditorGUI.BeginChangeCheck();
      
      //if this room node has a parent , or is the entrance, it will be locked
      if (parentRoomNodeIdList.Count > 0 || roomNodeType.isEntrance)
      {
          GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
          labelStyle.normal.textColor = Color.green;
          EditorGUILayout.LabelField(roomNodeType.roomNodeTypeName,labelStyle);
      }
      else
      {
          int selected =roomNodeTypeList.list.FindIndex(x=>x==roomNodeType);
          // Debug.Log(GetRoomNodeTypesToDisplay());
          int selection = EditorGUILayout.Popup( selected, GetRoomNodeTypesToDisplay());
          // Debug.Log(selection);
          roomNodeType = roomNodeTypeList.list[selection];

          //if the room type selection has changed making child connection potentially invalid
          if (roomNodeTypeList.list[selected].isCorridor && !roomNodeTypeList.list[selection].isCorridor
              || roomNodeTypeList.list[selection].isCorridor || roomNodeTypeList.list[selected].isBossRoom
              && roomNodeTypeList.list[selection].isBossRoom)
          {
              if (childRoomIdList.Count > 0)
              {
                  for (int i = childRoomIdList.Count-1; i >= 0; i--)
                  {
                      RoomNodeSO childNode = roomNodeGraph.GetRoomNode(childRoomIdList[i]);
                      if (childNode != null)
                      {
                          childNode.RemoveParentRoomNodeID(id);
                          RemoveChildRoomNodeID(childNode.id);
                      }
                  }
              }
          }
      }
      
      if (EditorGUI.EndChangeCheck())
      {
          EditorUtility.SetDirty(this);
      }
      GUILayout.EndArea();
  }

  /// <summary>
  /// Populate a string array with the room node types to display that can be selected
  /// </summary>
  /// <returns></returns>
  private string[] GetRoomNodeTypesToDisplay()
  {
      string [] roomArray = new string[roomNodeTypeList.list.Count];
      for (int i = 0; i<roomNodeTypeList.list.Count; i++)
      {
          if (roomNodeTypeList.list[i].displayInNodeGraphEditor)
          {
              roomArray[i] = roomNodeTypeList.list[i].roomNodeTypeName;
          }
      }
      
      return roomArray;
  }

  public void ProcessEvents(Event e)
  {
      switch (e.type)
      {
          case EventType.MouseDown:
              ProcessMouseDownEvent(e);
              break;
          case EventType.MouseUp:
              ProcessMouseUpEvent(e);
              break;
          case EventType.MouseDrag:
              ProcessMouseDragEvent(e);
              break;
      }
  }

  private void ProcessMouseDragEvent(Event e)
  {
      if (e.button == 0)
      {
          ProcessLeftMouseDragEvent(e);
      }
  }



  private void ProcessLeftMouseDragEvent(Event e)
  {
      isLeftClickDragging = true;
      DragNode(e.delta);
      GUI.changed = true;
  }

  public void DragNode(Vector2 eDelta)
  {
      rect.position += eDelta;
      EditorUtility.SetDirty(this);
  }

  private void ProcessMouseUpEvent(Event e)
  {
      if (e.button == 0)
      {
          ProcessLeftClickUpEvent();
      }
  }

  private void ProcessLeftClickUpEvent()
  {
      if (isLeftClickDragging)
      {
          isLeftClickDragging = false;
      }
  }

  private void ProcessMouseDownEvent(Event e)
  {
      if (e.button == 0)
      {
          ProcessLefClickDownEvent();
      }else if (e.button == 1)
      {
          ProcessRightClickDownEvent(e);
      }
  }
  private void ProcessRightClickDownEvent(Event e)
  {
      //this click the right mouse button on a room node, then set the start point and end point
      roomNodeGraph.SetNodeToDrawConnectionLineFrom(this, e.mousePosition);
  }
  private void ProcessLefClickDownEvent()
  {
      Selection.activeObject = this;
      
      isSelected =!isSelected;
  }

  public bool AddChildRoomNodeID(string childId)
  {
      if (!IsChildRoomValid(childId)) return false;
      childRoomIdList.Add(childId);
      return true;
  }

  public bool RemoveChildRoomNodeID(string childId)
  {
      if (childRoomIdList.Contains(childId))
      {
          childRoomIdList.Remove(childId);
          return true;
      }
      return false;
  }

  public bool AddParentRoomNodeID(string parentId)
  {
      parentRoomNodeIdList.Add(parentId);
      return true;
  }

  public bool RemoveParentRoomNodeID(string parentId)
  {
      if (parentRoomNodeIdList.Contains(parentId))
      {
          parentRoomNodeIdList.Remove(parentId);
          return true;
      }
      return false;
  }
  public bool IsChildRoomValid(string childId)
  {
      bool isConnectedBossNodeAlready=false;
      foreach (var node in roomNodeGraph.roomNodeList)
      {
          //check if there is already a connected boos room in the node graph, which means there is only one boss room
          if (node.roomNodeType.isBossRoom && node.parentRoomNodeIdList.Count > 0)
          {
              isConnectedBossNodeAlready=true;
          }
      }

      //if the child node has a type of boss room and there is already a connected boss room node then return false
      if (roomNodeGraph.GetRoomNode(childId).roomNodeType.isBossRoom && isConnectedBossNodeAlready) return false;

      if (roomNodeGraph.GetRoomNode(childId).roomNodeType.isNone) return false;

      //if the node already has a child with this child ID return false
      if (childRoomIdList.Contains(childId)) return false;
      
      if(id==childId) return false;
      
      if(parentRoomNodeIdList.Contains(childId)) return false;
      
      //if the child node already has a parent return false;
      if(roomNodeGraph.GetRoomNode(childId).parentRoomNodeIdList.Count>0) return false;
      
      //if this node is a corridor, and its child is also a corridor, then return false;
      if(roomNodeGraph.GetRoomNode(childId).roomNodeType.isCorridor && roomNodeType.isCorridor) return false;
      
      //if this room is room and its child is also a room, return false.(because there are several types of room)
      if (!roomNodeGraph.GetRoomNode(childId).roomNodeType.isCorridor && !roomNodeType.isCorridor) return false;
      
      //if adding a corridor check that this node has less than the maximum permitted child corridors
      if (roomNodeGraph.GetRoomNode(childId).roomNodeType.isCorridor &&
          childRoomIdList.Count >= Settings.MaxChildCorridors) return false;
      
      //if the child room type is an entrance, return false;
      if(roomNodeGraph.GetRoomNode(childId).roomNodeType.isEntrance) return false;
      
      //if adding a room to a corridor check that this corridor node doesn't already have a room added
      if(!roomNodeGraph.GetRoomNode(childId).roomNodeType.isCorridor&&childRoomIdList.Count>0) return false;
      
      return true;
  }
#endif
}

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class RoomNodeGraphEditor : EditorWindow
{
    private GUIStyle _roomNodeStyle;
    private GUIStyle _roomNodeSelectedStyle;
    private static RoomNodeGraphSO _currentRoomNodeGraph;
    private RoomNodeTypeListSO _roomNodeTypeList;
    private RoomNodeSO _currentRoomNode;

    private Vector2 graphOffset;
    private Vector2 graphDrag;
    
    private const float NodeWidth = 120f;
    private const float NodeHeight = 60f;
    private const int NodePadding = 20;
    private const int NodeBorder= 10;
    private const float ConnectingLineWidth = 3f;
    private const float ArrowSize = 12f;
    private const float GridLarge = 100f;
    private const float GridSmall = 25f;
    
    [MenuItem("Room Node Graph Editor", menuItem = "Window/Dungeon Editor/Room Node Graph Editor")]
    public static void OpenWindow()
    {
        GetWindow<RoomNodeGraphEditor>("Room Node Graph Editor");
    }

    /// <summary>
    /// Open the room node graph editor window if a room node graph scriptable object asset is double-clicked in the inspector
    /// </summary>
    [OnOpenAsset(0)]
    public static bool OnClickAsset(int instanceID, int line)
    {
        //this function name is customized, but the attribute decorating this function determines the timing of execution of this function 
        
        //the below line is to try to get an object if the instanceID is corresponding to RoomNodeGraph. If succeeding, then recording this value for the future usage.
        RoomNodeGraphSO roomNodeGraphSo= EditorUtility.InstanceIDToObject(instanceID) as RoomNodeGraphSO;
        if (roomNodeGraphSo != null)
        {
            OpenWindow();
            _currentRoomNodeGraph = roomNodeGraphSo;
            return true;
        }
        return false;
    }
    private void OnEnable()
    {
        Selection.selectionChanged += InspectorSelectionChanged;
        //initialize the style of room node
        _roomNodeStyle = new GUIStyle
        {
            normal =
            {
                background = EditorGUIUtility.Load("node2") as Texture2D,
                textColor = Color.white
            },
            padding = new RectOffset(NodePadding, NodePadding, NodePadding, NodePadding),
            border = new RectOffset(NodeBorder,NodeBorder,NodeBorder,NodeBorder),
        };

        _roomNodeSelectedStyle = new GUIStyle
        {
            normal =
            {
                background = EditorGUIUtility.Load("node1 on") as Texture2D,
                textColor = Color.white
            },
            padding = new RectOffset(NodePadding, NodePadding, NodePadding, NodePadding),
            border = new RectOffset(NodeBorder, NodeBorder, NodeBorder, NodeBorder),
        };
        
        //load room node types
        _roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= InspectorSelectionChanged;
    }

    private void InspectorSelectionChanged()
    {
        RoomNodeGraphSO roomNodeGraph = Selection.activeObject as RoomNodeGraphSO;
        if (roomNodeGraph != null)
        {
            _currentRoomNodeGraph = roomNodeGraph;
            GUI.changed = true;
        }
    }

    private void OnGUI()
    {
        #region test code

        // GUILayout.BeginArea(new Rect(new Vector2(100f,100f), new Vector2(nodeWidth,nodeHeight)), roomNodeStyle);
        // EditorGUILayout.LabelField("Node 1");
        // GUILayout.EndArea();       
        // GUILayout.BeginArea(new Rect(new Vector2(100f,300f), new Vector2(nodeWidth,nodeHeight)), roomNodeStyle);
        // EditorGUILayout.LabelField("Node 2");
        // GUILayout.EndArea();    

        #endregion

        if (_currentRoomNodeGraph != null)
        {
            DrawBackgroundGrid(GridSmall, 0.2f, Color.gray);
            DrawBackgroundGrid(GridLarge, 0.3f, Color.gray);
            
            //this function is for draw the line when dragging the mouse, but it is for drawing line when connecting two nodes. it will be cleared when mouse up event called.
            DrawDraggedLine();
            //process events
            ProcessEvents(Event.current);

            DrawRoomConnections();
            //draw room nodes
            DrawRoomNodes();
        }

        if (GUI.changed)
        {
            Repaint();
        }
    }

    private void DrawBackgroundGrid(float size, float opacity, Color color)
    {
        int verticalLineCount = Mathf.CeilToInt((position.width + size) / size);
        int horizontalLineCount = Mathf.CeilToInt((position.height + size) / size);
        
        Handles.color = new Color(color.r, color.g, color.b, opacity);
        graphOffset += graphDrag * 0.5f;
        
        Vector3 gridOffset = new Vector3(graphOffset.x % size, graphOffset.y % size, 0);

        for (int i = 0; i < verticalLineCount; i++)
        {
            Handles.DrawLine(new Vector3(size*i,-size,0)+gridOffset, new Vector3(size*i,position.height+size,0)+gridOffset);
        }

        for (int i = 0; i < horizontalLineCount; i++)
        {
            Handles.DrawLine(new Vector3(-size,size*i,0)+gridOffset, new Vector3(position.width+size,size*i,0)+gridOffset);
        }
        //I guess this color is a global variable, after drawing the line, we need to set the color back
        Handles.color = Color.white;
    }


    private void DrawRoomConnections()
    {
        foreach (var roomNode in _currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.childRoomIdList.Count > 0)
            {
                foreach (var childNodeId in roomNode.childRoomIdList)
                {
                    if (_currentRoomNodeGraph.RoomNodeDictionary.ContainsKey(childNodeId))
                    {
                        DrawConnectionLine(roomNode, _currentRoomNodeGraph.RoomNodeDictionary[childNodeId]);
                        GUI.changed = true;
                    }
                }
            }
        }
    }

    private void DrawConnectionLine(RoomNodeSO parent, RoomNodeSO child)
    {
        Vector2 startPos = parent.rect.center;
        Vector2 endPos = child.rect.center;
        
        //draw two lines as an arrow
        //1. find the middle point in the middle of connecting line
        Vector2 midPos = (startPos + endPos) / 2f;
        //get the direction of the line
        Vector2 dir = (endPos-startPos).normalized;
        //draw two lines as the arrow
        Vector2 arrowTaiPoint1 = midPos - new Vector2(-dir.y,dir.x)  * ArrowSize;
        Vector2 arrowTaiPoint2 = midPos + new Vector2(-dir.y,dir.x) * ArrowSize;
        
        Vector2 arrowEnd = midPos + new Vector2(dir.x,dir.y) * ArrowSize;
        Handles.DrawBezier(arrowTaiPoint1,arrowEnd,arrowTaiPoint1,arrowEnd,Color.green,null,3);
        Handles.DrawBezier(arrowTaiPoint2,arrowEnd,arrowTaiPoint2,arrowEnd,Color.green,null,3);
        
        
        Handles.DrawBezier(startPos, endPos, startPos, endPos, Color.white, null, ConnectingLineWidth);
        GUI.changed = true;
    }

    private void DrawDraggedLine()
    {
        if (_currentRoomNodeGraph.linePosition != Vector2.zero)
        {
            //todo:to check the parameters of this function
            Handles.DrawBezier(_currentRoomNodeGraph.lineFromNode.rect.center,_currentRoomNodeGraph.linePosition,
                _currentRoomNodeGraph.lineFromNode.rect.center, _currentRoomNodeGraph.linePosition,
                Color.white,null,ConnectingLineWidth);
        }
    }

    private void ProcessEvents(Event e)
    {
        //reset graph drag
        graphDrag = Vector2.zero;
        
        //get room node that mouse is over if it's null or not currently being dragged
        if (_currentRoomNode == null || _currentRoomNode.isLeftClickDragging == false)
        {
            _currentRoomNode = IsMouseOverRoomNode(e);
        }

        //if mouse on a node, then executing the event of this node, otherwise, then dealing with the situation not on the node, which is creating a node
        if (_currentRoomNode == null ||_currentRoomNodeGraph.lineFromNode != null)
        {
            ProcessRoomNodeGraphEvents(e);
        }
        else
        {
            _currentRoomNode.ProcessEvents(e);
        }
    }

    private RoomNodeSO IsMouseOverRoomNode(Event e)
    {
        for (int i = _currentRoomNodeGraph.roomNodeList.Count-1; i>=0; i--)
        {
            if (_currentRoomNodeGraph.roomNodeList[i].rect.Contains(e.mousePosition))
            {
                return _currentRoomNodeGraph.roomNodeList[i];
            }
        }
        return null;
    }

    private void ProcessRoomNodeGraphEvents(Event e)
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

    private void ProcessMouseUpEvent(Event e)
    {
        if (e.button == 1 && _currentRoomNodeGraph.lineFromNode != null)
        {
            RoomNodeSO roomNode = IsMouseOverRoomNode(e);
            if (roomNode != null)
            {
                if (_currentRoomNodeGraph.lineFromNode.AddChildRoomNodeID(roomNode.id))
                {
                    roomNode.AddParentRoomNodeID(_currentRoomNodeGraph.lineFromNode.id);
                }
            }
            ClearLineDrag();
        }
    }

    private void ClearLineDrag()
    {
        _currentRoomNodeGraph.lineFromNode = null;
        _currentRoomNodeGraph.linePosition = Vector2.zero;
        GUI.changed = true;
    }

    private void ProcessMouseDragEvent(Event e)
    {
        switch (e.button)
        {
            case 1:
                ProcessRightMouseDragEvent(e);
                break;
            case 0:
                ProcessLeftMouseDragEvent(e.delta);
                break;
        }
    }

    private void ProcessLeftMouseDragEvent(Vector2 eDelta)
    {
        //dragging the canvas means selecting all grids and dragging all of them
        graphDrag = eDelta;
        for (int i = 0; i < _currentRoomNodeGraph.roomNodeList.Count; i++)
        {
            _currentRoomNodeGraph.roomNodeList[i].DragNode(eDelta);
        }
        GUI.changed = true;
    }


    private void ProcessRightMouseDragEvent(Event e)
    {
        if (_currentRoomNodeGraph.lineFromNode != null)
        {
            DragConnectingLine(e.delta);
            GUI.changed = true;
        }
    }

    private void DragConnectingLine(Vector2 eDelta)
    {
        _currentRoomNodeGraph.linePosition+=eDelta;
    }

    private void ProcessMouseDownEvent(Event e)
    {
        if (e.button == 1)
        {
            //to show the context menu at the mouse position
            ShowContextMenu(e.mousePosition);
        }else if (e.button == 0)
        {
            ClearLineDrag();
            ClearAllSelectedRoomNodes();
        }
    }

    private void ClearAllSelectedRoomNodes()
    {
        foreach (var node in _currentRoomNodeGraph.roomNodeList)
        {
            if (node.isSelected)
            {
                node.isSelected = false;
                GUI.changed = true;
            }
        }
    }

    private void ShowContextMenu(Vector2 mousePosition)
    {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Create room node"), false,CreateRoomNode, mousePosition);
        genericMenu.AddSeparator("");
        genericMenu.AddItem(new GUIContent("Select All Room Nodes"), false,SelectAllRoomNodes);
        genericMenu.AddSeparator("");
        genericMenu.AddItem(new GUIContent("Delete Selected Room Node Links"),false,DeleteSelectedRoomNodeLinks);
        genericMenu.AddItem(new GUIContent("Delete Selected Room Nodes"), false,DeleteSelectedRoomNodes);
        genericMenu.AddSeparator("");
        
        
        genericMenu.ShowAsContext();
    }


    private void SelectAllRoomNodes()
    {
        foreach (var node in _currentRoomNodeGraph.roomNodeList)
        {
            node.isSelected = true;
        }
        
        GUI.changed = true;
    }

    private void CreateRoomNode(object mousePositionObject)
    {
        if (_currentRoomNodeGraph.roomNodeList.Count == 0)
        {
            CreateRoomNode(new Vector2(200f, 200f), _roomNodeTypeList.list.Find(x=>x.isEntrance));
        }
        //create a node setting the default value is none
        CreateRoomNode(mousePositionObject, _roomNodeTypeList.list.Find(x=>x.isNone));
    }

    private void CreateRoomNode(object mousePositionObject, RoomNodeTypeSO roomNodeType)
    {
        Vector2 mousePosition = (Vector2)mousePositionObject;
        RoomNodeSO roomNode = ScriptableObject.CreateInstance<RoomNodeSO>();
        _currentRoomNodeGraph.roomNodeList.Add(roomNode);
        //set the position and size of the node when drawing
        roomNode.Initialise(new Rect(mousePosition, new Vector2(NodeWidth, NodeHeight)), _currentRoomNodeGraph,
            roomNodeType);
        AssetDatabase.AddObjectToAsset(roomNode,_currentRoomNodeGraph);
        AssetDatabase.SaveAssets();
        _currentRoomNodeGraph.OnValidate();
    }
    
    private void DeleteSelectedRoomNodes()
    {
        Queue<RoomNodeSO> roomNodeDeletion = new Queue<RoomNodeSO>();
        foreach (var node in _currentRoomNodeGraph.roomNodeList)
        {
            if (node.isSelected && !node.roomNodeType.isEntrance)
            {
               roomNodeDeletion.Enqueue(node);
               foreach (var childId in node.childRoomIdList)
               {
                   RoomNodeSO childNode = _currentRoomNodeGraph.GetRoomNode(childId);
                   if (childNode != null)
                   {
                       childNode.RemoveParentRoomNodeID(node.id);
                   }
               }
               foreach (var parentId in node.parentRoomNodeIdList)
               {
                   RoomNodeSO parentNode = _currentRoomNodeGraph.GetRoomNode(parentId);
                   if (parentNode != null)
                   {
                       parentNode.RemoveChildRoomNodeID(node.id);
                   }
               }
            }
        }

        while (roomNodeDeletion.Count > 0)
        {
            RoomNodeSO nodeToDelete = roomNodeDeletion.Dequeue();
            _currentRoomNodeGraph.RoomNodeDictionary.Remove(nodeToDelete.id);
            _currentRoomNodeGraph.roomNodeList.Remove(nodeToDelete);
            //remove node from asse database;
            DestroyImmediate(nodeToDelete, true);
            AssetDatabase.SaveAssets();
        }
    }

    private void DeleteSelectedRoomNodeLinks()
    {
        foreach (var node in _currentRoomNodeGraph.roomNodeList)
        {
            if (node.isSelected && node.childRoomIdList.Count > 0)
            {
                for (int i = node.childRoomIdList.Count - 1; i >= 0; i--)
                {
                    RoomNodeSO childNode = _currentRoomNodeGraph.GetRoomNode(node.childRoomIdList[i]);
                    if (childNode != null && childNode.isSelected)
                    {
                        node.RemoveChildRoomNodeID(childNode.id);
                        childNode.RemoveParentRoomNodeID(node.id);
                    }
                }
            }
        }
        ClearAllSelectedRoomNodes();
    }


    private void DrawRoomNodes()
    {
        foreach (var roomNode in _currentRoomNodeGraph.roomNodeList)
        {
            roomNode.Draw( roomNode.isSelected ? _roomNodeSelectedStyle : _roomNodeStyle);
        }

        GUI.changed = true;
    }
}

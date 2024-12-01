using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RoomNodeGraphEditor : EditorWindow
{
    private GUIStyle roomNodeStyle;
    
    private const float nodeWidth = 160f;
    private const float nodeHeight = 75f;
    private const int nodePadding = 25;
    private const int nodeBorder= 10;
    [MenuItem("Room Node Graph Editor", menuItem = "Window/Dungeon Editor/Room Node Graph Editor")]
    public static void OpenWindow()
    {
        GetWindow<RoomNodeGraphEditor>("Room Node Graph Editor");
        
    }

    private void OnEnable()
    {
        roomNodeStyle = new GUIStyle
        {
            normal =
            {
                background = EditorGUIUtility.Load("node1") as Texture2D,
                textColor = Color.white
            },
            padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding),
            border = new RectOffset(nodeBorder,nodeBorder,nodeBorder,nodeBorder),
        };
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(new Vector2(100f,100f), new Vector2(nodeWidth,nodeHeight)), roomNodeStyle);
        EditorGUILayout.LabelField("Node 1");
        GUILayout.EndArea();       
        GUILayout.BeginArea(new Rect(new Vector2(100f,300f), new Vector2(nodeWidth,nodeHeight)), roomNodeStyle);
        EditorGUILayout.LabelField("Node 2");
        GUILayout.EndArea();      
    }
}

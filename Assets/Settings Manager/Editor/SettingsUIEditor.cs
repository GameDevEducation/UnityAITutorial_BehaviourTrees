﻿using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SettingsUI))]
public class SettingsUIEditor : Editor
{
    void OnEnable()
    {
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Generate UI"))
        {
            SettingsUI target = serializedObject.targetObject as SettingsUI;
            target.GenerateUI();
        }

        if (GUILayout.Button("Clear UI"))
        {
            SettingsUI target = serializedObject.targetObject as SettingsUI;
            target.ClearUI();
        }
    }
}

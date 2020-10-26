﻿using Innoactive.Creator.XRInteraction;
using Innoactive.CreatorEditor.UI;
using Innoactive.CreatorEditor.XRInteraction;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class SnapZoneSettingsProvider : SettingsProvider
{
    const string Path = "Project/Creator/Snap Zones";

    private Editor editor;
    
    public SnapZoneSettingsProvider() : base(Path, SettingsScope.Project) {}

    public static bool IsSettingsAvailable()
    {
        return true;
    }

    public override void OnGUI(string searchContext)
    {
        EditorGUILayout.Space();
        GUIStyle labelStyle = CreatorEditorStyles.ApplyPadding(CreatorEditorStyles.Paragraph, 0); 
        GUILayout.Label("These settings help you to configure Snap Zones within your scenes. You can define colors and other values that will be set to Snap Zones created by clicking the 'Create Snap Zone' button of a Snappable Property.", labelStyle);
        EditorGUILayout.Space();
        
        editor.OnInspectorGUI();
            
        EditorGUILayout.Space(20f);

        if (GUILayout.Button("Apply settings in current scene"))
        {
            SnapZone[] snapZones = Resources.FindObjectsOfTypeAll<SnapZone>();
                
            foreach (SnapZone snapZone in snapZones)
            {
                SnapZoneSettings.Instance.ApplySettingsToSnapZone(snapZone);
            }
        }
    }

    public override void OnActivate(string searchContext, VisualElement rootElement)
    {
        editor = Editor.CreateEditor(SnapZoneSettings.Instance);
    }

    [SettingsProvider]
    public static SettingsProvider Provider()
    {
        if (IsSettingsAvailable())
        {
            SettingsProvider provider = new SnapZoneSettingsProvider();
            return provider;
        }

        return null;
    }
}

﻿using VPG.XRInteraction;
using VPG.Editor.UI;
using VPG.Editor.XRInteraction;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

internal class SnapZoneSettingsProvider : SettingsProvider
{
    const string Path = "Project/VR Process Gizmo/Snap Zones";

    private Editor editor;
    
    public SnapZoneSettingsProvider() : base(Path, SettingsScope.Project) {}

    public override void OnGUI(string searchContext)
    {
        EditorGUILayout.Space();
        GUIStyle labelStyle = VPGEditorStyles.ApplyPadding(VPGEditorStyles.Paragraph, 0); 
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
        SettingsProvider provider = new SnapZoneSettingsProvider();
        return provider;
    }
}

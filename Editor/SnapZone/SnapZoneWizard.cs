using UnityEditor;
using UnityEngine;
using Innoactive.Creator.XRInteraction;

namespace Innoactive.CreatorEditor.XRInteraction
{
    /// <summary>
    /// Editor Window used to setup settings for <see cref="SnapZone"/>s.
    /// Can be used e.g. for the creation functionality provided in <see cref="Innoactive.Creator.XRInteraction.Properties.SnappableProperty"/>.
    /// </summary>
    public class SnapZoneWizard : EditorWindow
    {
        private SnapZoneSettings settings;
        private Editor snapzoneEditor;
        private static SnapZoneWizard window;
        private const string menuPath = "Innoactive/Creator/Windows/Snap Zone Settings";

        [MenuItem(menuPath, false, 60)]
        private static void ShowWizard()
        {
            window = GetWindow<SnapZoneWizard>();

            window.Show();
            window.Focus();
        }
        
        private void OnEnable()
        {
            settings = SnapZoneSettings.Settings;
            snapzoneEditor = Editor.CreateEditor(settings);
        }

        private void OnGUI()
        {
            titleContent = new GUIContent("Snap Zone Settings");

            if (settings == null)
            {
                window.Close();
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("These settings help you to configure Snap Zones within your scenes. You can define colors and other values that will be set to Snap Zones created by clicking the 'Create Snap Zone' button of a Snappable Property.", MessageType.Info);
            EditorGUILayout.Space();

            snapzoneEditor.OnInspectorGUI();
            
            EditorGUILayout.Space(20f);

            if (GUILayout.Button("Apply settings in current scene"))
            {
                SnapZone[] snapZones = Resources.FindObjectsOfTypeAll<SnapZone>();
                
                foreach (SnapZone snapZone in snapZones)
                {
                    settings.ApplySettingsToSnapZone(snapZone);
                }
            }
        }
    }
}

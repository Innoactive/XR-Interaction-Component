#if XRIT_1_0_OR_NEWER
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Innoactive.Creator.Core.Configuration;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Innoactive.CreatorEditor.PackageManager.XRInteraction
{
    /// <summary>
    /// Utility class that ensures all Creator's prefabs are properly setup in the project.
    /// </summary>
    [InitializeOnLoad]
    internal static class XRSimulatorImporter
    {
        private const string SimulatorPrefabName = "XR Device Simulator";
        private const string ActionRigName = "[XR_Setup_Action_Based]";
        private const string PrefabPath = "Assets/Resources/[XR_Simulator_Setup].prefab";

        static XRSimulatorImporter()
        {
            if (AssetDatabase.GetMainAssetTypeAtPath(PrefabPath) == null)
            {
                DependencyManager.OnPostProcess += OnPostProcess;
            }
        }

        private static void OnPostProcess(object sender, DependencyManager.DependenciesEnabledEventArgs e)
        {
            DependencyManager.OnPostProcess -= OnPostProcess;
            ImportSimulatorRig();
        }
        
        private static void ImportSimulatorRig()
        {
            GameObject simulator = LoadPrefab(SimulatorPrefabName, "Samples", out string simulatorPath);
            GameObject actionRig = LoadPrefab(ActionRigName, "Innoactive", out string actionRigPath);

            if (simulator == null || actionRig == null)
            {
                Debug.Log($"Simulator: {simulator == null}, Action Rig: {actionRig == null}");
                return;
            }

            string resourcesPath = Path.GetDirectoryName(PrefabPath);
            
            if (Directory.Exists(resourcesPath) == false)
            {
                Directory.CreateDirectory(resourcesPath);
            }

            simulator.transform.SetParent(actionRig.transform);
            PrefabUtility.SaveAsPrefabAsset(actionRig, PrefabPath);
        }
        
        private static GameObject LoadPrefab(string prefabName, string searchFolder, out string assetPath)
        {
            string filter = $"t:Prefab {prefabName}";
            string[] prefabsGUIDs = AssetDatabase.FindAssets(filter, new[] {$"Assets/{searchFolder}"});

            foreach (string prefabGUID in prefabsGUIDs)
            {
                assetPath = AssetDatabase.GUIDToAssetPath(prefabGUID);

                // GameObject runtimeConfigurator = new GameObject().AddComponent<RuntimeConfigurator>().gameObject;
                // Scene roomPreview = EditorSceneManager.NewPreviewScene();
                //
                // SceneManager.MoveGameObjectToScene(runtimeConfigurator, roomPreview);
                // runtimeConfigurator.AddComponent<RuntimeConfigurator>();
                //
                // PrefabUtility.LoadPrefabContentsIntoPreviewScene(assetPath, roomPreview);
                // return roomPreview.GetRootGameObjects().First(g => g.name == prefabName);
                
                return PrefabUtility.LoadPrefabContents(assetPath);
            }

            assetPath = string.Empty;
            return null;
        }
    }
}
#endif
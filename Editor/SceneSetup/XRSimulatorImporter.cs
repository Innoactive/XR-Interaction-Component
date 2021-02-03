#if XRIT_1_0_OR_NEWER
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Innoactive.CreatorEditor.PackageManager.XRInteraction
{
    /// <summary>
    /// Utility class that ensures all Creator's prefabs are properly setup in the project.
    /// </summary>
    [InitializeOnLoad]
    internal static class XRSimulatorImporter
    {
        private const string SamplePrefabName = "XR Device Simulator";
        private const string ActionRigName = "[XR_Setup_Action_Based]";
        private const string PrefabName = "[XR_Setup_Simulator]";
        private const string SimulatorPathKey = "";
        
        static XRSimulatorImporter()
        {
            string simulatorRigPath = EditorPrefs.GetString(SimulatorPathKey);
            
            if (string.IsNullOrEmpty(simulatorRigPath) || AssetDatabase.GetMainAssetTypeAtPath(simulatorRigPath) == null)
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
            GameObject simulator = LoadPrefab(SamplePrefabName, "Samples", out string simulatorRigPath);
            GameObject actionRig = LoadPrefab(ActionRigName, "Innoactive", out string actionRigPath);

            if (simulator == null || actionRig == null)
            {
                Debug.LogError($"{PrefabName} could not be generated. {(simulator == null ? SamplePrefabName : ActionRigName)} is missing.");
                return;
            }

            simulatorRigPath = $"{Path.GetDirectoryName(actionRigPath)}/{PrefabName}.prefab";

            simulator.transform.SetParent(actionRig.transform);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(actionRig, simulatorRigPath);
            
            EditorPrefs.SetString(SimulatorPathKey, simulatorRigPath);
            
            PrefabUtility.UnloadPrefabContents(simulator);
            PrefabUtility.UnloadPrefabContents(actionRig);
            PrefabUtility.UnloadPrefabContents(prefab);
        }
        
        private static GameObject LoadPrefab(string prefabName, string searchFolder, out string assetPath)
        {
            string filter = $"t:Prefab {prefabName}";
            string[] prefabsGUIDs = AssetDatabase.FindAssets(filter, new[] {$"Assets/{searchFolder}"});

            foreach (string prefabGUID in prefabsGUIDs)
            {
                assetPath = AssetDatabase.GUIDToAssetPath(prefabGUID);
                return PrefabUtility.LoadPrefabContents(assetPath);
            }

            assetPath = string.Empty;
            return null;
        }
    }
}
#endif
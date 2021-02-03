#if XRIT_1_0_OR_NEWER
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Innoactive.CreatorEditor.PackageManager.XRInteraction
{
    /// <summary>
    /// 
    /// </summary>
    internal class XRSimulatorImporter
    {
        public string SimulatorRigPath = null;
        public string SimulatorPathKey = null;
        
        private string samplePrefabName = "XR Device Simulator";
        private string actionRigName = "[XR_Setup_Action_Based]";
        private string simulatorPrefabName = "[XR_Setup_Simulator]";

        public XRSimulatorImporter()
        {
            SimulatorRigPath = EditorPrefs.GetString(SimulatorPathKey);
        }
        
        public void ImportSimulatorRig()
        {
            GameObject simulator = LoadPrefab(samplePrefabName, "Samples", out string simulatorRigPath);
            GameObject actionRig = LoadPrefab(actionRigName, "Innoactive", out string actionRigPath);

            if (simulator == null || actionRig == null)
            {
                Debug.LogError($"{simulatorPrefabName} could not be generated. {(simulator == null ? samplePrefabName : actionRigName)} is missing.");
                return;
            }

            simulatorRigPath = $"{Path.GetDirectoryName(actionRigPath)}/{simulatorPrefabName}.prefab";

            simulator.transform.SetParent(actionRig.transform);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(actionRig, simulatorRigPath);
            
            EditorPrefs.SetString(SimulatorPathKey, simulatorRigPath);
            
            PrefabUtility.UnloadPrefabContents(simulator);
            PrefabUtility.UnloadPrefabContents(actionRig);
            PrefabUtility.UnloadPrefabContents(prefab);
        }
        
        private GameObject LoadPrefab(string prefabName, string searchFolder, out string assetPath)
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
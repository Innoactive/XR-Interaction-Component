using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace VPG.CreatorEditor.PackageManager.XRInteraction
{
    /// <summary>
    /// Helper class to generate a new XR Simulator Rig out of the `[XR_Setup_Action_Based]` and the `XR Device Simulator` sample.
    /// </summary>
    internal class XRSimulatorImporter
    {
        public string SimulatorRigPath { get; } = null;
        
        private const string SimulatorPathKey = "SimulatorRigPath";
        private const string SamplePrefabName = "XR Device Simulator";
        private const string ActionRigName = "[XR_Setup_Action_Based]";
        private const string SimulatorPrefabName = "[XR_Setup_Simulator]";

        public XRSimulatorImporter()
        {
            SimulatorRigPath = EditorPrefs.GetString(SimulatorPathKey);
        }
        
        /// <summary>
        /// Imports a new `[XR_Setup_Simulator]` prefab based on the `[XR_Setup_Action_Based]` and the `XR Device Simulator` prefabs.
        /// </summary>
        /// <remarks>The generated prefab is imported into the `XR Interaction Component`’s `Resources` folder.</remarks>
        public void ImportSimulatorRig()
        {
            GameObject simulator = LoadPrefab(SamplePrefabName, "Samples", out string simulatorRigPath);
            GameObject actionRig = LoadPrefab(ActionRigName, "VPG", out string actionRigPath);

            if (simulator == null || actionRig == null)
            {
                Debug.LogError($"{SimulatorPrefabName} could not be generated. {(simulator == null ? SamplePrefabName : ActionRigName)} is missing.");
                return;
            }

            simulatorRigPath = $"{Path.GetDirectoryName(actionRigPath)}/{SimulatorPrefabName}.prefab";

            simulator.transform.SetParent(actionRig.transform);
            PrefabUtility.SaveAsPrefabAsset(actionRig, simulatorRigPath);
            
            EditorPrefs.SetString(SimulatorPathKey, simulatorRigPath);
            PrefabUtility.UnloadPrefabContents(simulator);
        }
        
        private GameObject LoadPrefab(string prefabName, string searchFolder, out string assetPath)
        {
            string filter = $"t:Prefab {prefabName}";
            string prefabGUID = AssetDatabase.FindAssets(filter, new[] {$"Assets/{searchFolder}"}).FirstOrDefault();

            if (string.IsNullOrEmpty(prefabGUID) == false)
            {
                assetPath = AssetDatabase.GUIDToAssetPath(prefabGUID);
                return PrefabUtility.LoadPrefabContents(assetPath);
            }

            assetPath = string.Empty;
            return null;
        }
    }
}

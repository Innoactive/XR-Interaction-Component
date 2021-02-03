using UnityEditor;
using UnityEngine;
using Innoactive.CreatorEditor.BasicInteraction;
using Innoactive.CreatorEditor.PackageManager.XRInteraction;

namespace Innoactive.CreatorEditor.XRInteraction
{
    /// <summary>
    /// Scene setup for XR-Interaction.
    /// </summary>
    public class XRInteractionSceneSetup : InteractionFrameworkSceneSetup
    {
        private const string Title = "Obsolete XR Ring detected";
        
        /// <inheritdoc />
        public override string Key { get; } = "XRInteractionSetup";
        
        /// <inheritdoc />
        public override void Setup()
        {
            DeleteStaticObject("[XR_Setup]");
            
#if XRIT_1_0_OR_NEWER
            XRSimulatorImporter simulatorImporter = new XRSimulatorImporter();
            string simulatorRigPath = EditorPrefs.GetString(simulatorImporter.SimulatorPathKey);

            if (string.IsNullOrEmpty(simulatorRigPath) || AssetDatabase.GetMainAssetTypeAtPath(simulatorRigPath) == null)
            {
                simulatorImporter.ImportSimulatorRig();
            }
#endif
        }

        private void DeleteStaticObject(string objectName)
        {
            GameObject objectToDelete = GameObject.Find(objectName);
            
            if (objectToDelete != null)
            {
                string message = $"Creator changed the XR Rig loading to a new dynamic system, you have a static {objectName} in the current scene, do you want to delete it?";
                
                if (EditorUtility.DisplayDialog(Title, message, "Delete", "Skip"))
                {
                    EditorUtility.SetDirty(objectToDelete);
                    Object.DestroyImmediate(objectToDelete);
                }
            }
        }
    }
}
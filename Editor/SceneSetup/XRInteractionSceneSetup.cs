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

            if (string.IsNullOrEmpty(simulatorImporter.SimulatorRigPath) || AssetDatabase.GetMainAssetTypeAtPath(simulatorImporter.SimulatorRigPath) == null)
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
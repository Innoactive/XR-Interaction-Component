using UnityEditor;
using UnityEngine;
using Innoactive.CreatorEditor.BasicInteraction;

namespace Innoactive.CreatorEditor.XRInteraction
{
    /// <summary>
    /// Scene setup for XR-Interaction.
    /// </summary>
    public class XRInteractionSceneSetup : InteractionFrameworkSceneSetup
    {
        private const string Title = "Obsolete XR Ring detected";

        /// <inheritdoc />
        public override void Setup()
        {
            SetupXR();
        }
        
        private void SetupXR()
        {
            DeleteStaticObject("[XR_Setup]");
            RemoveMainCamera();
            
#if ENABLE_INPUT_SYSTEM && XRIT_0_10_OR_NEWER
            DeleteStaticObject("[XR_Setup_Device_Based]");
            SetupPrefab("[XR_Setup_Action_Based]");
#else
            DeleteStaticObject("[XR_Setup_Action_Based]");
            SetupPrefab("[XR_Setup_Device_Based]");
#endif
        }

        private void DeleteStaticObject(string objectName)
        {
            GameObject oldRig = GameObject.Find(objectName);
            
            if (oldRig != null)
            {
                string Message = $"Creator changed the XR Rig loading to a new dynamic system, you have a static {objectName} in the current scene, do you want to delete it?";
                
                if (EditorUtility.DisplayDialog(Title, Message, "Delete", "Skip"))
                {
                    EditorUtility.SetDirty(oldRig);
                    Object.DestroyImmediate(oldRig);
                }
            }
        }
    }
}
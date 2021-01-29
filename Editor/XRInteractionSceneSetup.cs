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
        private const string Message = "Creator changed the Rig loading to a new dynamic system, you still have the old XR_Setup in the current scene, do you want to delete it?";
        
        /// <inheritdoc />
        public override void Setup()
        {
            SetupXR();
        }
        
        private void SetupXR()
        {
            GameObject oldRig = GameObject.Find("[XR_Setup]");
            
            if (oldRig != null)
            {
                if (EditorUtility.DisplayDialog(Title, Message, "Delete", "Skip"))
                {
                    EditorUtility.SetDirty(oldRig);
                    Object.DestroyImmediate(oldRig);
                }
            }
            
            RemoveMainCamera();
            
#if ENABLE_INPUT_SYSTEM && XRIT_0_10_OR_NEWER
            SetupPrefab("[XR_Setup_Action_Based]");
#else
            SetupPrefab("[XR_Setup_Device_Based]");
#endif
        }
    }
}
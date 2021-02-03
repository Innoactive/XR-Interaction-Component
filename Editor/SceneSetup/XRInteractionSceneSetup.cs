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
            DeleteStaticObject("[XR_Setup]");
        }

        private void DeleteStaticObject(string objectName)
        {
            GameObject objectToDelete = GameObject.Find(objectName);
            
            if (objectToDelete != null)
            {
                string Message = $"Creator changed the XR Rig loading to a new dynamic system, you have a static {objectName} in the current scene, do you want to delete it?";
                
                if (EditorUtility.DisplayDialog(Title, Message, "Delete", "Skip"))
                {
                    EditorUtility.SetDirty(objectToDelete);
                    Object.DestroyImmediate(objectToDelete);
                }
            }
        }
    }
}
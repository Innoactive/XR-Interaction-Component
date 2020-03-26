using Innoactive.CreatorEditor.BasicInteraction;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Innoactive.CreatorEditor.XRInteraction
{
    /// <summary>
    /// Scene setup for XR-Interaction.
    /// </summary>
    public class XRInteractionSceneSetup : InteractionFrameworkSceneSetup
    {
        /// <inheritdoc />
        public override void Setup()
        {
            SetupXR();
        }
        
        private void SetupXR()
        {
            Camera camera = Camera.main;
            
            if (camera != null)
            {
                Object.DestroyImmediate(camera.gameObject);
            }
        
            GameObject xrSetup = GameObject.Find("[XR_Setup]");
            
            if (xrSetup == null)
            {
                GameObject prefab = Object.Instantiate(Resources.Load("[XR_Setup]", typeof(GameObject))) as GameObject;
                prefab.name = prefab.name.Replace("(Clone)", string.Empty);
            }
        }
    }
}
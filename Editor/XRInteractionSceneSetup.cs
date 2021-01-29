using Innoactive.CreatorEditor.BasicInteraction;

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
            RemoveMainCamera();
#if ENABLE_INPUT_SYSTEM && XRIT_0_10_OR_NEWER
            SetupPrefab("[XR_Setup_Action_Based]");
#else
            SetupPrefab("[XR_Setup_Device_Based]");
#endif
        }
    }
}
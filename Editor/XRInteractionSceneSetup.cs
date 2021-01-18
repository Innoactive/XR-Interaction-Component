using Innoactive.CreatorEditor.BasicInteraction;
using UnityEngine;

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
            GameObject xrRig = GameObject.Find("[XR_Setup]");
            if (xrRig != null)
            {
                GameObject.Destroy(xrRig);
            }
        }
    }
}
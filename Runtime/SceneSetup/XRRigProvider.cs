using Innoactive.Creator.Components.Runtime.SceneSetup;

namespace Innoactive.Creator.XRInteraction.SceneSetup
{
    public class XRRigProvider : InteractionRigProvider
    {
        public override string Name { get; } = "Unity XR Rig";
        public override string PrefabName { get; } = "[XR_Setup]";
    }
}
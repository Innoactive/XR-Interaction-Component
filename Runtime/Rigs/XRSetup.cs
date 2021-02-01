﻿using Innoactive.Creator.BasicInteraction.RigSetup;

namespace Innoactive.Creator.Components.Runtime.Rigs
{
    /// <summary>
    /// Setup for the standard XR rig.
    /// </summary>
    public class XRSetup : XRSetupBase
    {
        /// <inheritdoc />
        public override string Name { get; } = "XR Rig";
        
        /// <inheritdoc />
        public override string PrefabName { get; } = "[XR_Setup_Action_Based]";
        
        /// <inheritdoc />
        public override bool CanBeUsed()
        {
#if ENABLE_INPUT_SYSTEM
            return IsEventManagerInScene() == false;
#else
            return false;
#endif
        }
        
        /// <inheritdoc />
        public override string GetSetupTooltip()
        {
#if ENABLE_INPUT_SYSTEM
            return "Can't be used while there is already a XRInteractionManager in the scene.";
#else
            return "Enable the new input system to allow using this rig.";
#endif
        }
    }
}

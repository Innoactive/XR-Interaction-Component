using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Innoactive.Creator.Components.Runtime.Rigs
{
    /// <summary>
    /// 
    /// </summary>
    public class XRSimulatorSetup : XRSetupBase
    {
        /// <inheritdoc />
        public override string Name { get; } = "XR Simulator";
        
        /// <inheritdoc />
        public override string PrefabName { get; } = "[XR_Simulator_Setup]";

        /// <inheritdoc />
        public override bool CanBeUsed()
        {
#if ENABLE_INPUT_SYSTEM && XRIT_1_0_OR_NEWER
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

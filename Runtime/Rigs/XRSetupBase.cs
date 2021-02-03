using Innoactive.Creator.BasicInteraction.RigSetup;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Innoactive.Creator.Components.Runtime.Rigs
{
    public abstract class XRSetupBase : InteractionRigProvider
    {
        protected bool IsEventManagerInScene()
        {
            return Object.FindObjectOfType<XRInteractionManager>() != null;
        }
    }
}
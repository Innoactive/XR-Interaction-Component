using Innoactive.Creator.BasicInteraction.RigSetup;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Management;

namespace Innoactive.Creator.Components.Runtime.Rigs
{
    public abstract class XRSetupBase : InteractionRigProvider
    {
        protected readonly bool IsPrefabMissing;
        
        public XRSetupBase()
        {
            IsPrefabMissing = Resources.Load(PrefabName) == null;
            Resources.UnloadUnusedAssets();
        }

        /// <summary>
        /// Makes sure the XR loader and subsystem is set up.
        /// </summary>
        public override void PreSetup()
        {
            if (XRGeneralSettings.Instance.Manager.activeLoader == null)
            {
                XRGeneralSettings.Instance.Manager.InitializeLoaderSync();
                XRGeneralSettings.Instance.Manager.StartSubsystems();
            }
        }

        protected bool IsEventManagerInScene()
        {
            return Object.FindObjectOfType<XRInteractionManager>() != null;
        }
    }
}
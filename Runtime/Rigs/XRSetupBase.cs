﻿using VPG.BasicInteraction.RigSetup;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace VPG.Components.Runtime.Rigs
{
    public abstract class XRSetupBase : InteractionRigProvider
    {
        protected readonly bool IsPrefabMissing;
        
        public XRSetupBase()
        {
            IsPrefabMissing = Resources.Load(PrefabName) == null;
            Resources.UnloadUnusedAssets();
        }
        
        protected bool IsEventManagerInScene()
        {
            return Object.FindObjectOfType<XRInteractionManager>() != null;
        }
    }
}
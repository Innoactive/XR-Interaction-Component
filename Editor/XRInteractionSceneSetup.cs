﻿using Innoactive.CreatorEditor.BasicInteraction;
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
            RemoveMainCamera();
            SetupPrefab("[XR_Setup]");
        }
    }
}
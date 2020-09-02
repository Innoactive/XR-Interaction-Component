﻿namespace Innoactive.CreatorEditor.PackageManager.XRInteraction
{
    /// <summary>
    /// Adds Unity's XR-Interaction-Toolkit package as a dependency and sets specified symbol for script compilation.
    /// </summary>
    public class XRInteractionPackageEnabler : Dependency 
    {        
        /// <inheritdoc/>
        public override string Package { get; } = "com.unity.xr.interaction.toolkit";

        /// <inheritdoc/>
        public override int Priority { get; } = 4;
        
        /// <inheritdoc/>
        protected override string[] Layers { get; } = {"XR Teleport"};
    }
}
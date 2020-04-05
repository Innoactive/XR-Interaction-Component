using System;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

namespace Innoactive.CreatorEditor.PackageManager.XRInteraction
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
        
        private const string CreatorXRInteractionSymbol = "CREATOR_XR_INTERACTION";

        public XRInteractionPackageEnabler()
        {
            OnPackageEnabled += PostProcess;
        }

        private void PostProcess(object sender, EventArgs e)
        {
            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup buildTargetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
            List<string> symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup).Split(';').ToList();
        
            if (symbols.Contains(CreatorXRInteractionSymbol) == false)
            {
                symbols.Add(CreatorXRInteractionSymbol);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, string.Join(";", symbols.ToArray()));
            }
            
            OnPackageEnabled -= PostProcess;
        }
    }
}
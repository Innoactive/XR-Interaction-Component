#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using System;
using UnityEditor;
using Innoactive.CreatorEditor.UI.Wizard;

namespace Innoactive.CreatorEditor.XRInteraction
{
    /// <summary>
    /// Helper class for alerting a potential risk of the new Input System affecting all IMGUI interactions.
    /// </summary>
    [InitializeOnLoad]
    public class InputSystemChecker
    {
        static InputSystemChecker()
        {
            CreatorSetupWizard.SetupFinished += OnCreatorSetupFinished;
        }

        private static void OnCreatorSetupFinished(object sender, EventArgs e)
        {
            const string message = "The new InputSystem is enabled. This currently disables UIs created with IMGUI.\n\nTo fix this go to \nEdit > ProjectSettings... > Player > Other Settings and set \"Active Input Handling\" to \"Both\"";
            EditorUtility.DisplayDialog("Warning", message, "Understood");
        }
    }
}
#endif

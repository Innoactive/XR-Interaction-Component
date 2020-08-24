using Innoactive.Creator.XRInteraction;
using UnityEditor;
using UnityEngine;

namespace Innoactive.CreatorEditor.XRInteraction
{
    /// <summary>
    /// Drawer class for <see cref="DirectInteractor"/>.
    /// </summary>
    [CustomEditor(typeof(DirectInteractor))]
    internal class DirectInteractorEditor : Editor
    {
        private SerializedProperty interactionManager;
        private SerializedProperty interactionLayerMask;
        private SerializedProperty attachTransform;
        private SerializedProperty startingSelectedInteractable;
        private SerializedProperty selectActionTrigger;
        private SerializedProperty hideControllerOnSelect;
        private SerializedProperty precisionGrab;

        private SerializedProperty playAudioClipOnSelectEnter;
        private SerializedProperty audioClipForOnSelectEnter;
        private SerializedProperty playAudioClipOnSelectExit;
        private SerializedProperty audioClipForOnSelectExit;
        private SerializedProperty playAudioClipOnHoverEnter;
        private SerializedProperty audioClipForOnHoverEnter;
        private SerializedProperty playAudioClipOnHoverExit;
        private SerializedProperty audioClipForOnHoverExit;

        private SerializedProperty playHapticsOnSelectEnter;
        private SerializedProperty hapticSelectEnterIntensity;
        private SerializedProperty hapticSelectEnterDuration;
        private SerializedProperty playHapticsOnHoverEnter;
        private SerializedProperty hapticHoverEnterIntensity;
        private SerializedProperty hapticHoverEnterDuration;
        private SerializedProperty playHapticsOnSelectExit;
        private SerializedProperty hapticSelectExitIntensity;
        private SerializedProperty hapticSelectExitDuration;
        private SerializedProperty playHapticsOnHoverExit;
        private SerializedProperty hapticHoverExitIntensity;
        private SerializedProperty hapticHoverExitDuration;

        private SerializedProperty onHoverEnter;
        private SerializedProperty onHoverExit;
        private SerializedProperty onSelectEnter;
        private SerializedProperty onSelectExit;
        
        private bool showInteractorEvents;
        private bool showSoundEvents = false;
        private bool showHapticEvents = false;

        private static class Tooltips
        {
            public static readonly GUIContent InteractionManager = new GUIContent("Interaction Manager", "Manager to handle all interaction management (will find one if empty).");
            public static readonly GUIContent InteractionLayerMask = new GUIContent("Interaction Layer Mask", "Only interactables with this Layer Mask will respond to this interactor.");
            public static readonly GUIContent AttachTransform = new GUIContent("Attach Transform", "Attach Transform to use for this Interactor.  Will create empty GameObject if none set.");
            public static readonly GUIContent StartingSelectedInteractable = new GUIContent("Starting Selected Interactable", "Interactable that will be selected upon start.");
            public static readonly GUIContent SelectActionTrigger = new GUIContent("Select Action Trigger", "Choose whether the select action is triggered by current state or state transitions.");
            public static readonly GUIContent HideControllerOnSelect = new GUIContent("Hide Controller On Select", "Hide controller on select.");
            public static readonly GUIContent PrecisionGrab = new GUIContent("Precision Grab", "Toggles precision grab on this interactor.");
            public static readonly GUIContent PlayAudioClipOnSelectEnter = new GUIContent("Play AudioClip On Select Enter", "Play an audio clip when the Select state is entered.");
            public static readonly GUIContent AudioClipForOnSelectEnter = new GUIContent("AudioClip To Play On Select Enter", "The audio clip to play when the Select state is entered.");
            public static readonly GUIContent PlayAudioClipOnSelectExit = new GUIContent("Play AudioClip On Select Exit", "Play an audio clip when the Select state is exited.");
            public static readonly GUIContent AudioClipForOnSelectExit = new GUIContent("AudioClip To Play On Select Exit", "The audio clip to play when the Select state is exited.");
            public static readonly GUIContent PlayAudioClipOnHoverEnter = new GUIContent("Play AudioClip On Hover Enter", "Play an audio clip when the Hover state is entered.");
            public static readonly GUIContent AudioClipForOnHoverEnter = new GUIContent("AudioClip To Play On Hover Enter", "The audio clip to play when the Hover state is entered.");
            public static readonly GUIContent PlayAudioClipOnHoverExit = new GUIContent("Play AudioClip On Hover Exit", "Play an audio clip when the Hover state is exited.");
            public static readonly GUIContent AudioClipForOnHoverExit = new GUIContent("AudioClip To Play On Hover Exit", "The audio clip to play when the Hover state is exited.");
            public static readonly GUIContent PlayHapticsOnSelectEnter = new GUIContent("Play Haptics On Select Enter", "Play haptics when the select state is entered.");
            public static readonly GUIContent HapticSelectEnterIntensity = new GUIContent("Haptic Select Enter Intensity", "Haptics intensity to play when the select state is entered.");
            public static readonly GUIContent HapticSelectEnterDuration = new GUIContent("Haptic Select Enter Duration", "Haptics Duration to play when the select state is entered.");
            public static readonly GUIContent PlayHapticsOnHoverEnter = new GUIContent("Play Haptics On Hover Enter", "Play haptics when the hover state is entered.");
            public static readonly GUIContent HapticHoverEnterIntensity = new GUIContent("Haptic Hover Enter Intensity", "Haptics intensity to play when the hover state is entered.");
            public static readonly GUIContent HapticHoverEnterDuration = new GUIContent("Haptic Hover Enter Duration", "Haptics Duration to play when the hover state is entered.");
            public static readonly GUIContent PlayHapticsOnSelectExit = new GUIContent("Play Haptics On Select Exit", "Play haptics when the select state is exited.");
            public static readonly GUIContent HapticSelectExitIntensity = new GUIContent("Haptic Select Exit Intensity", "Haptics intensity to play when the select state is exited.");
            public static readonly GUIContent HapticSelectExitDuration = new GUIContent("Haptic Select Exit Duration", "Haptics Duration to play when the select state is exited.");
            public static readonly GUIContent PlayHapticsOnHoverExit = new GUIContent("Play Haptics On Hover Exit", "Play haptics when the hover state is exited.");
            public static readonly GUIContent HapticHoverExitIntensity = new GUIContent("Haptic Hover Exit Intensity", "Haptics intensity to play when the hover state is exited.");
            public static readonly GUIContent HapticHoverExitDuration = new GUIContent("Haptic Hover Exit Duration", "Haptics Duration to play when the hover state is exited.");
            public static readonly string StartingInteractableWarning = "A Starting Selected Interactable will be instantly deselected unless the Interactor's Toggle Select Mode is set to 'Toggle' or 'Sticky'.";
        }

        private void OnEnable()
        {
            interactionManager = serializedObject.FindProperty("m_InteractionManager");
            interactionLayerMask = serializedObject.FindProperty("m_InteractionLayerMask");
            attachTransform = serializedObject.FindProperty("m_AttachTransform");
            startingSelectedInteractable = serializedObject.FindProperty("m_StartingSelectedInteractable");
            selectActionTrigger = serializedObject.FindProperty("m_SelectActionTrigger");
            
            precisionGrab = serializedObject.FindProperty("precisionGrab");
            hideControllerOnSelect = serializedObject.FindProperty("m_HideControllerOnSelect");
            playAudioClipOnSelectEnter = serializedObject.FindProperty("m_PlayAudioClipOnSelectEnter");
            audioClipForOnSelectEnter = serializedObject.FindProperty("m_AudioClipForOnSelectEnter");
            playAudioClipOnSelectExit = serializedObject.FindProperty("m_PlayAudioClipOnSelectExit");
            audioClipForOnSelectExit = serializedObject.FindProperty("m_AudioClipForOnSelectExit");
            playAudioClipOnHoverEnter = serializedObject.FindProperty("m_PlayAudioClipOnHoverEnter");
            audioClipForOnHoverEnter = serializedObject.FindProperty("m_AudioClipForOnHoverEnter");
            playAudioClipOnHoverExit = serializedObject.FindProperty("m_PlayAudioClipOnHoverExit");
            audioClipForOnHoverExit = serializedObject.FindProperty("m_AudioClipForOnHoverExit");
            playHapticsOnSelectEnter = serializedObject.FindProperty("m_PlayHapticsOnSelectEnter");
            hapticSelectEnterIntensity = serializedObject.FindProperty("m_HapticSelectEnterIntensity");
            hapticSelectEnterDuration = serializedObject.FindProperty("m_HapticSelectEnterDuration");
            playHapticsOnHoverEnter = serializedObject.FindProperty("m_PlayHapticsOnHoverEnter");
            hapticHoverEnterIntensity = serializedObject.FindProperty("m_HapticHoverEnterIntensity");
            hapticHoverEnterDuration = serializedObject.FindProperty("m_HapticHoverEnterDuration");
            playHapticsOnSelectExit = serializedObject.FindProperty("m_PlayHapticsOnSelectExit");
            hapticSelectExitIntensity = serializedObject.FindProperty("m_HapticSelectExitIntensity");
            hapticSelectExitDuration = serializedObject.FindProperty("m_HapticSelectExitDuration");
            playHapticsOnHoverExit = serializedObject.FindProperty("m_PlayHapticsOnHoverExit");
            hapticHoverExitIntensity = serializedObject.FindProperty("m_HapticHoverExitIntensity");
            hapticHoverExitDuration = serializedObject.FindProperty("m_HapticHoverExitDuration");

            onSelectEnter = serializedObject.FindProperty("m_OnSelectEnter");
            onSelectExit = serializedObject.FindProperty("m_OnSelectExit");
            onHoverEnter = serializedObject.FindProperty("m_OnHoverEnter");
            onHoverExit = serializedObject.FindProperty("m_OnHoverExit");
        }

        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((DirectInteractor)target), typeof(DirectInteractor), false);
            GUI.enabled = true;
            
            serializedObject.Update();

            EditorGUILayout.PropertyField(interactionManager, Tooltips.InteractionManager);
            EditorGUILayout.PropertyField(interactionLayerMask, Tooltips.InteractionLayerMask);
            EditorGUILayout.PropertyField(attachTransform, Tooltips.AttachTransform);
            EditorGUILayout.PropertyField(startingSelectedInteractable, Tooltips.StartingSelectedInteractable);
            EditorGUILayout.PropertyField(selectActionTrigger, Tooltips.SelectActionTrigger);
            
            if (startingSelectedInteractable.objectReferenceValue != null && (selectActionTrigger.enumValueIndex == 2 || selectActionTrigger.enumValueIndex == 3))
            {
                EditorGUILayout.HelpBox(Tooltips.StartingInteractableWarning, MessageType.Warning, true);
            }
            
            EditorGUILayout.PropertyField(hideControllerOnSelect, Tooltips.HideControllerOnSelect);
            EditorGUILayout.PropertyField(precisionGrab, Tooltips.PrecisionGrab);

            EditorGUILayout.Space();

            showSoundEvents = EditorGUILayout.Foldout(showSoundEvents, "Sound Events");
            if (showSoundEvents)
            {
                EditorGUILayout.PropertyField(playAudioClipOnSelectEnter, Tooltips.PlayAudioClipOnSelectEnter);
                
                if (playAudioClipOnSelectEnter.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(audioClipForOnSelectEnter, Tooltips.AudioClipForOnSelectEnter);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.PropertyField(playAudioClipOnSelectExit, Tooltips.PlayAudioClipOnSelectExit);
                
                if (playAudioClipOnSelectExit.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(audioClipForOnSelectExit, Tooltips.AudioClipForOnSelectExit);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.PropertyField(playAudioClipOnHoverEnter, Tooltips.PlayAudioClipOnHoverEnter);
                
                if (playAudioClipOnHoverEnter.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(audioClipForOnHoverEnter, Tooltips.AudioClipForOnHoverEnter);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.PropertyField(playAudioClipOnHoverExit, Tooltips.PlayAudioClipOnHoverExit);
                
                if (playAudioClipOnHoverExit.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(audioClipForOnHoverExit, Tooltips.AudioClipForOnHoverExit);
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUILayout.Space();

            showHapticEvents = EditorGUILayout.Foldout(showHapticEvents, "Haptic Events");
            if (showHapticEvents)
            {
                EditorGUILayout.PropertyField(playHapticsOnSelectEnter, Tooltips.PlayHapticsOnSelectEnter);
                
                if (playHapticsOnSelectEnter.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(hapticSelectEnterIntensity, Tooltips.HapticSelectEnterIntensity);
                    EditorGUILayout.PropertyField(hapticSelectEnterDuration, Tooltips.HapticSelectEnterDuration);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.PropertyField(playHapticsOnSelectExit, Tooltips.PlayHapticsOnSelectExit);
                
                if (playHapticsOnSelectExit.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(hapticSelectExitIntensity, Tooltips.HapticSelectExitIntensity);
                    EditorGUILayout.PropertyField(hapticSelectExitDuration, Tooltips.HapticSelectExitDuration);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.PropertyField(playHapticsOnHoverEnter, Tooltips.PlayHapticsOnHoverEnter);
                
                if (playHapticsOnHoverEnter.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(hapticHoverEnterIntensity, Tooltips.HapticHoverEnterIntensity);
                    EditorGUILayout.PropertyField(hapticHoverEnterDuration, Tooltips.HapticHoverEnterDuration);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.PropertyField(playHapticsOnHoverExit, Tooltips.PlayHapticsOnHoverExit);
                
                if (playHapticsOnHoverExit.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(hapticHoverExitIntensity, Tooltips.HapticHoverExitIntensity);
                    EditorGUILayout.PropertyField(hapticHoverExitDuration, Tooltips.HapticHoverExitDuration);
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUILayout.Space();

            showInteractorEvents = EditorGUILayout.Toggle("Show Interactor Events", showInteractorEvents);
            if (showInteractorEvents)
            {
                // UnityEvents have not yet supported Tooltips
                EditorGUILayout.PropertyField(onHoverEnter);
                EditorGUILayout.PropertyField(onHoverExit);
                EditorGUILayout.PropertyField(onSelectEnter);
                EditorGUILayout.PropertyField(onSelectExit);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
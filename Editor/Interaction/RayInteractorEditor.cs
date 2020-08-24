using UnityEditor;
using UnityEngine;
using Innoactive.Creator.XRInteraction;

namespace Innoactive.CreatorEditor.XRInteraction
{
    /// <summary>
    /// Drawer class for <see cref="RayInteractor"/>.
    /// </summary>
    [CustomEditor(typeof(RayInteractor))]
    [CanEditMultipleObjects]
    internal class RayInteractorEditor : Editor
    {
        private SerializedProperty interactionManager;
        private SerializedProperty interactionLayerMask;
        private SerializedProperty attachTransform;
        private SerializedProperty startingSelectedInteractable;
        private SerializedProperty selectActionTrigger;
        private SerializedProperty hideControllerOnSelect;

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

        private SerializedProperty maxRaycastDistance;
        private SerializedProperty hitDetectionType;
        private SerializedProperty sphereCastRadius;        
        private SerializedProperty raycastMask;
        private SerializedProperty raycastTriggerInteraction;
        private SerializedProperty hoverToSelect;
        private SerializedProperty hoverTimeToSelect;
        private SerializedProperty enableUIInteraction;

        private SerializedProperty lineType;
        private SerializedProperty endPointDistance;
        private SerializedProperty endPointHeight;
        private SerializedProperty controlPointDistance;
        private SerializedProperty controlPointHeight;
        private SerializedProperty sampleFrequency;

        private SerializedProperty velocity;
        private SerializedProperty acceleration;
        private SerializedProperty additionalFlightTime;
        private SerializedProperty referenceFrame;

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
            public static readonly GUIContent SelectActionTrigger = new GUIContent("Select Action Trigger", "Choose how the select action is triggered by current state, state transitions, toggle when the select button is pressed, or [Sticky] toggle on when the select button is pressed and off the second time the select button is depressed.");
            public static readonly GUIContent HideControllerOnSelect = new GUIContent("Hide Controller On Select", "Hide controller on select.");
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
            public static readonly GUIContent MAXRaycastDistance = new GUIContent("Max Raycast Distance", "Max distance of ray cast. Increase this value will let you reach further.");
            public static readonly GUIContent SphereCastRadius = new GUIContent("Sphere Cast Radius", "Radius of this Interactor's ray, used for spherecasting.");
            public static readonly GUIContent RaycastMask = new GUIContent("Raycast Mask", "Layer mask used for limiting raycast targets.");
            public static readonly GUIContent RaycastTriggerInteraction = new GUIContent("Raycast Trigger Interaction", "Type of interaction with trigger colliders via raycast.");
            public static readonly GUIContent HoverToSelect = new GUIContent("Hover To Select", "If true, this interactor will simulate a Select event if hovered over an Interactable for some amount of time. Selection will be exited when the Interactor is no longer hovering over the Interactable.");
            public static readonly GUIContent HoverTimeToSelect = new GUIContent("Hover Time To Select", "Number of seconds for which this interactor must hover over an object to select it.");
            public static readonly GUIContent EnableUIInteraction = new GUIContent("Enable Interaction with UI GameObjects", "If checked, this interactor will be able to affect UI.");
            public static readonly GUIContent LineType = new GUIContent("Line Type", "Line type of the ray cast.");
            public static readonly GUIContent EndPointDistance = new GUIContent("End Point Distance", "Increase this value distance will make the end of curve further from the start point.");
            public static readonly GUIContent ControlPointDistance = new GUIContent("Control Point Distance", "Increase this value will make the peak of the curve further from the start point.");
            public static readonly GUIContent EndPointHeight = new GUIContent("End Point Height", "Decrease this value will make the end of the curve drop lower relative to the start point.");
            public static readonly GUIContent ControlPointHeight = new GUIContent("Control Point Height", "Increase this value will make the peak of the curve higher relative to the start point.");
            public static readonly GUIContent SampleFrequency = new GUIContent("Sample Frequency", "Gets or sets the number of sample points of the curve, should be at least 3, the higher the better quality.");
            public static readonly GUIContent Velocity = new GUIContent("Velocity", "Initial velocity of the projectile. Increase this value will make the curve reach further.");
            public static readonly GUIContent Acceleration = new GUIContent("Acceleration", "Gravity of the projectile in the reference frame.");
            public static readonly GUIContent AdditionalFlightTime = new GUIContent("Additional FlightTime", "Additional flight time after the projectile lands at the same height of the start point in the tracking space. Increase this value will make the end point drop lower in height.");
            public static readonly GUIContent ReferenceFrame = new GUIContent("Reference Frame", "The reference frame of the projectile. If not set it will try to find the XRRig object, if the XRRig does not exist it will use self");
            public static readonly GUIContent HitDetectionType = new GUIContent("Hit Detection Type", "The type of hit detection used to hit interactable objects.");
            public static readonly string StartingInteractableWarning = "A Starting Selected Interactable will be instantly deselected unless the Interactor's Toggle Select Mode is set to 'Toggle' or 'Sticky'.";
        }

        private void OnEnable()
        {
            interactionManager = serializedObject.FindProperty("m_InteractionManager");
            interactionLayerMask = serializedObject.FindProperty("m_InteractionLayerMask");
            attachTransform = serializedObject.FindProperty("m_AttachTransform");
            startingSelectedInteractable = serializedObject.FindProperty("m_StartingSelectedInteractable");
            selectActionTrigger = serializedObject.FindProperty("m_SelectActionTrigger");
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
            maxRaycastDistance = serializedObject.FindProperty("m_MaxRaycastDistance");
            sphereCastRadius = serializedObject.FindProperty("m_SphereCastRadius");
            hitDetectionType = serializedObject.FindProperty("m_HitDetectionType");
            raycastMask = serializedObject.FindProperty("m_RaycastMask");
            raycastTriggerInteraction = serializedObject.FindProperty("m_RaycastTriggerInteraction");
            hoverToSelect = serializedObject.FindProperty("m_HoverToSelect");
            hoverTimeToSelect = serializedObject.FindProperty("m_HoverTimeToSelect");
            enableUIInteraction = serializedObject.FindProperty("m_EnableUIInteraction");

            lineType = serializedObject.FindProperty("m_LineType");
            endPointDistance = serializedObject.FindProperty("m_EndPointDistance");
            endPointHeight = serializedObject.FindProperty("m_EndPointHeight");
            controlPointDistance = serializedObject.FindProperty("m_ControlPointDistance");
            controlPointHeight = serializedObject.FindProperty("m_ControlPointHeight");
            sampleFrequency = serializedObject.FindProperty("m_SampleFrequency");

            referenceFrame = serializedObject.FindProperty("m_ReferenceFrame");
            velocity = serializedObject.FindProperty("m_Velocity");
            acceleration = serializedObject.FindProperty("m_Acceleration");
            additionalFlightTime = serializedObject.FindProperty("m_AdditionalFlightTime");

            onHoverEnter = serializedObject.FindProperty("m_OnHoverEnter");
            onHoverExit = serializedObject.FindProperty("m_OnHoverExit");
            onSelectEnter = serializedObject.FindProperty("m_OnSelectEnter");
            onSelectExit = serializedObject.FindProperty("m_OnSelectExit");
        }

        public override void OnInspectorGUI()
        {

            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((RayInteractor)target), typeof(RayInteractor), false);
            GUI.enabled = true;

            serializedObject.Update();

            EditorGUILayout.PropertyField(interactionManager, Tooltips.InteractionManager);
            EditorGUILayout.PropertyField(interactionLayerMask, Tooltips.InteractionLayerMask);
            EditorGUILayout.PropertyField(attachTransform, Tooltips.AttachTransform);
            EditorGUILayout.PropertyField(startingSelectedInteractable, Tooltips.StartingSelectedInteractable);
            EditorGUILayout.PropertyField(selectActionTrigger, Tooltips.SelectActionTrigger);
            if (startingSelectedInteractable.objectReferenceValue != null 
                && (selectActionTrigger.enumValueIndex == 2 || selectActionTrigger.enumValueIndex == 3))
            {
                EditorGUILayout.HelpBox(Tooltips.StartingInteractableWarning, MessageType.Warning, true);
            }
            EditorGUILayout.PropertyField(hideControllerOnSelect, Tooltips.HideControllerOnSelect);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(lineType, Tooltips.LineType);

            EditorGUI.indentLevel++;
            if(lineType.enumValueIndex  == 0) // straight line
            {
                 EditorGUILayout.PropertyField(maxRaycastDistance, Tooltips.MAXRaycastDistance);
            }
            else if (lineType.enumValueIndex == 1) // projectile
            {
                EditorGUILayout.PropertyField(referenceFrame, Tooltips.ReferenceFrame);
                EditorGUILayout.PropertyField(velocity, Tooltips.Velocity);
                EditorGUILayout.PropertyField(acceleration, Tooltips.Acceleration);
                EditorGUILayout.PropertyField(additionalFlightTime, Tooltips.AdditionalFlightTime);
                EditorGUILayout.PropertyField(sampleFrequency, Tooltips.SampleFrequency);
            }

            else if (lineType.enumValueIndex == 2) // bezier
            {
                EditorGUILayout.PropertyField(endPointDistance, Tooltips.EndPointDistance);
                EditorGUILayout.PropertyField(endPointHeight, Tooltips.EndPointHeight);
                EditorGUILayout.PropertyField(controlPointDistance, Tooltips.ControlPointDistance);
                EditorGUILayout.PropertyField(controlPointHeight, Tooltips.ControlPointHeight);
                EditorGUILayout.PropertyField(sampleFrequency, Tooltips.SampleFrequency);
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(hitDetectionType, Tooltips.HitDetectionType);
            using (new EditorGUI.DisabledScope(hitDetectionType.enumValueIndex != (int)RayInteractor.HitDetectionType.SphereCast))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(sphereCastRadius, Tooltips.SphereCastRadius);
                EditorGUI.indentLevel--;
            }
        
            EditorGUILayout.Space();


            EditorGUILayout.PropertyField(raycastMask, Tooltips.RaycastMask);
            EditorGUILayout.PropertyField(raycastTriggerInteraction, Tooltips.RaycastTriggerInteraction);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(hoverToSelect, Tooltips.HoverToSelect);
            if (hoverToSelect.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(hoverTimeToSelect, Tooltips.HoverTimeToSelect);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(enableUIInteraction, Tooltips.EnableUIInteraction);

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

            showInteractorEvents = EditorGUILayout.Foldout(showInteractorEvents, "Interactor Events");

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
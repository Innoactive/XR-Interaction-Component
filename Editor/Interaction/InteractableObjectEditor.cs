using Innoactive.Creator.XRInteraction;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace Innoactive.CreatorEditor.XRInteraction
{
    /// <summary>
    /// Copy of 'XRGrabInteractableEditor' adapted to edit <see cref="InteractableObject"/>.
    /// </summary>
    [CustomEditor(typeof(InteractableObject))]
    internal class InteractableObjectEditor : Editor
    {
        private SerializedProperty attachTransformProperty;
        private SerializedProperty attachEaseInTimeProperty;
        private SerializedProperty movementTypeProperty;
        private SerializedProperty trackPositionProperty;
        private SerializedProperty smoothPositionProperty;
        private SerializedProperty smoothPositionAmountProperty;
        private SerializedProperty tightenPositionProperty;
        private SerializedProperty trackRotationProperty;
        private SerializedProperty smoothRotationProperty;
        private SerializedProperty smoothRotationAmountProperty;
        private SerializedProperty tightenRotationProperty;
        private SerializedProperty throwOnDetachProperty;
        private SerializedProperty throwSmoothingDurationProperty;
        private SerializedProperty throwSmoothingCurveProperty;
        private SerializedProperty throwVelocityScaleProperty;
        private SerializedProperty throwAngularVelocityScaleProperty;
        private SerializedProperty gravityOnDetachProperty;
        private SerializedProperty retainTransformParentProperty;
        private SerializedProperty onFirstHoverEnterProperty;
        private SerializedProperty onHoverEnterProperty;
        private SerializedProperty onHoverExitProperty;
        private SerializedProperty onLastHoverExitProperty;
        private SerializedProperty onSelectEnterProperty;
        private SerializedProperty onSelectExitProperty;
        private SerializedProperty onActivateProperty;
        private SerializedProperty onDeactivateProperty;
        private SerializedProperty collidersProperty;
        private SerializedProperty interactionLayerMaskProperty;
        private SerializedProperty isTouchableProperty;
        private SerializedProperty isGrabbableProperty;
        private SerializedProperty isUsableProperty;

        private bool showInteractableEvents;
        private bool showInteractionOptions;
        private bool showHighlightOptions;
        private InteractableObject interactableObject;

        static class Tooltips
        {
            public static readonly GUIContent AttachTransform = new GUIContent("Attach Transform", "Attach point to use on this Interactable (will use RigidBody center if none set).");
            public static readonly GUIContent AttachEaseInTime = new GUIContent("Attach Ease In Time", "Time it takes to ease in the attach (time of 0.0 indicates no easing).");
            public static readonly GUIContent MovementType = new GUIContent("Movement Type", "Type of movement for RigidBody.");
            public static readonly GUIContent TrackPosition = new GUIContent("Track Position", "Whether the this interactable should track the position of the interactor.");
            public static readonly GUIContent SmoothPosition = new GUIContent("Smooth Position", "Apply smoothing to the follow position of the object.");
            public static readonly GUIContent SmoothPositionAmount = new GUIContent("Smooth Position Amount", "Smoothing applied to the object's position when following.");
            public static readonly GUIContent TightenPosition = new GUIContent("Tighten Position", "Reduces the maximum follow position difference when using smoothing.");
            public static readonly GUIContent TrackRotation = new GUIContent("Track Rotation", "Whether the this interactable should track the rotation of the interactor.");
            public static readonly GUIContent SmoothRotation = new GUIContent("Smooth Rotation", "Apply smoothing to the follow rotation of the object.");
            public static readonly GUIContent SmoothRotationAmount = new GUIContent("Smooth Rotation Amount", "Smoothing multiple applied to the object's rotation when following.");
            public static readonly GUIContent TightenRotation = new GUIContent("Tighten Rotation", "Reduces the maximum follow rotation difference when using smoothing.");
            public static readonly GUIContent ThrowOnDetach = new GUIContent("Throw On Detach", "Object inherits the interactor's velocity when released.");
            public static readonly GUIContent ThrowSmoothingDuration = new GUIContent("Throw Smoothing Duration", "Time period to average thrown velocity over");
            public static readonly GUIContent ThrowSmoothingCurve = new GUIContent("ThrowSmoothingCurve", "The curve to use to weight velocity smoothing (most recent frames to the right.");
            public static readonly GUIContent ThrowVelocityScale = new GUIContent("Throw Velocity Scale", "Scale the velocity used when throwing.");
            public static readonly GUIContent ThrowAngularVelocityScale = new GUIContent("Throw Angular Velocity Scale", "Scale the angular velocity used when throwing");
            public static readonly GUIContent GravityOnDetach = new GUIContent("Gravity On Detach", "Object has gravity when released (will still use pre-grab value if this is false).");
            public static readonly GUIContent Colliders = new GUIContent("Colliders", "Colliders to include when selecting/interacting with an interactable");
            public static readonly GUIContent InteractionLayerMask = new GUIContent("InteractionLayerMask", "Only Interactors with this LayerMask will interact with this Interactable.");
            public static readonly GUIContent RetainTransformParent = new GUIContent("RetainTransformParent", "If enabled, this Interactable have its parent retained after it is released from an interactor.");
            public static readonly GUIContent IsTouchable = new GUIContent("Is Touchable", "Determines if this Interactable Object can be touched.");
            public static readonly GUIContent IsGrabbable = new GUIContent("Is Grabbable", "Determines if this Interactable Object can be grabbed.");
            public static readonly GUIContent IsUsable = new GUIContent("Is Usable", "Determines if this Interactable Object can be used.");
            public static readonly GUIContent HighlightOptions = new GUIContent("Enable Highlighting", "Adds an InteractableHighlighter component to this Interactable.");
        }

        private void OnEnable()
        {
            interactableObject = target as InteractableObject;
            showHighlightOptions = interactableObject.GetComponent<InteractableHighlighter>() == null;

            attachTransformProperty = serializedObject.FindProperty("m_AttachTransform");
            movementTypeProperty = serializedObject.FindProperty("m_MovementType");
            attachEaseInTimeProperty = serializedObject.FindProperty("m_AttachEaseInTime");
            trackPositionProperty = serializedObject.FindProperty("m_TrackPosition");
            smoothPositionProperty = serializedObject.FindProperty("m_SmoothPosition");
            smoothPositionAmountProperty = serializedObject.FindProperty("m_SmoothPositionAmount");
            tightenPositionProperty = serializedObject.FindProperty("m_TightenPosition");
            trackRotationProperty = serializedObject.FindProperty("m_TrackRotation");
            smoothRotationProperty = serializedObject.FindProperty("m_SmoothRotation");
            smoothRotationAmountProperty = serializedObject.FindProperty("m_SmoothRotationAmount");
            tightenRotationProperty = serializedObject.FindProperty("m_TightenRotation");
            throwOnDetachProperty = serializedObject.FindProperty("m_ThrowOnDetach");
            throwSmoothingDurationProperty = serializedObject.FindProperty("m_ThrowSmoothingDuration");
            throwSmoothingCurveProperty = serializedObject.FindProperty("m_ThrowSmoothingCurve");
            throwVelocityScaleProperty = serializedObject.FindProperty("m_ThrowVelocityScale");
            throwAngularVelocityScaleProperty = serializedObject.FindProperty("m_ThrowAngularVelocityScale");
            gravityOnDetachProperty = serializedObject.FindProperty("m_GravityOnDetach");
            retainTransformParentProperty = serializedObject.FindProperty("m_RetainTransformParent");
            onFirstHoverEnterProperty = serializedObject.FindProperty("m_OnFirstHoverEnter");
            onHoverEnterProperty = serializedObject.FindProperty("m_OnHoverEnter");
            onHoverExitProperty = serializedObject.FindProperty("m_OnHoverExit");
            onLastHoverExitProperty = serializedObject.FindProperty("m_OnLastHoverExit");
            onSelectEnterProperty = serializedObject.FindProperty("m_OnSelectEnter");
            onSelectExitProperty = serializedObject.FindProperty("m_OnSelectExit");
            onActivateProperty = serializedObject.FindProperty("m_OnActivate");
            onDeactivateProperty = serializedObject.FindProperty("m_OnDeactivate");
            collidersProperty = serializedObject.FindProperty("m_Colliders");
            interactionLayerMaskProperty = serializedObject.FindProperty("m_InteractionLayerMask");
            isTouchableProperty = serializedObject.FindProperty("isTouchable");
            isGrabbableProperty = serializedObject.FindProperty("isGrabbable");
            isUsableProperty = serializedObject.FindProperty("isUsable");
        }

        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour(interactableObject), typeof(InteractableObject), false);
            GUI.enabled = true;

            serializedObject.Update();
            
            showInteractionOptions = EditorGUILayout.Foldout(showInteractionOptions, "Interactable Options");
            if (showInteractionOptions)
            {
                EditorGUILayout.PropertyField(isTouchableProperty, Tooltips.IsTouchable);
                EditorGUILayout.PropertyField(isGrabbableProperty, Tooltips.IsGrabbable);
                EditorGUILayout.PropertyField(isUsableProperty, Tooltips.IsUsable);
            }

            EditorGUILayout.PropertyField(attachTransformProperty, Tooltips.AttachTransform);
            EditorGUILayout.PropertyField(attachEaseInTimeProperty, Tooltips.AttachEaseInTime);
            EditorGUILayout.PropertyField(movementTypeProperty, Tooltips.MovementType);

            EditorGUILayout.PropertyField(collidersProperty, Tooltips.Colliders, true);

            EditorGUILayout.PropertyField(interactionLayerMaskProperty, Tooltips.InteractionLayerMask);

            EditorGUILayout.PropertyField(retainTransformParentProperty, Tooltips.RetainTransformParent);

            EditorGUILayout.PropertyField(trackPositionProperty, Tooltips.TrackPosition);
            if (trackPositionProperty.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(smoothPositionProperty, Tooltips.SmoothPosition);
                if (smoothPositionProperty.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(smoothPositionAmountProperty, Tooltips.SmoothPositionAmount);
                    EditorGUILayout.PropertyField(tightenPositionProperty, Tooltips.TightenPosition);
                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(trackRotationProperty, Tooltips.TrackRotation);
            if (trackRotationProperty.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(smoothRotationProperty, Tooltips.SmoothRotation);
                if (smoothRotationProperty.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(smoothRotationAmountProperty, Tooltips.SmoothRotationAmount);
                    EditorGUILayout.PropertyField(tightenRotationProperty, Tooltips.TightenRotation);
                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(throwOnDetachProperty, Tooltips.ThrowOnDetach);
            if (throwOnDetachProperty.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(throwSmoothingDurationProperty, Tooltips.ThrowSmoothingDuration);
                EditorGUILayout.PropertyField(throwSmoothingCurveProperty, Tooltips.ThrowSmoothingCurve);
                EditorGUILayout.PropertyField(throwVelocityScaleProperty, Tooltips.ThrowVelocityScale);
                EditorGUILayout.PropertyField(throwAngularVelocityScaleProperty, Tooltips.ThrowAngularVelocityScale);
                EditorGUILayout.PropertyField(gravityOnDetachProperty, Tooltips.GravityOnDetach);
                EditorGUI.indentLevel--;
            }

            showInteractableEvents = EditorGUILayout.Foldout(showInteractableEvents, "Interactable Events");
            if (showInteractableEvents)
            {
                // UnityEvents have not yet supported Tooltips
                EditorGUILayout.PropertyField(onFirstHoverEnterProperty);
                EditorGUILayout.PropertyField(onHoverEnterProperty);
                EditorGUILayout.PropertyField(onHoverExitProperty);
                EditorGUILayout.PropertyField(onLastHoverExitProperty);
                EditorGUILayout.PropertyField(onSelectEnterProperty);
                EditorGUILayout.PropertyField(onSelectExitProperty);
                EditorGUILayout.PropertyField(onActivateProperty);
                EditorGUILayout.PropertyField(onDeactivateProperty);
            }

            if (showHighlightOptions && GUILayout.Button(Tooltips.HighlightOptions))
            {
                showHighlightOptions = false;
                interactableObject.gameObject.AddComponent<InteractableHighlighter>();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
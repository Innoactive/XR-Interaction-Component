using Innoactive.Creator.XRInteraction;
using UnityEditor;
using UnityEngine;

namespace Innoactive.CreatorEditor.XRInteraction
{
    /// <summary>
    /// Drawer class for <see cref="SnapZone"/>.
    /// </summary>
    [CustomEditor(typeof(SnapZone))]
    internal class SnapZoneEditor : Editor
    {
        private SerializedProperty shownHighlightObject;
        private SerializedProperty shownHighlightObjectColor;
        
        private SerializedProperty interactionManager;
        private SerializedProperty interactionLayerMask;
        private SerializedProperty attachTransform;
        private SerializedProperty startingSelectedInteractable;

        private SerializedProperty interactableHoverMeshMaterial;
        
        private SerializedProperty onHoverEnter;
        private SerializedProperty onHoverExit;
        private SerializedProperty onSelectEnter;
        private SerializedProperty onSelectExit;
        private bool showInteractorEvents;
        
        private static class Tooltips
        {
            public static readonly GUIContent InteractionManager = new GUIContent("Interaction Manager", "Manager to handle all interaction management (will find one if empty).");
            public static readonly GUIContent InteractionLayerMask = new GUIContent("Interaction Layer Mask", "Only interactables with this Layer Mask will respond to this interactor.");
            public static readonly GUIContent AttachTransform = new GUIContent("Attach Transform", "Attach Transform to use for this Interactor.  Will create empty GameObject if none set.");
            public static readonly GUIContent StartingSelectedInteractable = new GUIContent("Starting Selected Interactable", "Interactable that will be selected upon start.");
            
            public static readonly GUIContent ShownHighlightObject = new GUIContent("Shown Highlight Object", "The game object whose mesh is drawn to emphasize the position of the snap zone. If none is supplied, no highlight object is shown.");
            public static readonly GUIContent ShownHighlightObjectColor = new GUIContent("Shown Highlight Object Color", "The color of the material used to draw the \"Shown Highlight Object\". Use the alpha value to specify the degree of transparency.");
            public static readonly GUIContent InteractableHoverMeshMaterial = new GUIContent("Validation Hover Material", "Material used for rendering interactable meshes on hover (a default material will be created if none is supplied).");
        }

        private void OnEnable()
        {
            shownHighlightObject = serializedObject.FindProperty("shownHighlightObject");
            shownHighlightObjectColor = serializedObject.FindProperty("shownHighlightObjectColor");
            
            interactionManager = serializedObject.FindProperty("m_InteractionManager");
            interactionLayerMask = serializedObject.FindProperty("m_InteractionLayerMask");
            attachTransform = serializedObject.FindProperty("m_AttachTransform");
            startingSelectedInteractable = serializedObject.FindProperty("m_StartingSelectedInteractable");

            interactableHoverMeshMaterial = serializedObject.FindProperty("validationMaterial");
            
            onHoverEnter = serializedObject.FindProperty("m_OnHoverEnter");
            onHoverExit = serializedObject.FindProperty("m_OnHoverExit");
            onSelectEnter = serializedObject.FindProperty("m_OnSelectEnter");
            onSelectExit = serializedObject.FindProperty("m_OnSelectExit");
        }

        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((SnapZone)target), typeof(SnapZone), false);
            GUI.enabled = true;

            serializedObject.Update();

            EditorGUILayout.PropertyField(interactionManager, Tooltips.InteractionManager);
            EditorGUILayout.PropertyField(interactionLayerMask, Tooltips.InteractionLayerMask);
            EditorGUILayout.PropertyField(attachTransform, Tooltips.AttachTransform);
            EditorGUILayout.PropertyField(startingSelectedInteractable, Tooltips.StartingSelectedInteractable);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Snap Zone", EditorStyles.boldLabel); 
            
            EditorGUILayout.PropertyField(shownHighlightObject, Tooltips.ShownHighlightObject);
            EditorGUILayout.PropertyField(shownHighlightObjectColor, Tooltips.ShownHighlightObjectColor);
            EditorGUILayout.PropertyField(interactableHoverMeshMaterial, Tooltips.InteractableHoverMeshMaterial);
            
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

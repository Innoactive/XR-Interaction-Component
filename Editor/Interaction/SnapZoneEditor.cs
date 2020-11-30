﻿using Innoactive.Creator.XRInteraction;
using UnityEditor;
using UnityEngine;

namespace Innoactive.CreatorEditor.XRInteraction
{
    /// <summary>
    /// Drawer class for <see cref="SnapZone"/>.
    /// </summary>
    [CustomEditor(typeof(SnapZone)), CanEditMultipleObjects]
    internal class SnapZoneEditor : Editor
    {
        private SerializedProperty socketActive;
        private SerializedProperty showHighlightInEditor;
        private SerializedProperty shownHighlightObject;
        private SerializedProperty shownHighlightObjectColor;

        private SerializedProperty interactionManager;
        private SerializedProperty interactionLayerMask;
        private SerializedProperty attachTransform;
        private SerializedProperty startingSelectedInteractable;

        private SerializedProperty interactableHoverMeshMaterial;
        
        private SerializedProperty onHoverEntered;
        private SerializedProperty onHoverExited;
        private SerializedProperty onSelectEntered;
        private SerializedProperty onSelectExited;
        
        private static class Tooltips
        {
            public static readonly GUIContent SocketActive = new GUIContent("Snap Zone Active", "Turn snap zone interaction on/off.");
            public static readonly GUIContent InteractionManager = new GUIContent("Interaction Manager", "Manager to handle all interaction management (will find one if empty).");
            public static readonly GUIContent InteractionLayerMask = new GUIContent("Interaction Layer Mask", "Only interactables with this Layer Mask will respond to this interactor.");
            public static readonly GUIContent AttachTransform = new GUIContent("Attach Transform", "Attach Transform to use for this Interactor.  Will create empty GameObject if none set.");
            public static readonly GUIContent StartingSelectedInteractable = new GUIContent("Starting Selected Interactable", "Interactable that will be selected upon start.");
            
            public static readonly GUIContent ShowHighlightInEditor = new GUIContent("Show Highlight in Editor", "Enable this to show how the object will be positioned later on.");
            public static readonly GUIContent ShownHighlightObject = new GUIContent("Shown Highlight Object", "The game object whose mesh is drawn to emphasize the position of the snap zone. If none is supplied, no highlight object is shown.");
            public static readonly GUIContent ShownHighlightObjectColor = new GUIContent("Shown Highlight Object Color", "The color of the material used to draw the \"Shown Highlight Object\". Use the alpha value to specify the degree of transparency.");
            public static readonly GUIContent InteractableHoverMeshMaterial = new GUIContent("Validation Hover Material", "Material used for rendering interactable meshes on hover (a default material will be created if none is supplied).");
        }

        private void OnEnable()
        {
            socketActive = serializedObject.FindProperty("m_SocketActive");
            showHighlightInEditor = serializedObject.FindProperty("ShowHighlightInEditor");
            shownHighlightObject = serializedObject.FindProperty("shownHighlightObject");
            shownHighlightObjectColor = serializedObject.FindProperty("shownHighlightObjectColor");
            
            interactionManager = serializedObject.FindProperty("m_InteractionManager");
            interactionLayerMask = serializedObject.FindProperty("m_InteractionLayerMask");
            attachTransform = serializedObject.FindProperty("m_AttachTransform");
            startingSelectedInteractable = serializedObject.FindProperty("m_StartingSelectedInteractable");

            interactableHoverMeshMaterial = serializedObject.FindProperty("validationMaterial");
            
#if XRIT_0_10_OR_NEWER
            onHoverEntered = serializedObject.FindProperty("m_OnHoverEntered");
            onHoverExited = serializedObject.FindProperty("m_OnHoverExited");
            onSelectEntered = serializedObject.FindProperty("m_OnSelectEntered");
            onSelectExited = serializedObject.FindProperty("m_OnSelectExited");
#else
            onHoverEntered = serializedObject.FindProperty("m_OnHoverEnter");
            onHoverExited = serializedObject.FindProperty("m_OnHoverExit");
            onSelectEntered = serializedObject.FindProperty("m_OnSelectEnter");
            onSelectExited = serializedObject.FindProperty("m_OnSelectExit");
#endif
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(EditorGUIUtility.TrTempContent("Script"), MonoScript.FromMonoBehaviour((SnapZone)target), typeof(SnapZone), false);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.PropertyField(interactionManager, Tooltips.InteractionManager);
            EditorGUILayout.PropertyField(interactionLayerMask, Tooltips.InteractionLayerMask);
            EditorGUILayout.PropertyField(attachTransform, Tooltips.AttachTransform);
            EditorGUILayout.PropertyField(startingSelectedInteractable, Tooltips.StartingSelectedInteractable);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(EditorGUIUtility.TrTempContent("Snap Zone"), EditorStyles.boldLabel); 
            
            EditorGUILayout.PropertyField(showHighlightInEditor, Tooltips.ShowHighlightInEditor);
            
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(shownHighlightObject, Tooltips.ShownHighlightObject);
            EditorGUILayout.PropertyField(shownHighlightObjectColor, Tooltips.ShownHighlightObjectColor);
            bool isPreviewMeshChanged = EditorGUI.EndChangeCheck();
            
            EditorGUILayout.PropertyField(interactableHoverMeshMaterial, Tooltips.InteractableHoverMeshMaterial);
            EditorGUI.indentLevel--;
            
            EditorGUILayout.PropertyField(socketActive, Tooltips.SocketActive);
            
            EditorGUILayout.Space();
            
            onHoverEntered.isExpanded = EditorGUILayout.Foldout(onHoverEntered.isExpanded, EditorGUIUtility.TrTempContent("Interactor Events"), true);
            
            if (onHoverEntered.isExpanded)
            {
                // UnityEvents have not yet supported Tooltips
                EditorGUILayout.PropertyField(onHoverEntered);
                EditorGUILayout.PropertyField(onHoverExited);
                EditorGUILayout.PropertyField(onSelectEntered);
                EditorGUILayout.PropertyField(onSelectExited);
            }
            
            serializedObject.ApplyModifiedProperties();

            if (isPreviewMeshChanged)
            {
                SnapZone snapZone = (SnapZone) target;
                snapZone.PreviewMesh = null;
                
                SnapZonePreviewDrawer preview = snapZone.attachTransform.gameObject.GetComponent<SnapZonePreviewDrawer>();
                
                if (preview != null)
                {
                    preview.UpdateMesh();
                }
            }
        }
    }
}

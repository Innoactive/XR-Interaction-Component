﻿using UnityEngine;
using UnityEditor;
using Innoactive.Creator.XRInteraction;

namespace Innoactive.CreatorEditor.XRInteraction
{
    /// <summary>
    /// Draws for configuration settings for <see cref="InteractableHighlighter"/>.
    /// </summary>
    [CustomEditor(typeof(InteractableHighlighter)), CanEditMultipleObjects]
    public class InteractableHighlighterEditor : Editor
    {
        /// <summary>
        /// Keeps references for different interaction highlight cases.
        /// </summary>
        internal class HighlightCase
        {
            /// <summary>
            /// Title and tooltip for a specified drawer.
            /// </summary>
            public readonly GUIContent GUISectionTitleContent;
            
            /// <summary>
            /// Title and tooltip for a specified drawer.
            /// </summary>
            public readonly GUIContent GUIColorPropertyContent = new GUIContent("Highlight Color", "Color to be used for highlighting this Interactable Object's section if no material is present.");
            
            /// <summary>
            /// Title and tooltip for a specified drawer.
            /// </summary>
            public readonly GUIContent GUIMaterialPropertyContent = new GUIContent("Highlight Material", "Material to be used for highlighting this Interactable Object's section.");
            
            /// <summary>
            /// Data stream to an specified field of <see cref="InteractableHighlighter"/>.
            /// </summary>
            public readonly SerializedProperty HighlightMaterialProperty;
            
            /// <summary>
            /// Data stream to an specified field of <see cref="InteractableHighlighter"/>.
            /// </summary>
            public readonly SerializedProperty HighlightColorProperty;

            /// <summary>
            /// Determines if this highlight case should be drawn.
            /// </summary>
            public bool ShowSection
            {
                get => HighlightEnablingProperty.boolValue;
                set => HighlightEnablingProperty.boolValue = value;
            }

            /// <summary>
            /// Keeps track of currently selected tab index.
            /// </summary>
            public int TabIndex;
            
            private SerializedProperty HighlightEnablingProperty;

            internal HighlightCase(SerializedObject serializedObject, string sectionTitle, string colorPropertyName, string materialPropertyName, string highlightEnablingPropertyName, bool showSection)
            {
                GUISectionTitleContent = new GUIContent(sectionTitle, $"Shows settings corresponding to {sectionTitle}");
                HighlightColorProperty = serializedObject.FindProperty(colorPropertyName);
                HighlightMaterialProperty = serializedObject.FindProperty(materialPropertyName);
                HighlightEnablingProperty = serializedObject.FindProperty(highlightEnablingPropertyName);
                ShowSection = showSection;
            }
        }
        
        private readonly string[] tabs = {"Color", "Material"};
        private HighlightCase onTouchHighlighting;
        private HighlightCase onGrabHighlighting;
        private HighlightCase onUseHighlighting;

        private void OnEnable()
        {
            onTouchHighlighting = new HighlightCase(serializedObject, "On Touch Highlight", "touchHighlightColor", "touchHighlightMaterial", "allowOnTouchHighlight", true);
            onGrabHighlighting = new HighlightCase(serializedObject, "On Grab Highlight", "grabHighlightColor", "grabHighlightMaterial", "allowOnGrabHighlight", false);
            onUseHighlighting = new HighlightCase(serializedObject, "On Use Highlight", "useHighlightColor", "useHighlightMaterial", "allowOnUseHighlight", false);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            DrawTouchHighlightSection(onTouchHighlighting);
            DrawTouchHighlightSection(onGrabHighlighting);
            DrawTouchHighlightSection(onUseHighlighting);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawTouchHighlightSection(HighlightCase highlightCase)
        {
            highlightCase.ShowSection = EditorGUILayout.ToggleLeft(highlightCase.GUISectionTitleContent, highlightCase.ShowSection);
            EditorGUILayout.Separator();
            
            if (highlightCase.ShowSection)
            {
                DrawHighlightOptions(highlightCase);
            }
        }

        private void DrawHighlightOptions(HighlightCase highlightCase)
        {
            highlightCase.TabIndex = GUILayout.Toolbar (highlightCase.TabIndex, tabs);
            EditorGUILayout.Separator();
            
            switch (highlightCase.TabIndex)
            {
                case 0:
                    EditorGUILayout.PropertyField(highlightCase.HighlightColorProperty, highlightCase.GUIColorPropertyContent);
                    break;
                case 1:
                    EditorGUILayout.PropertyField(highlightCase.HighlightMaterialProperty, highlightCase.GUIMaterialPropertyContent);
                    break;
            }
            
            EditorGUILayout.Separator();
        }
    }
}

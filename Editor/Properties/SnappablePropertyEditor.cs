using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Innoactive.Creator.XRInteraction;
using Innoactive.Creator.XRInteraction.Properties;
using UnityEditor;
using UnityEngine;

namespace Innoactive.CreatorEditor.XRInteraction
{
    /// <summary>
    /// Custom inspector for <see cref="SnappableProperty"/>, adding a button to create <see cref="Innoactive.Creator.XRInteraction.SnapZone"/>s automatically.
    /// </summary>
    [CustomEditor(typeof(SnappableProperty))]
    public class SnappablePropertyEditor : Editor
    {
        private const string PrefabPath = "Assets/Resources/SnapZones/Prefabs";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            SnappableProperty snappable = (SnappableProperty)target;

            EditorGUILayout.Space();

            if (GUILayout.Button("Create Snap Zone"))
            {
                CreateSnapZone(snappable);
            }
        }
        
        private void CreateSnapZone(SnappableProperty snappable)
        {
            SnapZoneSettings settings = SnapZoneSettings.Settings;
            GameObject snapZoneBlueprint = DuplicateObject(snappable.gameObject);
            
            SetHighlightMaterial(snapZoneBlueprint, settings.HighlightMaterial);
            GameObject snapZonePrefab = SaveSnapZonePrefab(snapZoneBlueprint);

            GameObject snapObject = new GameObject($"{snappable.name}SnapZone");
            Undo.RegisterCreatedObjectUndo(snapObject, $"Create {snapObject.name}");
            EditorUtility.CopySerialized(snappable.transform, snapObject.transform);
            
            SnapZone snapZone = snapObject.AddComponent<SnapZoneProperty>().SnapZone;
            snapZone.ShownHighlightObject = snapZonePrefab;
            settings.ApplySettingsToSnapZone(snapZone);

            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);

            foreach (Renderer renderer in snapZoneBlueprint.GetComponentsInChildren<Renderer>())
            {
                bounds.Encapsulate(renderer.bounds);
            }
            
            BoxCollider boxCollider = snapObject.AddComponent<BoxCollider>();
            boxCollider.center = bounds.center;
            boxCollider.size = bounds.size;
            boxCollider.isTrigger = true;

            DestroyImmediate(snapZoneBlueprint);
        }
        
        private GameObject DuplicateObject(GameObject originalObject, Transform parent = null)
        {
            GameObject cloneObject = new GameObject($"{originalObject.name}Highlight.prefab");
            
            if (parent != null)
            {
                cloneObject.transform.SetParent(parent);
            }

            foreach (Component component in GetRenderersFrom(originalObject))
            {
                Type componentType = component.GetType();

                if (componentType == typeof(SkinnedMeshRenderer))
                {
                    SkinnedMeshRenderer skinnedMeshRenderer = component as SkinnedMeshRenderer;
                    
                    MeshRenderer meshRenderer = cloneObject.AddComponent<MeshRenderer>();
                    MeshFilter meshFilter = cloneObject.AddComponent<MeshFilter>();

                    meshRenderer.sharedMaterials = skinnedMeshRenderer.sharedMaterials;
                    meshFilter.sharedMesh = skinnedMeshRenderer.sharedMesh;
                }
                else
                {
                    Component targetComponent = cloneObject.AddComponent(componentType);
                    EditorUtility.CopySerialized(component, targetComponent);
                }
            }
            
            EditorUtility.CopySerialized(originalObject.transform, cloneObject.transform);

            foreach (Transform child in originalObject.transform)
            {
                DuplicateObject(child.gameObject, cloneObject.transform);
            }

            return cloneObject;
        }
        
        private IEnumerable<Component> GetRenderersFrom(GameObject sourceObject)
        {
            List<Component> components = new List<Component>();

            SkinnedMeshRenderer skinnedMeshRenderer = sourceObject.GetComponent<SkinnedMeshRenderer>();
            
            if (skinnedMeshRenderer != null)
            {
                components.Add(skinnedMeshRenderer);
            }
            
            MeshRenderer meshRenderer = sourceObject.GetComponent<MeshRenderer>();

            if (meshRenderer != null)
            {
                MeshFilter meshFilter = sourceObject.GetComponent<MeshFilter>();
                
                components.Add(meshRenderer);
                components.Add(meshFilter);
            }

            return components;
        }
        
        private void SetHighlightMaterial(GameObject snapZonePrefab, Material highlightMaterial)
        {
            foreach (Renderer renderer in snapZonePrefab.GetComponentsInChildren<Renderer>())
            {
                renderer.sharedMaterials = new[] { highlightMaterial };
            }
        }

        private GameObject SaveSnapZonePrefab(GameObject snapZoneBlueprint)
        {
            if (Directory.Exists(PrefabPath) == false)
            {
                Directory.CreateDirectory(PrefabPath);
            }
            
            snapZoneBlueprint.transform.localScale = Vector3.one;
            snapZoneBlueprint.transform.position = Vector3.zero;
            snapZoneBlueprint.transform.rotation = Quaternion.identity;

            string prefabName = NamePrefab(snapZoneBlueprint.name);
            string prefabPath = $"{PrefabPath}/{prefabName}";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(snapZoneBlueprint, prefabPath);

            if (prefab != null)
            {
                Debug.LogWarningFormat("A new Snap Zone prefab was saved at {0}", prefabPath);
            }
            
            return prefab;
        }

        private string NamePrefab(string originalName)
        {
            int invalidCharacterIndex;
            StringBuilder prefabName = new StringBuilder(originalName);

            // Unity replaces invalid characters with '_' when creating new prefabs in the editor.
            // We try to simulate that behavior.
            while ((invalidCharacterIndex = prefabName.ToString().IndexOfAny(Path.GetInvalidFileNameChars())) >= 0)
            {
                prefabName.Replace(prefabName[invalidCharacterIndex], '_');
            }
            
            return prefabName.ToString().Replace('\\', '_');
        }
    }
}
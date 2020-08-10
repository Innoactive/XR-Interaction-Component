using System;
using System.IO;
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
            // Retrieves a SnapZoneSettings and creates a clone for the snappable object
            SnapZoneSettings settings = SnapZoneSettings.Settings;
            GameObject snapZoneBlueprint = DuplicateObject(snappable.gameObject);
            
            // Sets the highlight materials to the cloned object and saves it as highlight prefab.
            SetHighlightMaterial(snapZoneBlueprint, settings.HighlightMaterial);
            GameObject snapZonePrefab = SaveSnapZonePrefab(snapZoneBlueprint);

            // Creates a new object for the SnapZone.
            GameObject snapObject = new GameObject($"{snappable.name}SnapZone");
            Undo.RegisterCreatedObjectUndo(snapObject, $"Create {snapObject.name}");
            
            // Positions the Snap Zone at the same position, rotation and scale as the snappable object.
            snapObject.transform.SetParent(snappable.transform);
            snapObject.transform.SetPositionAndRotation(snappable.transform.position, snappable.transform.rotation);
            snapObject.transform.localScale = Vector3.one;
            snapObject.transform.SetParent(null);
            
            // Adds a Snap Zone component to our new object.
            SnapZone snapZone = snapObject.AddComponent<SnapZoneProperty>().SnapZone;
            snapZone.ShownHighlightObject = snapZonePrefab;
            settings.ApplySettingsToSnapZone(snapZone);

            // Calculates the volume of the Snap Zone out of the snappable object.
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);

            foreach (Renderer renderer in snapZoneBlueprint.GetComponentsInChildren<Renderer>())
            {
                bounds.Encapsulate(renderer.bounds);
            }
            
            // Adds a BoxCollider and sets it up.
            BoxCollider boxCollider = snapObject.AddComponent<BoxCollider>();
            boxCollider.center = bounds.center;
            boxCollider.size = bounds.size;
            boxCollider.isTrigger = true;

            // Disposes the cloned object.
            DestroyImmediate(snapZoneBlueprint);
        }
        
        private GameObject DuplicateObject(GameObject originalObject, Transform parent = null)
        {
            GameObject cloneObject = new GameObject($"{originalObject.name}_Clone");
            
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

            string prefabName = snapZoneBlueprint.name.Replace("_Clone", "Highlight.prefab");
            return PrefabUtility.SaveAsPrefabAsset(snapZoneBlueprint, $"{PrefabPath}/{prefabName}");
        }
    }
}
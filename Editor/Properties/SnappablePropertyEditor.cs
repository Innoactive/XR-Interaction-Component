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
            
            GameObject highlight = DuplicateObject(snappable.gameObject);

            highlight.transform.position = Vector3.zero;
            highlight.transform.rotation = Quaternion.identity;

            Vector3 offsetPosition = Vector3.one;

            Rigidbody rigidbody = snappable.GetComponent<Rigidbody>();
            if (rigidbody)
            {
                offsetPosition = -rigidbody.centerOfMass;
                GameObject centerOfMass = new GameObject("Center Of Mass");
                centerOfMass.transform.SetParent(highlight.transform, false);
                centerOfMass.transform.localPosition = rigidbody.centerOfMass;
            }

            // Calculates the volume of the Snap Zone out of the snappable object.
            Renderer[] renderers = highlight.GetComponentsInChildren<Renderer>();
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            bounds.center += offsetPosition;

            // Sets the highlight materials to the cloned object and saves it as highlight prefab.
            SetHighlightMaterial(highlight, settings.HighlightMaterial);
            GameObject highlightPrefab = SaveSnapZonePrefab(highlight);

            // Creates a new object for the SnapZone.
            GameObject snapZone = new GameObject($"{CleanName(snappable.name)}_SnapZone");
            Undo.RegisterCreatedObjectUndo(snapZone, $"Create {snapZone.name}");

            // Positions the Snap Zone at the same position, rotation and scale as the snappable object.
            snapZone.transform.SetPositionAndRotation(snappable.transform.position, snappable.transform.rotation);
            snapZone.transform.localScale = Vector3.one;

            // Adds a BoxCollider and sets it up.
            BoxCollider boxCollider = snapZone.AddComponent<BoxCollider>();
            boxCollider.center = bounds.center - offsetPosition;
            boxCollider.size = bounds.size;
            boxCollider.isTrigger = true;

            // Adds a Snap Zone component to our new object.
            SnapZone snapZoneProperty = snapZone.AddComponent<SnapZoneProperty>().SnapZone;
            snapZoneProperty.ShownHighlightObject = highlightPrefab;
            settings.ApplySettingsToSnapZone(snapZoneProperty);
            
            AssetDatabase.Refresh();

            // Disposes the cloned object.
            DestroyImmediate(highlight);
        }

        private GameObject DuplicateObject(GameObject originalObject, Transform parent = null)
        {
            GameObject cloneObject = new GameObject($"{CleanName(originalObject.name)}_Highlight.prefab");
            cloneObject.SetActive(originalObject.activeSelf);

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

                    meshRenderer.enabled = skinnedMeshRenderer.enabled;
                }
                else
                {
                    Component targetComponent = cloneObject.AddComponent(componentType);
                    EditorUtility.CopySerialized(component, targetComponent);
                }
            }

            EditorUtility.CopySerialized(originalObject.transform, cloneObject.transform);

            if (parent == null)
            {
                cloneObject.transform.localScale = originalObject.transform.lossyScale;
            }

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

            snapZoneBlueprint.transform.position = Vector3.zero;
            snapZoneBlueprint.transform.rotation = Quaternion.identity;

            string prefabPath = $"{PrefabPath}/{snapZoneBlueprint.name}";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(snapZoneBlueprint, prefabPath);

            if (prefab != null)
            {
                Debug.LogWarningFormat("A new highlight prefab was saved at {0}", prefabPath);
            }

            return prefab;
        }

        private string CleanName(string originalName)
        {
            // Unity replaces invalid characters with '_' when creating new prefabs in the editor.
            // We try to simulate that behavior.
            foreach (char invalidCharacter in Path.GetInvalidFileNameChars())
            {
                originalName = originalName.Replace(invalidCharacter, '_');
            }

            // Non windows systems consider '\' as a valid file name. 
            return originalName.Replace('\\', '_');
        }
    }
}
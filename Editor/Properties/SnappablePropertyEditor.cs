using System;
using System.IO;
using System.Collections.Generic;
using Innoactive.Creator.XRInteraction;
using Innoactive.Creator.Core.SceneObjects;
using Innoactive.Creator.XRInteraction.Properties;
using Innoactive.Creator.BasicInteraction.Validation;
using UnityEditor;
using UnityEngine;

namespace Innoactive.CreatorEditor.XRInteraction
{
    /// <summary>
    /// Custom inspector for <see cref="SnappableProperty"/>, adding a button to create <see cref="Innoactive.Creator.XRInteraction.SnapZone"/>s automatically.
    /// </summary>
    [CustomEditor(typeof(SnappableProperty)), CanEditMultipleObjects]
    internal class SnappablePropertyEditor : Editor
    {
        private const string PrefabPath = "Assets/Resources/SnapZones/Prefabs";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            if (GUILayout.Button("Create Snap Zone"))
            {
                foreach (UnityEngine.Object targetObject in serializedObject.targetObjects)
                {
                    if (targetObject is SnappableProperty snappable)
                    {
                        CreateSnapZone(snappable);
                    }
                }
            }
        }
        
        private void CreateSnapZone(SnappableProperty snappable)
        {
            // Retrieves a SnapZoneSettings and creates a clone for the snappable object
            SnapZoneSettings settings = SnapZoneSettings.Settings;
            GameObject snapZoneBlueprint = DuplicateObject(snappable.gameObject, settings.HighlightMaterial);
            
            // Saves it as highlight prefab.
            GameObject snapZonePrefab = SaveSnapZonePrefab(snapZoneBlueprint);

            // Creates a new object for the SnapZone.
            GameObject snapObject = new GameObject($"{CleanName(snappable.name)}_SnapZone");
            Undo.RegisterCreatedObjectUndo(snapObject, $"Create {snapObject.name}");
            
            // Positions the Snap Zone at the same position, rotation and scale as the snappable object.
            snapObject.transform.SetParent(snappable.transform);
            snapObject.transform.SetPositionAndRotation(snappable.transform.position, snappable.transform.rotation);
            snapObject.transform.localScale = Vector3.one;
            snapObject.transform.SetParent(null);
            
            // Adds a Snap Zone component to our new object.
            SnapZone snapZone = snapObject.AddComponent<SnapZoneProperty>().SnapZone;
            snapZone.ShownHighlightObject = snapZonePrefab;
            IsTrainingSceneObjectValidation validation = snapZone.gameObject.AddComponent<IsTrainingSceneObjectValidation>();
            validation.AddTrainingSceneObject(snappable.GetComponent<TrainingSceneObject>());

            settings.ApplySettingsToSnapZone(snapZone);

            GameObject snapPoint = new GameObject("SnapPoint");
            snapPoint.transform.SetParent(snapZone.transform);
            snapPoint.transform.localPosition = Vector3.zero;
            snapPoint.transform.localScale = Vector3.one;
            snapPoint.transform.localRotation = Quaternion.identity;
            snapPoint.AddComponent<SnapZonePreviewDrawer>();

            SerializedObject snapZoneSerialization = new SerializedObject(snapZone);
            SerializedProperty property = snapZoneSerialization.FindProperty("m_AttachTransform");
            property.objectReferenceValue = snapPoint.transform;
            snapZoneSerialization.ApplyModifiedPropertiesWithoutUndo();

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

            Selection.activeGameObject = snapZone.gameObject;
        }
        
        private GameObject DuplicateObject(GameObject originalObject, Material sharedMaterial, Transform parent = null)
        {
            GameObject cloneObject = new GameObject($"{CleanName(originalObject.name)}_Highlight.prefab");
            
            if (parent != null)
            {
                cloneObject.transform.SetParent(parent);
            }
            
            ProcessRenderer(originalObject, cloneObject, sharedMaterial);
            
            EditorUtility.CopySerialized(originalObject.transform, cloneObject.transform);

            foreach (Transform child in originalObject.transform)
            {
                DuplicateObject(child.gameObject, sharedMaterial, cloneObject.transform);
            }

            return cloneObject;
        }

        private void ProcessRenderer(GameObject originalObject, GameObject cloneObject, Material sharedMaterial)
        {
            Renderer renderer = originalObject.GetComponent<Renderer>();
            
            Type renderType = renderer.GetType();

            if (renderType == typeof(SkinnedMeshRenderer))
            {
                SkinnedMeshRenderer skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
                    
                MeshRenderer meshRenderer = cloneObject.AddComponent<MeshRenderer>();
                MeshFilter meshFilter = cloneObject.AddComponent<MeshFilter>();
                List<Material> sharedMaterials = new List<Material>();
                    
                for (int i = 0; i < skinnedMeshRenderer.sharedMesh.subMeshCount; i++)
                {
                    sharedMaterials.Add(sharedMaterial);
                }

                meshRenderer.sharedMaterials = sharedMaterials.ToArray();
                meshFilter.sharedMesh = skinnedMeshRenderer.sharedMesh;
            }
            
            if (renderType == typeof(MeshRenderer))
            {
                MeshRenderer originalMeshRenderer = renderer as MeshRenderer;
                MeshFilter originalMeshFilter = originalObject.GetComponent<MeshFilter>();
                
                MeshRenderer meshRenderer = cloneObject.AddComponent<MeshRenderer>();
                MeshFilter meshFilter = cloneObject.AddComponent<MeshFilter>();
                List<Material> sharedMaterials = new List<Material>();
                    
                for (int i = 0; i < originalMeshFilter.sharedMesh.subMeshCount; i++)
                {
                    sharedMaterials.Add(sharedMaterial);
                }
                
                meshRenderer.sharedMaterials = sharedMaterials.ToArray();
                meshFilter.sharedMesh = originalMeshFilter.sharedMesh;
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
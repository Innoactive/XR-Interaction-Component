using System;
using UnityEngine;

namespace Innoactive.Creator.XRInteraction
{
    /// <summary>
    /// Draws a preview of SnapZone highlight.
    /// </summary>
    [ExecuteInEditMode]
    public class SnapZonePreviewDrawer : MonoBehaviour
    {
        /// <summary>
        /// The parent SnapZone.
        /// </summary>
        public SnapZone Parent { get; set; }
        
        private void Awake()
        {
            if (Application.isPlaying)
            {
                DestroyPreview();
                DestroyImmediate(this);
            }
        }

        private void Update()
        {
            if (Parent == null)
            {
                Parent = transform.parent.GetComponent<SnapZone>();
                if (Parent == null)
                {
                    DestroyPreview();
                }
            }
            
            bool isPreviewing = gameObject.GetComponent<MeshFilter>() != null;
            
            if (Parent.ShowHighlightInEditor && Parent.PreviewMesh && isPreviewing == false)
            {
                MeshFilter meshFilter =  gameObject.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = Parent.PreviewMesh;
                
                MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
                meshRenderer.material = Parent.HighlightMeshMaterial;
            }
            else if (Parent.ShowHighlightInEditor == false && isPreviewing)
            {
                DestroyPreview();
            }
        }

        private void DestroyPreview()
        {
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                DestroyImmediate(meshRenderer);
            }
            
            MeshFilter filter = GetComponent<MeshFilter>();
            if (filter != null)
            {
                DestroyImmediate(filter);
            }
            
            DestroyImmediate(this);
        }
    }
}
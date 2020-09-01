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
        [SerializeField]
        [HideInInspector]
        private SnapZone parent;

        private void Start()
        {
            if (Application.isPlaying)
            {
                DestroyImmediate(this);
                return;
            }
            
            if (parent == null)
            {
                parent = transform.parent.GetComponent<SnapZone>();
            }
        }

        private void Update()
        {
            if (parent.ShowHighlightInEditor)
            {
                foreach (Mesh previewMesh in parent.PreviewMeshes)
                {
                    for (int i = 0; i < previewMesh.subMeshCount; i++)
                    {
                        Graphics.DrawMesh(previewMesh, transform.localToWorldMatrix, parent.HighlightMeshMaterial, parent.gameObject.layer, null, i);
                    }
                }
            }
        }
    }
}
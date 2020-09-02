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
                for (int i = 0; i < parent.PreviewMeshes.subMeshCount; i++)
                {
                    Graphics.DrawMesh(parent.PreviewMeshes, transform.localToWorldMatrix, parent.HighlightMeshMaterial, parent.gameObject.layer, null, i);
                }
            }
        }
    }
}
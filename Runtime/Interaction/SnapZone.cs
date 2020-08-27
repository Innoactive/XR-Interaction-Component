using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Innoactive.Creator.XRInteraction
{
    /// <summary>
    /// Interactor used for holding interactables via a socket.  This component is not designed to be attached to a controller
    /// (thus does not derive from XRBaseControllerInteractor) and instead will always attempt to select an interactable that it is
    /// hovering (though will not perform exclusive selection of that interactable).
    /// </summary>
    /// <remarks>
    /// Adds the functionality to force the selection and unselection of specific interactables.
    /// It also adds a highlighter to emphasize the position of the socket.
    /// </remarks>
    public class SnapZone : XRSocketInteractor
    {
        [SerializeField]
        private GameObject shownHighlightObject = null;

        /// <summary>
        /// The 'GameObject' whose mesh is drawn to emphasize the position of the snap zone.
        /// If none is supplied, no highlight object is shown.
        /// </summary>
        public GameObject ShownHighlightObject
        {
            get { return shownHighlightObject; }
            set
            {
                shownHighlightObject = value;
                UpdateHighlightMeshFilterCache();
            }
        }
        
        [SerializeField]
        private Color shownHighlightObjectColor = new Color(0.8f, 0.0f, 1.0f, 0.6f);
        
        /// <summary>
        /// The color of the material used to draw the <see cref="ShownHighlightObject"/>.
        /// Use the alpha value to specify the degree of transparency.
        /// </summary>
        public Color ShownHighlightObjectColor
        {
            get { return shownHighlightObjectColor; }
            set
            {
                shownHighlightObjectColor = value;
                UpdateHighlightMeshFilterCache();
            }
        }

        /// <summary>
        /// Gets or sets whether <see cref="ShownHighlightObject"/> is shown or not.
        /// </summary>
        public bool ShowHighlightObject { get; set; }
        
        private Material highlightMeshMaterial;

        /// <summary>
        /// The material used for drawing the mesh of the <see cref="ShownHighlightObject"/>. 
        /// </summary>
        public Material HighlightMeshMaterial
        {
            get
            {
                if (highlightMeshMaterial == null)
                {
                    highlightMeshMaterial = CreateTransparentMaterial();
                }

                return highlightMeshMaterial;
            }
            set { highlightMeshMaterial = value; }
        }

        [SerializeField]
        private Material validationMaterial;

        /// <summary>
        /// The material used for drawing when an <see cref="InteractableObject"/> is hovering this <see cref="SnapZone"/>.
        /// </summary>
        public Material ValidationMaterial
        {
            get
            {
                if (validationMaterial == null)
                {
                    validationMaterial = CreateTransparentMaterial();
                }

                return validationMaterial;
            }
            set { validationMaterial = value; }
        }

        /// <summary>
        /// Forces the socket interactor to unselect the given target, if it is not null.
        /// </summary>
        protected XRBaseInteractable ForceUnselectTarget { get; set; }
        
        /// <summary>
        /// Forces the socket interactor to select the given target, if it is not null.
        /// </summary>
        protected XRBaseInteractable ForceSelectTarget { get; set; }

        private Material previewMaterial;
        private Mesh previewMesh;

        protected override void Awake()
        {
            base.Awake();

            Collider triggerCollider = gameObject.GetComponentsInChildren<Collider>().FirstOrDefault(foundCollider => foundCollider.isTrigger);
            if (triggerCollider == null)
            {
                Debug.LogErrorFormat(gameObject, "The Snap Zone '{0}' does not have any trigger collider. "
                    + "Make sure you have at least one collider with the property `Is Trigger` enabled.", gameObject.name);
            }

            ShowHighlightObject = ShownHighlightObject != null;

            if (ShownHighlightObject != null)
            {
                UpdateHighlightMeshFilterCache();
            }
        }
        
        private void OnDrawGizmos()
        {
            Collider collider = GetComponent<Collider>();

            if (collider == null)
            {
                return;
            }

            Gizmos.color = shownHighlightObjectColor;
            Gizmos.matrix = transform.localToWorldMatrix;

            switch (collider)
            {
                case BoxCollider boxCollider:
                    Gizmos.DrawCube(boxCollider.center, boxCollider.size);
                    break;
                case SphereCollider sphereCollider:
                    Gizmos.DrawSphere(sphereCollider.center, sphereCollider.radius);
                    break;
            }
        }

        protected virtual void Update()
        {
            if (socketActive && selectTarget == null)
            {
                DrawHighlightMesh();
            }
        }

        /// <summary>
        /// Creates a transparent <see cref="Material"/> using Unity's "Standard" shader.
        /// </summary>
        /// <returns>A transparent <see cref="Material"/>. Null, otherwise, if Unity's "Standard" shader cannot be found.</returns>
        protected virtual Material CreateTransparentMaterial()
        {
            Material highlightMeshMaterial = new Material(Shader.Find("Standard"));
            if (highlightMeshMaterial != null)
            {
                highlightMeshMaterial.SetFloat("_Mode", 3);
                highlightMeshMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                highlightMeshMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                highlightMeshMaterial.SetInt("_ZWrite", 0);
                highlightMeshMaterial.DisableKeyword("_ALPHATEST_ON");
                highlightMeshMaterial.EnableKeyword("_ALPHABLEND_ON");
                highlightMeshMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                highlightMeshMaterial.SetColor("_Color", ShownHighlightObjectColor);
                highlightMeshMaterial.renderQueue = 3000;
            }

            return highlightMeshMaterial;
        }

        /// <summary>
        /// Updates the <see cref="HighlightMeshFilterCache"/> property using the current <see cref="ShownHighlightObject"/>.
        /// </summary>
        protected virtual void UpdateHighlightMeshFilterCache()
        {
            if (ShownHighlightObject == null)
            {
                previewMesh = null;
                return;
            }
            
            List<CombineInstance> meshes = new List<CombineInstance>();

            foreach (SkinnedMeshRenderer skinnedMeshRenderer in ShownHighlightObject.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                CombineInstance combineInstance = new CombineInstance();
                combineInstance.mesh = skinnedMeshRenderer.sharedMesh;
                combineInstance.transform = skinnedMeshRenderer.transform.localToWorldMatrix;
                
                meshes.Add(combineInstance);
            }
            
            foreach (MeshFilter meshFilter in ShownHighlightObject.GetComponentsInChildren<MeshFilter>())
            {
                CombineInstance combineInstance = new CombineInstance();
                combineInstance.mesh = meshFilter.sharedMesh;
                combineInstance.transform = meshFilter.transform.localToWorldMatrix;
                
                meshes.Add(combineInstance);
            }

            if (meshes.Any())
            {
                previewMesh = new Mesh();
                previewMesh.CombineMeshes(meshes.ToArray());
            }
            else
            {
                Debug.LogErrorFormat(ShownHighlightObject, "Shown Highlight Object '{0}' has no MeshFilter. It cannot be drawn.", ShownHighlightObject);
            }
        }
        
        /// <summary>
        /// This method is called by the interaction manager to update the interactor. 
        /// Please see the interaction manager documentation for more details on update order.
        /// </summary>
        public override void ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            if (updatePhase != XRInteractionUpdateOrder.UpdatePhase.Dynamic)
            {
                base.ProcessInteractor(updatePhase);
            }

            if (socketActive)
            {
                if (m_HoverTargets.Count == 0 && ShowHighlightObject)
                {
                    previewMaterial = HighlightMeshMaterial;
                } 
                else if (m_HoverTargets.Count > 0 && showInteractableHoverMeshes)
                {
                    previewMaterial = ValidationMaterial;
                }
            }
        }

        /// <summary>
        /// Draws a highlight mesh.
        /// </summary>
        protected virtual void DrawHighlightMesh()
        {
            if (previewMesh != null)
            {
                Matrix4x4 matrix = Matrix4x4.TRS(attachTransform.position, transform.rotation, transform.localScale);
                Graphics.DrawMesh(previewMesh, matrix, previewMaterial, gameObject.layer);
            }
        }

        /// <summary>
        /// Forces to unselect any selected interactable object.
        /// </summary>
        public virtual void ForceUnselect()
        {
            if (selectTarget != null)
            {
                ForceUnselectTarget = selectTarget;
            }
        }

        /// <summary>
        /// Unselects any selected interactable object and forces the provided interactable object to be selected if it is selectable.
        /// </summary>
        /// <param name="interactable">Interactable object to be selected.</param>
        public virtual void ForceSelect(XRBaseInteractable interactable)
        {
            ForceUnselect();

            if (interactable.IsSelectableBy(this))
            {
                OnTriggerEnter(interactable.GetComponent<Collider>());
                interactable.transform.position = attachTransform.position;
                ForceSelectTarget = interactable;
            }
            else
            {
                Debug.LogWarningFormat(interactable.gameObject, 
                    "Interactable '{0}' is not selectable by Snap Zone '{1}'. "
                    + "(Maybe the Interaction Layer Masks settings are not correct or the interactable object is locked?)", 
                    interactable.gameObject.name, 
                    gameObject.name);
            }
        }

        /// <summary>Determines if the interactable is valid for selection this frame.</summary>
        /// <param name="interactable">Interactable to check.</param>
        /// <returns><c>true</c> if the interactable can be selected this frame.</returns>
        /// <remarks>Adds the functionality of selecting and unselecting specific interactables.</remarks>
        public override bool CanSelect(XRBaseInteractable interactable)
        {
            // If one specific target should be unselected,
            if (ForceUnselectTarget == interactable)
            {
                ForceUnselectTarget = null;
                return false;
            }
            
            // If one specific target should be selected,
            if (ForceSelectTarget != null)
            {
                if (ForceSelectTarget != interactable)
                {
                    return false;
                }

                ForceSelectTarget = null;
                return true;
            }

            // Otherwise, normal routine.
            return base.CanSelect(interactable);
        }
    }
}

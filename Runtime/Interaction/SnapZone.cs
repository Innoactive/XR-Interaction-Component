using System.Linq;
using System.Collections.Generic;
using Innoactive.Creator.BasicInteraction.Validation;
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
        /// <summary>
        /// Gets or sets whether <see cref="ShownHighlightObject"/> is shown or not.
        /// </summary>
        public bool ShowHighlightObject { get; set; }
        
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
        /// Shows the highlight 
        /// </summary>
        public bool ShowHighlightInEditor = true;

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
        }

        [SerializeField]
        [Tooltip("Will be used when a valid object hovers the SnapZone")]
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
            set => validationMaterial = value;
        }
        
        [SerializeField]
        [Tooltip("Will be used when an invalid object hovers the SnapZone")]
        private Material invalidMaterial;

        /// <summary>
        /// The material used for drawing when an invalid <see cref="InteractableObject"/> is hovering this <see cref="SnapZone"/>.
        /// </summary>
        public Material InvalidMaterial
        {
            get
            {
                if (invalidMaterial == null)
                {
                    invalidMaterial = CreateTransparentMaterial();
                }

                return invalidMaterial;
            }
            set => invalidMaterial = value;
        }

        /// <summary>
        /// Forces the socket interactor to unselect the given target, if it is not null.
        /// </summary>
        protected XRBaseInteractable ForceUnselectTarget { get; set; }
        
        /// <summary>
        /// Forces the socket interactor to select the given target, if it is not null.
        /// </summary>
        protected XRBaseInteractable ForceSelectTarget { get; set; }
        
        private Mesh previewMesh;
        
        /// <summary>
        /// Returns the preview mesh used for this SnapZone.
        /// </summary>
        public Mesh PreviewMesh 
        {
            get
            {
                if (previewMesh == null && shownHighlightObject != null)
                {
                    UpdateHighlightMeshFilterCache();
                }

                return previewMesh;
            }
            
            set
            {
                previewMesh = value;
            }
        }

        private Transform initialParent;
        private Material activeMaterial;
        private Vector3 tmpCenterOfMass;
        
        private List<Validator> validators = new List<Validator>();
        
        private List<XRBaseInteractable> hoverTargets = new List<XRBaseInteractable>();
        
        protected override void Awake()
        {
            base.Awake();
            
            validators = GetComponents<Validator>().ToList();

            if (GetComponentsInChildren<Collider>()?.Any(foundCollider => foundCollider.isTrigger) == false)
            {
                Debug.LogError($"The Snap Zone '{name}' does not have any trigger collider. "
                    + "Make sure you have at least one collider with the property `Is Trigger` enabled.", gameObject);
            }

            ShowHighlightObject = ShownHighlightObject != null;

            activeMaterial = HighlightMeshMaterial;
            
            if (ShownHighlightObject != null)
            {
                UpdateHighlightMeshFilterCache();
            }

            initialParent = transform.parent;

            if (initialParent != null)
            {
                transform.SetParent(null);
            }
        }

        internal void AddHoveredInteractable(XRBaseInteractable interactable)
        {  
            if (interactable != null)
            {
                hoverTargets.Add(interactable);
            }
        }

        internal void RemoveHoveredInteractable(XRBaseInteractable interactable)
        {
            hoverTargets.Remove(interactable);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            
            hoverTargets.Clear();
            
            onSelectEnter.AddListener(OnAttach);
            onSelectExit.AddListener(OnDetach);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            hoverTargets.Clear();
            
            onSelectEnter.RemoveListener(OnAttach);
            onSelectExit.RemoveListener(OnDetach);
        }

        private void OnAttach(XRBaseInteractable interactable)
        {
            Rigidbody rigid = interactable.gameObject.GetComponent<Rigidbody>();
            tmpCenterOfMass = rigid.centerOfMass;
            rigid.centerOfMass = Vector3.zero;
        }
        
        private void OnDetach(XRBaseInteractable interactable)
        {
            Rigidbody rigid = interactable.gameObject.GetComponent<Rigidbody>();
            rigid.centerOfMass = tmpCenterOfMass;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawIcon(transform.position, "Import", false);
        }

        protected virtual void Update()
        {
            if (initialParent != null)
            {
                transform.SetParent(initialParent);
                initialParent = null;
            }
            
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
                if (skinnedMeshRenderer.sharedMesh == null)
                {
                    continue;
                }
                
                for (int i = 0; i < skinnedMeshRenderer.sharedMesh.subMeshCount; i++)
                {
                    CombineInstance combineInstance = new CombineInstance();
                    combineInstance.mesh = skinnedMeshRenderer.sharedMesh;
                    combineInstance.subMeshIndex = i;
                    combineInstance.transform = skinnedMeshRenderer.transform.localToWorldMatrix;

                    meshes.Add(combineInstance);
                }
            }
            
            foreach (MeshFilter meshFilter in ShownHighlightObject.GetComponentsInChildren<MeshFilter>())
            {
                if (meshFilter.sharedMesh == null)
                {
                    continue;
                }

                for (int i = 0; i < meshFilter.sharedMesh.subMeshCount; i++)
                {
                    CombineInstance combineInstance = new CombineInstance();
                    combineInstance.mesh = meshFilter.sharedMesh;
                    combineInstance.subMeshIndex = i;
                    combineInstance.transform = meshFilter.transform.localToWorldMatrix;
                
                    meshes.Add(combineInstance);
                }
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
                hoverTargets.RemoveAll(target => target == null || target.enabled == false);
                
                CheckForReleasedHoverTargets();
                
                ShowHighlight();
            }
        }

        private void CheckForReleasedHoverTargets()
        {
            if (selectTarget != null)
            {
                return;
            }
            
            foreach (XRBaseInteractable target in hoverTargets)
            {
                if (m_HoverTargets.Contains(target) || target.isSelected)
                {
                    continue;
                }

                if (CanSelect(target))
                {
                    ForceSelect(target);
                    return;
                }
            }
        }

        private void ShowHighlight()
        {
            if (hoverTargets.Count == 0 && ShowHighlightObject)
            {
                activeMaterial = HighlightMeshMaterial;
            }
            else if (hoverTargets.Count > 0 && showInteractableHoverMeshes)
            {
                activeMaterial = hoverTargets.Any(CanSelect) ? ValidationMaterial : InvalidMaterial;
            }
        }

        /// <summary>
        /// Draws a highlight mesh.
        /// </summary>
        protected virtual void DrawHighlightMesh()
        {
            if (PreviewMesh != null)
            {
                for (int i = 0; i < PreviewMesh.subMeshCount; i++)
                {
                    Graphics.DrawMesh(PreviewMesh, attachTransform.localToWorldMatrix, activeMaterial, gameObject.layer, null, i);
                }
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
                OnSelectEnter(interactable);
                interactable.transform.position = attachTransform.position;
                interactable.transform.rotation = attachTransform.rotation;
                ForceSelectTarget = interactable;
            }
            else
            {
                Debug.LogWarning($"Interactable '{interactable.name}' is not selectable by Snap Zone '{name}'. "
                    + $"(Maybe the Interaction Layer Masks settings are not correct or the interactable object is locked?)", interactable.gameObject);
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

            // If this object cannot be selected, ignore it.
            if (base.CanSelect(interactable) == false)
            {
                return false;
            }
            
            // If one active validator does not allow this to be snapped, return false.
            foreach (Validator validator in validators)
            {
                if (validator.isActiveAndEnabled && validator.Validate(interactable.gameObject) == false)
                {
                    return false;
                }
            }

            return true;
        }
    }
}

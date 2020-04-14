using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Innoactive.Creator.Unity;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Innoactive.Creator.XRInteraction
{
    /// <summary>
    /// Handles highlighting for attached <see cref="InteractableObject"/>.
    /// </summary>
    public sealed class InteractableHighlighter : MonoBehaviour
    {
        /// <summary>
        /// Reference to the <see cref="InteractableObject"/>.
        /// </summary>
        public InteractableObject InteractableObject => interactableObject;

        /// <summary>
        /// Determines if this <see cref="InteractableObject"/> should be highlighted when touched.
        /// </summary>
        public bool AllowOnTouchHighlight
        {
            get => allowOnTouchHighlight;
            set => allowOnTouchHighlight = value;
        }

        /// <summary>
        /// Determines if this <see cref="InteractableObject"/> should be highlighted when grabbed.
        /// </summary>
        public bool AllowOnGrabHighlight
        {
            get => allowOnGrabHighlight;
            set => allowOnGrabHighlight = value;
        }
        
        /// <summary>
        /// Determines if this <see cref="InteractableObject"/> should be highlighted when used.
        /// </summary>
        public bool AllowOnUseHighlight
        {
            get => allowOnUseHighlight;
            set => allowOnUseHighlight = value;
        }

        [SerializeField]
        private bool allowOnTouchHighlight = true;
        [SerializeField]
        private bool allowOnGrabHighlight;
        [SerializeField]
        private bool allowOnUseHighlight;
        
        [SerializeField]
        private Material touchHighlightMaterial;
        [SerializeField]
        private Material grabHighlightMaterial;
        [SerializeField]
        private Material useHighlightMaterial;
        
        [SerializeField]
        private Color touchHighlightColor = new Color32(64, 200, 255, 50);
        [SerializeField]
        private Color grabHighlightColor = new Color32(255, 0, 0, 50);
        [SerializeField]
        private Color useHighlightColor = new Color32(0, 255, 0, 50);

        private Dictionary<string, bool> externalHighlights = new Dictionary<string, bool>();
        private InteractableObject interactableObject;
        private SkinnedMeshRenderer[] cachedSkinnedRenderers = {};
        private MeshRenderer[] cachedMeshRenderers = {};
        private MeshFilter[] cachedMeshFilters = {};

        private void Awake()
        {
            interactableObject = gameObject.GetComponent<InteractableObject>(true);
        }

        private void OnEnable()
        {
            interactableObject.onFirstHoverEnter.AddListener(OnTouched);
            interactableObject.onSelectEnter.AddListener(OnGrabbed);
            interactableObject.onSelectExit.AddListener(OnReleased);
            interactableObject.onActivate.AddListener(OnUsed);
            interactableObject.onDeactivate.AddListener(OnUnused);
        }

        private void OnDisable()
        {
            interactableObject.onFirstHoverEnter.RemoveListener(OnTouched);
            interactableObject.onSelectEnter.RemoveListener(OnGrabbed);
            interactableObject.onSelectExit.RemoveListener(OnReleased);
            interactableObject.onActivate.RemoveListener(OnUsed);
            interactableObject.onDeactivate.RemoveListener(OnUnused);
        }
        
        private void OnValidate()
        {
            if (allowOnTouchHighlight && touchHighlightMaterial != null)
            {
                touchHighlightMaterial.color = touchHighlightColor;
            }
            
            if (allowOnGrabHighlight && grabHighlightMaterial != null)
            {
                grabHighlightMaterial.color = grabHighlightColor;
            }
            
            if (allowOnUseHighlight && useHighlightMaterial != null)
            {
                useHighlightMaterial.color = useHighlightColor;
            }
        }
        
        /// <summary>
        /// Highlights this <see cref="InteractableObject"/> with given <paramref name="highlightMaterial"/>.
        /// </summary>
        /// <remarks>Every highlight requires an ID to avoid duplications.</remarks>
        public void StartHighlighting(string highlightID, Material highlightMaterial)
        {
            if (externalHighlights.ContainsKey(highlightID) == false)
            {
                bool shouldContinueHighlighting = true;
                externalHighlights.Add(highlightID, shouldContinueHighlighting);
                StartCoroutine(Highlight(highlightMaterial, ()=> externalHighlights[highlightID], highlightID));
            }
        }
        
        /// <summary>
        /// Highlights this <see cref="InteractableObject"/> with given <paramref name="highlightColor"/>.
        /// </summary>
        /// <remarks>Every highlight requires an ID to avoid duplications.</remarks>
        public void StartHighlighting(string highlightID, Color highlightColor)
        {
            if (externalHighlights.ContainsKey(highlightID) == false)
            {
                bool shouldContinueHighlighting = true;
                externalHighlights.Add(highlightID, shouldContinueHighlighting);
                Material highlightMaterial = NewHighlightMaterial(highlightColor);
                StartCoroutine(Highlight(highlightMaterial, ()=> externalHighlights[highlightID], highlightID));
            }
        }
        
        /// <summary>
        /// Highlights this <see cref="InteractableObject"/> with given <paramref name="highlightTexture"/>.
        /// </summary>
        /// <remarks>Every highlight requires an ID to avoid duplications.</remarks>
        public void StartHighlighting(string highlightID, Texture highlightTexture)
        {
            if (externalHighlights.ContainsKey(highlightID) == false)
            {
                bool shouldContinueHighlighting = true;
                externalHighlights.Add(highlightID, shouldContinueHighlighting);
                Material highlightMaterial = NewHighlightMaterial(highlightTexture);
                StartCoroutine(Highlight(highlightMaterial, ()=> externalHighlights[highlightID], highlightID));
            }
        }
        
        /// <summary>
        /// Stops a highlight of given <paramref name="highlightID"/>.
        /// </summary>
        public void StopHighlighting(string highlightID)
        {
            if (externalHighlights.ContainsKey(highlightID))
            {
                externalHighlights[highlightID] = false;
            }
        }

        private void OnTouched(XRBaseInteractor interactor)
        {
            OnTouchHighlight();
        }

        private void OnGrabbed(XRBaseInteractor interactor)
        {
            OnGrabHighlight();
        }

        private void OnReleased(XRBaseInteractor interactor)
        {
            OnTouchHighlight();
        }
        
        private void OnUsed(XRBaseInteractor interactor)
        {
            OnUseHighlight();
        }
        
        private void OnUnused(XRBaseInteractor interactor)
        {
            OnGrabHighlight();
        }

        private IEnumerator Highlight(Material highlightMaterial, Func<bool> shouldContinueHighlighting, string highlightID = "")
        {
            if (cachedSkinnedRenderers.Length == 0 && cachedMeshRenderers.Length == 0)
            {
                RefreshCachedRenderers();
            }
            
            while (shouldContinueHighlighting())
            {
                DisableRenders(cachedSkinnedRenderers);
                DisableRenders(cachedMeshRenderers);
                
                foreach (SkinnedMeshRenderer skinnedRenderer in cachedSkinnedRenderers)
                {
                    DrawHighlightedObject(skinnedRenderer.sharedMesh, skinnedRenderer, highlightMaterial);
                }
                
                foreach (MeshFilter meshFilter in cachedMeshFilters)
                {
                    DrawHighlightedObject(meshFilter.sharedMesh, meshFilter, highlightMaterial);
                }

                yield return null;
            }

            ReenableRenderers(cachedSkinnedRenderers);
            ReenableRenderers(cachedMeshRenderers);

            if (string.IsNullOrEmpty(highlightID) == false && externalHighlights.ContainsKey(highlightID))
            {
                externalHighlights.Remove(highlightID);
            }
        }
        
        private void OnTouchHighlight()
        {
            if (ShouldHighlightTouching())
            {
                if (touchHighlightMaterial == null)
                {
                    touchHighlightMaterial = NewHighlightMaterial(touchHighlightColor);
                }
                
                RefreshCachedRenderers();
                StartCoroutine(Highlight(touchHighlightMaterial, ShouldHighlightTouching));
            }
        }

        private void OnGrabHighlight()
        {
            if (ShouldHighlightGrabbing())
            {
                if (grabHighlightMaterial == null)
                {
                    grabHighlightMaterial = NewHighlightMaterial(grabHighlightColor);
                }
                
                StartCoroutine(Highlight(grabHighlightMaterial, ShouldHighlightGrabbing));
            }
        }

        private void OnUseHighlight()
        {
            if (ShouldHighlightUsing())
            {
                if ( useHighlightMaterial == null)
                {
                    useHighlightMaterial = NewHighlightMaterial(useHighlightColor);
                }
                
                StartCoroutine(Highlight(useHighlightMaterial, ShouldHighlightUsing));
            }
        }

        private void RefreshCachedRenderers()
        {
            if (cachedSkinnedRenderers.Any() && cachedSkinnedRenderers.First().enabled == false || cachedMeshRenderers.Any() && cachedMeshRenderers.First().enabled == false)
            {
                return;
            }
            
            cachedSkinnedRenderers = GetComponentsInChildren<SkinnedMeshRenderer>(true).Where(meshRenderer => meshRenderer.enabled).ToArray();
            cachedMeshRenderers = GetComponentsInChildren<MeshRenderer>(true).Where(meshRenderer => meshRenderer.enabled).ToArray();
            cachedMeshFilters = cachedMeshRenderers.Select(meshRenderer => meshRenderer.GetComponent<MeshFilter>()).ToArray();
        }

        private void DisableRenders(IEnumerable<Renderer> renderers)
        {
            foreach (Renderer activeRenderer in renderers)
            {
                activeRenderer.enabled = false;
            }
        }
        
        private void ReenableRenderers(IEnumerable<Renderer> renderers)
        {
            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = true;
            }
        }

        private void DrawHighlightedObject(Mesh mesh, Component renderer, Material material)
        {
            LayerMask layerMask = renderer.gameObject.layer;
            Transform rendersTransform = renderer.transform;
            Matrix4x4 matrix = Matrix4x4.TRS(rendersTransform.position, rendersTransform.rotation, rendersTransform.lossyScale);
            
            Graphics.DrawMesh(mesh, matrix, material, layerMask);
        }

        private bool ShouldHighlightTouching()
        {
            if (interactableObject.IsInSocket)
            {
                return allowOnTouchHighlight && interactableObject.isHovered;
            }
            
            return allowOnTouchHighlight && interactableObject.isHovered && interactableObject.isSelected == false;
        }

        private bool ShouldHighlightGrabbing()
        {
            if (interactableObject.IsInSocket)
            {
                return false;
            }
            
            return allowOnGrabHighlight && interactableObject.isSelected && interactableObject.IsActivated == false;
        }
        
        private bool ShouldHighlightUsing()
        {
            return allowOnUseHighlight && interactableObject.IsActivated && interactableObject.isSelected;
        }

        private Material NewHighlightMaterial(Color highlightColor)
        {
            Material material = CreateHighlightMaterial();
            material.color = highlightColor;
            return material;
        }
        
        private Material NewHighlightMaterial(Texture mainTexture)
        {
            Material material = CreateHighlightMaterial();
            material.mainTexture = mainTexture;
            return material;
        }
        
        private Material CreateHighlightMaterial()
        {
            Shader shader = Shader.Find("Standard");

            if (shader == null)
            {
                throw new NullReferenceException("Standard shader could not be found.");
            }
            
            return new Material(shader);
        }
    }
}
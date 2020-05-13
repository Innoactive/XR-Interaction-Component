using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Innoactive.Creator.Unity;
using JetBrains.Annotations;
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
        
        private Material colorTouchMaterial;
        private Material colorGrabMaterial;
        private Material colorUseMaterial;

        private Dictionary<string, bool> externalHighlights = new Dictionary<string, bool>();
        private InteractableObject interactableObject;
        private SkinnedMeshRenderer[] cachedSkinnedRenderers = {};
        private MeshRenderer[] cachedMeshRenderers = {};
        private MeshFilter[] cachedMeshFilters = {};

        private IEnumerator Start()
        {
            interactableObject = GetComponent<InteractableObject>();
            yield return null;
            
            if (interactableObject != null)
            {
               OnEnable();
            }
            else
            {
                throw new NullReferenceException(string.Format("Every {0} requires a {1}", GetType().Name, nameof(InteractableObject)));
            }
        }

        private void OnEnable()
        {
            if (interactableObject != null)
            {
                interactableObject.onFirstHoverEnter.AddListener(OnTouched);
                interactableObject.onSelectEnter.AddListener(OnGrabbed);
                interactableObject.onSelectExit.AddListener(OnReleased);
                interactableObject.onActivate.AddListener(OnUsed);
                interactableObject.onDeactivate.AddListener(OnUnused);
            }
        }

        private void OnDisable()
        {
            if (interactableObject != null)
            {
                interactableObject.onFirstHoverEnter.RemoveListener(OnTouched);
                interactableObject.onSelectEnter.RemoveListener(OnGrabbed);
                interactableObject.onSelectExit.RemoveListener(OnReleased);
                interactableObject.onActivate.RemoveListener(OnUsed);
                interactableObject.onDeactivate.RemoveListener(OnUnused);
            }
        }

        private void RegisterDelegates()
        {
            
        }

        private void OnValidate()
        {
            if (allowOnTouchHighlight && colorTouchMaterial != null)
            {
                colorTouchMaterial.color = touchHighlightColor;
            }

            if (allowOnGrabHighlight && colorGrabMaterial != null)
            {
                colorGrabMaterial.color = grabHighlightColor;
            }

            if (allowOnUseHighlight && colorUseMaterial != null)
            {
                colorUseMaterial.color = useHighlightColor;
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
                Material highlightMaterial = null;
                
                if (touchHighlightMaterial != null)
                {
                    highlightMaterial = touchHighlightMaterial;
                }
                else 
                {
                    if (colorTouchMaterial == null)
                    {
                        colorTouchMaterial = NewHighlightMaterial(touchHighlightColor);
                    }

                    highlightMaterial = colorTouchMaterial;
                }

                RefreshCachedRenderers();
                StartCoroutine(Highlight(highlightMaterial, ShouldHighlightTouching));
            }
        }

        private void OnGrabHighlight()
        {
            if (ShouldHighlightGrabbing())
            {
                Material highlightMaterial = null;
                
                if (grabHighlightMaterial != null)
                {
                    highlightMaterial = grabHighlightMaterial;
                }
                else 
                {
                    if (colorGrabMaterial == null)
                    {
                        colorGrabMaterial = NewHighlightMaterial(grabHighlightColor);
                    }

                    highlightMaterial = colorGrabMaterial;
                }
                
                StartCoroutine(Highlight(highlightMaterial, ShouldHighlightGrabbing));
            }
        }

        private void OnUseHighlight()
        {
            if (ShouldHighlightUsing())
            {
                Material highlightMaterial = null;
                
                if (useHighlightMaterial != null)
                {
                    highlightMaterial = useHighlightMaterial;
                }
                else 
                {
                    if (colorUseMaterial == null)
                    {
                        colorUseMaterial = NewHighlightMaterial(useHighlightColor);
                    }

                    highlightMaterial = colorUseMaterial;
                }
                
                StartCoroutine(Highlight(highlightMaterial, ShouldHighlightUsing));
            }
        }

        private void RefreshCachedRenderers()
        {
            if (cachedSkinnedRenderers.Any() && cachedSkinnedRenderers.First().enabled == false || cachedMeshRenderers.Any() && cachedMeshRenderers.First().enabled == false)
            {
                return;
            }

            cachedSkinnedRenderers = GetComponentsInChildren<SkinnedMeshRenderer>(true).Where(meshRenderer => meshRenderer.gameObject.activeSelf && meshRenderer.enabled).ToArray();
            cachedMeshRenderers = GetComponentsInChildren<MeshRenderer>(true).Where(meshRenderer => meshRenderer.gameObject.activeSelf && meshRenderer.enabled).ToArray();
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

            // In case the color has some level of transparency,
            // we set the Material's Rendering Mode to Transparent. 
            if (Mathf.Approximately(highlightColor.a, 1f) == false)
            {
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
            }
            
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

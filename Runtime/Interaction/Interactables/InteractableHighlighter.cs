using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Innoactive.Creator.Unity;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Interaction.Toolkit;

namespace Innoactive.Creator.XRInteraction
{
    /// <summary>
    /// Handles highlighting for attached <see cref="InteractableObject"/>.
    /// </summary>
    [RequireComponent(typeof(InteractableObject))]
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
        private bool allowOnGrabHighlight = false;
        
        [SerializeField]
        private bool allowOnUseHighlight = false;

        [SerializeField]
        private Material touchHighlightMaterial = null;
        
        [SerializeField]
        private Material grabHighlightMaterial = null;
        
        [SerializeField]
        private Material useHighlightMaterial = null;

        [SerializeField]
        private Color touchHighlightColor = new Color32(64, 200, 255, 50);
        
        [SerializeField]
        private Color grabHighlightColor = new Color32(255, 0, 0, 50);
        
        [SerializeField]
        private Color useHighlightColor = new Color32(0, 255, 0, 50);

        private bool isBeingHighlighted;
        private Material colorTouchMaterial;
        private Material colorGrabMaterial;
        private Material colorUseMaterial;

        private Dictionary<string, bool> externalHighlights = new Dictionary<string, bool>();
        private InteractableObject interactableObject;
        
        [SerializeField]
        private Renderer[] renderers = {};
        
        [SerializeField]
        private MeshRenderer highlightMeshRenderer = null;
        
        [SerializeField]
        private MeshFilter highlightMeshFilter = null;
        
        private void Reset()
        {
            RefreshCachedRenderers();
        }

        private void OnEnable()
        {
            if (interactableObject == false)
            {
                interactableObject = gameObject.GetComponent<InteractableObject>();
            }

#if XRIT_1_0_OR_NEWER
            interactableObject.firstHoverEntered.AddListener(OnTouched);
            interactableObject.selectEntered.AddListener(OnGrabbed);
            interactableObject.selectExited.AddListener(OnReleased);
            interactableObject.activated.AddListener(OnUsed);
            interactableObject.deactivated.AddListener(OnUnused);
#elif XRIT_0_10_OR_NEWER
            interactableObject.onFirstHoverEntered.AddListener(OnTouched);
            interactableObject.onSelectEntered.AddListener(OnGrabbed);
            interactableObject.onSelectExited.AddListener(OnReleased);
            interactableObject.onActivate.AddListener(OnUsed);
            interactableObject.onDeactivate.AddListener(OnUnused);
#else
            interactableObject.onFirstHoverEnter.AddListener(OnTouched);
            interactableObject.onSelectEnter.AddListener(OnGrabbed);
            interactableObject.onSelectExit.AddListener(OnReleased);
            interactableObject.onActivate.AddListener(OnUsed);
            interactableObject.onDeactivate.AddListener(OnUnused);
#endif
        }

        private void OnDisable()
        {
            if (isBeingHighlighted)
            {
                ReenableRenderers();
                externalHighlights.Clear();
            }
            
            if (interactableObject == false)
            {
                return;
            }
            
#if XRIT_1_0_OR_NEWER
            interactableObject.firstHoverEntered.RemoveListener(OnTouched);
            interactableObject.selectEntered.RemoveListener(OnGrabbed);
            interactableObject.selectExited.RemoveListener(OnReleased);
            interactableObject.activated.RemoveListener(OnUsed);
            interactableObject.deactivated.RemoveListener(OnUnused);
#elif XRIT_0_10_OR_NEWER
            interactableObject.onFirstHoverEntered.RemoveListener(OnTouched);
            interactableObject.onSelectEntered.RemoveListener(OnGrabbed);
            interactableObject.onSelectExited.RemoveListener(OnReleased);
            interactableObject.onActivate.RemoveListener(OnUsed);
            interactableObject.onDeactivate.RemoveListener(OnUnused);
#else
            interactableObject.onFirstHoverEnter.RemoveListener(OnTouched);
            interactableObject.onSelectEnter.RemoveListener(OnGrabbed);
            interactableObject.onSelectExit.RemoveListener(OnReleased);
            interactableObject.onActivate.RemoveListener(OnUsed);
            interactableObject.onDeactivate.RemoveListener(OnUnused);
#endif
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
            if (CanObjectBeHighlighted() == false)
            {
                return;
            }
            
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
            if (CanObjectBeHighlighted() == false)
            {
                return;
            }
            
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
            if (CanObjectBeHighlighted() == false)
            {
                return;
            }
            
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

#if XRIT_1_0_OR_NEWER
        private void OnTouched(HoverEnterEventArgs arguments)
        {
            OnTouchHighlight();
        }
        
        private void OnGrabbed(SelectEnterEventArgs arguments)
        {
            OnGrabHighlight();
        }
        
        private void OnReleased(SelectExitEventArgs arguments)
        {
            OnTouchHighlight();
        }
        
        private void OnUsed(ActivateEventArgs arguments)
        {
            OnUseHighlight();
        }
        
        private void OnUnused(DeactivateEventArgs arg0)
        {
            OnGrabHighlight();
        }
#else

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
#endif
        private IEnumerator Highlight(Material highlightMaterial, Func<bool> shouldContinueHighlighting, string highlightID = "")
        {
            if (highlightMeshRenderer == null || renderers == null || renderers.Any() == false)
            {
                RefreshCachedRenderers();
            }

            while (shouldContinueHighlighting())
            {
                DisableRenders();
                highlightMeshRenderer.sharedMaterial = highlightMaterial;
            
                yield return null;
            }

            ReenableRenderers();

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

        internal void ForceRefreshCachedRenderers()
        {
            if (isBeingHighlighted)
            {
                return;
            }
            
            ReenableRenderers();

            if (Application.isPlaying && gameObject.isStatic)
            {
                return;
            }
            
            renderers = default;
            RefreshCachedRenderers();
        }

        private void RefreshCachedRenderers()
        {
            if (highlightMeshRenderer != null && renderers != null && renderers.Any())
            {
                return;
            }
            
            if (highlightMeshRenderer == null)
            {
                GenerateHighlightRenderer();
            }
            else
            {
                highlightMeshRenderer.enabled = false;
                highlightMeshRenderer.gameObject.SetActive(false);
            }

            renderers = GetComponentsInChildren<SkinnedMeshRenderer>()
                .Where(skinnedMeshRenderer => skinnedMeshRenderer.gameObject.activeInHierarchy && skinnedMeshRenderer.enabled)
                .Concat<Renderer>(GetComponentsInChildren<MeshRenderer>()
                    .Where(meshRenderer => meshRenderer.gameObject.activeInHierarchy && meshRenderer.enabled)).ToArray();

            if (renderers == null || renderers.Any() == false)
            {
                throw new NullReferenceException($"{name} has no renderers to be highlighted.");
            }

            GeneratePreviewMesh();
        }

        private void GenerateHighlightRenderer()
        {
            Transform child = transform.Find("Highlight Renderer");

            if (child == null)
            {
                child = new GameObject("Highlight Renderer").transform;
            }
            
            child.SetPositionAndRotation(transform.position, transform.rotation);
            child.SetParent(transform);
            
            highlightMeshFilter = child.gameObject.GetOrAddComponent<MeshFilter>();
            highlightMeshRenderer = child.gameObject.GetOrAddComponent<MeshRenderer>();

            highlightMeshRenderer.enabled = false;
            highlightMeshRenderer.gameObject.SetActive(false);
        }

        private void GeneratePreviewMesh()
        {
            bool isAnyPartOfStaticBatch = false;
            List<CombineInstance> meshes = new List<CombineInstance>();

            Vector3 cachedPosition = transform.position;
            Quaternion cachedRotation = transform.rotation;
            
            transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            try
            {
                foreach (Renderer renderer in renderers)
                {
                    Type renderType = renderer.GetType();

                    if (renderType == typeof(MeshRenderer))
                    {
                        MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();

                        if (meshFilter.sharedMesh == null)
                        {
                            continue;
                        }

                        if (renderer.isPartOfStaticBatch)
                        {
                            isAnyPartOfStaticBatch = true;
                        }

                        for (int i = 0; i < meshFilter.sharedMesh.subMeshCount; i++)
                        {
                            CombineInstance combineInstance = new CombineInstance
                            {
                                subMeshIndex = i,
                                mesh = meshFilter.sharedMesh,
                                transform = meshFilter.transform.localToWorldMatrix
                            };

                            meshes.Add(combineInstance);
                        }
                    }
                    else if (renderType == typeof(SkinnedMeshRenderer))
                    {
                        SkinnedMeshRenderer skinnedMeshRenderer = renderer as SkinnedMeshRenderer;

                        if (skinnedMeshRenderer.sharedMesh == null)
                        {
                            continue;
                        }

                        if (renderer.isPartOfStaticBatch)
                        {
                            isAnyPartOfStaticBatch = true;
                        }

                        for (int i = 0; i < skinnedMeshRenderer.sharedMesh.subMeshCount; i++)
                        {
                            CombineInstance combineInstance = new CombineInstance
                            {
                                subMeshIndex = i,
                                mesh = skinnedMeshRenderer.sharedMesh,
                                transform = skinnedMeshRenderer.transform.localToWorldMatrix
                            };

                            meshes.Add(combineInstance);
                        }
                    }
                }
            }
            finally
            {
                transform.SetPositionAndRotation(cachedPosition, cachedRotation);
            }

            if (isAnyPartOfStaticBatch)
            {
                throw new NullReferenceException($"{name} is marked as 'Batching Static', no preview mesh to be highlighted could be generated at runtime.\n" +
                                                 "In order to fix this issue, please either remove the static flag of this GameObject or simply " +
                                                 "select it in edit mode so a preview mesh could be generated and cached.");
            } 
            
            if (meshes.Any())
            {
                Mesh previewMesh = new Mesh();
                previewMesh.CombineMeshes(meshes.ToArray());
                
                highlightMeshFilter.mesh = previewMesh;            }
            else
            {
                throw new NullReferenceException($"{name} has no valid meshes to be highlighted.");
            }
        }

        private void DisableRenders()
        {
            if (highlightMeshRenderer != null)
            {
                highlightMeshRenderer.enabled = true;
                highlightMeshRenderer.gameObject.SetActive(true);
            }

            foreach (Renderer renderer in renderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = false;
                }
            }

            isBeingHighlighted = true;
        }

        private void ReenableRenderers()
        {
            if (highlightMeshRenderer != null)
            {
                highlightMeshRenderer.enabled = false;
                highlightMeshRenderer.gameObject.SetActive(false);
            }

            foreach (Renderer renderer in renderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = true;
                }
            }

            isBeingHighlighted = false;
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
            string shaderName = GraphicsSettings.currentRenderPipeline ? "Universal Render Pipeline/Lit" : "Standard";
            Shader defaultShader = Shader.Find(shaderName);

            if (defaultShader == null)
            {
                throw new NullReferenceException($"{name} failed to create a default material," + 
                    $" shader \"{shaderName}\" was not found. Make sure the shader is included into the game build.");
            }

            return new Material(defaultShader);
        }

        private bool CanObjectBeHighlighted()
        {
            if (enabled == false)
            {
                Debug.LogError($"{GetType().Name} component is disabled for {name} and can not be highlighted.", gameObject);
                return false;
            }
            
            if (gameObject.activeInHierarchy == false)
            {
                Debug.LogError($"{name} is disabled and can not be highlighted.", gameObject);
                return false;
            }

            return true;
        }
    }
}

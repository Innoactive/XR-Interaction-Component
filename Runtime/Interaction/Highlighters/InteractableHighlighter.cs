using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Innoactive.Creator.XRInteraction
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class InteractableHighlighter : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        public InteractableObject InteractableObject => interactableObject;

        /// <summary>
        /// 
        /// </summary>
        public bool AllowOnTouchHighlight
        {
            get => allowOnTouchHighlight;
            set => allowOnTouchHighlight = value;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool AllowOnGrabHighlight
        {
            get => allowOnGrabHighlight;
            set => allowOnGrabHighlight = value;
        }

        [SerializeField]
        private bool allowOnTouchHighlight = true;
        [SerializeField]
        private bool allowOnGrabHighlight;
        
        [SerializeField]
        private  Material touchHighlightMaterial;
        [SerializeField]
        private  Material grabHighlightMaterial;
        
        [SerializeField]
        private Color touchHighlightColor = new Color32(64, 200, 255, 50);
        [SerializeField]
        private Color grabHighlightColor;

        private Dictionary<string, bool> externalHighlights = new Dictionary<string, bool>();
        private InteractableObject interactableObject;
        private SkinnedMeshRenderer[] cachedSkinnedRenderers;
        private MeshRenderer[] cachedMeshRenderers;
        private MeshFilter[] cachedMeshFilters;

        private void Awake()
        {
            interactableObject = GetComponent<InteractableObject>();
        }

        private void OnEnable()
        {
            interactableObject.onFirstHoverEnter.AddListener(OnHoverBegin);
            interactableObject.onSelectEnter.AddListener(OnGrabbed);
            interactableObject.onSelectExit.AddListener(OnUngrabbed);
        }

        private void OnDisable()
        {
            interactableObject.onFirstHoverEnter.RemoveListener(OnHoverBegin);
            interactableObject.onSelectEnter.RemoveListener(OnGrabbed);
            interactableObject.onSelectExit.RemoveListener(OnUngrabbed);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="highlightID"></param>
        /// <param name="highlightMaterial"></param>
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
        /// 
        /// </summary>
        /// <param name="highlightID"></param>
        /// <param name="highlightColor"></param>
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
        /// 
        /// </summary>
        /// <param name="highlightID"></param>
        /// <param name="highlightTexture"></param>
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
        /// 
        /// </summary>
        /// <param name="highlightID"></param>
        public void StopHighlighting(string highlightID)
        {
            if (externalHighlights.ContainsKey(highlightID))
            {
                externalHighlights[highlightID] = false;
            }
        }

        private void OnHoverBegin(XRBaseInteractor interactor)
        {
            if (ShouldHighlightHovering())
            {
                if (touchHighlightMaterial == null)
                {
                    touchHighlightMaterial = NewHighlightMaterial(touchHighlightColor);
                }
                
                RefreshCachedRenderers();
                StartCoroutine(Highlight(touchHighlightMaterial, ShouldHighlightHovering));
            }
        }

        private void OnGrabbed(XRBaseInteractor interactor)
        {
            if (ShouldHighlightSelecting())
            {
                if (grabHighlightMaterial == null)
                {
                    grabHighlightMaterial = NewHighlightMaterial(grabHighlightColor);
                }
                
                StartCoroutine(Highlight(grabHighlightMaterial, ShouldHighlightSelecting));
            }
        }

        private void OnUngrabbed(XRBaseInteractor interactor)
        {
            if (ShouldHighlightHovering())
            {
                if (touchHighlightMaterial == null)
                {
                    touchHighlightMaterial = NewHighlightMaterial(touchHighlightColor);
                }
                
                StartCoroutine(Highlight(touchHighlightMaterial, ShouldHighlightHovering));
            }
        }

        private IEnumerator Highlight(Material highlightMaterial, Func<bool> shouldContinueHighlighting, string highlightID = "")
        {
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

        private void RefreshCachedRenderers()
        {
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
            Matrix4x4 matrix = Matrix4x4.TRS(renderer.transform.position, renderer.transform.rotation, renderer.transform.lossyScale);
            Graphics.DrawMesh(mesh, matrix, material, renderer.transform.gameObject.layer);
        }

        private bool ShouldHighlightHovering()
        {
            return allowOnTouchHighlight && interactableObject.isHovered && interactableObject.isSelected == false;
        }

        private bool ShouldHighlightSelecting()
        {
            return allowOnGrabHighlight && interactableObject.isSelected;
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
                throw new NullReferenceException();
            }
            
            return new Material(shader);
        }
    }
}
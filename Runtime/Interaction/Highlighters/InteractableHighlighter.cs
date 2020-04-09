using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Innoactive.Creator.Unity;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Innoactive.Creator.XRInteraction
{
    /// <summary>
    /// 
    /// </summary>
    [RequireComponent(typeof(InteractableObject))]
    public sealed class InteractableHighlighter : MonoBehaviour
    {
        private InteractableObject interactableObject;
        
        [SerializeField]
        private  Material touchHighlightMat;
        
        [SerializeField]
        private  Material grabHighlightMat;
        // [Tooltip("An array of child gameObjects to not render a highlight for. Things like transparent parts, vfx, etc.")]
        // public GameObject[] hideHighlight;
        //
        // [Tooltip("Higher is better")]
        // public int hoverPriority = 0;
        
        private SkinnedMeshRenderer[] cachedSkinnedRenderers;
        private MeshRenderer[] cachedMeshRenderers;
        private MeshFilter[] cachedMeshFilters;

        /// <summary>
        /// 
        /// </summary>
        public InteractableObject InteractableObject => interactableObject;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                RefreshCachedRenderers();
                StartHighlighting("Test");
            }
            
            if (Input.GetKeyDown(KeyCode.S))
            {
                StopHighlighting("Test");
            }
        }

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

        private void OnHoverBegin(XRBaseInteractor interactor)
        {
            if (ShouldHighlightHovering())
            {
                RefreshCachedRenderers();
                StartCoroutine(Highlight(touchHighlightMat, ShouldHighlightHovering));
            }
        }

        private void OnGrabbed(XRBaseInteractor interactor)
        {
            if (ShouldHighlightSelecting())
            {
                StartCoroutine(Highlight(grabHighlightMat, ShouldHighlightSelecting));
            }
        }

        private void OnUngrabbed(XRBaseInteractor interactor)
        {
            if (ShouldHighlightHovering())
            { 
                StartCoroutine(Highlight(touchHighlightMat, ShouldHighlightHovering));
            }
        }
        
        private Dictionary<string, bool> externalHighlights = new Dictionary<string, bool>();

        public void StartHighlighting(string highlightID, Material highlightMaterial = null)
        {
            if (externalHighlights.ContainsKey(highlightID) == false)
            {
                bool shouldContinueHighlighting = true;
                externalHighlights.Add(highlightID, shouldContinueHighlighting);
                StartCoroutine(Highlight(grabHighlightMat, ()=> externalHighlights[highlightID]));
            }
        }
        
        public void StopHighlighting(string highlightID)
        {
            if (externalHighlights.ContainsKey(highlightID))
            {
                externalHighlights[highlightID] = false;
                externalHighlights.Remove(highlightID);
            }
        }

        private IEnumerator Highlight(Material highlightMaterial, Func<bool> shouldContinueHighlighting)
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
            return touchHighlightMat != null && interactableObject.isHovered && interactableObject.isSelected == false;
        }

        private bool ShouldHighlightSelecting()
        {
            return grabHighlightMat != null && interactableObject.isSelected;
        }
    }
}
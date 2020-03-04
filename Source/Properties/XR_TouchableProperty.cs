﻿#if CREATOR_XR_INTERACTION

using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Innoactive.Hub.Training.SceneObjects.Properties;
using Innoactive.Hub.Training.SceneObjects.Interaction.Properties;

namespace Innoactive.Creator.XR.SceneObjects.Properties
{ 
    /// <summary>
    /// XR implementation of <see cref="ITouchableProperty"/>.
    /// </summary>
    public class XR_TouchableProperty : LockableProperty, ITouchableProperty
    {
        public event EventHandler<EventArgs> Touched;
        public event EventHandler<EventArgs> Untouched;

        /// <summary>
        /// Returns true if the GameObject is touched.
        /// </summary>
        public virtual bool IsBeingTouched
        {
            get
            {
                return Interactable != null && Interactable.isHovered;
            }
        }

        /// <summary>
        /// Reference to attached 'XRGrabInteractable'.
        /// </summary>
        protected XRBaseInteractable Interactable;

        private LayerMask cacheLayers = 0;
        
        protected override void OnEnable()
        {
            base.OnEnable();

            Interactable = gameObject.GetComponent<XRBaseInteractable>();
            
            if (Interactable == null)
            {
                Interactable = gameObject.AddComponent<XRGrabInteractable>();
            }

            Interactable.onFirstHoverEnter.AddListener(HandleXRTouched);
            Interactable.onLastHoverExit.AddListener(HandleXRUntouched);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            Interactable.onFirstHoverEnter.RemoveListener(HandleXRTouched);
            Interactable.onLastHoverExit.RemoveListener(HandleXRUntouched);
        }

        private void HandleXRTouched(XRBaseInteractor interactor)
        {
            EmitTouched();
        }

        private void HandleXRUntouched(XRBaseInteractor interactor)
        {
            EmitUntouched();
        }

        protected void EmitTouched()
        {
            Touched?.Invoke(this, EventArgs.Empty);
        }

        protected void EmitUntouched()
        {
            Untouched?.Invoke(this, EventArgs.Empty);
        }

        protected override void InternalSetLocked(bool lockState)
        {
            if (lockState)
            {
                StopHovering();
            }
            else
            {
                StartHovering();
            }
        }

        /// <inheritdoc />
        public void FastForwardTouch()
        {
            if (Interactable.isHovered)
            {
                StopHovering();
                StartHovering();
            }
            else
            {
                EmitTouched();
                EmitUntouched();
            }
        }

        /// <summary>
        /// Starts interaction with <see cref="Interactable"/>.
        /// </summary>
        protected virtual void StartHovering()
        {
            Interactable.interactionLayerMask = cacheLayers;
        }
        
        /// <summary>
        /// Stops interaction with <see cref="Interactable"/>.
        /// </summary>
        protected virtual void StopHovering()
        {
            cacheLayers = Interactable.interactionLayerMask;
            Interactable.interactionLayerMask = 0;
        }
    }
}

#endif
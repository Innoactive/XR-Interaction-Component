#if CREATOR_XR_INTERACTION

using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Innoactive.Hub.Unity;
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
        protected XRInteractableObject Interactable;

        private LayerMask cacheLayers = 0;
        
        protected override void OnEnable()
        {
            base.OnEnable();

            Interactable = gameObject.GetComponent<XRInteractableObject>(true);

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
            Interactable.IsTouchable = lockState == false;
            
            if (IsBeingTouched)
            {
                if (lockState)
                {
                    Interactable.ForceStopInteracting();
                }
            }
        }

        /// <inheritdoc />
        public void FastForwardTouch()
        {
            if (IsBeingTouched)
            {
                Interactable.ForceStopInteracting();
            }
            else
            {
                EmitTouched();
                EmitUntouched();
            }
        }
    }
}

#endif
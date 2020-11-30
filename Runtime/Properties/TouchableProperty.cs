using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Innoactive.Creator.Core.Properties;
using Innoactive.Creator.BasicInteraction.Properties;

namespace Innoactive.Creator.XRInteraction.Properties
{ 
    /// <summary>
    /// XR implementation of <see cref="ITouchableProperty"/>.
    /// </summary>
    [RequireComponent(typeof(InteractableObject))]
    public class TouchableProperty : LockableProperty, ITouchableProperty
    {
        public event EventHandler<EventArgs> Touched;
        public event EventHandler<EventArgs> Untouched;

        /// <summary>
        /// Returns true if the GameObject is touched.
        /// </summary>
        public virtual bool IsBeingTouched => Interactable != null && Interactable.isHovered;

        /// <summary>
        /// Reference to attached <see cref="InteractableObject"/>.
        /// </summary>
        protected InteractableObject Interactable
        {
            get
            {
                if (interactable == false)
                {
                    interactable = GetComponent<InteractableObject>();
                }

                return interactable;
            }
        }

        private InteractableObject interactable;

        protected override void OnEnable()
        {
            base.OnEnable();
#if XRIT_0_10_OR_NEWER
            Interactable.onFirstHoverEntered.AddListener(HandleXRTouched);
            Interactable.onLastHoverExited.AddListener(HandleXRUntouched);
#else
            Interactable.onFirstHoverEnter.AddListener(HandleXRTouched);
            Interactable.onLastHoverExit.AddListener(HandleXRUntouched);
#endif

            InternalSetLocked(IsLocked);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
#if XRIT_0_10_OR_NEWER
            Interactable.onFirstHoverEntered.RemoveListener(HandleXRTouched);
            Interactable.onLastHoverExited.RemoveListener(HandleXRUntouched);
#else
            Interactable.onFirstHoverEnter.RemoveListener(HandleXRTouched);
            Interactable.onLastHoverExit.RemoveListener(HandleXRUntouched);
#endif
        }
        
        protected void Reset()
        {
            Interactable.IsTouchable = true;
            gameObject.GetComponent<Rigidbody>().isKinematic = false;
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
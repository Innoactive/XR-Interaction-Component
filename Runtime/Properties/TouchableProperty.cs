using System;
using Innoactive.Creator.BasicInteraction.Properties;
using Innoactive.Creator.Core.Properties;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

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
        public virtual bool IsBeingTouched
        {
            get
            {
                return Interactable != null && Interactable.isHovered;
            }
        }

        /// <summary>
        /// Reference to attached <see cref="InteractableObject"/>.
        /// </summary>
        protected InteractableObject Interactable;

        protected override void OnEnable()
        {
            base.OnEnable();

            Interactable = gameObject.GetComponent<InteractableObject>();

            if (Interactable == false)
            {
                Interactable = gameObject.AddComponent<InteractableObject>();
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
using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Innoactive.Hub.Training.SceneObjects.Properties;
using Innoactive.Hub.Training.SceneObjects.Interaction.Properties;

namespace Innoactive.Creator.SceneObjects.Properties
{
    /// <summary>
    /// XR implementation of <see cref="IUsableProperty"/>.
    /// </summary>
    [RequireComponent(typeof(XR_GrabbableProperty))]
    public class XR_UsableProperty : LockableProperty, IUsableProperty
    {
        public event EventHandler<EventArgs> UsageStarted;
        public event EventHandler<EventArgs> UsageStopped;

        private bool isBeingUsed;

        /// <summary>
        /// Returns true if the GameObject is being used.
        /// </summary>
        public virtual bool IsBeingUsed
        {
            get { return isBeingUsed; }
        }

        /// <summary>
        /// Reference to attached 'XRGrabInteractable'.
        /// </summary>
        protected XRBaseInteractable Interactable;

        protected override void OnEnable()
        {
            base.OnEnable();

            Interactable = gameObject.GetComponent<XRBaseInteractable>();
            
            if (Interactable == null)
            {
                Interactable = gameObject.AddComponent<XRGrabInteractable>();
            }

            Interactable.onActivate.AddListener(HandleXRUsageStarted);
            Interactable.onDeactivate.AddListener(HandleXRUsageStopped);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            Interactable.onActivate.RemoveListener(HandleXRUsageStarted);
            Interactable.onDeactivate.RemoveListener(HandleXRUsageStopped);
        }

        private void HandleXRUsageStarted(XRBaseInteractor interactor)
        {
            isBeingUsed = true;
            EmitUsageStarted();
        }

        private void HandleXRUsageStopped(XRBaseInteractor interactor)
        {
            isBeingUsed = false;
            EmitUsageStopped();
        }

        protected void EmitUsageStarted()
        {
            UsageStarted?.Invoke(this, EventArgs.Empty);
        }

        protected void EmitUsageStopped()
        {
            UsageStopped?.Invoke(this, EventArgs.Empty);
        }

        protected override void InternalSetLocked(bool lockState)
        {
            if (lockState)
            {
                StopUsing();
            }
            else
            {
                StartUsing();
            }
        }

        /// <summary>
        /// Instantaneously simulate that the object was used.
        /// </summary>
        public void FastForwardUse()
        {
            if (IsBeingUsed)
            {
                StopUsing();
                StartUsing();
            }
            else
            {
                EmitUsageStarted();
                EmitUsageStopped();
            }
        }
        
        /// <summary>
        /// Starts interaction with <see cref="Interactable"/>.
        /// </summary>
        protected virtual void StartUsing()
        {
            Interactable.interactionLayerMask = -1;
        }
        
        /// <summary>
        /// Stops interaction with <see cref="Interactable"/>.
        /// </summary>
        protected virtual void StopUsing()
        {
            Interactable.interactionLayerMask = 0;
        }
    }
}

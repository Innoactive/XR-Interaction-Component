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
    /// XR implementation of <see cref="IUsableProperty"/>.
    /// </summary>
    [RequireComponent(typeof(GrabbableProperty))]
    public class UsableProperty : LockableProperty, IUsableProperty
    {
        public event EventHandler<EventArgs> UsageStarted;
        public event EventHandler<EventArgs> UsageStopped;

        /// <summary>
        /// Returns true if the GameObject is being used.
        /// </summary>
        public virtual bool IsBeingUsed
        {
            get
            {
                return Interactable != null && Interactable.IsActivated;
            }
        }

        /// <summary>
        /// Reference to attached 'XRGrabInteractable'.
        /// </summary>
        protected InteractableObject Interactable;
        
        private LayerMask cacheLayers = 0;

        protected override void OnEnable()
        {
            base.OnEnable();

            Interactable = gameObject.GetComponent<InteractableObject>(true);

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
            EmitUsageStarted();
        }

        private void HandleXRUsageStopped(XRBaseInteractor interactor)
        {
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
            Interactable.IsUsable = lockState == false;
            
            if (IsBeingUsed)
            {
                if (lockState)
                {
                    Interactable.ForceStopInteracting();
                }
            }
        }

        /// <summary>
        /// Instantaneously simulate that the object was used.
        /// </summary>
        public void FastForwardUse()
        {
            if (IsBeingUsed)
            {
                Interactable.ForceStopInteracting();
            }
            else
            {
                EmitUsageStarted();
                EmitUsageStopped();
            }
        }
    }
}

#endif
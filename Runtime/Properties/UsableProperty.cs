using System;
using Innoactive.Creator.BasicInteraction.Properties;
using Innoactive.Creator.Core.Properties;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Innoactive.Creator.XRInteraction.Properties
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
        /// Reference to attached <see cref="InteractableObject"/>.
        /// </summary>
        protected InteractableObject Interactable;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (Interactable == false)
            {
                Interactable = gameObject.GetOrAddComponent<InteractableObject>();
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
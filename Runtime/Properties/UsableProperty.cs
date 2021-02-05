using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Innoactive.Creator.Core.Properties;
using Innoactive.Creator.BasicInteraction.Properties;

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
            
#if XRIT_1_0_OR_NEWER
            Interactable.activated.AddListener(HandleXRUsageStarted);
            Interactable.deactivated.AddListener(HandleXRUsageStopped);
#else
            Interactable.onActivate.AddListener(HandleXRUsageStarted);
            Interactable.onDeactivate.AddListener(HandleXRUsageStopped);
#endif
            InternalSetLocked(IsLocked);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

#if XRIT_1_0_OR_NEWER
            Interactable.activated.RemoveListener(HandleXRUsageStarted);
            Interactable.deactivated.RemoveListener(HandleXRUsageStopped);
#else
            Interactable.onActivate.RemoveListener(HandleXRUsageStarted);
            Interactable.onDeactivate.RemoveListener(HandleXRUsageStopped);
#endif
        }

        protected void Reset()
        {
            Interactable.IsUsable = true;
            gameObject.GetComponent<Rigidbody>().isKinematic = false;
        }

#if XRIT_1_0_OR_NEWER
        private void HandleXRUsageStarted(ActivateEventArgs arguments)
#else
        private void HandleXRUsageStarted(XRBaseInteractor interactor)
#endif
        {
            EmitUsageStarted();
        }

#if XRIT_1_0_OR_NEWER
        private void HandleXRUsageStopped(DeactivateEventArgs arguments)
#else
        private void HandleXRUsageStopped(XRBaseInteractor interactor)
#endif
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
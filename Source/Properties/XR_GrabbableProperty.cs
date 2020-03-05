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
    /// XR implementation of <see cref="IGrabbableProperty"/>.
    /// </summary>
    [RequireComponent(typeof(XR_TouchableProperty))]
    public class XR_GrabbableProperty : LockableProperty, IGrabbableProperty
    {
        public event EventHandler<EventArgs> Grabbed;
        public event EventHandler<EventArgs> Ungrabbed;

        /// <summary>
        /// Returns true if the Interactable of this property is grabbed.
        /// </summary>
        public virtual bool IsGrabbed
        {
            get
            {
                return Interactable != null && Interactable.isSelected;
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

            Interactable.onSelectEnter.AddListener(HandleXRGrabbed);
            Interactable.onSelectExit.AddListener(HandleXRUngrabbed);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            Interactable.onSelectEnter.RemoveListener(HandleXRGrabbed);
            Interactable.onSelectExit.RemoveListener(HandleXRUngrabbed);
        }

        private void HandleXRGrabbed(XRBaseInteractor interactor)
        {
            EmitGrabbed();
        }

        private void HandleXRUngrabbed(XRBaseInteractor interactor)
        {
            EmitUngrabbed();
        }

        protected void EmitGrabbed()
        {
            Grabbed?.Invoke(this, EventArgs.Empty);
        }

        protected void EmitUngrabbed()
        {
            Ungrabbed?.Invoke(this, EventArgs.Empty);
        }

        protected override void InternalSetLocked(bool lockState)
        {
            if (IsGrabbed)
            {
                if (lockState)
                {
                    Interactable.ForceStopInteracting();
                }
            }
            
            Interactable.IsGrabbable = lockState == false;
        }

        /// <summary>
        /// Instantaneously simulate that the object was grabbed.
        /// </summary>
        public void FastForwardGrab()
        {
            if (Interactable.isSelected)
            {
                Interactable.AttemptGrab();
                Interactable.ForceStopInteracting();
            }
            else
            {
                EmitGrabbed();
                EmitUngrabbed();
            }
        }

        /// <summary>
        /// Instantaneously simulate that the object was ungrabbed.
        /// </summary>
        public void FastForwardUngrab()
        {
            if (Interactable.isSelected)
            {
                Interactable.ForceStopInteracting();
            }
            else
            {
                EmitGrabbed();
                EmitUngrabbed();
            }
        }
    }
}

#endif
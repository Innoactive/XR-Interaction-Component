#if CREATOR_XR_INTERACTION

using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
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
            if (lockState)
            {
                StopSelecting();
            }
            else
            {
                StartSelecting();
            }
        }

        /// <summary>
        /// Instantaneously simulate that the object was grabbed.
        /// </summary>
        public void FastForwardGrab()
        {
            SimulateGrabActions();
        }

        /// <summary>
        /// Instantaneously simulate that the object was ungrabbed.
        /// </summary>
        public void FastForwardUngrab()
        {
            SimulateGrabActions();
        }

        private void SimulateGrabActions()
        {
            if (Interactable.isSelected)
            {
                StopSelecting();
                StartSelecting();
            }
            else
            {
                EmitGrabbed();
                EmitUngrabbed();
            }
        }
        
        /// <summary>
        /// Starts interaction with <see cref="Interactable"/>.
        /// </summary>
        protected virtual void StartSelecting()
        {
            Interactable.interactionLayerMask = cacheLayers;
        }
        
        /// <summary>
        /// Stops interaction with <see cref="Interactable"/>.
        /// </summary>
        protected virtual void StopSelecting()
        {
            cacheLayers = Interactable.interactionLayerMask;
            Interactable.interactionLayerMask = 0;
        }
    }
}

#endif
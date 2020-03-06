﻿#if CREATOR_XR_INTERACTION

using System;
using System.Collections;
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
    [RequireComponent(typeof(TouchableProperty))]
    public class GrabbableProperty : LockableProperty, IGrabbableProperty
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
        protected InteractableObject Interactable;
        
        private LayerMask cacheLayers = 0;
        
        protected override void OnEnable()
        {
            base.OnEnable();

            Interactable = gameObject.GetComponent<InteractableObject>(true);

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
            Interactable.IsGrabbable = lockState == false;
            
            if (IsGrabbed)
            {
                if (lockState)
                {
                    Interactable.ForceStopInteracting();
                }
            }
        }

        /// <summary>
        /// Instantaneously simulate that the object was grabbed.
        /// </summary>
        public void FastForwardGrab()
        {
            if (Interactable.isSelected)
            {
                StartCoroutine(UngrabGrabAndRelease());
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

        private IEnumerator UngrabGrabAndRelease()
        {
            Interactable.ForceStopInteracting();
                
            XRBaseInteractor interactor = Interactable.selectingInteractor;

            yield return new WaitUntil(() => Interactable.isHovered == false && Interactable.isSelected == false);
            
            if (interactor != null)
            {
                DirectInteractor directInteractor = (DirectInteractor)interactor;
                directInteractor.AttemptGrab();

                yield return null;
                
                Interactable.ForceStopInteracting();
            }
        }
    }
}

#endif
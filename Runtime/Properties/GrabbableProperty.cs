using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using VPG.Core.Properties;
using VPG.BasicInteraction.Properties;

namespace VPG.XRInteraction.Properties
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
        public virtual bool IsGrabbed => Interactable != null && Interactable.isSelected;

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

            Interactable.selectEntered.AddListener(HandleXRGrabbed);
            Interactable.selectExited.AddListener(HandleXRUngrabbed);

            InternalSetLocked(IsLocked);
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
        
            Interactable.selectEntered.RemoveListener(HandleXRGrabbed);
            Interactable.selectExited.RemoveListener(HandleXRUngrabbed);
        }

        protected void Reset()
        {
            Interactable.IsGrabbable = true;
            GetComponent<Rigidbody>().isKinematic = false;
        }

        private void HandleXRGrabbed(SelectEnterEventArgs arguments)
        {
            EmitGrabbed();
        }

        private void HandleXRUngrabbed(SelectExitEventArgs arguments)
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
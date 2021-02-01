using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Innoactive.Creator.Core.Properties;
using Innoactive.Creator.BasicInteraction.Properties;

namespace Innoactive.Creator.XRInteraction.Properties
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

#if XRIT_1_0_OR_NEWER
            Interactable.selectEntered.AddListener(HandleXRGrabbed);
            Interactable.selectExited.AddListener(HandleXRUngrabbed);
#elif XRIT_0_10_OR_NEWER
            Interactable.onSelectEntered.AddListener(HandleXRGrabbed);
            Interactable.onSelectExited.AddListener(HandleXRUngrabbed);
#else
            Interactable.onSelectEnter.AddListener(HandleXRGrabbed);
            Interactable.onSelectExit.AddListener(HandleXRUngrabbed);
#endif

            InternalSetLocked(IsLocked);
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
        
#if XRIT_1_0_OR_NEWER
            Interactable.selectEntered.RemoveListener(HandleXRGrabbed);
            Interactable.selectExited.RemoveListener(HandleXRUngrabbed);
#elif XRIT_0_10_OR_NEWER
            Interactable.onSelectEntered.RemoveListener(HandleXRGrabbed);
            Interactable.onSelectExited.RemoveListener(HandleXRUngrabbed);
#else
            Interactable.onSelectEnter.RemoveListener(HandleXRGrabbed);
            Interactable.onSelectExit.RemoveListener(HandleXRUngrabbed);
#endif
        }

        protected void Reset()
        {
            Interactable.IsGrabbable = true;
            GetComponent<Rigidbody>().isKinematic = false;
        }

#if XRIT_1_0_OR_NEWER
        private void HandleXRGrabbed(SelectEnterEventArgs arguments)
#else
        private void HandleXRGrabbed(XRBaseInteractor interactor)
#endif
        {
            EmitGrabbed();
        }

#if XRIT_1_0_OR_NEWER
        private void HandleXRUngrabbed(SelectExitEventArgs arguments)
#else
        private void HandleXRUngrabbed(XRBaseInteractor interactor)
#endif
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
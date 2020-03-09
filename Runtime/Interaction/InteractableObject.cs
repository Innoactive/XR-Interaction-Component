#if CREATOR_XR_INTERACTION

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Innoactive.Creator.XR
{
    /// <inheritdoc />
    /// <remarks>Allows locking interactions.</remarks>
    public class InteractableObject : XRGrabInteractable
    {
        private bool isTouchable = true;
        private bool isGrabbable = true;
        private bool isUsable = true;

        public bool IsActivated { get; private set; }

        public bool IsInSocket
        {
            get { return selectingSocket != null; }
        }
        
        private XRSocketInteractor selectingSocket;
        
        /// <summary>
        /// Get the current selecting 'XRSocketInteractor' for this interactable.
        /// </summary>
        public XRSocketInteractor SelectingSocket { get { return selectingSocket; } } 

        /// <summary>
        /// Determines if the Interactable Object can be touched.
        /// </summary>
        public bool IsTouchable
        {
            set { isTouchable = value; }
        }

        /// <summary>
        /// Determines if the Interactable Object can be grabbed.
        /// </summary>
        public bool IsGrabbable
        {
            set { isGrabbable = value; }
        }

        /// <summary>
        /// Determines if the Interactable Object can be used.
        /// </summary>
        public bool IsUsable
        {
            set { isUsable = value; }
        }

        /// <inheritdoc />
        public override bool IsHoverableBy(XRBaseInteractor interactor)
        {
            return isTouchable && base.IsHoverableBy(interactor);
        }

        /// <inheritdoc />
        public override bool IsSelectableBy(XRBaseInteractor interactor)
        {
            if (IsInSocket && interactor == selectingSocket)
            {
                return true;
            }
            
            return isGrabbable && base.IsSelectableBy(interactor);
        }

        /// <summary>
        /// Forces all hovering and selecting interactors to not have interactions with this interactable for one frame.
        /// </summary>
        /// <remarks>Interactions are not disabled immediately.</remarks>
        public virtual void ForceStopInteracting()
        {
            if (IsActivated)
            {
                OnDeactivate(selectingInteractor);
            }
            
            StartCoroutine(StopInteractingForOneFrame());
        }

        /// <summary>
        /// Attempt to use the currently selected interactable without needing to press the use button on the controller.
        /// </summary>
        public virtual void ForceUse()
        {
            OnActivate(selectingInteractor);
        }

        /// <inheritdoc />
        protected override void OnSelectEnter(XRBaseInteractor interactor)
        {
            base.OnSelectEnter(interactor);

            if (IsInSocket == false)
            {
                XRSocketInteractor socket = interactor.GetComponent<XRSocketInteractor>();

                if (socket != null)
                {
                    selectingSocket = socket;
                }
            }
        }

        /// <inheritdoc />
        protected override void OnSelectExit(XRBaseInteractor interactor)
        {
            base.OnSelectExit(interactor);
            
            if (IsInSocket == false)
            {
                if (IsInSocket && interactor == selectingSocket)
                {
                    selectingSocket = null;
                }
            }
        }

        /// <inheritdoc />
        protected override void OnActivate(XRBaseInteractor interactor)
        {
            if (isUsable)
            {
                IsActivated = true;
                base.OnActivate(interactor);
            }
        }

        /// <inheritdoc />
        protected override void OnDeactivate(XRBaseInteractor interactor)
        {
            if (isUsable)
            {
                IsActivated = false;
                base.OnDeactivate(interactor);
            }
        }
        
        private IEnumerator StopInteractingForOneFrame()
        {
            List<XRBaseInteractor> interactors = new List<XRBaseInteractor>(hoveringInteractors);
            
            if (interactors.Contains(selectingInteractor) == false)
            {
                interactors.Add(selectingInteractor);
            }
            
            foreach (XRBaseInteractor interactor in interactors)
            {
                if (interactor != null)
                {
                    interactor.enableInteractions = false;
                }
            }
            
            yield return new WaitUntil(() => isHovered == false && isSelected == false);
            
            foreach (XRBaseInteractor interactor in interactors)
            {
                if (interactor != null)
                {
                    interactor.enableInteractions = true;
                }
            }
        }
    }
}

#endif
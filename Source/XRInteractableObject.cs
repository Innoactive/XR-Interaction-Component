#if CREATOR_XR_INTERACTION

using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Innoactive.Creator.XR
{
    /// <inheritdoc />
    /// <remarks>Allows locking interactions.</remarks>
    public class XRInteractableObject : XRGrabInteractable
    {
        private bool isTouchable = true;
        private bool isGrabbable = true;
        private bool isUsable = true;

        public bool IsActivated { get; private set; }

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
            return isGrabbable && base.IsSelectableBy(interactor);
        }

        /// <summary>
        /// Forces the interactable to no longer be interacted with and will cause an interactor to drop this interactable and stop touching it.
        /// </summary>
        public virtual void ForceStopInteracting()
        {
            if (IsActivated)
            {
                OnDeactivate(selectingInteractor);
            }
            
            StartCoroutine(ForceStopInteractingAtEndOfFrame());
        }

        /// <summary>
        /// Attempt to be grabbed by any hovering interactor without needing to press the grab button on the controller.
        /// </summary>
        public virtual void AttemptGrab()
        {
            XRBaseInteractor interactor = hoveringInteractors.First();
            OnSelectEnter(interactor);
        }

        /// <summary>
        /// Attempt to use the currently selected interactable without needing to press the use button on the controller.
        /// </summary>
        public virtual void ForceUse()
        {
            OnActivate(selectingInteractor);
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
        
        private IEnumerator ForceStopInteractingAtEndOfFrame()
        {
            bool wasHovered = isHovered;
            bool wasSelected = isSelected;
            
            if (wasHovered && isTouchable)
            {
                isTouchable = false;
            }
            
            if (wasSelected == isGrabbable)
            {
                isGrabbable = false;
            }

            yield return null;
            
            if (wasHovered && isHovered == false)
            {
                isTouchable = true;
            }
            
            if (wasSelected && isSelected == false)
            {
                isGrabbable = true;
            }
        }
    }
}

#endif
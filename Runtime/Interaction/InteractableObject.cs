using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Innoactive.Creator.XRInteraction
{
    /// <summary>
    /// Interactable component that allows basic "grab" functionality.
    /// Can attach to selecting interactor and follow it around while obeying physics (and inherit velocity when released).
    /// </summary>
    /// <remarks>Adds extra control over applicable interactions.</remarks>
    [RequireComponent(typeof(InteractableHighlighter))]
    public class InteractableObject : XRGrabInteractable
    {
        [SerializeField]
        private bool isTouchable = true;
        [SerializeField]
        
        private bool isGrabbable = true;
        
        [SerializeField]
        private bool isUsable = true;

        private XRSocketInteractor selectingSocket;
        
        /// <summary>
        /// Determines if this <see cref="InteractableObject"/> can be touched.
        /// </summary>
        public bool IsTouchable
        {
            set => isTouchable = value;
        }

        /// <summary>
        /// Determines if this <see cref="InteractableObject"/> can be grabbed.
        /// </summary>
        public bool IsGrabbable
        {
            set => isGrabbable = value;
        }

        /// <summary>
        /// Determines if this <see cref="InteractableObject"/> can be used.
        /// </summary>
        public bool IsUsable
        {
            set => isUsable = value;
        }
        
        /// <summary>
        /// Gets whether this <see cref="InteractableObject"/> is currently being activated.
        /// </summary>
        public bool IsActivated { get; private set; }

        /// <summary>
        /// Gets whether this <see cref="InteractableObject"/> is currently being selected by any 'XRSocketInteractor'.
        /// </summary>
        public bool IsInSocket => selectingSocket != null;

        /// <summary>
        /// Get the current selecting 'XRSocketInteractor' for this <see cref="InteractableObject"/>.
        /// </summary>
        public XRSocketInteractor SelectingSocket => selectingSocket;
        
        /// <summary>
        /// Sets the 'interactionLayerMask' to Default in order to not interact with Teleportation or UI rays.
        /// </summary>
        protected override void Reset()
        {
            base.Reset();
            interactionLayerMask = 1;
        }

        /// <summary>
        /// Determines if this <see cref="InteractableObject"/> can be hovered by a given interactor.
        /// </summary>
        /// <param name="interactor">Interactor to check for a valid hover state with.</param>
        /// <returns>True if hovering is valid this frame, False if not.</returns>
        /// <remarks>It always returns false when <see cref="IsTouchable"/> is false.</remarks>
        public override bool IsHoverableBy(XRBaseInteractor interactor)
        {
            return isTouchable && base.IsHoverableBy(interactor);
        }

        /// <summary>
        /// Determines if this <see cref="InteractableObject"/> can be selected by a given interactor.
        /// </summary>
        /// <param name="interactor">Interactor to check for a valid selection with.</param>
        /// <returns>True if selection is valid this frame, False if not.</returns>
        /// <remarks>It always returns false when <see cref="IsGrabbable"/> is false.</remarks>
        public override bool IsSelectableBy(XRBaseInteractor interactor)
        {
            if (IsInSocket && interactor == selectingSocket)
            {
                return true;
            }
            
            return isGrabbable && base.IsSelectableBy(interactor);
        }

        /// <summary>
        /// Forces all hovering and selecting interactors to not have interactions with this <see cref="InteractableObject"/> for one frame.
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
        /// Attempts to use this <see cref="InteractableObject"/>.
        /// </summary>
        public virtual void ForceUse()
        {
            OnActivate(selectingInteractor);
        }

        /// <summary>This method is called by the interaction manager 
        /// when the interactor first initiates selection of an interactable.</summary>
        /// <param name="interactor">Interactor that is initiating the selection.</param>
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

        /// <summary>This method is called by the interaction manager 
        /// when the interactor ends selection of an interactable.</summary>
        /// <param name="interactor">Interactor that is ending the selection.</param>
        protected override void OnSelectExit(XRBaseInteractor interactor)
        {
            base.OnSelectExit(interactor);
            
            if (IsInSocket && interactor == selectingSocket)
            {
                selectingSocket = null;
            }
        }

        /// <summary>This method is called by the interaction manager 
        /// when the interactor sends an activation event down to an interactable.</summary>
        /// <param name="interactor">Interactor that is sending the activation event.</param>
        protected override void OnActivate(XRBaseInteractor interactor)
        {
            if (isUsable)
            {
                IsActivated = true;
                base.OnActivate(interactor);
            }
        }

        /// <summary>This method is called by the interaction manager 
        /// when the interactor sends a deactivation event down to an interactable.</summary>
        /// <param name="interactor">Interactor that is sending the activation event.</param>
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
            
            foreach (XRBaseInteractor interactor in interactors.Where(interactor => interactor != null))
            {
                if (interactor.GetComponent<XRController>() != null)
                {
                    interactor.enableInteractions = false;
                }
            }
            
            yield return new WaitUntil(() => isHovered == false && (isSelected == false || IsInSocket));
            
            foreach (XRBaseInteractor interactor in interactors.Where(interactor => interactor != null))
            {
                if (interactor.GetComponent<XRController>() != null)
                {
                    interactor.enableInteractions = true;
                }
            }
        }
    }
}

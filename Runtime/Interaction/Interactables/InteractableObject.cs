﻿using System.Collections;
using VPG.BasicInteraction;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace VPG.XRInteraction
{
    /// <summary>
    /// Interactable component that allows basic "grab" functionality.
    /// Can attach to selecting interactor and follow it around while obeying physics (and inherit velocity when released).
    /// </summary>
    /// <remarks>Adds extra control over applicable interactions.</remarks>
    public class InteractableObject : XRGrabInteractable, IInteractableObject
    {
        [SerializeField]
        private bool isTouchable = true;
        
        [SerializeField]
        private bool isGrabbable = true;
        
        [SerializeField]
        private bool isUsable = true;
        
        private Rigidbody internalRigidbody;
        private XRSocketInteractor selectingSocket;

        /// <summary>
        /// This interactable's rigidbody.
        /// </summary>
        public Rigidbody Rigidbody
        {
            get
            {
                if (internalRigidbody == null)
                {
                    internalRigidbody = GetComponent<Rigidbody>();
                }
                
                return internalRigidbody;
            }
        }

        /// <inheritdoc/>
        public GameObject GameObject => gameObject;

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
        /// Reset to default values.
        /// </summary>
        protected override void Reset()
        {
            base.Reset();

            // Sets the 'interactionLayerMask' to Default in order to not interact with Teleportation or UI rays.
            interactionLayerMask = 1;
        }

        internal void OnTriggerEnter(Collider other)
        {
            SnapZone target = other.gameObject.GetComponent<SnapZone>();            
            if (target != null && target.enabled && !IsInSocket && isSelected)
            {
                target.AddHoveredInteractable(this);
            }
        }

        internal void OnTriggerExit(Collider other)
        {
            SnapZone target = other.gameObject.GetComponent<SnapZone>();            
            if (target != null && target.enabled)
            {
                target.RemoveHoveredInteractable(this);
            }
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
                OnDeactivated(new DeactivateEventArgs
                {
                    interactable =  this,
                    interactor = selectingInteractor
                });
            }
            
            StartCoroutine(StopInteractingForOneFrame());
        }

        /// <summary>
        /// Attempts to use this <see cref="InteractableObject"/>.
        /// </summary>
        public virtual void ForceUse()
        {
            OnActivated(new ActivateEventArgs
            {
                interactable = this,
                interactor = selectingInteractor
            });
        }

        internal void ForceSelectEnter(XRBaseInteractor interactor)
        {
            interactionManager.ForceSelect(interactor, this);
        }

        /// <summary>
        /// This method is called by the Interaction Manager
        /// right before the Interactor first initiates selection of an Interactable
        /// in a first pass.
        /// </summary>
        /// <param name="arguments">Event data containing the Interactor that is initiating the selection.</param>
        /// <remarks>
        /// <paramref name="arguments"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="OnSelectEntered(SelectEnterEventArgs)"/>
        protected override void OnSelectEntering(SelectEnterEventArgs arguments)
        {
            base.OnSelectEntering(arguments);
            XRBaseInteractor interactor = arguments.interactor;

            if (IsInSocket == false)
            {
                XRSocketInteractor socket = interactor.GetComponent<XRSocketInteractor>();

                if (socket != null)
                {
                    selectingSocket = socket;
                }
            }
        }
        
        [System.Obsolete("OnSelectEntering(XRBaseInteractor) has been deprecated. Please, upgrade the XR Interaction Toolkit from the Package Manager to the latest available version.")]
        protected new void OnSelectEntering(XRBaseInteractor interactor)
        {
            OnSelectEntering(new SelectEnterEventArgs
            {
                interactor = interactor,
                interactable = this
            });
        }

        [System.Obsolete("OnSelectEnter(XRBaseInteractor) has been deprecated. Please, upgrade the XR Interaction Toolkit from the Package Manager to the latest available version.")]
        protected void OnSelectEnter(XRBaseInteractor interactor)
        {
            OnSelectEntering(new SelectEnterEventArgs
            {
                interactor = interactor,
                interactable = this
            });
        }
        
        /// <summary>
        /// This method is called by the Interaction Manager
        /// right before the Interactor ends selection of an Interactable
        /// in a first pass.
        /// </summary>
        /// <param name="arguments">Event data containing the Interactor that is ending the selection.</param>
        /// <remarks>
        /// <paramref name="arguments"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="OnSelectExited(SelectExitEventArgs)"/>
        protected override void OnSelectExiting(SelectExitEventArgs arguments)
        {
            base.OnSelectExiting(arguments);
            XRBaseInteractor interactor = arguments.interactor;
            
            if (IsInSocket && interactor == selectingSocket)
            {
                selectingSocket = null;
            }
        }

        [System.Obsolete("OnSelectExiting(XRBaseInteractor) has been deprecated. Please, upgrade the XR Interaction Toolkit from the Package Manager to the latest available version.")]
        protected new void OnSelectExiting(XRBaseInteractor interactor)
        {
            OnSelectExiting(new SelectExitEventArgs
            {
                interactor = interactor,
                interactable = this
            });
        }

        [System.Obsolete("OnSelectExit(XRBaseInteractor) has been deprecated. Please, upgrade the XR Interaction Toolkit from the Package Manager to the latest available version.")]
        protected void OnSelectExit(XRBaseInteractor interactor)
        {
            OnSelectExiting(new SelectExitEventArgs
            {
                interactor = interactor,
                interactable = this
            });
        }

        /// <summary>
        /// This method is called by the <see cref="XRBaseControllerInteractor"/>
        /// when the Interactor begins an activation event on this selected Interactable.
        /// </summary>
        /// <param name="arguments">Event data containing the Interactor that is sending the activate event.</param>
        /// <remarks>
        /// <paramref name="arguments"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="OnDeactivated"/>
        protected override void OnActivated(ActivateEventArgs arguments)
        {
            if (isUsable)
            {
                IsActivated = true;
                base.OnActivated(arguments);
            }
        }

        [System.Obsolete("OnActivate(XRBaseInteractor) has been deprecated. Please, upgrade the XR Interaction Toolkit from the Package Manager to the latest available version.")]
        protected new void OnActivate(XRBaseInteractor interactor)
        {
            OnActivated(new ActivateEventArgs
            {
                interactor = interactor,
                interactable = this
            });
        }

        /// <summary>
        /// This method is called by the <see cref="XRBaseControllerInteractor"/>
        /// when the Interactor ends an activation event on this selected Interactable.
        /// </summary>
        /// <param name="arguments">Event data containing the Interactor that is sending the deactivate event.</param>
        /// <remarks>
        /// <paramref name="arguments"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="OnActivated"/>
        protected override void OnDeactivated(DeactivateEventArgs arguments)
        {
            if (isUsable)
            {
                IsActivated = false;
                base.OnDeactivated(arguments);
            }
        }

        [System.Obsolete("OnDeactivate(XRBaseInteractor) has been deprecated. Please, upgrade the XR Interaction Toolkit from the Package Manager to the latest available version.")]
        protected new void OnDeactivate(XRBaseInteractor interactor)
        {
            OnDeactivated(new DeactivateEventArgs
            {
                interactor = interactor,
                interactable = this
            });
        }

        private IEnumerator StopInteractingForOneFrame()
        {
            bool wasTouchable = isTouchable, wasGrabbable = isGrabbable, wasUsable = isUsable;
            isTouchable = isGrabbable = isUsable = false;

            if (selectingSocket != null)
            {
                SnapZone snapZone = selectingSocket as SnapZone;
                snapZone?.ForceUnselect();
            }
            
            yield return new WaitUntil(() => hoveringInteractors.Count == 0 && selectingInteractor == null);
            
            isTouchable = wasTouchable;
            isGrabbable = wasGrabbable;
            isUsable = wasUsable;
        }
    }
}
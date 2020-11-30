using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Innoactive.Creator.XRInteraction
{
    /// <summary>
    /// Interactor used for directly interacting with interactables that are touching. This is handled via trigger volumes
    /// that update the current set of valid targets for this interactor. This component must have a collision volume that is 
    /// set to be a trigger to work.
    /// </summary>
    /// <remarks>Adds extra control over applicable interactions.</remarks>
    public class DirectInteractor : XRDirectInteractor
    {
        [SerializeField]
        private bool precisionGrab = true;

        /// <summary>
        /// Toggles precision grab on this interactor.
        /// </summary>
        public bool PrecisionGrab
        {
            get { return precisionGrab; }
            set
            {
                attachTransform.localPosition = initialAttachPosition;
                attachTransform.localRotation = initialAttachRotation;
                precisionGrab = value;
            }
        }

        private Vector3 initialAttachPosition;
        private Quaternion initialAttachRotation;
        private bool forceGrab;

        protected override void Awake()
        {
            base.Awake();
            initialAttachPosition = attachTransform.localPosition;
            initialAttachRotation = attachTransform.localRotation;
        }

        /// <summary>
        /// Gets whether the selection state is active for this interactor. This will check if the controller has a valid selection
        /// state or whether toggle selection is currently on and active.
        /// </summary>
        public override bool isSelectActive
        {
            get
            {
                if (forceGrab)
                {
                    forceGrab = false;
                    return true;
                }

                return base.isSelectActive;
            }
        }

        /// <summary>
        /// Attempts to grab an interactable hovering this interactor without needing to press the grab button on the controller.
        /// </summary>
        public virtual void AttemptGrab()
        {
            forceGrab = true;
        }

#if XRIT_0_10_OR_NEWER
        /// <summary>
        /// This method is called when the interactor first initiates selection of an interactable.
        /// </summary>
        /// <param name="interactable">Interactable that is being selected.</param>
        protected override void OnSelectEntering(XRBaseInteractable interactable)
        {
            InteractableObject interactableObject = interactable as InteractableObject;
            
            if (precisionGrab && interactableObject.attachTransform == null)
            {
                switch (interactableObject.movementType)
                {
                    case XRBaseInteractable.MovementType.VelocityTracking:
                    case XRBaseInteractable.MovementType.Kinematic:
                        attachTransform.SetPositionAndRotation(interactable.transform.position, interactable.transform.rotation);
                        break;
                    case XRBaseInteractable.MovementType.Instantaneous:
                        Debug.LogWarning("Precision Grab is currently not compatible with interactable objects with Movement Type configured as Instantaneous.\n"
                        + $"Please change the Movement Type in {interactable.name}.", interactable);
                        break;
                }
            }
            
            base.OnSelectEntering(interactable);
        }
        
        /// <summary>
        /// This method is called by the interaction manager when the interactor ends selection of an interactable.
        /// </summary>
        /// <param name="interactable">Interactable that is no longer selected.</param>
        protected override void OnSelectExiting(XRBaseInteractable interactable)
        {
            base.OnSelectExiting(interactable);

            if (precisionGrab)
            {
                attachTransform.localPosition = initialAttachPosition;
                attachTransform.localRotation = initialAttachRotation;
            }
        }
#else
        /// <summary>
        /// This method is called when the interactor first initiates selection of an interactable.
        /// </summary>
        /// <param name="interactable">Interactable that is being selected.</param>
        protected override void OnSelectEnter(XRBaseInteractable interactable)
        {
            InteractableObject interactableObject = interactable as InteractableObject;
            
            if (precisionGrab && interactableObject.attachTransform == null)
            {
                switch (interactableObject.movementType)
                {
                    case XRBaseInteractable.MovementType.VelocityTracking:
                    case XRBaseInteractable.MovementType.Kinematic:
                        attachTransform.SetPositionAndRotation(interactableObject.Rigidbody.worldCenterOfMass, interactable.transform.rotation);
                        break;
                    case XRBaseInteractable.MovementType.Instantaneous:
                        attachTransform.SetPositionAndRotation(interactable.transform.position, interactable.transform.rotation);
                        break;
                }
            }
            
            base.OnSelectEnter(interactable);
        }
        
        /// <summary>
        /// This method is called by the interaction manager when the interactor ends selection of an interactable.
        /// </summary>
        /// <param name="interactable">Interactable that is no longer selected.</param>
        protected override void OnSelectExit(XRBaseInteractable interactable)
        {
            base.OnSelectExit(interactable);
            
            if (precisionGrab)
            {
                attachTransform.localPosition = initialAttachPosition;
                attachTransform.localRotation = initialAttachRotation;
            }
        }
#endif
    }
}

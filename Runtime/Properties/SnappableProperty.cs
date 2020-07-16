using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Innoactive.Creator.Core.Properties;
using Innoactive.Creator.BasicInteraction.Properties;

namespace Innoactive.Creator.XRInteraction.Properties
{
    /// <summary>
    /// XR implementation of <see cref="ISnappableProperty"/>.
    /// </summary>
    [RequireComponent(typeof(GrabbableProperty))]
    public class SnappableProperty : LockableProperty, ISnappableProperty
    {
        public event EventHandler<EventArgs> Snapped;
        public event EventHandler<EventArgs> Unsnapped;
        
        /// <summary>
        /// Returns true if the snappable object is snapped.
        /// </summary>
        public bool IsSnapped => SnappedZone != null;

        /// <summary>
        /// Will return the <see cref="SnapZoneProperty"/> of the <see cref="SnapZone"/> which snapped this object.
        /// </summary>
        public ISnapZoneProperty SnappedZone { get; set; }

        [SerializeField]
        [Tooltip("Will object be locked when it has been snapped.")]
        private bool lockObjectOnSnap;

        /// <inheritdoc />
        public bool LockObjectOnSnap
        {
            get => lockObjectOnSnap;
            set => lockObjectOnSnap = value;
        }

        /// <summary>
        /// Reference to attached <see cref="InteractableObject"/>.
        /// </summary>
        public XRBaseInteractable Interactable
        {
            get
            {
                if (interactable == null)
                {
                    interactable = GetComponent<InteractableObject>();
                }

                return interactable;
            }
        }

        private XRBaseInteractable interactable;
        
        protected new virtual void OnEnable()
        {
            base.OnEnable();

            Interactable.onSelectEnter.AddListener(HandleSnappedToDropZone);
            Interactable.onSelectExit.AddListener(HandleUnsnappedFromDropZone);
        }
        
        protected virtual void OnDisable()
        {
            Interactable.onSelectEnter.RemoveListener(HandleSnappedToDropZone);
            Interactable.onSelectEnter.RemoveListener(HandleUnsnappedFromDropZone);
        }
        
        private void HandleSnappedToDropZone(XRBaseInteractor interactor)
        {
            SnappedZone = interactor.GetComponent<SnapZoneProperty>();

            if (SnappedZone == null)
            {
                Debug.LogWarningFormat("Object '{0}' was snapped to SnapZone '{1}' without SnappableProperty", SceneObject.UniqueName, interactor.gameObject.name);
                return;
            }

            if (LockObjectOnSnap)
            {
                SceneObject.SetLocked(true);
            }
            
            EmitSnapped();
        }
        
        private void HandleUnsnappedFromDropZone(XRBaseInteractor interactor)
        {
            SnappedZone = null;
            EmitUnsnapped();
        }
        
        /// <inheritdoc />
        protected override void InternalSetLocked(bool lockState)
        {

        }
        
        /// <summary>
        /// Invokes the <see cref="EmitSnapped"/> event.
        /// </summary>
        protected void EmitSnapped()
        {
            Snapped?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Invokes the <see cref="EmitUnsnapped"/> event.
        /// </summary>
        protected void EmitUnsnapped()
        {
            Unsnapped?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Instantaneously snap the object into <paramref name="snapZone"/>
        /// </summary>
        public void FastForwardSnapInto(ISnapZoneProperty snapZone)
        {
            SnapZone snapDropZone = snapZone?.SnapZoneObject.GetComponent<SnapZone>();
            if (snapDropZone != null)
            {
                snapDropZone.ForceSelect(Interactable);
            }
        }
    }
}

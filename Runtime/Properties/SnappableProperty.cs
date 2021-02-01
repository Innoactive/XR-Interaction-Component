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

#if XRIT_1_0_OR_NEWER
            Interactable.selectEntered.AddListener(HandleSnappedToDropZone);
            Interactable.selectExited.AddListener(HandleUnsnappedFromDropZone);
#elif XRIT_0_10_OR_NEWER
            Interactable.onSelectEntered.AddListener(HandleSnappedToDropZone);
            Interactable.onSelectExited.AddListener(HandleUnsnappedFromDropZone);
#else
            Interactable.onSelectEnter.AddListener(HandleSnappedToDropZone);
            Interactable.onSelectExit.AddListener(HandleUnsnappedFromDropZone);
#endif

            InternalSetLocked(IsLocked);
        }
        
        protected new virtual void OnDisable()
        {
#if XRIT_1_0_OR_NEWER
            Interactable.selectEntered.RemoveListener(HandleSnappedToDropZone);
            Interactable.selectExited.RemoveListener(HandleUnsnappedFromDropZone);
#elif XRIT_0_10_OR_NEWER
            Interactable.onSelectEntered.RemoveListener(HandleSnappedToDropZone);
            Interactable.onSelectEntered.RemoveListener(HandleUnsnappedFromDropZone);
#else
            Interactable.onSelectEnter.RemoveListener(HandleSnappedToDropZone);
            Interactable.onSelectEnter.RemoveListener(HandleUnsnappedFromDropZone);
#endif
        }
        
#if XRIT_1_0_OR_NEWER
        private void HandleSnappedToDropZone(SelectEnterEventArgs arguments)
        {
            XRBaseInteractor interactor = arguments.interactor;
#else
        private void HandleSnappedToDropZone(XRBaseInteractor interactor)
        {
#endif
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
        
#if XRIT_1_0_OR_NEWER
        private void HandleUnsnappedFromDropZone(SelectExitEventArgs arguments)
#else
        private void HandleUnsnappedFromDropZone(XRBaseInteractor interactor)
#endif
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

using System;
using Innoactive.Creator.BasicInteraction.Properties;
using Innoactive.Creator.Core.Properties;
using Innoactive.Creator.Unity;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Innoactive.Creator.XRInteraction.Properties
{
    /// <summary>
    /// XR implementation of <see cref="ISnappableProperty"/>.
    /// </summary>
    [RequireComponent(typeof(GrabbableProperty))]
    public class SnappableProperty : TrainingSceneObjectProperty, ISnappableProperty
    {
        public event EventHandler<EventArgs> Snapped;
        public event EventHandler<EventArgs> Unsnapped;
        
        /// <summary>
        /// Returns true if the snappable object is snapped.
        /// </summary>
        public bool IsSnapped 
        { 
            get { return SnappedZone != null; }
        }
        
        /// <summary>
        /// Will return the <see cref="SnapZoneProperty"/> of the <see cref="SnapZone"/> which snapped this object.
        /// </summary>
        public ISnapZoneProperty SnappedZone { get; set; }

        [SerializeField]
        private bool lockObjectOnSnap = false;

        /// <inheritdoc />
        public bool LockObjectOnSnap
        {
            get
            {
                return lockObjectOnSnap;
            }
            set
            {
                lockObjectOnSnap = value;
            }
        }
        
        /// <summary>
        /// Reference to attached <see cref="InteractableObject"/>.
        /// </summary>
        public XRBaseInteractable Interactable { get; protected set; }
        
        protected override void OnEnable()
        {
            base.OnEnable();

            Interactable = gameObject.GetComponent<InteractableObject>(true);

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
            SnappedZone = interactor.gameObject.GetComponent<SnapZoneProperty>();

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

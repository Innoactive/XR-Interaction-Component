﻿using System;
using VPG.BasicInteraction;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using VPG.Core.Properties;
using VPG.Core.Configuration.Modes;
using VPG.BasicInteraction.Properties;

namespace VPG.XRInteraction.Properties
{
    /// <summary>
    /// XR implementation of <see cref="ISnapZoneProperty"/>.
    /// </summary>
    [RequireComponent(typeof(SnapZone))]
    public class SnapZoneProperty : LockableProperty, ISnapZoneProperty
    {
        public event EventHandler<EventArgs> ObjectSnapped;
        public event EventHandler<EventArgs> ObjectUnsnapped;
        
        public ModeParameter<bool> IsShowingHoverMeshes { get; private set; }
        public ModeParameter<bool> IsShowingHighlightObject { get; private set; }
        public ModeParameter<Color> HighlightColor { get; private set; }

        /// <inheritdoc />
        public bool IsObjectSnapped => SnappedObject != null;

        /// <inheritdoc />
        public ISnappableProperty SnappedObject { get; set; }

        /// <inheritdoc />
        public GameObject SnapZoneObject => SnapZone.gameObject;

        /// <summary>
        /// Returns the SnapZone component.
        /// </summary>
        public SnapZone SnapZone 
        {
            get
            {
                if (snapZone == null)
                {
                    snapZone = GetComponent<SnapZone>();
                }

                return snapZone;
            }
        }

        private SnapZone snapZone;
        
        protected override void OnEnable()
        {
            base.OnEnable();
        
            SnapZone.selectEntered.AddListener(HandleObjectSnapped);
            SnapZone.selectExited.AddListener(HandleObjectUnsnapped);
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();

            SnapZone.selectEntered.RemoveListener(HandleObjectSnapped);
            SnapZone.selectExited.RemoveListener(HandleObjectUnsnapped);
        }
        
        private void HandleObjectSnapped(SelectEnterEventArgs arguments)
        {
            XRBaseInteractable interactable = arguments.interactable;
            SnappedObject = interactable.gameObject.GetComponent<SnappableProperty>();
            if (SnappedObject == null)
            {
                Debug.LogWarningFormat("SnapZone '{0}' received snap from object '{1}' without XR_SnappableProperty", SceneObject.UniqueName, interactable.gameObject.name);
            }
            else
            {
                EmitSnapped();
            }
        }
        
        private void HandleObjectUnsnapped(SelectExitEventArgs arguments)
        {
            if (SnappedObject != null)
            {
                SnappedObject = null;
                EmitUnsnapped();
            }
        }
        
        private void InitializeModeParameters()
        {
            if (IsShowingHoverMeshes == null)
            {
                IsShowingHoverMeshes = new ModeParameter<bool>("ShowSnapzoneHoverMeshes", SnapZone.showInteractableHoverMeshes);
                IsShowingHoverMeshes.ParameterModified += (sender, args) =>
                {
                    SnapZone.showInteractableHoverMeshes = IsShowingHoverMeshes.Value;
                };
            }

            if (IsShowingHighlightObject == null)
            {
                IsShowingHighlightObject = new ModeParameter<bool>("ShowSnapzoneHighlightObject", SnapZone.ShowHighlightObject);
                IsShowingHighlightObject.ParameterModified += (sender, args) =>
                {
                    SnapZone.ShowHighlightObject = IsShowingHighlightObject.Value;
                };
            }

            if (HighlightColor == null)
            {
                HighlightColor = new ModeParameter<Color>("HighlightColor", SnapZone.ShownHighlightObjectColor);
                HighlightColor.ParameterModified += (sender, args) =>
                {
                    SnapZone.ShownHighlightObjectColor = HighlightColor.Value;
                };
            }
        }
        
        /// <summary>
        /// Configure snap zone properties according to the provided mode.
        /// </summary>
        /// <param name="mode">The current mode with the parameters to be changed.</param>
        public void Configure(IMode mode)
        {
            InitializeModeParameters();

            IsShowingHoverMeshes.Configure(mode);
            IsShowingHighlightObject.Configure(mode);
            HighlightColor.Configure(mode);
        }
        
        /// <summary>
        /// Invokes the <see cref="EmitSnapped"/> event.
        /// </summary>
        protected void EmitSnapped()
        {
            ObjectSnapped?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Invokes the <see cref="EmitUnsnapped"/> event.
        /// </summary>
        protected void EmitUnsnapped()
        {
            ObjectUnsnapped?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Locks or unlocks the snap zone according to the provided <paramref name="lockState"/>.
        /// </summary>
        protected override void InternalSetLocked(bool lockState)
        {
            SnapZone.enabled = lockState == false;
        }
    }
}

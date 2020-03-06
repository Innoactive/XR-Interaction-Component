﻿using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Innoactive.Hub.Unity;
using Innoactive.Hub.Training.Configuration;
using Innoactive.Hub.Training.Configuration.Modes;
using Innoactive.Hub.Training.SceneObjects.Properties;
using Innoactive.Hub.Training.SceneObjects.Interaction.Properties;

namespace Innoactive.Creator.XRInteraction.SceneObjects.Properties
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
        public bool IsObjectSnapped
        {
            get
            {
                return SnappedObject != null;
            }
        }

        /// <inheritdoc />
        public ISnappableProperty SnappedObject { get; set; }

        /// <inheritdoc />
        public GameObject SnapZoneObject
        {
            get
            {
                if (SnapZone != null)
                {
                    return SnapZone.gameObject;
                }
                return null;
            }
        }

        /// <summary>
        /// Returns the SnapZone component.
        /// </summary>
        public SnapZone SnapZone { get; protected set; }
        
        protected override void OnEnable()
        {
            base.OnEnable();

            if (SnapZone == null)
            {
                SnapZone = gameObject.GetComponent<SnapZone>(true);
            }

            SnapZone.onSelectEnter.AddListener(HandleObjectSnapped);
            SnapZone.onSelectExit.AddListener(HandleObjectUnsnapped);
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();

            SnapZone.onSelectEnter.RemoveListener(HandleObjectSnapped);
            SnapZone.onSelectExit.RemoveListener(HandleObjectUnsnapped);
        }
        
        private void HandleObjectSnapped(XRBaseInteractable interactable)
        {
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
        
        private void HandleObjectUnsnapped(XRBaseInteractable interactable)
        {
            if (SnappedObject != null)
            {
                SnappedObject = null;
                EmitUnsnapped();
            }
        }
        
        private void InitializeModeParameters()
        {
            if (SnapZone == null)
            {
                SnapZone = GetComponent<SnapZone>();
            }

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
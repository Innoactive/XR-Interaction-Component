using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Innoactive.Creator.Core.Properties;
using Innoactive.Creator.Core.Configuration.Modes;
using Innoactive.Creator.BasicInteraction.Properties;

namespace Innoactive.Creator.XRInteraction.Properties
{
    /// <summary>
    /// XR implementation of <see cref="ISnapZoneProperty"/>.
    /// </summary>
    [RequireComponent(typeof(SnapZone))]
    public class SnapZoneProperty : LockableProperty, ISnapZoneProperty
    {
        public event EventHandler<EventArgs> ObjectSnapped;
        public event EventHandler<EventArgs> ObjectUnsnapped;

        [Obsolete("XRSocketInteractor hover meshes are now always disabled.")]
        public ModeParameter<bool> IsShowingHoverMeshes { get; private set; }

        public ModeParameter<bool> IsShowingHighlightObject { get; private set; }
        public ModeParameter<Color> HighlightColor { get; private set; }

        /// <inheritdoc />
        public bool IsObjectSnapped
        {
            get { return SnappedObject != null; }
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

        protected virtual void Reset()
        {
            if (SnapZone.attachTransform)
            {
                return;
            }

            GameObject attach = new GameObject("Attach Point");
            attach.transform.SetParent(transform, false);
            attach.transform.localRotation = Quaternion.identity;
            attach.transform.localPosition = Vector3.zero;
            attach.transform.localScale = Vector3.one;

            FieldInfo attachField = typeof(XRBaseInteractor).GetField("m_AttachTransform", BindingFlags.Instance | BindingFlags.NonPublic);

            if (attachField != null)
            {
                attachField.SetValue(SnapZone, attach.transform);
            }
            else
            {
                DestroyImmediate(attach);

                // It seems that the Unity team has renamed that field. Check the SnapZone script. 
                Debug.LogError("We failed to add an attach point for this snap zone property. Please, drop us a word if you see this message.");
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            SnapZone.onSelectEnter.AddListener(HandleObjectSnapped);
            SnapZone.onSelectExit.AddListener(HandleObjectUnsnapped);

            Collider[] colliders = GetComponents<Collider>();
            Bounds snapZoneBounds = colliders[0].bounds;
            foreach (Collider snapZoneCollider in colliders.Where(snapZoneCollider => snapZoneCollider.enabled))
            {
                snapZoneBounds.Encapsulate(snapZoneCollider.bounds);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            SnapZone.onSelectEnter.RemoveListener(HandleObjectSnapped);
            SnapZone.onSelectExit.RemoveListener(HandleObjectUnsnapped);
        }

        private void HandleObjectSnapped(XRBaseInteractable interactable)
        {
            SnappableProperty snappedObject = interactable.gameObject.GetComponent<SnappableProperty>();
            SnappedObject = snappedObject;
            if (SnappedObject == null)
            {
                Debug.LogWarningFormat("SnapZone '{0}' received snap from object '{1}' without XR_SnappableProperty", SceneObject.UniqueName, interactable.gameObject.name);
            }
            else
            {
                if (interactable is XRGrabInteractable grabInteractable && grabInteractable.attachTransform)
                {
                    Destroy(grabInteractable.attachTransform.gameObject);
                    grabInteractable.attachTransform = null;
                    Debug.LogWarning("Snap zone's attach style is set to auto, but the snapped object has an attach transform. This is not allowed. We deleted the attach transform.");
                }

                SetAttachTransform(snappedObject);

                EmitSnapped();
            }
        }

        private void SetAttachTransform(SnappableProperty snappedObject)
        {
            Rigidbody snappedRigidbody = snappedObject.GetComponent<Rigidbody>();
            SnapZone.attachTransform.localPosition = snappedRigidbody.centerOfMass;
            SnapZone.attachTransform.localRotation = Quaternion.identity;
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
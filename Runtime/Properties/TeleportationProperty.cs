using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Innoactive.Creator.BasicInteraction.Properties;

namespace Innoactive.Creator.Core.Properties
{
    /// <summary>
    /// XR implementation of <see cref="ITeleportationProperty"/>.
    /// </summary>
    /// <remarks>
    /// This implementation is based on 'TeleportationAnchor'.
    /// </remarks>
    [RequireComponent(typeof(TeleportationAnchor), typeof(BoxCollider))]
    public class TeleportationProperty : LockableProperty, ITeleportationProperty
    {
        /// <inheritdoc />
        public event EventHandler<EventArgs> Teleported;

        /// <inheritdoc />
        public bool WasUsedToTeleport => wasUsedToTeleport;

        private TeleportationAnchor teleportationInteractable;
        private Renderer[] renderers;
        private bool wasUsedToTeleport;

        protected void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>();
            teleportationInteractable = GetComponent<TeleportationAnchor>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            
            teleportationInteractable.teleportationProvider.endLocomotion += EmitTeleported;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            teleportationInteractable.teleportationProvider.endLocomotion -= EmitTeleported;
        }

        /// <inheritdoc />
        public void Initialize()
        {
            wasUsedToTeleport = false;
        }

        /// <inheritdoc />
        public void FastForwardTeleport()
        {
            TeleportRequest teleportRequest = new TeleportRequest
            {
                requestTime = Time.time,
                matchOrientation = teleportationInteractable.matchOrientation,
                destinationPosition = teleportationInteractable.teleportAnchorTransform.position,
                destinationRotation = teleportationInteractable.teleportAnchorTransform.rotation
            };

            teleportationInteractable.teleportationProvider.QueueTeleportRequest(teleportRequest);
        }

        /// <inheritdoc />
        protected override void InternalSetLocked(bool lockState)
        {
            foreach (Collider collider in teleportationInteractable.colliders)
            {
                collider.enabled = !lockState;
            }
            
            teleportationInteractable.enabled = !lockState;

            if (renderers != null)
            {
                foreach (Renderer anchorRenderer in renderers)
                {
                    anchorRenderer.enabled = !lockState;
                }
            }
        }
        
        protected void EmitTeleported(LocomotionSystem locomotionSystem)
        {
            if (wasUsedToTeleport == false)
            {
                Vector3 rigPosition = locomotionSystem.xrRig.rig.transform.position;
                Vector3 anchorPosition = teleportationInteractable.teleportAnchorTransform.position;
                Vector2 flatRigPosition = new Vector2(rigPosition.x, rigPosition.z);
                Vector2 flatAnchorPosition = new Vector2(anchorPosition.x, anchorPosition.z);

                if (Vector3.Distance(flatRigPosition, flatAnchorPosition) < 0.1)
                {
                    wasUsedToTeleport = true;
                    Teleported?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }
}
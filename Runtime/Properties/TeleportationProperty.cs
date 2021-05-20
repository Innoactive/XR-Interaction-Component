using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using VPG.BasicInteraction.Properties;

namespace VPG.Core.Properties
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

            switch (teleportationInteractable.teleportTrigger)
            {
                case BaseTeleportationInteractable.TeleportTrigger.OnActivated:
                    teleportationInteractable.activated.AddListener(args =>
                    {
                        EmitTeleported();
                    });
                    break;
                case BaseTeleportationInteractable.TeleportTrigger.OnDeactivated:
                    teleportationInteractable.deactivated.AddListener(args =>
                    {
                        EmitTeleported();
                    });
                    break;
                case BaseTeleportationInteractable.TeleportTrigger.OnSelectEntered:
                    teleportationInteractable.selectEntered.AddListener(args =>
                    {
                        EmitTeleported();
                    });
                    break;
                case BaseTeleportationInteractable.TeleportTrigger.OnSelectExited:
                    teleportationInteractable.selectExited.AddListener(args =>
                    {
                        EmitTeleported();
                    });
                    break;
            }
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
        
        protected void EmitTeleported()
        {
            wasUsedToTeleport = true;
            Teleported?.Invoke(this, EventArgs.Empty);
        }
    }
}
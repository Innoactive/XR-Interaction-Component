using Innoactive.Creator.BasicInteraction;
using UnityEngine;

namespace Innoactive.Creator.XRInteraction
{
    /// <summary>
    /// Locomotion handler for Unity XR.
    /// </summary>
    [RequireComponent(typeof(RigManipulationProvider))]
    public class XRLocomotionHandler : BaseLocomotionHandler
    {
        private RigManipulationProvider rigManipulationProvider;
        
        /// <summary>
        /// Current rotation of the XR Rig.
        /// </summary>
        public override Quaternion CurrentRotation
        {
            get
            {
                if (rigManipulationProvider != null)
                {
                    return RigManipulationProvider.system.xrRig.rig.transform.rotation;
                }

                return Quaternion.identity;
            }
        }

        /// <summary>
        /// Current position of the XR Rig.
        /// </summary>
        public override Vector3 CurrentPosition
        {
            get
            {
                if (rigManipulationProvider != null)
                {
                    return RigManipulationProvider.system.xrRig.rig.transform.position;
                }

                return Vector3.forward;
            }
        }

        /// <summary>
        /// Locomotion provider to directly manipulate the XR Rig.
        /// </summary>
        protected RigManipulationProvider RigManipulationProvider
        {
            get
            {
                if (rigManipulationProvider == null)
                {
                    rigManipulationProvider = GetComponent<RigManipulationProvider>();
                }

                return rigManipulationProvider;
            }
        }
        
        /// <inheritdoc />
        public override void SetPositionAndRotation(Vector3 destinationPosition, Quaternion destinationRotation)
        {
            RigManipulationProvider.SetRigPositionAndRotation(destinationPosition, destinationRotation);
        }
    }
}
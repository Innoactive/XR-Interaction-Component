using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace VPG.XRInteraction
{
    /// <summary>
    /// Locomotion provider to directly manipulate the XRRig's position and rotation.
    /// </summary>
    public class RigManipulationProvider : LocomotionProvider
    {
        /// <summary>
        /// Sets a new position and rotation for the XR Rig.
        /// </summary>
        /// <param name="destinationPosition">Target position.</param>
        /// <param name="destinationRotation">Target rotation.</param>
        public void SetRigPositionAndRotation(Vector3 destinationPosition, Quaternion destinationRotation)
        {
            if (CanBeginLocomotion() == false)
            {
                return;
            }
            
            XRRig xrRig = system.xrRig;
            
            if (xrRig != null)
            {
                Vector3 heightAdjustment = xrRig.rig.transform.up * xrRig.cameraInRigSpaceHeight;
                Vector3 cameraDestination = destinationPosition + heightAdjustment;
                
                xrRig.MatchRigUpCameraForward(destinationRotation * Vector3.up, destinationRotation * Vector3.forward);
                xrRig.MoveCameraToWorldLocation(cameraDestination);
            }
        }
    }
}

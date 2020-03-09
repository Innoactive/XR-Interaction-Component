#if CREATOR_XR_INTERACTION

using UnityEngine.XR.Interaction.Toolkit;

namespace Innoactive.Creator.XR
{
    /// <inheritdoc />
    /// <remarks>Adds extra control over applicable interactions.</remarks>
    public class RayInteractor : XRRayInteractor
    {
        private bool forceGrab;

        /// <inheritdoc />
        public override bool isSelectActive
        {
            get
            {
                if (forceGrab)
                {
                    forceGrab = false;
                    return true;
                }

                return base.isSelectActive;
            }
        }

        /// <summary>
        /// Attempts to grab an interactable hovering this interactor without needing to press the grab button on the controller.
        /// </summary>
        public virtual void AttemptGrab()
        {
            forceGrab = true;
        }
    }
}

#endif
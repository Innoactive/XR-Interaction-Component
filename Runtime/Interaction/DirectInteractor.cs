#if CREATOR_XR_INTERACTION

using UnityEngine.XR.Interaction.Toolkit;

namespace Innoactive.Creator.XR
{
    public class DirectInteractor : XRDirectInteractor
    {
        private bool forceGrab;

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
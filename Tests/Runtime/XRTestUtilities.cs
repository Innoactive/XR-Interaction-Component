using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Innoactive.Creator.XRInteraction.Tests
{
    public static class XRTestUtilities
    {
        public static XRInteractionManager CreateInteractionManager()
        {
            GameObject manager = new GameObject("XR Interaction Manager");
            return manager.AddComponent<XRInteractionManager>();
        }

        internal static DirectInteractor CreateDirectInteractor()
        {
            GameObject interactorGO = new GameObject("XR Interactor");
            CreateGOSphereCollider(interactorGO);
            DirectInteractor interactor = interactorGO.AddComponent<DirectInteractor>();
            XRController controller = interactorGO.GetComponent<XRController>();
            controller.enableInputTracking = false;
            
            return interactor;
        }
        
        public static InteractableObject CreateInteractableObjcet()
        {
            GameObject interactableGO = new GameObject("XR Interactable");
            CreateGOSphereCollider(interactableGO, false);
            InteractableObject interactable = interactableGO.AddComponent<InteractableObject>();
            Rigidbody rigidBody = interactableGO.GetComponent<Rigidbody>();
            rigidBody.useGravity = false;
            rigidBody.isKinematic = true;
            
            return interactable;
        }
        
        public static XRSocketInteractor CreateSocketInteractor()
        {
            GameObject interactorGO = new GameObject("XR Socket Interactor");
            CreateGOSphereCollider(interactorGO);
            return interactorGO.AddComponent<XRSocketInteractor>();
        }

        private static void CreateGOSphereCollider(GameObject go, bool isTrigger = true)
        {
            SphereCollider collider = go.AddComponent<SphereCollider>();
            collider.radius = 1.0f;
            collider.isTrigger = isTrigger;
        }
    }
}
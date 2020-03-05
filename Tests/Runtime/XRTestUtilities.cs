#if UNITY_EDITOR && CREATOR_XR_INTERACTION

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Innoactive.Creator.XR.Tests
{
    public static class XRTestUtilities
    {
        public static XRInteractionManager CreateInteractionManager()
        {
            GameObject managerGO = new GameObject("XR Interaction Manager");
            XRInteractionManager manager = managerGO.AddComponent<XRInteractionManager>();
            return manager;
        }

        internal static XRDirectInteractor CreateDirectInteractor()
        {
            GameObject interactorGO = new GameObject("XR Interactor");
            CreateGOSphereCollider(interactorGO);
            XRDirectInteractor interactor = interactorGO.AddComponent<XRDirectInteractor>();
            XRController controller = interactorGO.GetComponent<XRController>();
            controller.enableInputTracking = false;
            return interactor;
        }
        
        public static XRInteractableObject CreateInteractableObjcet()
        {
            GameObject interactableGO = new GameObject("XR Interactable");
            CreateGOSphereCollider(interactableGO, false);
            XRInteractableObject interactable = interactableGO.AddComponent<XRInteractableObject>();
            Rigidbody rigidBody = interactableGO.GetComponent<Rigidbody>();
            rigidBody.useGravity = false;
            rigidBody.isKinematic = true;
            return interactable;
        }
        
        public static XRSocketInteractor CreateSocketInteractor()
        {
            GameObject interactorGO = new GameObject("XR Socket Interactor");
            CreateGOSphereCollider(interactorGO);
            XRSocketInteractor interactor = interactorGO.AddComponent<XRSocketInteractor>();
            return interactor;
        }

        private static void CreateGOSphereCollider(GameObject go, bool isTrigger = true)
        {
            SphereCollider collider = go.AddComponent<SphereCollider>();
            collider.radius = 1.0f;
            collider.isTrigger = isTrigger;
        }
    }
}

#endif
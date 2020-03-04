#if UNITY_EDITOR && CREATOR_XR_INTERACTION

using System.Reflection;
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
        
        public static XRGrabInteractable CreateGrabInteractable()
        {
            GameObject interactableGO = new GameObject("XR Interactable");
            CreateGOSphereCollider(interactableGO, false);
            XRGrabInteractable interactable = interactableGO.AddComponent<XRGrabInteractable>();
            Rigidbody rigidBody = interactableGO.GetComponent<Rigidbody>();
            rigidBody.useGravity = false;
            rigidBody.isKinematic = true;
            return interactable;
        }

        public static XRRayInteractor CreateRayInteractor()
        {
            GameObject interactorGO = new GameObject("Ray Interactor");
            XRRayInteractor interactor = interactorGO.AddComponent<XRRayInteractor>();
            XRController controller = interactorGO.GetComponent<XRController>();
            XRInteractorLineVisual ilv = interactorGO.AddComponent<XRInteractorLineVisual>();
            controller.enableInputTracking = false;
            interactor.enableUIInteraction = false;
            return interactor;
        }
        
        public static XRSocketInteractor CreateSocketInteractor()
        {
            GameObject interactorGO = new GameObject("XR Socket Interactor");
            CreateGOSphereCollider(interactorGO);
            XRSocketInteractor interactor = interactorGO.AddComponent<XRSocketInteractor>();
            return interactor;
        }

        public static void SimulateGrab(XRController controller)
        {
            InvokeControllerInteractionType(controller, new object[] { 0, true });
        }
        
        public static void SimulateUngrab(XRController controller)
        {
            InvokeControllerInteractionType(controller, new object[] { 0, false });
        }
        
        public static void SimulateUse(XRController controller)
        {
            InvokeControllerInteractionType(controller, new object[] { 1, true });
        }

        private static void InvokeControllerInteractionType(XRController controller, object[] args)
        {
            MethodInfo updateInteractionMethod = controller.GetType().GetMethod("UpdateInteractionType", BindingFlags.NonPublic | BindingFlags.Instance);
            updateInteractionMethod.Invoke(controller, args);
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
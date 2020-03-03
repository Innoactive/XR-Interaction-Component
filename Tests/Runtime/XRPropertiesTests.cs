using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.XR.Interaction.Toolkit;
using Innoactive.Creator.SceneObjects.Properties;
using Innoactive.Hub.Unity.Tests.Training;

namespace Innoactive.Creator.XR.Tests
{
    public class XRPropertiesTests : RuntimeTests
    {
        private bool actionDone;
        private bool actionUndone;
        private Type senderType;
        
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            XRTestUtilities.CreateInteractionManager();

            actionDone = false;
            actionUndone = false;
        }
        
        [UnityTest]
        public IEnumerator XRTouchablePropertyTest()
        {
            XRGrabInteractable interactable = XRTestUtilities.CreateGrabInteractable();
            XR_TouchableProperty touchProperty = interactable.gameObject.AddComponent<XR_TouchableProperty>();
            senderType = touchProperty.GetType();
            
            touchProperty.Touched += OnActionDone;
            touchProperty.Untouched += OnActionUndone;
            
            Assert.IsFalse(touchProperty.IsBeingTouched);
            
            XRDirectInteractor interactor = XRTestUtilities.CreateDirectInteractor();
            
            yield return new WaitUntil(()=> actionDone);
            
            Assert.IsTrue(touchProperty.IsBeingTouched);
            
            interactor.transform.position = Vector3.up * 100;
            
            yield return new WaitUntil(()=> actionUndone);
            
            Assert.IsFalse(touchProperty.IsBeingTouched);

            touchProperty.Touched -= OnActionDone;
            touchProperty.Untouched -= OnActionUndone;
        }
        
        [UnityTest]
        public IEnumerator XRTouchablePropertyFastForwardTouchTest()
        {
            XRGrabInteractable interactable = XRTestUtilities.CreateGrabInteractable();
            XR_TouchableProperty touchProperty = interactable.gameObject.AddComponent<XR_TouchableProperty>();
            senderType = touchProperty.GetType();
            
            touchProperty.Touched += OnActionDone;
            touchProperty.Untouched += OnActionUndone;
            
            Assert.IsFalse(touchProperty.IsBeingTouched);
            
            touchProperty.FastForwardTouch();
            
            yield return new WaitUntil(()=> actionDone && actionUndone);
            
            Assert.IsFalse(touchProperty.IsBeingTouched);
            
            touchProperty.Touched -= OnActionDone;
            touchProperty.Untouched -= OnActionUndone;
        }
        
        
        
        
        
        
        
        
        
        
        
        [UnityTest]
        public IEnumerator XRGrabbablePropertyTest()
        {
            XRGrabInteractable interactable = XRTestUtilities.CreateGrabInteractable();
            XR_GrabbableProperty grabbableProperty = interactable.gameObject.AddComponent<XR_GrabbableProperty>();
            senderType = grabbableProperty.GetType();
            
            grabbableProperty.Grabbed += OnActionDone;
            grabbableProperty.Ungrabbed += OnActionUndone;
            
            Assert.IsFalse(grabbableProperty.IsGrabbed);
            
            XRDirectInteractor interactor = XRTestUtilities.CreateDirectInteractor();
            XRController controller = interactor.GetComponent<XRController>();
            
            XRTestUtilities.SimulateGrab(controller, true);
            
            yield return new WaitUntil(()=> actionDone);
            
            Assert.IsTrue(grabbableProperty.IsGrabbed);
            
            XRTestUtilities.SimulateGrab(controller, false);
            
            yield return new WaitUntil(()=> actionUndone);
            
            Assert.IsFalse(grabbableProperty.IsGrabbed);

            grabbableProperty.Grabbed -= OnActionDone;
            grabbableProperty.Ungrabbed -= OnActionUndone;
        }

        // [UnityTest]
        // public IEnumerator XRUsablePropertyTest()
        // {
        //     XRGrabInteractable interactable = XRTestUtilities.CreateGrabInteractable();
        //     XR_UsableProperty usableProperty = interactable.gameObject.AddComponent<XR_UsableProperty>();
        //     senderType = usableProperty.GetType();
        //     
        //     usableProperty.UsageStarted += OnActionDone;
        //     usableProperty.UsageStopped += OnActionUndone;
        //     
        //     Assert.IsFalse(usableProperty.IsBeingUsed);
        //     
        //     XRDirectInteractor interactor = XRTestUtilities.CreateDirectInteractor();
        //     XRController controller = interactor.GetComponent<XRController>();
        //     
        //     XRTestUtilities.SimulateGrab(controller, true);
        //
        //     yield return null;
        //     
        //     XRTestUtilities.SimulateUse(controller, true);
        //     
        //     yield return new WaitUntil(()=> actionDone);
        //     
        //     Assert.IsTrue(usableProperty.IsBeingUsed);
        //     
        //     
        //     XRTestUtilities.SimulateGrab(controller, false);
        //     
        //     yield return null;
        //     
        //     XRTestUtilities.SimulateUse(controller, false);
        //     
        //     
        //     yield return new WaitUntil(()=> actionUndone);
        //     
        //     Assert.IsFalse(usableProperty.IsBeingUsed);
        //
        //     usableProperty.UsageStarted -= OnActionDone;
        //     usableProperty.UsageStopped -= OnActionUndone;
        // }

        private void OnActionDone(object sender, EventArgs e)
        {
            actionDone = true;
            Assert.That(sender.GetType() == senderType);
        }
        
        private void OnActionUndone(object sender, EventArgs e)
        {
            actionUndone = true;
            Assert.That(sender.GetType() == senderType);
        }
        
        
    }
}

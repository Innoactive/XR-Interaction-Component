#if UNITY_EDITOR && CREATOR_XR_INTERACTION

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
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            XRTestUtilities.CreateInteractionManager();
        }

        [UnityTest]
        public IEnumerator XRTouchablePropertyTest()
        {
            XRBaseInteractable interactable = XRTestUtilities.CreateGrabInteractable();
            XR_TouchableProperty touchProperty = interactable.gameObject.AddComponent<XR_TouchableProperty>();
            WaitForSecondsRealtime wait = new WaitForSecondsRealtime(0.25f);
            
            Assert.IsFalse(interactable.isHovered);
            Assert.IsFalse(touchProperty.IsBeingTouched);

            XRDirectInteractor interactor = XRTestUtilities.CreateDirectInteractor();
            
            yield return wait;

            Assert.IsTrue(interactable.isHovered);
            Assert.IsTrue(touchProperty.IsBeingTouched);

            interactor.transform.position = Vector3.up * 100;

            yield return wait;
            
            Assert.IsFalse(interactable.isHovered);
            Assert.IsFalse(touchProperty.IsBeingTouched);
        }

        [UnityTest]
        public IEnumerator XRGrabbablePropertyTest()
        {
            XRBaseInteractable interactable = XRTestUtilities.CreateGrabInteractable();
            XR_GrabbableProperty grabbableProperty = interactable.gameObject.AddComponent<XR_GrabbableProperty>();
            
            Assert.IsFalse(interactable.isSelected);
            Assert.IsFalse(grabbableProperty.IsGrabbed);
            
            XRDirectInteractor interactor = XRTestUtilities.CreateDirectInteractor();
            XRController controller = interactor.GetComponent<XRController>();
            
            XRTestUtilities.SimulateGrab(controller);
            
            yield return null;
            
            Assert.IsTrue(interactable.isSelected);
            Assert.IsTrue(grabbableProperty.IsGrabbed);
            
            XRTestUtilities.SimulateUngrab(controller);
            
            yield return null;
            
            Assert.IsFalse(interactable.isSelected);
            Assert.IsFalse(grabbableProperty.IsGrabbed);
        }
    }
}

#endif

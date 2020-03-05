#if UNITY_EDITOR && CREATOR_XR_INTERACTION

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Innoactive.Creator.XR.SceneObjects.Properties;
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
            XRInteractableObject interactable = XRTestUtilities.CreateInteractableObjcet();
            XR_TouchableProperty touchProperty = interactable.gameObject.AddComponent<XR_TouchableProperty>();
            
            Assert.IsFalse(touchProperty.IsBeingTouched);

            XRTestUtilities.CreateDirectInteractor();
            
            yield return new WaitUntil(()=>touchProperty.IsBeingTouched);
            
            Assert.IsTrue(touchProperty.IsBeingTouched);
        }

        [UnityTest]
        public IEnumerator XRGrabbablePropertyTest()
        {
            XRInteractableObject interactable = XRTestUtilities.CreateInteractableObjcet();
            XR_GrabbableProperty grabbableProperty = interactable.gameObject.AddComponent<XR_GrabbableProperty>();
            
            Assert.IsFalse(grabbableProperty.IsGrabbed);
            
            XRTestUtilities.CreateDirectInteractor();

            yield return null;
            
            interactable.AttemptGrab();
            
            yield return null;
            
            Assert.IsTrue(grabbableProperty.IsGrabbed);
        }
        
        [UnityTest]
        public IEnumerator XRUsablePropertyTest()
        {
            XRInteractableObject interactable = XRTestUtilities.CreateInteractableObjcet();
            XR_UsableProperty grabbableProperty = interactable.gameObject.AddComponent<XR_UsableProperty>();
            
            Assert.IsFalse(grabbableProperty.IsBeingUsed);

            interactable.ForceUse();
            
            Assert.IsTrue(grabbableProperty.IsBeingUsed);
            
            yield break;
        }
    }
}

#endif

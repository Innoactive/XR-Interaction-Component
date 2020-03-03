using System;
using System.Collections;
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
        private bool wasTouched;
        private bool wasUnTouched;
        
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            XRTestUtilities.CreateInteractionManager();

            wasTouched = false;
            wasUnTouched = false;
        }
        
        [UnityTest]
        public IEnumerator XRTouchablePropertyTest()
        {
            XRGrabInteractable interactable = XRTestUtilities.CreateGrabInteractable();
            XR_TouchableProperty touchProperty = interactable.gameObject.AddComponent<XR_TouchableProperty>();
            
            touchProperty.Touched += OnTouched;
            touchProperty.Untouched += OnUntouched;
            
            Assert.IsFalse(touchProperty.IsBeingTouched);
            
            XRDirectInteractor interactor = XRTestUtilities.CreateDirectInteractor();
            
            yield return new WaitUntil(()=> wasTouched);
            
            Assert.IsTrue(touchProperty.IsBeingTouched);
            
            interactor.transform.position = Vector3.up * 100;
            
            yield return new WaitUntil(()=> wasUnTouched);
            
            Assert.IsFalse(touchProperty.IsBeingTouched);

            touchProperty.Touched -= OnTouched;
            touchProperty.Untouched -= OnUntouched;
        }
        
        [UnityTest]
        public IEnumerator XRTouchablePropertyFastForwardTouchTest()
        {
            XRGrabInteractable interactable = XRTestUtilities.CreateGrabInteractable();
            XR_TouchableProperty touchProperty = interactable.gameObject.AddComponent<XR_TouchableProperty>();
            
            touchProperty.Touched += OnTouched;
            touchProperty.Untouched += OnUntouched;
            
            Assert.IsFalse(touchProperty.IsBeingTouched);
            
            touchProperty.FastForwardTouch();
            
            yield return new WaitUntil(()=> wasTouched && wasUnTouched);
            
            Assert.IsFalse(touchProperty.IsBeingTouched);
            
            touchProperty.Touched -= OnTouched;
            touchProperty.Untouched -= OnUntouched;
        }

        private void OnTouched(object sender, EventArgs e)
        {
            wasTouched = true;
            Debug.LogWarning("OnTouched");
            Assert.That(sender is XR_TouchableProperty);
        }
        
        private void OnUntouched(object sender, EventArgs e)
        {
            wasUnTouched = true;
            Debug.LogWarning("OnUntouched");
            Assert.That(sender is XR_TouchableProperty);
        }
        
        
    }
}

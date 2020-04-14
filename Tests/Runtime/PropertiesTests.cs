using System.Collections;
using System.Linq;
using Innoactive.Creator.BasicInteraction.Conditions;
using Innoactive.Creator.Core;
using Innoactive.Creator.Core.Properties;
using Innoactive.Creator.Tests.Builder;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Innoactive.Creator.Tests.Utils;
using Innoactive.Creator.XRInteraction.Properties;
using UnityEngine.XR.Interaction.Toolkit;

namespace Innoactive.Creator.XRInteraction.Tests
{
    public class PropertiesTests : RuntimeTests
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            XRTestUtilities.CreateInteractionManager();
        }
        
        [UnityTest]
        public IEnumerator TouchableProperty()
        {
            InteractableObject interactable = XRTestUtilities.CreateInteractableObjcet();
            TouchableProperty touchProperty = interactable.gameObject.AddComponent<TouchableProperty>();

            Assert.IsFalse(touchProperty.IsBeingTouched);

            XRDirectInteractor interactor = XRTestUtilities.CreateDirectInteractor();
            
            yield return new WaitUntil(()=> interactable.isHovered);
            
            Assert.IsTrue(touchProperty.IsBeingTouched);

            interactor.transform.position = Vector3.up * 10;
            
            yield return new WaitUntil(()=> interactable.isHovered == false);
            
            Assert.IsFalse(touchProperty.IsBeingTouched);
        }

        [UnityTest]
        public IEnumerator GrabbableProperty()
        {
            InteractableObject interactable = XRTestUtilities.CreateInteractableObjcet();
            GrabbableProperty grabbableProperty = interactable.gameObject.AddComponent<GrabbableProperty>();
            
            Assert.IsFalse(grabbableProperty.IsGrabbed);
            
            XRTestUtilities.CreateSocketInteractor();
            
            yield return new WaitUntil(()=> interactable.isSelected);
            
            Assert.IsTrue(grabbableProperty.IsGrabbed);
            
            interactable.ForceStopInteracting();
            
            yield return new WaitUntil(()=> interactable.isSelected == false);
            
            Assert.IsFalse(grabbableProperty.IsGrabbed);
        }

        [UnityTest]
        public IEnumerator UsableProperty()
        {
            InteractableObject interactable = XRTestUtilities.CreateInteractableObjcet();
            UsableProperty usableProperty = interactable.gameObject.AddComponent<UsableProperty>();
            
            Assert.IsFalse(usableProperty.IsBeingUsed);

            interactable.ForceUse();
            
            Assert.IsTrue(usableProperty.IsBeingUsed);
            
            interactable.ForceStopInteracting();
            
            Assert.IsFalse(usableProperty.IsBeingUsed);

            yield break;
        }
        
        [UnityTest]
        public IEnumerator FastForwardTouch()
        {
            InteractableObject interactable = XRTestUtilities.CreateInteractableObjcet();
            TouchableProperty touchProperty = interactable.gameObject.AddComponent<TouchableProperty>();
            bool wasTouched = false;

            touchProperty.Touched += (sender, args) =>
            {
                Assert.IsNotNull(sender);
                Assert.That(sender.GetType() == touchProperty.GetType());
                
                wasTouched = true;
            };

            interactable.gameObject.AddComponent<TraineeSceneObject>();
            
            ICourse course = new LinearTrainingBuilder("Test Course")
                .AddChapter(new LinearChapterBuilder("Chapter")
                    .AddStep(new BasicStepBuilder("Step")
                        .AddCondition(new TouchedCondition(touchProperty))))
                .Build();

            CourseRunner.Initialize(course);
            CourseRunner.Run();

            yield return new WaitUntil(()=> CourseRunner.IsRunning);
            
            IChapter chapter = CourseRunner.Current.Data.Current;
            IStep step = chapter.Data.Current;
            ITransition transition = step.Data.Transitions.Data.Transitions.First();
            
            CourseRunner.SkipStep(transition);

            yield return new WaitUntil(()=> wasTouched);
        }
        
        [UnityTest]
        public IEnumerator FastForwardGrab()
        {
            InteractableObject interactable = XRTestUtilities.CreateInteractableObjcet();
            GrabbableProperty grabbableProperty = interactable.gameObject.AddComponent<GrabbableProperty>();
            bool wasGrabbed = false;

            grabbableProperty.Grabbed += (sender, args) =>
            {
                Assert.IsNotNull(sender);
                Assert.That(sender.GetType() == grabbableProperty.GetType());
                
                wasGrabbed = true;
            };

            interactable.gameObject.AddComponent<TraineeSceneObject>();
            
            ICourse course = new LinearTrainingBuilder("Test Course")
                .AddChapter(new LinearChapterBuilder("Chapter")
                    .AddStep(new BasicStepBuilder("Step")
                        .AddCondition(new GrabbedCondition(grabbableProperty))))
                .Build();

            CourseRunner.Initialize(course);
            CourseRunner.Run();

            yield return new WaitUntil(()=> CourseRunner.IsRunning);
            
            IChapter chapter = CourseRunner.Current.Data.Current;
            IStep step = chapter.Data.Current;
            ITransition transition = step.Data.Transitions.Data.Transitions.First();
            
            CourseRunner.SkipStep(transition);

            yield return new WaitUntil(()=> wasGrabbed);
        }
        
        [UnityTest]
        public IEnumerator FastForwardUngrab()
        {
            InteractableObject interactable = XRTestUtilities.CreateInteractableObjcet();
            GrabbableProperty grabbableProperty = interactable.gameObject.AddComponent<GrabbableProperty>();
            bool wasUngrabbed = false;

            grabbableProperty.Ungrabbed += (sender, args) =>
            {
                Assert.IsNotNull(sender);
                Assert.That(sender.GetType() == grabbableProperty.GetType());
                
                wasUngrabbed = true;
            };

            interactable.gameObject.AddComponent<TraineeSceneObject>();
            
            ICourse course = new LinearTrainingBuilder("Test Course")
                .AddChapter(new LinearChapterBuilder("Chapter")
                    .AddStep(new BasicStepBuilder("Step")
                        .AddCondition(new ReleasedCondition(grabbableProperty))))
                .Build();

            CourseRunner.Initialize(course);
            CourseRunner.Run();

            yield return new WaitUntil(()=> CourseRunner.IsRunning);
            
            IChapter chapter = CourseRunner.Current.Data.Current;
            IStep step = chapter.Data.Current;
            ITransition transition = step.Data.Transitions.Data.Transitions.First();
            
            CourseRunner.SkipStep(transition);

            yield return new WaitUntil(()=> wasUngrabbed);
        }
        
        [UnityTest]
        public IEnumerator FastForwardUse()
        {
            InteractableObject interactable = XRTestUtilities.CreateInteractableObjcet();
            UsableProperty usableProperty = interactable.gameObject.AddComponent<UsableProperty>();
            bool wasUsed = false;

            usableProperty.UsageStarted += (sender, args) =>
            {
                Assert.IsNotNull(sender);
                Assert.That(sender.GetType() == usableProperty.GetType());
                
                wasUsed = true;
            };

            interactable.gameObject.AddComponent<TraineeSceneObject>();
            
            ICourse course = new LinearTrainingBuilder("Test Course")
                .AddChapter(new LinearChapterBuilder("Chapter")
                    .AddStep(new BasicStepBuilder("Step")
                        .AddCondition(new UsedCondition(usableProperty))))
                .Build();

            CourseRunner.Initialize(course);
            CourseRunner.Run();

            yield return new WaitUntil(()=> CourseRunner.IsRunning);
            
            IChapter chapter = CourseRunner.Current.Data.Current;
            IStep step = chapter.Data.Current;
            ITransition transition = step.Data.Transitions.Data.Transitions.First();
            
            CourseRunner.SkipStep(transition);

            yield return new WaitUntil(()=> wasUsed);
        }
    }
}

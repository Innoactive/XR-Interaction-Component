using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Innoactive.Creator.XR.SceneObjects.Properties;
using Innoactive.Hub.Training;
using Innoactive.Hub.Training.Conditions;
using Innoactive.Creator.Core.Tests.Utils;
using Innoactive.Hub.Training.Interaction.Conditions;
using Innoactive.Hub.Training.SceneObjects.Properties;
using Innoactive.Hub.Training.Utils.Builders;
using UnityEngine.XR.Interaction.Toolkit;

namespace Innoactive.Creator.XR.Tests
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
        public IEnumerator TouchablePropertyTest()
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
        public IEnumerator GrabbablePropertyTest()
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
        public IEnumerator UsablePropertyTest()
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
        public IEnumerator FastForwardTouchTest()
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

            TrainingRunner.Initialize(course);
            TrainingRunner.Run();

            yield return new WaitUntil(()=> TrainingRunner.IsRunning);
            
            IChapter chapter = TrainingRunner.Current.Data.Current;
            IStep step = chapter.Data.Current;
            ITransition transition = step.Data.Transitions.Data.Transitions.First();
            
            TrainingRunner.SkipStep(transition);

            yield return new WaitUntil(()=> wasTouched);
        }
        
        [UnityTest]
        public IEnumerator FastForwardGrabTest()
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

            TrainingRunner.Initialize(course);
            TrainingRunner.Run();

            yield return new WaitUntil(()=> TrainingRunner.IsRunning);
            
            IChapter chapter = TrainingRunner.Current.Data.Current;
            IStep step = chapter.Data.Current;
            ITransition transition = step.Data.Transitions.Data.Transitions.First();
            
            TrainingRunner.SkipStep(transition);

            yield return new WaitUntil(()=> wasGrabbed);
        }
        
        [UnityTest]
        public IEnumerator FastForwardUngrabTest()
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

            TrainingRunner.Initialize(course);
            TrainingRunner.Run();

            yield return new WaitUntil(()=> TrainingRunner.IsRunning);
            
            IChapter chapter = TrainingRunner.Current.Data.Current;
            IStep step = chapter.Data.Current;
            ITransition transition = step.Data.Transitions.Data.Transitions.First();
            
            TrainingRunner.SkipStep(transition);

            yield return new WaitUntil(()=> wasUngrabbed);
        }
        
        [UnityTest]
        public IEnumerator FastForwardUseTest()
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

            TrainingRunner.Initialize(course);
            TrainingRunner.Run();

            yield return new WaitUntil(()=> TrainingRunner.IsRunning);
            
            IChapter chapter = TrainingRunner.Current.Data.Current;
            IStep step = chapter.Data.Current;
            ITransition transition = step.Data.Transitions.Data.Transitions.First();
            
            TrainingRunner.SkipStep(transition);

            yield return new WaitUntil(()=> wasUsed);
        }
    }
}

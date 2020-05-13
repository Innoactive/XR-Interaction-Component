using System;
using System.Collections;
using UnityEngine.TestTools;
using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;
using Innoactive.Creator.Core;
using Innoactive.Creator.Core.Behaviors;
using Innoactive.Creator.Core.Configuration;
using Innoactive.Creator.Core.Configuration.Modes;
using Innoactive.Creator.Tests.Utils;
using Innoactive.Creator.XRInteraction;
using Innoactive.Creator.XRInteraction.Properties;
using Innoactive.Creator.XRInteraction.Tests;
using Object = UnityEngine.Object;

namespace Innoactive.Creator.XRInteraction.Tests.Behaviors
{
    public class HighlightObjectTests : RuntimeTests
    {
        private class HighlighterReporter : MonoBehaviour
        {
            public InteractableHighlighter Highlighter { get; private set; }
            public HighlightProperty Property { get; private set; }
            public bool IsHighlighted { get; private set; }
            public Color HighlightColor { get; private set; }

            private readonly Color highlightColor = new Color32(64, 200, 255, 50);
            
            private void Awake()
            {
                Highlighter = gameObject.AddComponent<InteractableHighlighter>();
                Property = gameObject.AddComponent<HighlightProperty>();
            }

            public void Highlight(Color? color = null, float duration = 0)
            {
                HighlightColor = color ?? highlightColor;
                IsHighlighted = true;
                
                Highlighter.StartHighlighting("DummyHighlight", HighlightColor);
            }

            public void Unhighlight(Color? color = null, float duration = 0)
            {
                HighlightColor = highlightColor; //null
                IsHighlighted = false;
                
                Highlighter.StopHighlighting("DummyHighlight");
            }
        }

        private const string targetName = "TestReference";

        [UnityTest]
        public IEnumerator CreateHighlighter()
        {
            // Given a VRTKObjectHighlight behavior with a game object without a highlighter,
            InteractableObject interactable = XRTestUtilities.CreateInteractableObjcet();
            HighlighterReporter highlighterReporter = interactable.gameObject.AddComponent<HighlighterReporter>();
            
            yield return null;
            
            HighlightProperty targetObject = highlighterReporter.Property;
            HighlightObjectBehavior highlight = new HighlightObjectBehavior(targetObject);
        
            // When we activate it,
            highlight.LifeCycle.Activate();
            
            yield return new WaitForSecondsRealtime(10);

        
            // Then it has the VRTK_BaseHighlighter and VRTK_MaterialColorSwapHighlighter components.
            Assert.AreEqual(1, interactable.GetComponents<InteractableHighlighter>().Length);
            
        
            yield return null;
        }

        [UnityTest]
        public IEnumerator StepWithHighlightNonInteractable()
        {
            // Given a VRTKObjectHighlight behavior with a proper game object and a linear chapter,
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            HighlighterReporter highlighterReporter = gameObject.AddComponent<HighlighterReporter>();
            HighlightProperty targetObject = gameObject.AddComponent<HighlightProperty>();
            targetObject.SceneObject.ChangeUniqueName(targetName);

            Color highlightColor = Color.yellow;

            HighlightObjectBehavior highlight = new HighlightObjectBehavior(targetObject, highlightColor);

            TestLinearChapterBuilder builder = TestLinearChapterBuilder.SetupChapterBuilder(1, true);
            Chapter chapter = builder.Build();
            builder.Steps[0].Data.Behaviors.Data.Behaviors.Add(highlight);
            chapter.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // When we activate the chapter,
            chapter.LifeCycle.Activate();

            while (highlight.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                chapter.Update();
            }

            Stage highlightStageInStep = highlight.LifeCycle.Stage;
            bool objectHighlightedActiveInStep = highlighterReporter.IsHighlighted;
            Color? colorInStep = highlighterReporter.HighlightColor;

            chapter.Data.FirstStep.LifeCycle.Deactivate();

            while (chapter.Data.FirstStep.LifeCycle.Stage != Stage.Inactive)
            {
                yield return null;
                chapter.Update();
            }

            Stage highlightStageAfterStep = highlight.LifeCycle.Stage;
            bool objectHighlightedActiveAfterStep = highlighterReporter.IsHighlighted;
            Color? colorAfterStep = highlighterReporter.HighlightColor;

            // Then the highlight behavior is active during the step and inactive after it.
            Assert.AreEqual(Stage.Active, highlightStageInStep, "Highlight behavior should be active during step");
            Assert.IsTrue(objectHighlightedActiveInStep, "VRTK Highlighter should be active during step");
            Assert.AreEqual(highlightColor, colorInStep, "Highlight color should be " + highlightColor.ToString());

            Assert.AreEqual(Stage.Inactive, highlightStageAfterStep, "Highlight behavior should be deactivated after step");
            Assert.IsFalse(objectHighlightedActiveAfterStep, "VRTK Highlighter should be inactive after step");
            Assert.IsNull(colorAfterStep, "Hightlight color should be null after deactivation of step.");
        }

        [UnityTest]
        public IEnumerator StepWithHighlightInteractable()
        {
            // Given a VRTKObjectHighlight behavior with a proper game object and a linear chapter,
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            HighlighterReporter highlighterReporter = gameObject.AddComponent<HighlighterReporter>();
            HighlightProperty targetObject = gameObject.AddComponent<HighlightProperty>();
            targetObject.SceneObject.ChangeUniqueName(targetName);

            Color highlightColor = Color.yellow;

            HighlightObjectBehavior highlight = new HighlightObjectBehavior(targetObject, highlightColor);

            TestLinearChapterBuilder builder = TestLinearChapterBuilder.SetupChapterBuilder(1, true);
            Chapter chapter = builder.Build();
            builder.Steps[0].Data.Behaviors.Data.Behaviors.Add(highlight);
            chapter.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // When we activate the chapter,
            chapter.LifeCycle.Activate();

            while (highlight.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                chapter.Update();
            }

            Stage highlightStageInStep = highlight.LifeCycle.Stage;
            bool objectHighlightedActiveInStep = highlighterReporter.IsHighlighted;
            Color? colorInStep = highlighterReporter.HighlightColor;

            chapter.Data.FirstStep.LifeCycle.Deactivate();

            while (chapter.Data.FirstStep.LifeCycle.Stage != Stage.Inactive)
            {
                yield return null;
                chapter.Update();
            }

            Stage highlightStageAfterStep = highlight.LifeCycle.Stage;
            bool objectHighlightedActiveAfterStep = highlighterReporter.IsHighlighted;
            Color? colorAfterStep = highlighterReporter.HighlightColor;

            // Then the highlight behavior is active during the step and inactive after it.
            Assert.AreEqual(Stage.Active, highlightStageInStep, "Highlight behavior should be active during step");
            Assert.IsTrue(objectHighlightedActiveInStep, "VRTK Highlighter should be active during step");
            Assert.AreEqual(highlightColor, colorInStep, "Highlight color should be " + highlightColor.ToString());

            Assert.AreEqual(Stage.Inactive, highlightStageAfterStep, "Highlight behavior should be deactivated after step");
            Assert.IsFalse(objectHighlightedActiveAfterStep, "VRTK Highlighter should be inactive after step");
            Assert.IsNull(colorAfterStep, "Hightlight color should be null after deactivation of step.");
        }

        [UnityTest]
        public IEnumerator ColorChangeNonInteractable()
        {
            // Given a VRTKObjectHighlight behavior with a proper game object and a color,
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            HighlighterReporter highlighterReporter = gameObject.AddComponent<HighlighterReporter>();
            HighlightProperty targetObject = gameObject.AddComponent<HighlightProperty>();
            targetObject.SceneObject.ChangeUniqueName(targetName);
        
            Color highlightColor = Color.cyan;
        
            HighlightObjectBehavior highlight = new HighlightObjectBehavior(targetObject, highlightColor);
        
            TestLinearChapterBuilder builder = TestLinearChapterBuilder.SetupChapterBuilder(1, true);
            Chapter chapter = builder.Build();
            builder.Steps[0].Data.Behaviors.Data.Behaviors.Add(highlight);
            chapter.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);
        
            // When we activate the chapter,
            chapter.LifeCycle.Activate();
        
            while (builder.Steps[0].LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                chapter.Update();
            }
        
            // Then the color is changed.
            Assert.AreEqual(highlightColor, highlight.Data.HighlightColor);
            Assert.AreEqual(highlightColor, highlighterReporter.HighlightColor);
            Assert.AreEqual(highlightColor, ((HighlightProperty)highlight.Data.ObjectToHighlight).CurrentHighlightColor);
        }
        
        [UnityTest]
        public IEnumerator ColorChangeInteractable()
        {
            // Given a VRTKObjectHighlight behavior with a proper game object and a color,
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            HighlighterReporter highlighterReporter = gameObject.AddComponent<HighlighterReporter>();
            HighlightProperty targetObject = gameObject.AddComponent<HighlightProperty>();
            targetObject.SceneObject.ChangeUniqueName(targetName);
            
            Color highlightColor = Color.cyan;
        
            HighlightObjectBehavior highlight = new HighlightObjectBehavior(targetObject, highlightColor);
        
            TestLinearChapterBuilder builder = TestLinearChapterBuilder.SetupChapterBuilder(1, true);
            Chapter chapter = builder.Build();
            builder.Steps[0].Data.Behaviors.Data.Behaviors.Add(highlight);
            chapter.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);
        
            // When we activate the chapter,
            chapter.LifeCycle.Activate();
        
            while (builder.Steps[0].LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                chapter.Update();
            }
        
            // Then the color is changed.
            Assert.AreEqual(highlight.Data.HighlightColor, highlightColor);
            Assert.AreEqual(highlighterReporter.HighlightColor, highlightColor);
            Assert.AreEqual(highlightColor, ((HighlightProperty)highlight.Data.ObjectToHighlight).CurrentHighlightColor);
        }
        
        [UnityTest]
        public IEnumerator HighlightColorIsSetByParameter()
        {
            // Given a HighlightProperty with a HighlightColor parameter set,
            DynamicRuntimeConfiguration testRuntimeConfiguration = new DynamicRuntimeConfiguration();
        
            Color highlightColor = Color.red;
        
            testRuntimeConfiguration.SetAvailableModes(new List<IMode>
            {
                new Mode("Test", new WhitelistTypeRule<IOptional>(), new Dictionary<string, object> {{"HighlightColor", highlightColor}}),
            });
        
            RuntimeConfigurator.Configuration = testRuntimeConfiguration;
        
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            HighlighterReporter highlighterReporter =  gameObject.AddComponent<HighlighterReporter>();
            HighlightProperty targetObject = gameObject.AddComponent<HighlightProperty>();
            targetObject.SceneObject.ChangeUniqueName(targetName);
        
            HighlightObjectBehavior highlight = new HighlightObjectBehavior(targetObject, Color.cyan);
            highlight.Configure(testRuntimeConfiguration.Modes.CurrentMode);
        
            // When we activate it,
            highlight.LifeCycle.Activate();
        
            // Then the highlight color is changed.
            Assert.AreEqual(highlightColor, highlight.Data.HighlightColor);
            Assert.AreEqual(highlightColor, highlighterReporter.HighlightColor);
            Assert.AreEqual(highlightColor, ((HighlightProperty)highlight.Data.ObjectToHighlight).CurrentHighlightColor);
        
            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveBehavior()
        {
            // Given a VRTKObjectHighlight behavior with a proper game object,
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gameObject.AddComponent<HighlighterReporter>();
            HighlightProperty targetObject = gameObject.AddComponent<HighlightProperty>();
            targetObject.SceneObject.ChangeUniqueName(targetName);

            HighlightObjectBehavior behavior = new HighlightObjectBehavior(targetObject, Color.cyan);

            // When we mark it to fast-forward,
            behavior.LifeCycle.MarkToFastForward();

            // Then it doesn't autocomplete because it hasn't been activated yet.
            Assert.AreEqual(Stage.Inactive, behavior.LifeCycle.Stage);

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveBehaviorAndActivateIt()
        {
            // Given a VRTKObjectHighlight behavior with a proper game object,
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gameObject.AddComponent<HighlighterReporter>();
            HighlightProperty targetObject = gameObject.AddComponent<HighlightProperty>();
            targetObject.SceneObject.ChangeUniqueName(targetName);

            HighlightObjectBehavior behavior = new HighlightObjectBehavior(targetObject, Color.red);

            // When we mark it to fast-forward and activate it,
            behavior.LifeCycle.MarkToFastForward();
            behavior.LifeCycle.Activate();

            // Then the behavior should be activated immediately.
            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveBehaviorAndDeactivateIt()
        {
            // Given a VRTKObjectHighlight behavior with a proper game object,
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gameObject.AddComponent<HighlighterReporter>();
            HighlightProperty targetObject = gameObject.AddComponent<HighlightProperty>();
            targetObject.SceneObject.ChangeUniqueName(targetName);

            HighlightObjectBehavior behavior = new HighlightObjectBehavior(targetObject, Color.red);

            // When we mark it to fast-forward, activate, and deactivate it,
            behavior.LifeCycle.MarkToFastForward();
            behavior.LifeCycle.Activate();
            behavior.LifeCycle.Deactivate();

            // Then the behavior should be deactivated immediately.
            Assert.AreEqual(Stage.Inactive, behavior.LifeCycle.Stage);

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardActivatingBehavior()
        {
            // Given an active VRTKObjectHighlight behavior with a proper game object,
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gameObject.AddComponent<HighlighterReporter>();
            HighlightProperty targetObject = gameObject.AddComponent<HighlightProperty>();
            targetObject.SceneObject.ChangeUniqueName(targetName);

            HighlightObjectBehavior behavior = new HighlightObjectBehavior(targetObject, Color.blue);

            behavior.LifeCycle.Activate();

            // When we mark it to fast-forward,
            behavior.LifeCycle.MarkToFastForward();

            // Then the behavior should be activated immediately.
            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardDeactivatingBehavior()
        {
            // Given a deactivating VRTKObjectHighlight behavior with a proper game object,
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gameObject.AddComponent<HighlighterReporter>();
            HighlightProperty targetObject = gameObject.AddComponent<HighlightProperty>();
            targetObject.SceneObject.ChangeUniqueName(targetName);

            HighlightObjectBehavior behavior = new HighlightObjectBehavior(targetObject, Color.blue);
            behavior.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            behavior.LifeCycle.Activate();

            while (behavior.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                behavior.Update();
            }

            behavior.LifeCycle.Deactivate();

            // When we mark it to fast-forward,
            behavior.LifeCycle.MarkToFastForward();

            // Then the behavior should be deactivated immediately.
            Assert.AreEqual(Stage.Inactive, behavior.LifeCycle.Stage);
        }
    }
}

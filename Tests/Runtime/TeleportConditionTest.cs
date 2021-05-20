﻿using System.Collections;
using System.Collections.Generic;
using VPG.BasicInteraction.Conditions;
using VPG.Core;
using VPG.Core.Properties;
using VPG.Tests.Utils;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace VPG.XRInteraction.Tests.Conditions
{
    public class TeleportConditionTest : RuntimeTests
    {
        public class TeleportationPropertyMock : TeleportationProperty
        {
            public new void EmitTeleported()
            {
                base.EmitTeleported();
            }
        }
        
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            XRTestUtilities.CreateXRRig();
        }
        
        [UnityTest]
        public IEnumerator CompleteWhenTeleported()
        {
            // Setup object with mocked teleport property and activate
            GameObject obj = new GameObject("T1");
            TeleportationPropertyMock mockedProperty = obj.AddComponent<TeleportationPropertyMock>();

            yield return new WaitForFixedUpdate();

            TeleportCondition condition = new TeleportCondition(mockedProperty);
            condition.LifeCycle.Activate();

            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            // When the object is teleported
            mockedProperty.EmitTeleported();

            yield return null;
            condition.Update();

            // Assert that condition is now completed
            Assert.IsTrue(condition.IsCompleted);
        }
        
        [UnityTest]
        public IEnumerator CompleteOnlyWhenTeleportedDuringStepExecution()
        {
            // Setup object with mocked teleport property and activate
            GameObject obj = new GameObject("T1");
            TeleportationPropertyMock mockedProperty = obj.AddComponent<TeleportationPropertyMock>();
            
            Assert.IsFalse(mockedProperty.WasUsedToTeleport);
            
            mockedProperty.EmitTeleported();
            
            Assert.IsTrue(mockedProperty.WasUsedToTeleport);
        
            yield return new WaitForFixedUpdate();
        
            TeleportCondition condition = new TeleportCondition(mockedProperty);
            condition.LifeCycle.Activate();
            
            float startTime = Time.time;
            while (condition.IsCompleted == false && Time.time < startTime + 0.1f)
            {
                yield return null;
                condition.Update();
            }
            
            Assert.IsFalse(mockedProperty.WasUsedToTeleport);
            
            // When the object is teleported
            mockedProperty.EmitTeleported();

            yield return null;
            condition.Update();
            
            // Assert that condition is now completed due WasUsedToTeleport being true
            Assert.IsTrue(mockedProperty.WasUsedToTeleport);
            Assert.IsTrue(condition.IsCompleted);
        }
        
        [UnityTest]
        public IEnumerator ConditionNotCompleted()
        {
            // Setup object with mocked teleport property and activate
            GameObject obj = new GameObject("T1");
            TeleportationPropertyMock mockedProperty = obj.AddComponent<TeleportationPropertyMock>();
            TeleportCondition condition = new TeleportCondition(mockedProperty);
            condition.LifeCycle.Activate();

            float startTime = Time.time;
            while (Time.time < startTime + 0.1f)
            {
                yield return null;
                condition.Update();
            }

            // Assert after doing nothing the condition is not completed.
            Assert.IsFalse(condition.IsCompleted);
        }
        
        [UnityTest]
        public IEnumerator AutoCompleteActive()
        {
            // Given a teleport condition,
            GameObject obj = new GameObject("T1");
            TeleportationPropertyMock mockedProperty = obj.AddComponent<TeleportationPropertyMock>();

            yield return new WaitForFixedUpdate();
            
            TeleportCondition condition = new TeleportCondition(mockedProperty);

            // When you activate and autocomplete it,
            condition.LifeCycle.Activate();

            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            condition.Autocomplete();
            yield return null;

            // Then it is completed.
            Assert.AreEqual(Stage.Active, condition.LifeCycle.Stage);
            Assert.IsTrue(condition.IsCompleted);
        }
        
        [UnityTest]
        public IEnumerator FastForwardDoesNotCompleteCondition()
        {
            // Given a teleport condition,
            GameObject obj = new GameObject("T1");
            TeleportationPropertyMock mockedProperty = obj.AddComponent<TeleportationPropertyMock>();

            yield return new WaitForFixedUpdate();

            TeleportCondition condition = new TeleportCondition(mockedProperty);

            // When you activate it,
            condition.LifeCycle.Activate();

            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            // When you fast-forward it
            condition.LifeCycle.MarkToFastForward();

            // Then nothing happens.
            Assert.AreEqual(Stage.Active, condition.LifeCycle.Stage);
            Assert.IsFalse(condition.IsCompleted);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System;
using System.Linq;

public class SmachTests
{
    [Test]
    public void NewMachineShouldCreateEmptyStateList()
    {
        var mach = new Smach.Machine();
        Assert.That(mach.States != null && mach.States.Count == 0);
    }

    [Test]
    public void AddStateWithoutActionsShouldThrowArgumentException()
    {
        var mach = new Smach.Machine();
        Assert.Throws<ArgumentException>(() => mach.Add("a"));
    }

    [Test]
    public void AddStateWithEmptyNameShouldThrowArgumentException()
    {
        var mach = new Smach.Machine();
        Assert.Throws<ArgumentException>(() => mach.Add(name: "", () => {}, () => {}, () => {}));
    }

    [Test]
    public void AddStateWithNullNameShouldThrowArgumentNullException()
    {
        var mach = new Smach.Machine();
        Assert.Throws<ArgumentNullException>(() => mach.Add(null, () => {}, () => {}, () => {}));
    }

    [Test]
    public void AddStartStateShouldSetMachineStartState()
    {
        var mach = new Smach.Machine();
        mach.Add(name: "a", () => {}, () => {}, () => {}, true);
        Assert.That(mach.States[mach.StartStateName] != null);
    }

    [Test]
    public void AddAnotherStartStateShouldOverwiteMachineStartState()
    {
        var mach = new Smach.Machine();
        mach.Add(name: "a", () => {}, () => {}, () => {}, true);
        mach.Add(name: "b", () => {}, () => {}, () => {}, true);
        Assert.That(mach.StartStateName == "b");
    }

    [Test]
    public void CallingToBeforeTickShouldThrowInvalidOperationException()
    {
        var mach = new Smach.Machine();
        mach.Add(name: "a", () => {}, () => {}, () => {}, true);
        mach.Add(name: "b", () => {}, () => {}, () => {});
        Assert.Throws<InvalidOperationException>(() => mach.To("b"));
    }

    [Test]
    public void CallingTickWithoutStartStateShouldThrowArgumentNullException()
    {
        var mach = new Smach.Machine();
        mach.Add(name: "a", () => {}, () => {}, () => {});
        mach.Add(name: "b", () => {}, () => {}, () => {});
        Assert.Throws<ArgumentNullException>(() => mach.Tick(), "Machine does not have a start state");
    }

    [Test]
    public void CallingResetWithoutStartStateShouldThrowInvalidOperationException()
    {
        var mach = new Smach.Machine();
        mach.Add(name: "a", () => {}, () => {}, () => {});
        mach.Add(name: "b", () => {}, () => {}, () => {});
        Assert.Throws<ArgumentNullException>(() => mach.Reset(), "Machine does not have a start state");
    }

    [Test]
    public void ResetShouldInvokeStateChangeForStartState()
    {
        var mach = new Smach.Machine();
        mach.Add(name: "a", () => {}, () => {}, () => {}, true);
        var cond = false;
        mach.StateChange += (state, transition) => cond = state == "a" && transition == Smach.Transition.Enter;
        Assert.That(cond = true);
    }

    [Test]
    public void ToShouldInvokeStateChangePatternEnterExitEnter()
    {
        var mach = new Smach.Machine();
        mach.Add(name: "a", () => {}, () => {}, () => {}, true);
        mach.Add(name: "b", () => {}, () => {}, () => {});
        var eventCount = 0;
        var condList = new List<bool>();

        mach.StateChange += (state, transition) => {
            if (eventCount == 0)
                Assert.That(state == "a" && transition == Smach.Transition.Enter);
            else if (eventCount == 1)
                Assert.That(state == "a" && transition == Smach.Transition.Exit);
            else if (eventCount == 2)
                Assert.That(state == "b" && transition == Smach.Transition.Enter);
            eventCount++;
        };

        mach.Tick();
        mach.To("b");

        Assert.That(eventCount == 3);
    }
}

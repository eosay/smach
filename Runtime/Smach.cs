using System.Collections;
using System.Collections.Generic;
using System;

namespace Smach
{
    public enum Transition
    {
        Enter,
        Exit
    }

    public delegate void StageChangeEvent(string state, Transition transition);
    
    public class State
    {
        public string Name;
        public Action Enter;
        public Action Update;
        public Action Exit;

        public State() {}

        public State(string name, Action enter, Action update, Action exit)
        {
            Name = name;
            Enter = enter;
            Update = update;
            Exit = exit;
        }
    }

    public class Machine
    {
        public Dictionary<String, State> States { get; }
        public event StageChangeEvent StateChange;
        public string StartStateName { get => start; }
        public State CurrentState { get => head; }

        State head;
        string start;

        public Machine()
        {
            States = new Dictionary<string, State>();
        }

        public void Add(string name, Action enter=null, Action update=null, Action exit=null, bool isStart=false)
        {
            if (enter == null && update == null && exit == null)
                throw new ArgumentException("All actions cannot be null");

            if (name == "")
                throw new ArgumentException("State name cannot be empty");
            
            if (name == null)
                throw new ArgumentNullException("State name cannot be null");

            var state = new State(name, enter, update, exit);
            States.Add(name, state);
            if (isStart)
                start = name;
        }

        public void Tick()
        {
            if (head == null)
                Reset();

            if (head.Update != null)
                head.Update();
        }

        public void To(string state)
        {
            if (head == null)
                throw new InvalidOperationException("State machine not initialized. Call Tick() before To()");

            var prev = head.Name;

            if (Attempt(head.Exit))
                StateChange?.Invoke(head.Name, Transition.Exit);

            head = States[state];

            if (Attempt(head.Enter))
                StateChange?.Invoke(head.Name, Transition.Enter);
        }

        public void Reset()
        {
            try
            {
                head = States[start];
            }
            catch (ArgumentNullException e)
            {
                throw new ArgumentNullException("Machine does not have a start state", e);
            }

            if (head.Enter != null)
            {
                head.Enter();
                StateChange?.Invoke(head.Name, Transition.Enter);
            }
        }

        bool Attempt(Action action)
        {
            if (action != null)
            {
                action();
                return true;
            }
            return false;
        }
    }
}
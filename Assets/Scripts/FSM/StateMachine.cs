using System;
using System.Collections.Generic;

namespace Core.FSM
{
    public class StateMachine 
    {
        class StateNode
        {
            public IState State { get; }
            public HashSet<ITransition> Transitions { get; }
            public StateNode(IState state)
            {
                State = state;
                Transitions = new HashSet<ITransition>();
            }

            public void AddTransition(IState toState, IPredicate condition)
            {
                Transitions.Add(new Transition(toState, condition));
            }
        }

        StateNode current;
        Dictionary<Type, StateNode> nodes = new();
        HashSet<ITransition> anyTransition = new();

        public void SetState(IState state) // Usually used for Initial State
        {
            current = nodes[state.GetType()];
            current.State.Enter();
        }

        private void ChangeState(IState state)
        {
            if (current == state) { return; }

            IState previousState = current.State;
            IState nextState = nodes[state.GetType()].State;

            previousState?.Exit();
            nextState?.Enter();
            current = nodes[state.GetType()];
        }

        private ITransition GetTransition()
        {
            foreach (ITransition transition in anyTransition)
            {
                if (transition.Condition.Evaluate())
                {
                    return transition;
                }
            }
            foreach (ITransition transition in current.Transitions)
            {
                if (transition.Condition.Evaluate())
                {
                    return transition;
                }
            }
            return null;
        }

        private StateNode GetOrAddNode(IState state)
        {
            StateNode node = nodes.GetValueOrDefault(state.GetType());
            if (node == null)
            {
                node = new StateNode(state);
                nodes.Add(state.GetType(), node);
            }
            return node;
        }

        public void AddTransition(IState fromState, IState toState, IPredicate condition)
        {
            GetOrAddNode(fromState).AddTransition(toState, condition);
        }

        public void Update()
        {
            ITransition transition = GetTransition();
            if (transition != null)
            {
                ChangeState(transition.To);
            }
            current.State?.Update();
        }
    }
}
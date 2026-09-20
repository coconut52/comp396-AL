namespace Core.FSM
{
    public class Transition : ITransition
    {
        public IState To { get; }
        public IPredicate Condition { get; }
        public Transition(IState toState, IPredicate condition)
        { 
            To = toState;
            Condition = condition;
        }
    }
}
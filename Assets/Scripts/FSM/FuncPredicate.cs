using System;

namespace Core.FSM
{
    public class FuncPredicate : IPredicate
    {
        readonly Func<bool> predicate;

        public FuncPredicate(Func<bool> func)
        {
            predicate = func;
        }

        public bool Evaluate()
        {
            return predicate.Invoke();
        }
    }
}
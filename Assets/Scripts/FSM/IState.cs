namespace Core.FSM
{
    public interface IState 
    {
        public void Enter();
        public void Update();
        public void Exit();
    }
}
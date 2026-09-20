using UnityEngine;
using UnityEngine.AI;

namespace Core.FSM
{
    public abstract class BaseState : IState
    {
        protected MeshRenderer meshRenderer;
        protected NavMeshAgent agent;
        protected BaseState(MeshRenderer renderer, NavMeshAgent agent)
        {
            meshRenderer = renderer;
            this.agent = agent;
        }
        public virtual void Enter() { }
        public virtual void Exit() 
        {
            Debug.Log("BaseState.Exit();");
            agent.isStopped = true;
        }
        public virtual void Update() { }
    }
}
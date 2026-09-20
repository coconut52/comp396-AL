using UnityEngine;
using UnityEngine.AI;

namespace Core.FSM
{
    public class RestState : BaseState
    {
        public RestState(MeshRenderer renderer, NavMeshAgent agent) : base(renderer, agent)
        {
        }

        public override void Enter()
        {
            meshRenderer.material.color = Color.cyan;
        }
    }
}
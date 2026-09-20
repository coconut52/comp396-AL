using UnityEngine;
using UnityEngine.AI;

namespace Core.FSM
{
    public class BlacksmithState : BaseState
    {
        public BlacksmithState(MeshRenderer renderer, NavMeshAgent agent) : base(renderer, agent)
        {
        }

        public override void Enter()
        {
            // The NPC would move to the forge, collect the requested materials, and heat the metal.
            Debug.Log("BlacksmithState.Enter()");
            meshRenderer.material.color = Color.red;
        }

        public override void Update()
        {
            // The NPC would hammer, reheat, and quench the item until the current work order is complete.
        }

        public override void Exit()
        {
            // The NPC would cool the forge, store the tools, and place the completed item for pickup.
            Debug.Log("BlacksmithState.Exit()");
            base.Exit();
        }
    }
}

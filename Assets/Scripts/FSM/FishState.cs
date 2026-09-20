using UnityEngine;
using UnityEngine.AI;

namespace Core.FSM
{
    public class FishState : BaseState
    {
        public FishState(MeshRenderer renderer, NavMeshAgent agent) : base(renderer, agent)
        {
        }

        public override void Enter()
        {
            // The NPC would walk to an available fishing spot, prepare the rod, and cast the line.
            Debug.Log("FishState.Enter()");
            meshRenderer.material.color = Color.blue;
        }

        public override void Update()
        {
            // The NPC would watch for a bite, reel in fish, and repeat until the basket is full.
        }

        public override void Exit()
        {
            // The NPC would pack away the fishing rod and carry the collected fish back to the village.
            Debug.Log("FishState.Exit()");
            base.Exit();
        }
    }
}

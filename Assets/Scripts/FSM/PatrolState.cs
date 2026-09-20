using UnityEngine;
using UnityEngine.AI;

namespace Core.FSM
{
    public class PatrolState : BaseState
    {
        private GameObject[] waypoints;
        public PatrolState(MeshRenderer renderer, NavMeshAgent agent, GameObject[] waypoints) : base(renderer, agent)
        {
            this.waypoints = waypoints;
        }

        public override void Enter()
        {
            meshRenderer.material.color = Color.yellow;
        }

        public override void Update()
        {
            foreach (GameObject waypoint in waypoints)
            {
                
            }
        }
    }
}
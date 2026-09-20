using UnityEngine;
using UnityEngine.AI;

namespace Core.FSM
{
    public class HarvestState : BaseState
    {
        private GameObject harvestingPlot;
        public HarvestState(MeshRenderer renderer, NavMeshAgent agent) : base(renderer, agent)
        {
        }

        public override void Enter()
        {
            base.Enter();
            Debug.Log("HarvestState.Enter()");
            meshRenderer.material.color = Color.green;
            harvestingPlot = GameObject.FindWithTag("HarvestingPlot");
            agent.SetDestination(harvestingPlot.transform.position);
            agent.isStopped = false;
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Exit()
        {
            base.Exit();
            harvestingPlot = null;
        }
    }
}
using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace Core.FSM
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NPCStateMachine : MonoBehaviour
    {
        private StateMachine stateMachine;
        [SerializeField] private VoidEventChannel harvestEvent;
        [SerializeField] private bool isHarvestReady;

        private void Awake()
        {
            // Creation of the StateMachine 
            stateMachine = new StateMachine();
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            NavMeshAgent agent = GetComponent<NavMeshAgent>();
            // Create instances for concrete stateNodes
            PatrolState patrol = new PatrolState(renderer, agent, new GameObject[2]);
            HarvestState harvest = new HarvestState(renderer, agent);
            RestState rest = new RestState(renderer, agent);

            stateMachine.AddTransition(rest, patrol, new FuncPredicate(() => Keyboard.current.pKey.wasPressedThisFrame));
            stateMachine.AddTransition(rest, harvest, new FuncPredicate(() => isHarvestReady));

            stateMachine.AddTransition(harvest, rest, new FuncPredicate(() => !isHarvestReady));
            stateMachine.AddTransition(patrol, rest, new FuncPredicate(() => Keyboard.current.rKey.wasPressedThisFrame));

            stateMachine.SetState(rest);

            harvestEvent.OnEventRaised += TransitionToHarvest;
        }

        private void Update()
        {
            stateMachine.Update();
        }

        private void OnDisable()
        {
            harvestEvent.OnEventRaised -= TransitionToHarvest;
        }

        private void TransitionToHarvest()
        {
            isHarvestReady = !isHarvestReady;
        }
    }
}
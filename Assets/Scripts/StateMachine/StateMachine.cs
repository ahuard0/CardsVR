using UnityEngine;

namespace CardsVR.States
{
    public class StateMachine : MonoBehaviour
    {
        public BaseState currentState;

        private void Start()
        {
            currentState = GetInitialState();
            if (currentState != null)
                currentState.Enter();
        }

        private void Update()
        {
            if (currentState != null)
                currentState.UpdateLogic();
        }

        private void LateUpdate()
        {
            if (currentState != null)
                currentState.UpdatePhysics();
        }

        public void ChangeState(BaseState newState)
        {
            if (currentState != null)
                currentState.Exit();
            currentState = newState;
            currentState.Enter();
        }

        protected virtual BaseState GetInitialState()
        {
            return null;
        }
    }
}

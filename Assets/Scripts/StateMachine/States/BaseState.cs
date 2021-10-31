using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardsVR.States
{
    public class BaseState
    {
        public string name;
        protected StateMachine stateMachine;

        public BaseState(string name, StateMachine stateMachine)
        {
            this.name = name;
            this.stateMachine = stateMachine;
        }

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void UpdateLogic() { }
        public virtual void UpdatePhysics() { }
    }

}

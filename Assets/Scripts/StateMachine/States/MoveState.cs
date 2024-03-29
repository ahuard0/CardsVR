namespace CardsVR.States
{
    public class MoveState : BaseState
    {
        private CardStateContext Context;

        public MoveState(CardStateContext stateMachine) : base("MoveState", stateMachine)
        {
            Context = stateMachine;
        }

        public override void Enter()
        {
            base.Enter();
        }

        public override void Exit()
        {
            base.Exit();
        }

        public override void UpdateLogic()
        {
            base.UpdateLogic();
        }

        public override void UpdatePhysics()
        {
            base.UpdatePhysics();

            Context.gameObject.transform.position = Context.Position;
            Context.gameObject.transform.rotation = Context.Rotation;
        }
    }
}

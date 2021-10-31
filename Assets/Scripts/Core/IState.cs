namespace CardsVR
{
    public interface IState<T>
    {
        public T Context { get; set; }
        public void Enter();
        public void Exit();
        public void UpdateLogic();
        public void UpdatePhysics();
    }
}

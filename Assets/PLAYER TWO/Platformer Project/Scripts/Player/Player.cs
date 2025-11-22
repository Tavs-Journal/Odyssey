public class Player : Entity<Player>
{
    public PlayerInputManager input {  get; protected set; }

    protected override void Awake()
    {
        base.Awake();
        InitializeInput();
    }

    protected virtual void InitializeInput() => input = GetComponent<PlayerInputManager>();
}

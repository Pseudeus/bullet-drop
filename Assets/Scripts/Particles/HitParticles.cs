using Godot;

public class HitParticles : Particles
{

    public async override void _Ready()
    {
        Emitting = true;
        await ToSignal(GetTree().CreateTimer(Lifetime), "timeout");
        QueueFree();
    }
}

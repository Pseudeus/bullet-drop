using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

[Tool]
public class ExplotionHandler : Spatial
{
    private List<Particles> _particles = new List<Particles>();
    private OmniLight _light;
    private Timer _time;

    public override void _Ready()
    {
        var childsImp = GetChildren();
        Node[] childs = new Node[childsImp.Count];
        childsImp.CopyTo(childs, 0);
        
        Parallel.ForEach(childs, i => Action(i));

        void Action(Node child)
        {
            if (child is Particles part) _particles.Add(part);
        }

        _light = GetNode<OmniLight>("OmniLight");
        _time = GetNode<Timer>("LightTimer");

        _light.Visible = false;
    }
    
    public async void EmitParticles()
    {
        _light.Visible = true;

        Parallel.ForEach(_particles, i => i.Emitting = true);

        await ToSignal(_time, "timeout");
        _light.Visible = false;
    }
}

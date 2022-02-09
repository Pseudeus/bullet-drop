using Godot;

public class PauseMenu : Control
{
    private Stats _stats;

    public override void _Ready()
    {
        _stats = GetNode<Stats>("../../Stats");

        GetNode<Button>("Background/MarginContainer/VBoxContainer/ResumeButton").Connect("button_down", this, nameof(OnResumeButtonButtonDown));
        GetNode<Button>("Background/MarginContainer/VBoxContainer/SettingsButton").Connect("button_down", this, nameof(OnSettingsButtonButtonDown));
        GetNode<Button>("Background/MarginContainer/VBoxContainer/QuitButton").Connect("button_down", this, nameof(OnQuitButtonButtonDown));
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey eventKey && eventKey.Scancode == (uint)KeyList.Escape && !eventKey.Echo && eventKey.Pressed && _stats.CurrentHealthPoints > 0)
        {
            var newPauseState = !GetTree().Paused;
            GetTree().Paused = newPauseState;
            Visible = newPauseState;
            Input.SetMouseMode(newPauseState ? Input.MouseMode.Visible : Input.MouseMode.Captured);
        }
    }

    private async void OnResumeButtonButtonDown()
    {   
        await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
        Visible = false;
        GetTree().Paused = false;
        Input.SetMouseMode(Input.MouseMode.Captured);
    }

    private void OnSettingsButtonButtonDown()
    {
        GD.Print("Settings.");
    }

    private void OnQuitButtonButtonDown()
    {
        GetTree().Quit();
    }
}

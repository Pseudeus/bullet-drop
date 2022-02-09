using Godot;

public class GameOver : Control
{
    public async override void _Ready()
    {
        GetNode<Button>("Background/MarginContainer/VBoxContainer/TryAgButton").Connect("button_down", this, nameof(OnTryAgainButtonButtonDown));
        GetNode<Button>("Background/MarginContainer/VBoxContainer/MainMenuButton").Connect("button_down", this, nameof(OnMainMenuButtonButtonDown));
        GetNode<Button>("Background/MarginContainer/VBoxContainer/QuitButton").Connect("button_down", this, nameof(OnQuitButtonButtonDown));

        await ToSignal(GetTree().CreateTimer(0.4f), "timeout");
        Input.SetMouseMode(Input.MouseMode.Visible);
        GetTree().Paused = true;
    }

    private async void OnTryAgainButtonButtonDown()
    {   
        await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
        GetTree().Paused = false;
        GetTree().ReloadCurrentScene();
        QueueFree();
    }

    private void OnMainMenuButtonButtonDown()
    {
        GD.Print("Main Menu");
    }

    private void OnQuitButtonButtonDown()
    {
        GetTree().Quit();
    }
}

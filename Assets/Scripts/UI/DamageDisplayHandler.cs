using Godot;

public class DamageDisplayHandler : Control
{
    [Export] private float impactTime = 0.7f;
    [Export] private float recuperationTmeFactor = 0.5f;
    [Export] private PackedScene gameOverScreen;

    private TextureRect _damageVignette;
    private IDamageable _playerHealth;
    private PlayerLifeBar _lifeBarUI;
    private KinematicBody _player;
    private Indicator _damageArrowIndicator;
    private Vector2 _playerPositionXZ = new Vector2();
    private byte _vignettePerHit = 127;
    private byte _alphaZero;
    private float _alphaState;

    public override void _Ready()
    {
        _damageVignette = GetNode<TextureRect>("DamagedTexture");
        _damageVignette.Visible = false;
        _damageVignette.Modulate = Color.Color8(255, 255, 255, 0);

        _playerHealth = GetTree().CurrentScene.GetNode<IDamageable>("Player/Stats");

        _damageArrowIndicator = GetNode<Indicator>("Indicator");
        _player = GetTree().CurrentScene.GetNode<KinematicBody>("Player");

        _lifeBarUI = GetNode<PlayerLifeBar>("TextureProgress");
        _lifeBarUI.HealthStats = _playerHealth;

        _playerHealth.ConnectToSignal("DamageTaken", this, nameof(OnStatsDamageTaken));
        _playerHealth.ConnectToSignal("CriticalDamageTaken", this, nameof(OnStatsCriticalDamageTaken));
    }

    public override void _Process(float delta)
    {
        if (_damageVignette.Modulate.a8 > _alphaZero)
        {
            _alphaState = Mathf.Lerp(_alphaState, _alphaZero, recuperationTmeFactor * delta);

            _damageVignette.Modulate = Color.Color8(255, 255, 255, (byte)_alphaState);
        }
        else if (_damageVignette.Modulate.a8 == 0)
        {
            _damageVignette.Visible = false;
        }
        GetPlayerPosition();
        _damageArrowIndicator.SetDamageCoordinates(_playerPositionXZ, _player.RotationDegrees.y);
    }

    private void OnStatsCriticalDamageTaken()
    {
        var gO = gameOverScreen.Instance<CanvasLayer>();

        AddChild(gO);
        gO.Owner = this;
    }

    private void OnStatsDamageTaken(ushort currentHealth, Vector3 damageOrigin)
    {
        ShowDamageImpact();
        CreateDamagePointer(new Vector2(damageOrigin.x, damageOrigin.z));
        _lifeBarUI.UpdateLifeBar(currentHealth);

        if (_alphaZero == 0 && currentHealth < _playerHealth.HealthPoints * 0.2f)
        {
            recuperationTmeFactor /= 2;
            _alphaZero = 64;
        }
    }

    private void GetPlayerPosition()
    {
        _playerPositionXZ.x = _player.GlobalTransform.origin.x;
        _playerPositionXZ.y = _player.GlobalTransform.origin.z;
    }

    public void CreateDamagePointer(Vector2 dmgOrigin)
    {
        _damageArrowIndicator.CreateDamagePointer(dmgOrigin, _playerPositionXZ, _player.RotationDegrees.y);
    }

    public void ShowDamageImpact()
    {
        if (_damageVignette.Modulate.a8 < 255)
        {
            _damageVignette.Visible = true;
            var a = _damageVignette.Modulate.a8 + _vignettePerHit;
            _damageVignette.Modulate = Color.Color8(255, 255, 255, (byte)Mathf.Clamp(a, 0, 255));
            _alphaState = Mathf.Clamp(a, 0, 255);
        }
    }
}

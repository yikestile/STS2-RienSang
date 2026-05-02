using Godot;
using System.Collections.Generic;
using System.Linq;

namespace RienSang;

public partial class PrescriptTransition : ColorRect
{
    private TextureRect? _iconTextureRect;
    private Label? _scrambleLabel;
    private AudioStreamPlayer? _audioPlayer;
    private AudioStreamWav? _startStream;
    private AudioStreamWav? _loopStream;
    
    private float _internalTime = 0f;
    private bool _isAnimating = false;
    private bool _hasPlayedLoop = false;
    private const float TargetDuration = 2.0f;
    
    private const string ScrambleChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+-=[]{}|;:,.<>?/";
    private string _settledString = "";
    private const int MaxChars = 40; 

    public PrescriptTransition()
    {
        Color = Colors.Black;
        SetAnchorsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;
        ZIndex = 2000; 
        Visible = false;
    }

    public override void _Ready()
    {
        _iconTextureRect = new TextureRect();
        _iconTextureRect.Texture = GD.Load<Texture2D>("res://RienSang/images/riensang/transitions/The_Index_Logo.png");
        _iconTextureRect.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        _iconTextureRect.StretchMode = TextureRect.StretchModeEnum.KeepCentered;
        _iconTextureRect.CustomMinimumSize = new Vector2(242, 284); 
        AddChild(_iconTextureRect);

        var font = GD.Load<Font>("res://RienSang/PixelMplus12-Regular.ttf");
        _scrambleLabel = new Label();
        _scrambleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _scrambleLabel.VerticalAlignment = VerticalAlignment.Center;
        _scrambleLabel.AddThemeFontOverride("font", font);
        _scrambleLabel.AddThemeFontSizeOverride("font_size", 36); 
        _scrambleLabel.AddThemeColorOverride("font_color", new Color("9ed1ff")); 
        _scrambleLabel.Text = ""; 
        AddChild(_scrambleLabel);

        _audioPlayer = new AudioStreamPlayer();
        _startStream = GD.Load<AudioStreamWav>("res://RienSang/images/riensang/transitions/index_message_2.wav");
        _loopStream = GD.Load<AudioStreamWav>("res://RienSang/images/riensang/transitions/index_message_1.wav");
        AddChild(_audioPlayer);

        UpdateLayout();
    }

    public override void _Notification(int what)
    {
        if (what == (int)NotificationResized)
        {
            UpdateLayout();
        }
    }

    private void UpdateLayout()
    {
        Vector2 screenCenter = GetViewportRect().Size / 2f;
        
        if (_iconTextureRect != null)
        {
            _iconTextureRect.Position = screenCenter - (_iconTextureRect.CustomMinimumSize / 2f);
            _iconTextureRect.Size = _iconTextureRect.CustomMinimumSize; 
        }

        if (_scrambleLabel != null && _iconTextureRect != null)
        {
            _scrambleLabel.Size = new Vector2(800, 40); 
            _scrambleLabel.Position = new Vector2(screenCenter.X - (_scrambleLabel.Size.X / 2f), _iconTextureRect.Position.Y + _iconTextureRect.Size.Y + 40);
        }
    }

    public void StartTransition()
    {
        if (_isAnimating) return;

        var rng = new RandomNumberGenerator();
        rng.Randomize();
        _settledString = "";
        for (int i = 0; i < MaxChars; i++)
        {
            _settledString += ScrambleChars[rng.RandiRange(0, ScrambleChars.Length - 1)];
        }

        _isAnimating = true;
        _internalTime = 0f;
        _hasPlayedLoop = false;
        Visible = true;

        if (_audioPlayer != null && _startStream != null)
        {
            _audioPlayer.Stream = _startStream;
            _audioPlayer.Play();
        }
    }

    public override void _Process(double delta)
    {
        if (!_isAnimating) return;

        _internalTime += (float)delta;
        float progress = Mathf.Clamp(_internalTime / TargetDuration, 0.0f, 1.0f);

        if (_audioPlayer != null && !_hasPlayedLoop)
        {
            if (!_audioPlayer.Playing && _loopStream != null)
            {
                _audioPlayer.Stream = _loopStream;
                _audioPlayer.Play();
                _hasPlayedLoop = true;
            }
        }

        if (_scrambleLabel != null)
        {
            float lockProgress = (progress - 0.5f) / 0.375f;
            int lockIndex = progress > 0.875f ? MaxChars : (progress > 0.5f ? (int)(MaxChars * lockProgress) : 0);

            string currentDisplay = "";
            var rng = new RandomNumberGenerator();
            rng.Seed = (ulong)(Time.GetTicksMsec() / 6); 

            for (int i = 0; i < MaxChars; i++)
            {
                if (i < lockIndex)
                {
                    currentDisplay += _settledString[i];
                }
                else
                {
                    currentDisplay += ScrambleChars[rng.RandiRange(0, ScrambleChars.Length - 1)];
                }
            }
            _scrambleLabel.Text = currentDisplay;
        }

        if (_internalTime >= TargetDuration)
        {
            _isAnimating = false;
            Visible = false;
            _audioPlayer?.Stop();
        }
    }
}
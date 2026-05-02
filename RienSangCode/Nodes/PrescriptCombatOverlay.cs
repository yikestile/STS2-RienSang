using Godot;
using System;
using System.Collections.Generic;
using System.Text;

namespace RienSang.RienSangCode.Nodes;

public partial class PrescriptCombatOverlay : Control
{
    private ColorRect? _backgroundRect;
    private Control? _textContainer; 
    private List<Sprite2D> _charSprites = new();

    private AudioStreamPlayer? _audioPlayer; 
    private AudioStreamPlayer? _audioPlayerBeep; 

    private AudioStreamWav? _startStream;
    private AudioStreamWav? _loopStream;

    private float _internalTime = 0f;
    private bool _isAnimating = false;
    private bool _startStreamPlayed = false; 

    private int _beepsPlayed = 0;
    private float _beepTimer = 0f;
    private const float BeepInterval = 0.2f; 

    private const float AnimationDuration = 2.0f;
    private const float ScramblePhaseEnd = 1.0f; 
    private const float SettlePhaseEnd = 1.5f;
    private const float BeepStartTime = 0.90f;

    private const string CharMap = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789.,_";
    
    private string _targetString = "";
    private const int MaxChars = 8; 

    private const int CharWidth = 5;
    private const int CharHeight = 7;
    private const int Columns = 13;
    private const float SpriteScale = 8.5f; 
    private const float CharSpacing = 1.0f; 

    public Vector2 Offset = new Vector2(0, 45);

    public PrescriptCombatOverlay()
    {
        MouseFilter = MouseFilterEnum.Ignore; 
        ZIndex = 20; 
        Visible = false;
        CustomMinimumSize = new Vector2(400, 80); 
        ClipContents = true; 
        Modulate = new Color(1, 1, 1, 1.0f);
    }

    public override void _Ready()
    {
        _backgroundRect = new ColorRect();
        _backgroundRect.Color = new Color(0, 0, 0, 0.10f);
        _backgroundRect.SetAnchorsPreset(LayoutPreset.FullRect); 
        AddChild(_backgroundRect);

        _textContainer = new Control();
        _textContainer.SetAnchorsPreset(LayoutPreset.Center); 
        _backgroundRect.AddChild(_textContainer); 

        _audioPlayer = new AudioStreamPlayer();
        _startStream = GD.Load<AudioStreamWav>("res://RienSang/images/riensang/transitions/index_message_1.wav"); 
        _loopStream = GD.Load<AudioStreamWav>("res://RienSang/images/riensang/transitions/index_message_2.wav");   
        if (_loopStream != null) _loopStream.LoopMode = AudioStreamWav.LoopModeEnum.Forward;
        AddChild(_audioPlayer);

        _audioPlayerBeep = new AudioStreamPlayer();
        var beepStream = GD.Load<AudioStreamWav>("res://RienSang/images/riensang/transitions/index_message_2.wav");
        if (beepStream != null) beepStream.LoopMode = AudioStreamWav.LoopModeEnum.Disabled;
        _audioPlayerBeep.Stream = beepStream;
        AddChild(_audioPlayerBeep);

        var tex = GD.Load<Texture2D>("res://RienSang/images/riensang/transitions/pixelfont.png");
        var shader = GD.Load<Shader>("res://RienSang/scenes/riensang/pixelfont.gdshader");

        for (int i = 0; i < MaxChars; i++)
        {
            var sprite = new Sprite2D();
            sprite.Texture = tex;
            sprite.RegionEnabled = true;
            sprite.Scale = new Vector2(SpriteScale, SpriteScale);
            sprite.TextureFilter = TextureFilterEnum.Nearest;
        
            var mat = new ShaderMaterial();
            mat.Shader = shader;
            sprite.Material = mat; 
            
            mat.SetShaderParameter("intensity", 7.0f);
            mat.SetShaderParameter("glow_color", new Color("425aaa"));
            
            float xPos = (i - (MaxChars / 2.0f) + 0.5f) * (CharWidth + CharSpacing) * SpriteScale;
            sprite.Position = new Vector2(Mathf.Round(xPos), 0);

            _textContainer.AddChild(sprite);
            _charSprites.Add(sprite);
        }
        UpdateLayout();
    }

    private void UpdateSpriteChar(int spriteIndex, char c)
    {
        int mapIndex = CharMap.IndexOf(c);

        if (mapIndex == -1) mapIndex = 0; 

        int col = mapIndex % Columns;
        int row = mapIndex / Columns;

        _charSprites[spriteIndex].RegionRect = new Rect2(
            (col * CharWidth) + 0.01f, 
            (row * CharHeight) + 0.01f, 
            CharWidth - 0.02f, 
            CharHeight - 0.02f
        );
    }

    public void Play(string text, bool isRandom)
    {
        if (_isAnimating) return;

        StringBuilder sb = new StringBuilder();
        foreach(char c in text)
        {
            if (CharMap.Contains(c)) 
            {
                sb.Append(c);
            }
        }
        
        string filtered = sb.ToString();

        if (filtered.Length < MaxChars)
        {
            var rng = new RandomNumberGenerator();
            while (filtered.Length < MaxChars)
            {
                filtered += CharMap[rng.RandiRange(0, CharMap.Length - 1)];
            }
        }

        _targetString = filtered.Substring(0, MaxChars); 
        
        _isAnimating = true;
        _internalTime = 0f;
        _startStreamPlayed = false; 
        _beepsPlayed = 0;
        _beepTimer = 0f;
        Visible = true;

        if (_audioPlayer != null && _startStream != null)
        {
            _audioPlayer.Stream = _startStream;
            _audioPlayer.Play();
            _startStreamPlayed = true;
        }
    }

    public override void _Process(double delta)
    {
        if (!_isAnimating) return;
        float fDelta = (float)delta;
        _internalTime += fDelta;

        // Audio Logic
        if (_audioPlayer != null)
        {
            if (_startStreamPlayed && _internalTime >= ScramblePhaseEnd && _loopStream != null && !_audioPlayer.Playing)
            {
                _audioPlayer.Stream = _loopStream;
                _audioPlayer.Play();
                _startStreamPlayed = false; 
            }
        }

        if (_internalTime >= BeepStartTime && _beepsPlayed < 6)
        {
            if (_beepsPlayed == 0 || _beepTimer >= BeepInterval)
            {
                _audioPlayerBeep?.Stop(); 
                _audioPlayerBeep?.Play();
                _beepsPlayed++;
                _beepTimer = 0f;
            }
            _beepTimer += fDelta;
        }

        // Animation / Scramble Logic
        var rng = new RandomNumberGenerator();
        rng.Seed = (ulong)(Time.GetTicksMsec() / 6); 
        float settleProgress = (_internalTime - ScramblePhaseEnd) / (SettlePhaseEnd - ScramblePhaseEnd);
        int lockIndex = (_internalTime < ScramblePhaseEnd) ? 0 : (_internalTime < SettlePhaseEnd) ? (int)(MaxChars * Mathf.Clamp(settleProgress, 0.0f, 1.0f)) : MaxChars;

        for (int i = 0; i < MaxChars; i++)
        {
            // Pick from CharMap only
            char charToDisplay = (i < lockIndex) ? _targetString[i] : CharMap[rng.RandiRange(0, CharMap.Length - 1)];
            UpdateSpriteChar(i, charToDisplay);
        }

        if (_internalTime >= AnimationDuration)
        {
            _isAnimating = false;
            Visible = false;
            _audioPlayer?.Stop();
            _audioPlayerBeep?.Stop();
            GD.Print($"Settle Result: '{_targetString}'");
        }
    }

    private void UpdateLayout()
    {
        float totalWidth = MaxChars * (CharWidth + CharSpacing) * SpriteScale;
        CustomMinimumSize = new Vector2(totalWidth + 40, (CharHeight * SpriteScale) + 20);
        Size = CustomMinimumSize;
    }
}
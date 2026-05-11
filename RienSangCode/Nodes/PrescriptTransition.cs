using Godot;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Saves;
using RienSang.RienSangCode;

namespace RienSang;

public partial class PrescriptTransition : ColorRect
{
    private TextureRect? _iconTextureRect;
    private Control? _textContainer; 
    
    private List<Sprite2D> _mainSprites = new();
    private List<Sprite2D> _redSprites = new();
    private List<Sprite2D> _blueSprites = new();

    private AudioStreamPlayer? _audioPlayer;
    private AudioStreamWav? _startStream;
    private AudioStreamWav? _loopStream;

    private float _internalTime = 0f;
    private bool _isAnimating = false;
    private bool _hasPlayedLoop = false;
    
    private const string CharMap = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789.,_";
    private string _settledString = "";
    private int _targetLength = 40; 
    private int _initialScrambleLength = 40;
    private const int MaxPossibleChars = 40;

    private const int CharWidth = 5;
    private const int CharHeight = 7;
    private const int Columns = 13;
    private const float SpriteScale = 6.0f; 
    private const float CharSpacing = 1.0f;

    private readonly Color PrescriptBlue = new Color("e1e7f1");

    public float TextCenterXOffset = 0f;

    public PrescriptTransition()
    {
        Color = Colors.Black;
        SetAnchorsPreset(LayoutPreset.FullRect);
        ZIndex = 100; 
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

        _textContainer = new Control();
        AddChild(_textContainer);

        var tex = GD.Load<Texture2D>("res://RienSang/images/riensang/transitions/pixelfont.png");

        for (int i = 0; i < MaxPossibleChars; i++)
        {
            float xPos = (i - (MaxPossibleChars / 2.0f) + 0.5f) * (CharWidth + CharSpacing) * SpriteScale;

            _blueSprites.Add(CreateCharSprite(tex, xPos + 2.0f, new Color(0.2f, 0.4f, 1.0f, 0.5f)));
            _redSprites.Add(CreateCharSprite(tex, xPos - 2.0f, new Color(1.0f, 0.2f, 0.2f, 0.5f)));
            _mainSprites.Add(CreateCharSprite(tex, xPos, PrescriptBlue));
        }

        _audioPlayer = new AudioStreamPlayer();
        _startStream = GD.Load<AudioStreamWav>("res://RienSang/images/riensang/transitions/index_message_2.wav");
        _loopStream = GD.Load<AudioStreamWav>("res://RienSang/images/riensang/transitions/index_message_1.wav");
        AddChild(_audioPlayer);

        UpdateLayout();
    }
    
    private void SetAudioVolume()
    {
        float localVolume = 0.3f;
        
        var settings = SaveManager.Instance.SettingsSave;
        float gameMaster = settings.VolumeMaster;
        float gameSfx = settings.VolumeSfx;
        float finalLinear = localVolume * gameMaster * gameSfx;

        float dbVolume = finalLinear > 0.0001f ? Mathf.LinearToDb(finalLinear) : -80.0f;

        if (_audioPlayer != null) _audioPlayer.VolumeDb = dbVolume;
    }

    private Sprite2D CreateCharSprite(Texture2D tex, float x, Color color)
    {
        var s = new Sprite2D();
        s.Texture = tex;
        s.RegionEnabled = true;
        s.Scale = new Vector2(SpriteScale, SpriteScale);
        s.TextureFilter = TextureFilterEnum.Nearest;
        s.Modulate = color;
        s.Position = new Vector2(x, 0);
        _textContainer?.AddChild(s);
        return s;
    }

    public void StartTransition()
    {
        if (_isAnimating) return;
        var rng = new RandomNumberGenerator();
        rng.Randomize();

        _targetLength = rng.RandiRange(10, 30); 
        _initialScrambleLength = rng.RandiRange(15, 40);
        
        _settledString = "";
        for (int i = 0; i < MaxPossibleChars; i++)
            _settledString += CharMap[rng.RandiRange(0, CharMap.Length - 1)];

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
        float progress = Mathf.Clamp(_internalTime / 2.0f, 0.0f, 1.0f);
        SetAudioVolume();
        
        if (_audioPlayer != null && !_hasPlayedLoop && !_audioPlayer.Playing && _loopStream != null)
        {
            _audioPlayer.Stream = _loopStream;
            _audioPlayer.Play();
            _hasPlayedLoop = true;
        }

        float lockProgress = (progress - 0.5f) / 0.4f;
        int lockCount = progress > 0.9f ? MaxPossibleChars : (progress > 0.5f ? (int)(MaxPossibleChars * lockProgress) : 0);

        int currentVisibleCount = (int)Mathf.Lerp(_initialScrambleLength, _targetLength, progress);
        
        int startSlot = (MaxPossibleChars - currentVisibleCount) / 2;
        int endSlot = startSlot + currentVisibleCount;

        int targetStart = (MaxPossibleChars - _targetLength) / 2;
        int targetEnd = targetStart + _targetLength;

        var rng = new RandomNumberGenerator();
        rng.Seed = (ulong)(Time.GetTicksMsec() / 8); 

        for (int i = 0; i < MaxPossibleChars; i++)
        {

            bool isVisible = (progress > 0.9f) 
                ? (i >= targetStart && i < targetEnd) 
                : (i >= startSlot && i < endSlot);
            
            _mainSprites[i].Visible = isVisible;
            _redSprites[i].Visible = isVisible;
            _blueSprites[i].Visible = isVisible;

            if (isVisible)
            {
                char charToDraw = (i < lockCount) ? _settledString[i] : CharMap[rng.RandiRange(0, CharMap.Length - 1)];
                
                int mapIndex = CharMap.IndexOf(charToDraw);
                if (mapIndex == -1) mapIndex = 0;
                Rect2 region = new Rect2((mapIndex % Columns) * CharWidth + 0.01f, (mapIndex / Columns) * CharHeight + 0.01f, CharWidth - 0.02f, CharHeight - 0.02f);
                
                _mainSprites[i].RegionRect = region;
                _redSprites[i].RegionRect = region;
                _blueSprites[i].RegionRect = region;
            }
        }

        if (_internalTime >= 2.0f) { _isAnimating = false; Visible = false; }
    }

    private void UpdateLayout()
    {
        Vector2 screenCenter = GetViewportRect().Size / 2f;
        if (_iconTextureRect != null)
            _iconTextureRect.Position = screenCenter - (_iconTextureRect.CustomMinimumSize / 2f);

        if (_textContainer != null && _iconTextureRect != null)
        {
            _textContainer.Position = new Vector2(screenCenter.X + TextCenterXOffset, _iconTextureRect.Position.Y + _iconTextureRect.Size.Y + 60);
        }
    }

    public override void _Notification(int what)
    {
        if (what == (int)NotificationResized) UpdateLayout();
    }
}
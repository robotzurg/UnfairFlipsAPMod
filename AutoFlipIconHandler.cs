using System.IO;
using BepInEx;
using UnityEngine;
using UnityEngine.UI;

namespace UnfairFlipsAPMod;

public class AutoFlipIconHandler
{
    private static Sprite _autoFlipOnSprite;
    private static Sprite _autoFlipOffSprite;
    private static GameObject _autoFlipButtonObject;
    public static bool IsVisible
    {
        get => _autoFlipButtonObject != null && _autoFlipButtonObject.activeSelf;
        set
        {
            if (_autoFlipButtonObject != null && _autoFlipButtonObject.activeSelf != value)
                _autoFlipButtonObject.SetActive(value);
        }
    }

    public static bool IsAutoFlipEnabled;
    
    public static void CreateButton() {
        var reference = Object.FindObjectOfType<AudioButton>().gameObject;
        var referenceParent = reference.transform.parent;
        
        _autoFlipButtonObject = Object.Instantiate(reference, referenceParent);
        _autoFlipButtonObject.name = "AutoFlipButton";
        
        Object.Destroy(_autoFlipButtonObject.GetComponent<AudioButton>());
        
        var image = _autoFlipButtonObject.GetComponent<Image>();
        var imagePath = Path.Combine(UnfairFlipsAPMod.PluginDir, "AutoFlipIcon.png");
        var imageData = File.ReadAllBytes(imagePath);
        var texture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
        texture.LoadImage(imageData);
        texture.filterMode = FilterMode.Point;
        _autoFlipOnSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, 33f, 34f),
            new Vector2(0.5f, 0.5f),
            100f
        );
        
        _autoFlipOffSprite = Sprite.Create(
            texture,
            new Rect(34f, 0f, 34f, 34f),
            new Vector2(0.5f, 0.5f),
            100f
        );
        
        image.sprite = _autoFlipOffSprite;
        image.preserveAspect = true;
        
        IsAutoFlipEnabled = false;

        _autoFlipButtonObject.transform.localPosition = new Vector3(250f, 400f, 0f);
        var autoFlipButton = _autoFlipButtonObject.GetComponent<Button>();
        autoFlipButton.onClick = new Button.ButtonClickedEvent();
        autoFlipButton.onClick.AddListener(() =>
        {
            IsAutoFlipEnabled = !IsAutoFlipEnabled;
            if (IsAutoFlipEnabled)
            {
                GameHandler.QueueNextAutoFlip();
                UnfairFlipsAPMod.ArchipelagoHandler.DisplayAutoFlipMsg();
            }
            image.sprite = IsAutoFlipEnabled ? _autoFlipOnSprite : _autoFlipOffSprite;
        });
        
        IsVisible = UnfairFlipsAPMod.SaveDataHandler?.SaveData?.HasAutoFlip ?? false;
    }
}
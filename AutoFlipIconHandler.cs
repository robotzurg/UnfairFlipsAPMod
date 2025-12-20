using System.IO;
using BepInEx;
using UnityEngine;
using UnityEngine.UI;

namespace UnfairFlipsAPMod;

public class AutoFlipIconHandler
{
    private static Sprite _autoFlipOnSprite;
    private static Sprite _autoFlipOffSprite;
    public static bool IsAutoFlipEnabled;
    
    public static void CreateButton() {
        var reference = Object.FindObjectOfType<AudioButton>().gameObject;
        var referenceParent = reference.transform.parent;
        
        var autoFlipButtonObject = Object.Instantiate(reference, referenceParent);
        autoFlipButtonObject.name = "AutoFlipButton";
        
        Object.Destroy(autoFlipButtonObject.GetComponent<AudioButton>());
        
        var image = autoFlipButtonObject.GetComponent<Image>();
        var imagePath = Path.Combine(Paths.PluginPath, "UnfairFlipsAPMod/AutoFlipIcon.png");
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

        autoFlipButtonObject.transform.localPosition = new Vector3(250f, 400f, 0f);
        var autoFlipButton = autoFlipButtonObject.GetComponent<Button>();
        autoFlipButton.onClick = new Button.ButtonClickedEvent();
        autoFlipButton.onClick.AddListener(() =>
        {
            IsAutoFlipEnabled = !IsAutoFlipEnabled;
            if (IsAutoFlipEnabled)
                GameHandler.QueueNextAutoFlip();
            image.sprite = IsAutoFlipEnabled ? _autoFlipOnSprite : _autoFlipOffSprite;
        });
    }
}
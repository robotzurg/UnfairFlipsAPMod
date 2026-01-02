using System.IO;
using BepInEx;
using UnityEngine;
using UnityEngine.UI;

namespace UnfairFlipsAPMod;

public class ReceivedItemsButtonHandler
{
    private static Sprite _buttonSprite;
    
    public static void CreateButton() {
        var reference = Object.FindObjectOfType<AudioButton>().gameObject;
        var referenceParent = reference.transform.parent;
        
        var receivedItemsButtonObject = Object.Instantiate(reference, referenceParent);
        receivedItemsButtonObject.name = "ReceivedItemsButton";
        
        Object.Destroy(receivedItemsButtonObject.GetComponent<AudioButton>());
        var image = receivedItemsButtonObject.GetComponent<Image>();
        
        var imagePath = Path.Combine(Paths.PluginPath, "UnfairFlipsAPMod/ArchipelagoLogo.png");
        if (File.Exists(imagePath))
        {
            var imageData = File.ReadAllBytes(imagePath);
            var texture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            texture.LoadImage(imageData);
            texture.filterMode = FilterMode.Point;
            _buttonSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f
            );
            image.sprite = _buttonSprite;
            image.color = Color.white;
            image.preserveAspect = true;
        }

        receivedItemsButtonObject.transform.localPosition = new Vector3(250f, 330f, 0f);
        var button = receivedItemsButtonObject.GetComponent<Button>();
        button.onClick = new Button.ButtonClickedEvent();
        button.onClick.AddListener(() =>
        {
            UnfairFlipsAPMod.ArchipelagoHandler.DisplayItemCounts();
        });
    }
}

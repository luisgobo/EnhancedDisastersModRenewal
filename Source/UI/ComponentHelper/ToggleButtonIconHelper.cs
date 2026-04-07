using System;
using System.Reflection;
using ColossalFramework.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NaturalDisastersRenewal.UI.ComponentHelper;

public static class ToggleButtonIconHelper
{
    private const string IconSpriteComponentName = "NaturalDisastersRenewalToggleButtonIcon";
    private const string ActiveAtlasName = "NaturalDisastersRenewal.ToggleButton.ActiveAtlas";
    private const string InactiveAtlasName = "NaturalDisastersRenewal.ToggleButton.InactiveAtlas";
    private const string ActiveSpriteName = "NaturalDisastersRenewal.ToggleButton.Active";
    private const string InactiveSpriteName = "NaturalDisastersRenewal.ToggleButton.Inactive";
    private const string ActiveResourceName = "NaturalDisastersRenewal.Resources.Images.icon-active.png";
    private const string InactiveResourceName = "NaturalDisastersRenewal.Resources.Images.icon-inactive.png";
    private const float IconSize = 48f;

    private static UITextureAtlas _activeAtlas;
    private static UITextureAtlas _inactiveAtlas;

    public static bool Apply(UIButton button, bool isActive)
    {
        if (button == null)
            return false;

        var sprite = EnsureIconSprite(button);
        if (sprite == null)
            return false;

        var atlas = isActive ? GetAtlas(true) : GetAtlas(false);
        var spriteName = isActive ? ActiveSpriteName : InactiveSpriteName;
        if (atlas == null)
        {
            Hide(button);
            return false;
        }

        ClearButtonSprites(button);

        sprite.atlas = atlas;
        sprite.spriteName = spriteName;
        sprite.size = new Vector2(IconSize, IconSize);
        sprite.relativePosition = new Vector3((button.width - IconSize) * 0.5f, (button.height - IconSize) * 0.5f);
        sprite.isVisible = true;
        return true;
    }

    public static void Hide(UIButton button)
    {
        if (button == null)
            return;

        var sprite = button.Find(IconSpriteComponentName, typeof(UISprite)) as UISprite;
        if (sprite != null)
            sprite.isVisible = false;
    }

    private static UISprite EnsureIconSprite(UIButton button)
    {
        var existing = button.Find(IconSpriteComponentName, typeof(UISprite)) as UISprite;
        if (existing != null)
            return existing;

        var sprite = button.AddUIComponent<UISprite>();
        sprite.name = IconSpriteComponentName;
        sprite.size = new Vector2(IconSize, IconSize);
        sprite.relativePosition = new Vector3((button.width - IconSize) * 0.5f, (button.height - IconSize) * 0.5f);
        sprite.isInteractive = false;
        return sprite;
    }

    private static void ClearButtonSprites(UIButton button)
    {
        button.normalBgSprite = string.Empty;
        button.hoveredBgSprite = string.Empty;
        button.focusedBgSprite = string.Empty;
        button.pressedBgSprite = string.Empty;
        button.disabledBgSprite = string.Empty;
        button.normalFgSprite = string.Empty;
        button.hoveredFgSprite = string.Empty;
        button.focusedFgSprite = string.Empty;
        button.pressedFgSprite = string.Empty;
        button.disabledFgSprite = string.Empty;
    }

    private static UITextureAtlas GetAtlas(bool active)
    {
        if (active)
        {
            if (_activeAtlas == null)
                _activeAtlas = CreateAtlas(ActiveAtlasName, ActiveSpriteName, ActiveResourceName);

            return _activeAtlas;
        }

        if (_inactiveAtlas == null)
            _inactiveAtlas = CreateAtlas(InactiveAtlasName, InactiveSpriteName, InactiveResourceName);

        return _inactiveAtlas;
    }

    private static UITextureAtlas CreateAtlas(string atlasName, string spriteName, string resourceName)
    {
        try
        {
            var texture = GetTextureFromAssemblyManifest(resourceName);
            if (texture == null)
                return null;

            var shader = Shader.Find("UI/Default UI Shader");
            if (shader == null)
            {
                Debug.LogError("Unable to find the default UI shader for toggle button icon.");
                return null;
            }

            var material = new Material(shader)
            {
                mainTexture = texture
            };

            var atlas = ScriptableObject.CreateInstance<UITextureAtlas>();
            atlas.name = atlasName;
            atlas.material = material;
            atlas.AddSprite(new UITextureAtlas.SpriteInfo
            {
                name = spriteName,
                texture = texture,
                region = new Rect(0f, 0f, 1f, 1f)
            });

            return atlas;
        }
        catch (Exception ex)
        {
            Debug.LogError("Error creating toggle button atlas '" + atlasName + "': " + ex);
            return null;
        }
    }

    private static Texture2D GetTextureFromAssemblyManifest(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using (var stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream == null)
            {
                Debug.LogError("Toggle button icon resource not found: " + resourceName);
                return null;
            }

            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);

            var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            texture.filterMode = FilterMode.Bilinear;

            if (!texture.LoadImage(buffer))
            {
                Object.Destroy(texture);
                Debug.LogError("Unable to load toggle button icon resource: " + resourceName);
                return null;
            }

            texture.wrapMode = TextureWrapMode.Clamp;
            texture.Apply();
            return texture;
        }
    }
}
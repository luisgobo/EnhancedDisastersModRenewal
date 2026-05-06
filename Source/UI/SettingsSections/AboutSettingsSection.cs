using System.Reflection;
using ColossalFramework.UI;
using NaturalDisastersRenewal.Common;
using NaturalDisastersRenewal.UI.ComponentHelper;
using NaturalDisastersRenewal.UI.Extensions;
using UnityEngine;

namespace NaturalDisastersRenewal.UI.SettingsSections
{
    internal sealed class AboutSettingsSection
    {
        private const string AboutImageResourceName = "NaturalDisastersRenewal.Resources.Images.logo_1.3.0_3.png";

        private const string AboutCompatibilityImageResourceName =
            "NaturalDisastersRenewal.Resources.Images.mod_compatibility.png";

        private const string AboutGithubImageResourceName =
            "NaturalDisastersRenewal.Resources.Images.github.png";

        private const string AboutDonationImageResourceName =
            "NaturalDisastersRenewal.Resources.Images.kofi_dark.png";

        private const string AboutSteamImageResourceName =
            "NaturalDisastersRenewal.Resources.Images.steam.png";

        private const string AboutGitUrl = "https://github.com/luisgobo/EnhancedDisastersModRenewal";
        private const string AboutDonationUrl = "https://ko-fi.com/luisgobo2986";
        private const string AboutSteamModUrl = "https://steamcommunity.com/sharedfiles/filedetails/?id=2957578256";
        private const string AboutLastUpdatedDate = "2026-04-09";

        public void Build(ref UIHelper helper)
        {
            var aboutGroup = helper.AddGroup(LocalizationService.Get("settings.about"));
            helper.AddSpacing();

            var aboutUiHelper = aboutGroup as UIHelper;
            if (aboutUiHelper == null)
                return;

            var aboutPanel = aboutUiHelper.self as UIPanel;
            if (aboutPanel == null)
                return;

            var contentWidth = aboutPanel.width > 0f ? aboutPanel.width - 20f : 680f;
            var image = CreateAboutImageSprite(
                aboutPanel,
                AboutImageResourceName,
                Mathf.Min(contentWidth, 450f),
                new Vector3(0f, 0f));
            var imageBottom = image != null ? image.relativePosition.y + image.height : 0f;

            var titleLabel = aboutPanel.AddUIComponent<UILabel>();
            titleLabel.text = LocalizationService.Get("about.mod_name");
            titleLabel.textScale = 1.1f;
            titleLabel.textColor = UIStyleHelper.PrimaryTextColor;
            titleLabel.relativePosition = new Vector3(0f, imageBottom + 12f);

            var versionLabel = aboutPanel.AddUIComponent<UILabel>();
            versionLabel.text = LocalizationService.Format("about.version", GetModVersion());
            versionLabel.textScale = 0.9f;
            versionLabel.textColor = UIStyleHelper.SecondaryTextColor;
            versionLabel.relativePosition = new Vector3(0f, titleLabel.relativePosition.y + 28f);

            var updatedLabel = aboutPanel.AddUIComponent<UILabel>();
            updatedLabel.text = LocalizationService.Format("about.updated", AboutLastUpdatedDate);
            updatedLabel.textScale = 0.9f;
            updatedLabel.textColor = UIStyleHelper.SecondaryTextColor;
            updatedLabel.relativePosition = new Vector3(0f, versionLabel.relativePosition.y + 22f);

            var compatibilityImage = CreateAboutImageSprite(
                aboutPanel,
                AboutCompatibilityImageResourceName,
                contentWidth,
                new Vector3(0f, updatedLabel.relativePosition.y + 34f));

            var compatibilityBottom = compatibilityImage != null
                ? compatibilityImage.relativePosition.y + compatibilityImage.height
                : updatedLabel.relativePosition.y + 34f;

            CreateAboutLinkImage(
                aboutPanel,
                AboutGithubImageResourceName,
                180f,
                new Vector3(0f, compatibilityBottom + 18f),
                AboutGitUrl,
                LocalizationService.Get("about.git_link"));

            CreateAboutLinkImage(
                aboutPanel,
                AboutSteamImageResourceName,
                180f,
                new Vector3(0f, compatibilityBottom + 62f),
                AboutSteamModUrl,
                LocalizationService.Get("about.steam_link"));

            CreateAboutLinkImage(
                aboutPanel,
                AboutDonationImageResourceName,
                220f,
                new Vector3(0f, compatibilityBottom + 62f),
                AboutDonationUrl,
                LocalizationService.Get("about.donate_link"));
        }

        private static UITextureSprite CreateAboutImageSprite(
            UIPanel parentPanel,
            string resourceName,
            float maxWidth,
            Vector3 position)
        {
            var texture = LoadEmbeddedTexture(resourceName);
            if (texture == null)
                return null;

            var sprite = parentPanel.AddUIComponent<UITextureSprite>();
            sprite.texture = texture;
            sprite.relativePosition = position;
            sprite.color = Color.white;

            var width = texture.width;
            var height = texture.height;
            var scale = width > maxWidth ? maxWidth / width : 1f;
            sprite.size = new Vector2(width * scale, height * scale);
            return sprite;
        }

        private static UITextureSprite CreateAboutLinkImage(
            UIPanel parentPanel,
            string resourceName,
            float maxWidth,
            Vector3 position,
            string url,
            string tooltip)
        {
            var sprite = CreateAboutImageSprite(parentPanel, resourceName, maxWidth, position);
            if (sprite == null)
                return null;

            sprite.isInteractive = true;
            sprite.tooltip = tooltip;
            sprite.eventClick += delegate { Application.OpenURL(url); };
            return sprite;
        }

        private static string GetModVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            if (version == null)
                return "1.0.0";

            return version.Build > 0
                ? string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build)
                : string.Format("{0}.{1}", version.Major, version.Minor);
        }

        private static Texture2D LoadEmbeddedTexture(string resourceName)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    return null;

                var buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);

                var texture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                texture.filterMode = FilterMode.Bilinear;
                texture.LoadImage(buffer);
                texture.Apply();
                return texture;
            }
        }
    }
}
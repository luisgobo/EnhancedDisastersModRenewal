using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using ColossalFramework.UI;
using UnityEngine;

namespace NaturalDisastersRenewal.UI.ComponentHelper
{
    public static class ActionButtonHelper
    {
        private const int SvgIconTextureSize = 64;
        private const int SvgIconSupersampling = 3;
        private static readonly Dictionary<string, UITextureAtlas> SvgIconAtlases = new Dictionary<string, UITextureAtlas>();

        private sealed class SvgPathShape
        {
            public readonly List<List<Vector2>> SubPaths;
            public readonly Color FillColor;

            public SvgPathShape(List<List<Vector2>> subPaths, Color fillColor)
            {
                SubPaths = subPaths;
                FillColor = fillColor;
            }
        }

        public static void CreateTextButton(UIComponent parent, string name, string text, Vector3 position,
            Vector2 size, string tooltip, MouseEventHandler clickHandler, Color? textColor,
            string normalBgSprite, string hoveredBgSprite, string pressedBgSprite, RectOffset textPadding)
        {
            var button = parent.AddUIComponent<UIButton>();
            button.name = name;
            button.relativePosition = position;
            button.size = size;
            button.normalBgSprite = normalBgSprite;
            button.hoveredBgSprite = hoveredBgSprite;
            button.pressedBgSprite = pressedBgSprite;
            button.disabledBgSprite = normalBgSprite;
            button.focusedBgSprite = hoveredBgSprite;
            button.focusedColor = UIStyleHelper.SurfaceAltColor;
            button.color = UIStyleHelper.MutedColor;
            button.textColor = textColor ?? UIStyleHelper.PrimaryTextColor;
            button.disabledTextColor = UIStyleHelper.SecondaryTextColor;
            button.focusedTextColor = UIStyleHelper.PrimaryTextColor;
            button.hoveredTextColor = UIStyleHelper.PrimaryTextColor;
            button.pressedTextColor = UIStyleHelper.PrimaryTextColor;
            button.textScale = 0.85f;
            button.textHorizontalAlignment = UIHorizontalAlignment.Center;
            button.textVerticalAlignment = UIVerticalAlignment.Middle;
            button.text = text;
            button.tooltip = tooltip;
            if (textPadding != null) button.textPadding = textPadding;
            if (clickHandler != null) button.eventClick += clickHandler;
        }

        public static void CreateSvgIconButton(UIComponent parent, string name, string resourceName,
            string atlasName, string spriteName, Vector3 position, Vector2 size, string tooltip,
            MouseEventHandler clickHandler)
        {
            var button = parent.AddUIComponent<UIButton>();
            button.name = name;
            button.relativePosition = position;
            button.size = size;
            button.tooltip = tooltip;
            button.text = string.Empty;
            UIStyleHelper.ApplyActionButtonStyle(button);
            button.textPadding = new RectOffset(0, 0, 0, 0);

            if (clickHandler != null)
                button.eventClick += clickHandler;

            var atlas = GetSvgIconAtlas(resourceName, atlasName, spriteName);
            if (atlas == null)
                return;

            var iconSize = Mathf.Max(18f, Mathf.Min(size.y - 8f, size.x - 18f));
            var icon = button.AddUIComponent<UISprite>();
            icon.name = name + "Icon";
            icon.atlas = atlas;
            icon.spriteName = spriteName;
            icon.size = new Vector2(iconSize, iconSize);
            icon.relativePosition = new Vector3((size.x - icon.width) * 0.5f, (size.y - icon.height) * 0.5f);
            icon.isInteractive = false;
        }

        private static UITextureAtlas GetSvgIconAtlas(string resourceName, string atlasName, string spriteName)
        {
            UITextureAtlas cachedAtlas;
            if (SvgIconAtlases.TryGetValue(resourceName, out cachedAtlas))
                return cachedAtlas;

            var texture = LoadSvgIconTexture(resourceName);
            var shader = Shader.Find("UI/Default UI Shader");
            if (texture == null || shader == null)
                return null;

            var material = new Material(shader);
            material.mainTexture = texture;

            var atlas = ScriptableObject.CreateInstance<UITextureAtlas>();
            atlas.name = atlasName;
            atlas.material = material;
            atlas.AddSprite(new UITextureAtlas.SpriteInfo
            {
                name = spriteName,
                texture = texture,
                region = new Rect(0f, 0f, 1f, 1f)
            });

            SvgIconAtlases[resourceName] = atlas;
            return atlas;
        }

        private static Texture2D LoadSvgIconTexture(string resourceName)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        Debug.LogError("Action button icon resource not found: " + resourceName);
                        return null;
                    }

                    using (var reader = new StreamReader(stream))
                    {
                        return RasterizeSvgIcon(reader.ReadToEnd(), resourceName);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Error loading action button icon '" + resourceName + "': " + ex);
                return null;
            }
        }

        private static Texture2D RasterizeSvgIcon(string svg, string resourceName)
        {
            var viewBox = ParseViewBox(svg);
            var shapes = ParseSvgShapes(svg);
            if (shapes.Count == 0)
            {
                Debug.LogError("No SVG paths found for action button icon: " + resourceName);
                return null;
            }

            var texture = new Texture2D(SvgIconTextureSize, SvgIconTextureSize, TextureFormat.ARGB32, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            for (var y = 0; y < SvgIconTextureSize; y++)
            {
                for (var x = 0; x < SvgIconTextureSize; x++)
                {
                    var totalSamples = SvgIconSupersampling * SvgIconSupersampling;
                    var red = 0f;
                    var green = 0f;
                    var blue = 0f;
                    var alpha = 0f;

                    for (var sampleY = 0; sampleY < SvgIconSupersampling; sampleY++)
                    {
                        for (var sampleX = 0; sampleX < SvgIconSupersampling; sampleX++)
                        {
                            var sampleOffsetX = (sampleX + 0.5f) / SvgIconSupersampling;
                            var sampleOffsetY = (sampleY + 0.5f) / SvgIconSupersampling;
                            var svgX = viewBox.x + (x + sampleOffsetX) / SvgIconTextureSize * viewBox.width;
                            var svgY = viewBox.y + (SvgIconTextureSize - y - sampleOffsetY) /
                                SvgIconTextureSize * viewBox.height;

                            var sampleColor = Color.clear;
                            var samplePoint = new Vector2(svgX, svgY);
                            for (var shapeIndex = 0; shapeIndex < shapes.Count; shapeIndex++)
                            {
                                var shape = shapes[shapeIndex];
                                if (IsPointInsideEvenOdd(shape.SubPaths, samplePoint))
                                    sampleColor = shape.FillColor;
                            }

                            red += sampleColor.r;
                            green += sampleColor.g;
                            blue += sampleColor.b;
                            alpha += sampleColor.a;
                        }
                    }

                    if (alpha <= 0f)
                    {
                        texture.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    texture.SetPixel(x, y,
                        new Color(red / totalSamples, green / totalSamples, blue / totalSamples,
                            alpha / totalSamples));
                }
            }

            texture.Apply();
            return texture;
        }

        private static Rect ParseViewBox(string svg)
        {
            var match = Regex.Match(svg, "viewBox\\s*=\\s*\"([^\"]+)\"", RegexOptions.IgnoreCase);
            if (!match.Success)
                return new Rect(0f, 0f, 640f, 640f);

            var values = Regex.Matches(match.Groups[1].Value, "-?\\d+(?:\\.\\d+)?");
            if (values.Count < 4)
                return new Rect(0f, 0f, 640f, 640f);

            return new Rect(
                ParseFloat(values[0].Value),
                ParseFloat(values[1].Value),
                ParseFloat(values[2].Value),
                ParseFloat(values[3].Value));
        }

        private static Color ParseFillColor(string svg)
        {
            var rgbMatch = Regex.Match(svg, "fill\\s*=\\s*\"rgb\\((\\d+),\\s*(\\d+),\\s*(\\d+)\\)\"",
                RegexOptions.IgnoreCase);
            if (rgbMatch.Success)
            {
                return new Color(
                    ParseByte(rgbMatch.Groups[1].Value) / 255f,
                    ParseByte(rgbMatch.Groups[2].Value) / 255f,
                    ParseByte(rgbMatch.Groups[3].Value) / 255f,
                    1f);
            }

            var hexMatch = Regex.Match(svg, "fill\\s*=\\s*\"#([0-9a-f]{6})\"", RegexOptions.IgnoreCase);
            if (!hexMatch.Success)
                return Color.white;

            var hex = hexMatch.Groups[1].Value;
            return new Color(
                Convert.ToInt32(hex.Substring(0, 2), 16) / 255f,
                Convert.ToInt32(hex.Substring(2, 2), 16) / 255f,
                Convert.ToInt32(hex.Substring(4, 2), 16) / 255f,
                1f);
        }

        private static List<SvgPathShape> ParseSvgShapes(string svg)
        {
            var result = new List<SvgPathShape>();
            var classFillColors = ParseClassFillColors(svg);
            var fallbackColor = ParseFillColor(svg);
            var pathMatches = Regex.Matches(svg, "<path\\b[^>]*>", RegexOptions.IgnoreCase);

            foreach (Match pathMatch in pathMatches)
            {
                var pathTag = pathMatch.Value;
                var dataMatch = Regex.Match(pathTag, "\\sd\\s*=\\s*\"([^\"]+)\"", RegexOptions.IgnoreCase);
                if (!dataMatch.Success)
                    continue;

                var subPaths = ParseSvgPathData(dataMatch.Groups[1].Value);
                if (subPaths.Count == 0)
                    continue;

                result.Add(new SvgPathShape(subPaths, ResolvePathFillColor(pathTag, classFillColors, fallbackColor)));
            }

            return result;
        }

        private static Dictionary<string, Color> ParseClassFillColors(string svg)
        {
            var result = new Dictionary<string, Color>();
            var styleMatches = Regex.Matches(svg, "\\.([A-Za-z0-9_-]+)\\s*\\{[^}]*fill\\s*:\\s*([^;\\s}]+)",
                RegexOptions.IgnoreCase);

            foreach (Match styleMatch in styleMatches)
            {
                Color color;
                if (TryParseColor(styleMatch.Groups[2].Value, out color))
                    result[styleMatch.Groups[1].Value] = color;
            }

            return result;
        }

        private static Color ResolvePathFillColor(string pathTag, Dictionary<string, Color> classFillColors,
            Color fallbackColor)
        {
            var fillMatch = Regex.Match(pathTag, "\\sfill\\s*=\\s*\"([^\"]+)\"", RegexOptions.IgnoreCase);
            if (fillMatch.Success)
            {
                Color color;
                if (TryParseColor(fillMatch.Groups[1].Value, out color))
                    return color;
            }

            var classMatch = Regex.Match(pathTag, "\\sclass\\s*=\\s*\"([^\"]+)\"", RegexOptions.IgnoreCase);
            if (classMatch.Success)
            {
                var classes = classMatch.Groups[1].Value.Split(new[] { ' ', '\t', '\r', '\n' },
                    StringSplitOptions.RemoveEmptyEntries);
                for (var i = 0; i < classes.Length; i++)
                {
                    Color color;
                    if (classFillColors.TryGetValue(classes[i], out color))
                        return color;
                }
            }

            return fallbackColor;
        }

        private static bool TryParseColor(string value, out Color color)
        {
            var rgbMatch = Regex.Match(value, "rgb\\((\\d+),\\s*(\\d+),\\s*(\\d+)\\)", RegexOptions.IgnoreCase);
            if (rgbMatch.Success)
            {
                color = new Color(
                    ParseByte(rgbMatch.Groups[1].Value) / 255f,
                    ParseByte(rgbMatch.Groups[2].Value) / 255f,
                    ParseByte(rgbMatch.Groups[3].Value) / 255f,
                    1f);
                return true;
            }

            var hexMatch = Regex.Match(value, "#([0-9a-f]{6})", RegexOptions.IgnoreCase);
            if (hexMatch.Success)
            {
                var hex = hexMatch.Groups[1].Value;
                color = new Color(
                    Convert.ToInt32(hex.Substring(0, 2), 16) / 255f,
                    Convert.ToInt32(hex.Substring(2, 2), 16) / 255f,
                    Convert.ToInt32(hex.Substring(4, 2), 16) / 255f,
                    1f);
                return true;
            }

            color = Color.white;
            return false;
        }

        private static List<List<Vector2>> ParseSvgPathData(string pathData)
        {
            var result = new List<List<Vector2>>();
            var tokens = Regex.Matches(pathData, "[MmLlHhVvCcZz]|-?\\d+(?:\\.\\d+)?");
            var index = 0;
            var command = '\0';
            var current = Vector2.zero;
            var subPathStart = Vector2.zero;
            List<Vector2> currentSubPath = null;

            while (index < tokens.Count)
            {
                var token = tokens[index].Value;
                if (IsCommand(token))
                {
                    command = token[0];
                    index++;
                }

                switch (command)
                {
                    case 'M':
                    case 'm':
                        CloseOpenSubPath(currentSubPath, result);
                        current = ReadPoint(tokens, ref index, current, command == 'm');
                        subPathStart = current;
                        currentSubPath = new List<Vector2> { current };
                        command = command == 'm' ? 'l' : 'L';
                        break;
                    case 'L':
                    case 'l':
                        current = ReadPoint(tokens, ref index, current, command == 'l');
                        if (currentSubPath != null)
                            currentSubPath.Add(current);
                        break;
                    case 'H':
                    case 'h':
                        current = ReadHorizontalPoint(tokens, ref index, current, command == 'h');
                        if (currentSubPath != null)
                            currentSubPath.Add(current);
                        break;
                    case 'V':
                    case 'v':
                        current = ReadVerticalPoint(tokens, ref index, current, command == 'v');
                        if (currentSubPath != null)
                            currentSubPath.Add(current);
                        break;
                    case 'C':
                    case 'c':
                        var control1 = ReadPoint(tokens, ref index, current, command == 'c');
                        var control2 = ReadPoint(tokens, ref index, current, command == 'c');
                        var end = ReadPoint(tokens, ref index, current, command == 'c');
                        AddCubicBezier(currentSubPath, current, control1, control2, end);
                        current = end;
                        break;
                    case 'Z':
                    case 'z':
                        if (currentSubPath != null && currentSubPath.Count > 0)
                            currentSubPath.Add(subPathStart);
                        CloseOpenSubPath(currentSubPath, result);
                        currentSubPath = null;
                        current = subPathStart;
                        command = '\0';
                        break;
                    default:
                        index++;
                        break;
                }
            }

            CloseOpenSubPath(currentSubPath, result);
            return result;
        }

        private static Vector2 ReadPoint(MatchCollection tokens, ref int index, Vector2 current, bool relative)
        {
            var point = new Vector2(ParseFloat(tokens[index].Value), ParseFloat(tokens[index + 1].Value));
            index += 2;
            return relative ? current + point : point;
        }

        private static Vector2 ReadHorizontalPoint(MatchCollection tokens, ref int index, Vector2 current,
            bool relative)
        {
            var x = ParseFloat(tokens[index].Value);
            index++;
            return new Vector2(relative ? current.x + x : x, current.y);
        }

        private static Vector2 ReadVerticalPoint(MatchCollection tokens, ref int index, Vector2 current,
            bool relative)
        {
            var y = ParseFloat(tokens[index].Value);
            index++;
            return new Vector2(current.x, relative ? current.y + y : y);
        }

        private static void AddCubicBezier(List<Vector2> subPath, Vector2 start, Vector2 control1,
            Vector2 control2, Vector2 end)
        {
            if (subPath == null)
                return;

            const int segments = 18;
            for (var i = 1; i <= segments; i++)
            {
                var t = i / (float)segments;
                var inverseT = 1f - t;
                var point =
                    inverseT * inverseT * inverseT * start +
                    3f * inverseT * inverseT * t * control1 +
                    3f * inverseT * t * t * control2 +
                    t * t * t * end;
                subPath.Add(point);
            }
        }

        private static void CloseOpenSubPath(List<Vector2> subPath, List<List<Vector2>> subPaths)
        {
            if (subPath != null && subPath.Count > 2)
                subPaths.Add(subPath);
        }

        private static bool IsPointInsideEvenOdd(List<List<Vector2>> subPaths, Vector2 point)
        {
            var inside = false;
            foreach (var subPath in subPaths)
            {
                for (int i = 0, j = subPath.Count - 1; i < subPath.Count; j = i++)
                {
                    var pointI = subPath[i];
                    var pointJ = subPath[j];
                    if (((pointI.y > point.y) != (pointJ.y > point.y)) &&
                        (point.x < (pointJ.x - pointI.x) * (point.y - pointI.y) /
                         (pointJ.y - pointI.y) + pointI.x))
                    {
                        inside = !inside;
                    }
                }
            }

            return inside;
        }

        private static bool IsCommand(string token)
        {
            return token.Length == 1 && char.IsLetter(token[0]);
        }

        private static float ParseFloat(string value)
        {
            return float.Parse(value, CultureInfo.InvariantCulture);
        }

        private static byte ParseByte(string value)
        {
            return byte.Parse(value, CultureInfo.InvariantCulture);
        }
    }
}

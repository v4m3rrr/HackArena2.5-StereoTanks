using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework;

namespace GameClient;

/// <summary>
/// Represents a class that provides localization support.
/// </summary>
internal static class Localization
{
    private static readonly string LanguageTranslationFile = PathUtils.GetAbsolutePath("Content/Localization/{0}.xml");
    private static readonly string NativeLanguageNamesFile = PathUtils.GetAbsolutePath("Content/Localization/NativeLanguageNames.xml");
    private static readonly Dictionary<Language, string> NativeNames = [];
    private static readonly Dictionary<string, string> Texts = [];

    /// <summary>
    /// Gets the localized string for the specified key.
    /// </summary>
    /// <param name="key">The key of the localized string.</param>
    /// <returns>
    /// The localized string if it is available;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    public static string? Get(string key)
    {
        return Texts.TryGetValue(key, out var value) ? value : null;
    }

    /// <summary>
    /// Gets the native name of the specified language.
    /// </summary>
    /// <param name="language">The language to get the native name of.</param>
    /// <returns>
    /// The native name of the specified language if it is available;
    /// otherwise, the name of the language.
    /// </returns>
    public static string GetNativeLanguageName(Language language)
    {
        return NativeNames.TryGetValue(language, out var value) ? value : language.ToString();
    }

    /// <summary>
    /// Gets the localized font path based on the current language setting.
    /// </summary>
    /// <returns>The path to the localized font file.</returns>
    public static string GetLocalizedFontPath()
    {
        var path = GameSettings.Language switch
        {
            Language.English or Language.French => Styles.Fonts.Paths.Main,
            Language.Polish => "Content\\Fonts\\Exo-SemiBold.ttf",
            Language.Russian => "Content\\Fonts\\GrtskZetta-Semibold.ttf",
            _ => null,
        };

        if (path is null)
        {
            DebugConsole.SendMessage($"Missing font path for {GameSettings.Language} language", Color.Yellow);
            DebugConsole.SendMessage("Using default font path", Color.Yellow);
            path = Styles.Fonts.Paths.Main;
        }

        return path;
    }

    /// <summary>
    /// Loads the localization content.
    /// </summary>
    public static void Initialize()
    {
        LoadNativeLanguageNames();
        LoadLanguage();
        GameSettings.LanguageChanged += (s, e) => LoadLanguage();
    }

    private static void LoadNativeLanguageNames()
    {
        NativeNames.Clear();

        var xml = new XmlDocument();
        xml.Load(NativeLanguageNamesFile);

        foreach (XmlNode child in xml.DocumentElement!.ChildNodes)
        {
            NativeNames[(Language)Enum.Parse(typeof(Language), child.Name)] = child.InnerText;
        }
    }

    private static void LoadLanguage()
    {
        Texts.Clear();

        var xml = new XmlDocument();
        xml.Load(string.Format(LanguageTranslationFile, GameSettings.Language));

        XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml.NameTable);
        nsmgr.AddNamespace("ns", "http://tempuri.org/Language.xsd");

        var root = xml.DocumentElement;

        var nodeNames = new List<string>();
        foreach (XmlNode child in root!.ChildNodes)
        {
            nodeNames.Add(child.Name);
        }

        foreach (var nodeName in nodeNames)
        {
            var node = root.SelectSingleNode($"ns:{nodeName}", nsmgr);
            LoadLanguageFromNode(node!);
        }
    }

    private static void LoadLanguageFromNode(XmlNode categoryNode)
    {
        foreach (XmlNode child in categoryNode.ChildNodes)
        {
            Texts[$"{categoryNode.Name}.{child.Name}"] = child.InnerText;
        }
    }
}

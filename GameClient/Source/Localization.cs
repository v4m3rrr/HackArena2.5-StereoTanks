using System;
using System.Collections.Generic;
using System.Xml;

namespace GameClient;

/// <summary>
/// Represents a class that provides localization support.
/// </summary>
internal static class Localization
{
    private const string LanguageTranslationFile = "Content/Localization/{0}.xml";
    private const string NativeLanguageNamesFile = "Content/Localization/NativeLanguageNames.xml";

    private static readonly Dictionary<Language, string> NativeNames = new();
    private static readonly Dictionary<string, string> Texts = new();

    static Localization()
    {
        LoadLanguage();
        LoadNativeLanguageNames();
        GameSettings.LanguageChanged += (s, e) => LoadLanguage();
    }

    /// <summary>
    /// Gets the localized string for the specified key.
    /// </summary>
    /// <param name="key">The key of the localized string.</param>
    /// <returns>The localized string.</returns>
    public static string Get(string key)
    {
        return Texts.TryGetValue(key, out var value) ? value : key;
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
        foreach (XmlNode child in root.ChildNodes)
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

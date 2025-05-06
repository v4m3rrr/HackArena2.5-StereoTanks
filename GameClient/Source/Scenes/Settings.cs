using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.Scenes;

/// <summary>
/// Represents the settings scene.
/// </summary>
[AutoInitialize]
[AutoLoadContent]
internal class Settings : Scene, IOverlayScene
{
    private SolidColor overlayBackground = default!;

    /// <summary>
    /// Initializes a new instance of the <see cref="Settings"/> class.
    /// </summary>
    public Settings()
        : base(Color.Transparent)
    {
    }

    /// <inheritdoc/>
    public int Priority => 1 << 5;

    /// <inheritdoc/>
    public IEnumerable<IComponent> OverlayComponents => [this.BaseComponent];

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        if (!this.IsDisplayedOverlay)
        {
            MainEffect.Rotation -= 0.1f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            MainEffect.Rotation %= MathHelper.TwoPi;
        }

        base.Update(gameTime);
    }

    /// <inheritdoc/>
    public override void Draw(GameTime gameTime)
    {
        ScreenController.GraphicsDevice.Clear(Color.Black);

        if (this.IsDisplayedOverlay)
        {
            this.overlayBackground.Draw(gameTime);
        }

        MainEffect.Draw();
        base.Draw(gameTime);
    }

    /// <inheritdoc/>
    protected override void Initialize(Component baseComponent)
    {
        this.overlayBackground = new SolidColor(Color.Black * 0.95f);

        this.Hiding += (s, e) => GameSettings.SaveSettings();

        var titleFont = new LocalizedScalableFont(22)
        {
            AutoResize = true,
            Spacing = 5,
        };

        var title = new LocalizedText(titleFont, Color.White)
        {
            Parent = baseComponent,
            Value = new LocalizedString("Buttons.Settings"),
            TextAlignment = Alignment.Top,
            Case = TextCase.Upper,
            Transform =
            {
                Alignment = Alignment.Top,
                RelativeOffset = new Vector2(0.0f, 0.08f),
                RelativeSize = new Vector2(0.5f, 0.1f),
            },
        };

        var backButton = new Button<Container>(new Container())
        {
            Parent = baseComponent,
            Transform =
            {
                Alignment = Alignment.BottomLeft,
                RelativeSize = new Vector2(0.2f, 0.07f),
            },
        };

        backButton.ApplyStyle(Styles.UI.BackButtonStyle);

        this.Showing += (s, e) =>
        {
            if (e?.Overlay ?? false)
            {
                backButton.GetDescendant<LocalizedText>()!.Value = new LocalizedString("Buttons.Close");
                backButton.Transform.RelativeOffset = new Vector2(0.06f, -0.08f);
                var texture = backButton.GetDescendant<ScalableTexture2D>()!;
                texture.Texture?.Dispose();
                texture.AssetPath = "Images/Icons/close.svg";
                texture.Load();
                baseComponent.ForceUpdate(withTransform: true);
            }
            else
            {
                backButton.GetDescendant<LocalizedText>()!.Value = new LocalizedString("Buttons.Back");
                backButton.Transform.RelativeOffset = new Vector2(0.08f, -0.08f);
                var texture = backButton.GetDescendant<ScalableTexture2D>()!;
                texture.Texture?.Dispose();
                texture.AssetPath = "Images/Icons/back.svg";
                texture.Load();
                baseComponent.ForceUpdate(withTransform: true);
            }
        };

        backButton.Clicked += (s, e) =>
        {
            if (this.IsDisplayedOverlay)
            {
                HideOverlay<Settings>();
            }
            else
            {
                ChangeToPreviousOrDefault<MainMenu>();
            }
        };

        var listBox = new FlexListBox()
        {
            Parent = baseComponent,
            Orientation = Orientation.Vertical,
            Spacing = 20,
            Transform =
            {
                Alignment = Alignment.Center,
                RelativeSize = new Vector2(0.6f, 0.5f),
                MinSize = new Point(620, 1),
            },
        };

        var itemFont = new LocalizedScalableFont(13)
        {
            AutoResize = true,
            Spacing = 5,
            MinSize = 6,
        };

        List<ListBox> sections = [];

        var generalSection = CreateSection(listBox.ContentContainer);
        sections.Add(generalSection);

        // Language
        {
            var languagesCount = Enum.GetNames(typeof(Language)).Length;
            var (_, selector) = CreateItem<Language>(
                generalSection,
                new LocalizedString("Labels.Language"),
                itemFont,
                scrollable: false,
                localized: false,
                0.95f * languagesCount);

            selector.CurrentItemPredicate = (x) => x == GameSettings.Language;
            selector.ItemSelected += (s, item) =>
            {
                selector.InactiveContainer.GetChild<Text>()!.Value = item?.Name ?? string.Empty;
            };

            GameSettings.LanguageChanged += (s, e) => selector.SelectCurrentItem();

            foreach (string languageName in Enum.GetNames(typeof(Language)))
            {
                var language = (Language)Enum.Parse(typeof(Language), languageName);
                var nativeName = Localization.GetNativeLanguageName(language);

                var button = new Button<Container>(new Container());
                button.ApplyStyle(Styles.Settings.GetSelectorButtonItem());

                var text = button.Component.GetChild<Text>()!;
                text.Font = LocalizedScalableFont.GetNativeLocalizedFont(
                    language,
                    itemFont.Size,
                    itemFont.MinSize,
                    itemFont.MaxSize,
                    itemFont.Spacing,
                    itemFont.AutoResize);
                text.Value = nativeName;
                text.Case = TextCase.Upper;

                var item = new Selector<Language>.Item(button, language, nativeName);
                selector.AddItem(item);

                button.Clicked += (s, e) => GameSettings.Language = language;
                button.Clicked += (s, e) => selector.SelectItem(item);

                selector.ItemSelecting += (s, i) =>
                {
                    if (item == i)
                    {
                        selector.InactiveContainer.GetChild<Text>()!.Font = text.Font;
                    }
                };
            }

            selector.SelectCurrentItem();
        }

        var graphicsSection = CreateSection(listBox.ContentContainer);
        sections.Add(graphicsSection);

        // Resolution
        {
            var (_, selector) = CreateItem<Point>(
                graphicsSection,
                new LocalizedString("Labels.Resolution"),
                itemFont,
                scrollable: true,
                localized: false,
                3.1f);

            selector.RelativeHeight = 5f;
            selector.ScrollToSelected = true;
            selector.ElementFixedHeight = (int)(60 * ScreenController.Scale.Y);
            selector.CurrentItemPredicate = (x) => x == ScreenController.CurrentSize;
            selector.ItemSelected += (s, item) =>
            {
                selector.InactiveContainer.GetDescendant<Text>()!.Value = item?.Name
                    ?? GetResolutionWithAspectRatio(ScreenController.CurrentSize);
            };

            GameSettings.ResolutionChanged += (s, e) =>
            {
                selector.SelectCurrentItem();
                selector.ElementFixedHeight = (int)(60 * ScreenController.Scale.Y);
            };

            static string GetResolutionWithAspectRatio(Point resolution)
            {
                Ratio ratio = (resolution.X / (float)resolution.Y).ToRatio(epsilon: 0.02);
                string ratioText = ratio.ToString().Replace("8:5", "16:10");
                return $"{resolution.X}x{resolution.Y}  ({ratioText})";
            }

            List<Point> resolutions = ScreenController.GraphicsDevice.Adapter.SupportedDisplayModes
                .Select(x => new Point(x.Width, x.Height))
                .Where(x => x.X >= GameClientCore.MinWindowSize.X)
                .Where(x => x.Y >= GameClientCore.MinWindowSize.Y)
                .ToList();

            foreach (Point resolution in resolutions)
            {
                string description = GetResolutionWithAspectRatio(resolution);

                var button = new Button<Container>(new Container());
                button.ApplyStyle(Styles.Settings.GetSelectorButtonItem());
                var text = button.Component.GetChild<Text>()!;
                text.Value = description;
                text.Case = TextCase.Lower;

                if (selector.ListBox is ScrollableListBox scrollableListBox)
                {
                    scrollableListBox.ScrollBar.Scrolled += (s, e) =>
                    {
                        if (e.ClampedDelta != 0)
                        {
                            button.ResetHover();
                        }
                    };
                }

                var item = new Selector<Point>.Item(button, resolution, description);
                selector.AddItem(item);

                button.Clicked += async (s, e) =>
                {
                    await GameSettings.SetResolution(resolution.X, resolution.Y);
                    selector.SelectItem(item);
                };
            }

            selector.SelectCurrentItem();
        }

        // Display mode
        {
            var (_, selector) = CreateItem<ScreenType>(
                graphicsSection,
                new LocalizedString("Labels.DisplayMode"),
                itemFont,
                scrollable: false,
                localized: true,
                2.1f);

            selector.CurrentItemPredicate = (x) => x == ScreenController.ScreenType;
            selector.ItemSelected += (s, item) =>
            {
                selector.InactiveContainer.GetChild<LocalizedText>()!.Value = new LocalizedString($"Labels.DisplayMode{item!.Value}");
            };

            GameSettings.ScreenTypeChanged += (s, e) => selector.SelectCurrentItem();

            foreach (string typeName in Enum.GetNames(typeof(ScreenType)))
            {
                var screenType = (ScreenType)Enum.Parse(typeof(ScreenType), typeName);

                if (screenType == ScreenType.Borderless)
                {
                    // Borderless mode is not working properly for now
                    continue;
                }

                var button = new Button<Container>(new Container());
                button.ApplyStyle(Styles.Settings.GetSelectorButtonItem(localized: true));

                var text = button.Component.GetChild<LocalizedText>()!;
                text.Value = new LocalizedString($"Labels.DisplayMode{typeName}");
                text.Case = TextCase.Upper;

                var item = new Selector<ScreenType>.Item(button, screenType, typeName);
                selector.AddItem(item);

                button.Clicked += async (s, e) =>
                {
                    await GameSettings.SetScreenType(screenType);
                    selector.SelectItem(item);
                };
            }

            selector.SelectCurrentItem();
        }

        ResizeSections(listBox, sections);
    }

    /// <inheritdoc/>
    protected override void LoadSceneContent()
    {
        this.overlayBackground.Load();

        var textures = this.BaseComponent.GetAllDescendants<TextureComponent>();
        textures.ToList().ForEach(x => x.Load());
    }

    private static void ResizeSections(ListBox baseListBox, IEnumerable<ListBox> sections)
    {
        if (baseListBox is not FlexListBox baseFlex)
        {
            return;
        }

        foreach (ListBox section in sections)
        {
            var itemCount = section.Components.Count();
            var paddingYW = section.Transform.RelativePadding.Y + section.Transform.RelativePadding.W;
            baseFlex.SetResizeFactor(section, 1 + ((itemCount - 1) * (1 + paddingYW)));
        }
    }

    private static FlexListBox CreateSection(IComponent parent)
    {
        var container = new FlexListBox()
        {
            Parent = parent,
            Orientation = Orientation.Vertical,
            Transform =
            {
                RelativePadding = new Vector4(0.05f, 0.1f, 0.05f, 0.1f),
            },
        };

        // Background
        _ = new RoundedSolidColor(GameClientCore.ThemeColor, 30)
        {
            Parent = container,
            Opacity = 0.35f,
            Transform = { IgnoreParentPadding = true },
        };

        return container;
    }

    private static (Container Container, Selector<T_Item> Selector) CreateItem<T_Item>(
        ListBox parent,
        LocalizedString name,
        ScalableFont font,
        bool scrollable,
        bool localized = false,
        float relativeHeight = 1f)
        where T_Item : notnull
    {
        var container = new Container()
        {
            Parent = parent.ContentContainer,
        };

        // Name
        _ = new LocalizedText(font, Color.White)
        {
            Parent = container,
            Value = name,
            TextShrink = TextShrinkMode.Width,
            Case = TextCase.Upper,
            TextAlignment = Alignment.Left,
        };

        ListBox listBox = scrollable
            ? new ScrollableListBox()
            : new FlexListBox();

        var selector = new Selector<T_Item>(listBox)
        {
            Parent = container,
            ActiveContainerAlignment = Alignment.Top,
            RelativeHeight = relativeHeight,
            Transform =
            {
                Alignment = Alignment.Right,
                RelativeSize = new Vector2(0.4f, 0.6f),
            },
            ScrollToSelected = scrollable,
            CloseAfterSelect = true,
        };

        selector.ApplyStyle(Styles.Settings.GetSelectorStyle(localized));

        return (container, selector);
    }
}

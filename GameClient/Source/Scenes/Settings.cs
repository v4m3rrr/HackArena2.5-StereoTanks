using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.Scenes;

/// <summary>
/// Represents the settings scene.
/// </summary>
internal class Settings : Scene, IOverlayScene
{
    private readonly List<Component> overlayComponents = new();
    private ListBox listBox = default!;
    private Button<Frame> backButton = default!;

    /// <summary>
    /// Initializes a new instance of the <see cref="Settings"/> class.
    /// </summary>
    public Settings()
        : base()
    {
    }

    /// <inheritdoc/>
    public event EventHandler<(int Before, int After)>? PriorityChanged;

    /// <inheritdoc/>
    public int Priority => 1;

    /// <inheritdoc/>
    public IEnumerable<IReadOnlyComponent> OverlayComponents => this.overlayComponents;

    /// <inheritdoc/>
    protected override void Initialize(Component baseComponent)
    {
        this.Showed += (s, e) =>
        {
            if (this.IsDisplayedOverlay)
            {
                this.SetBackground(Color.Black * 0.5f);
                this.backButton.GetDescendant<LocalizedText>()!.Value = new LocalizedString("Buttons.Close");
            }
            else
            {
                this.SetBackground(Color.MediumSpringGreen * 0.7f);
                this.backButton.GetDescendant<LocalizedText>()!.Value = new LocalizedString("Buttons.Back");
            }
        };

        var font = new ScalableFont("Content\\Fonts\\Consolas.ttf", 12);

        var baseFrame = new Frame(Color.Black, 2)
        {
            Parent = this.BaseComponent,
            Transform =
            {
                Alignment = Alignment.Center,
                RelativeSize = new Vector2(0.75f),
                Ratio = new Ratio(4, 3),
                MinSize = new Point(520, 390),
                MaxSize = new Point(800, 600),
            },
        };
        this.overlayComponents.Add(baseFrame);

        var baseBackground = new SolidColor(Color.Black * 0.75f) { Parent = baseFrame.InnerContainer };

        this.listBox = new ListBox()
        {
            Parent = baseFrame.InnerContainer,
            Orientation = Orientation.Vertical,
            IsScrollable = true,
            Spacing = 10,
            ContentContainerRelativeMargin = new Vector4(0.01f, 0.02f, 0.01f, 0.02f),
            ScrollBar =
            {
                FrameThickness = 1,
                FrameColor = Color.DarkGray * 0.75f,
                BackgroundColor = Color.Gray * 0.65f,
                ThumbColor = Color.DarkSalmon,
                Parent = baseFrame,
            },
        };

        // Language
        {
            var frame = CreateLabelFrame(this.listBox, font, "Labels.Language");
            var selector = CreateSelector<Language>(font, frame.InnerContainer);
            selector.Opened += (s, e) => this.UpdateOtherOptions();
            selector.Closed += (s, e) => this.UpdateOtherOptions();
            selector.CurrentItemPredicate = (x) => x == GameSettings.Language;
            selector.ItemSelected += (s, item) =>
            {
                selector.Close();
                selector.InactiveContainer.GetDescendant<Text>()!.Value = item?.Name ?? string.Empty;
            };

            GameSettings.LanguageChanged += (s, e) => selector.SelectCurrentItem();

            foreach (string languageName in Enum.GetNames(typeof(Language)))
            {
                Language language = (Language)Enum.Parse(typeof(Language), languageName);
                var nativeName = Localization.GetNativeLanguageName(language);

                var button = new Button<Frame>(new Frame()).ApplyStyle(Styles.Settings.SelectorItem);
                button.Clicked += (s, e) => GameSettings.Language = language;
                _ = new Text(Styles.Settings.Font, Color.White)
                {
                    Parent = button.Component.InnerContainer,
                    Value = nativeName,
                    TextAlignment = Alignment.Center,
                };

                var item = new Selector<Language>.Item(button, language, nativeName);
                selector.AddItem(item);

                button.Clicked += (s, e) => selector.SelectItem(item);
            }
        }

        // Resolution
        {
            var frame = CreateLabelFrame(this.listBox, font, "Labels.Resolution");
            var selector = CreateSelector<Point>(font, frame.InnerContainer);
            selector.RelativeHeight = 5f;
            selector.ListBox.ResizeContent = false;
            selector.ListBox.IsScrollable = true;
            selector.ListBox.ScrollBar.FrameThickness = 1;
            selector.ListBox.ScrollBar.FrameColor = Color.DarkGray;
            selector.ListBox.ScrollBar.BackgroundColor = Color.Gray * 0.65f;
            selector.ListBox.ScrollBar.ThumbColor = Color.Gainsboro;
            selector.ListBox.ScrollBar.RelativeSize = 0.07f;
            selector.ScrollToSelected = true;
            selector.Opened += (s, e) => this.UpdateOtherOptions();
            selector.Closed += (s, e) => this.UpdateOtherOptions();
            selector.CurrentItemPredicate = (x) => x == ScreenController.CurrentSize;
            selector.ItemSelected += (s, item) =>
            {
                selector.Close();
                selector.InactiveContainer.GetDescendant<Text>()!.Value = item?.Name
                    ?? GetResolutionWithAspectRatio(ScreenController.CurrentSize);
            };

            GameSettings.ResolutionChanged += (s, e) => selector.SelectCurrentItem();

            static string GetResolutionWithAspectRatio(Point resolution)
            {
                Ratio ratio = (resolution.X / (float)resolution.Y).ToRatio(epsilon: 0.02);
                string ratioText = ratio.ToString().Replace("8:5", "16:10");
                return $"{resolution.X}x{resolution.Y} ({ratioText})";
            }

            List<Point> resolutions = ScreenController.GraphicsDevice.Adapter.SupportedDisplayModes
                .Select(x => new Point(x.Width, x.Height))
                .Where(x => x.X >= MonoTanks.MinWindowSize.X)
                .Where(x => x.Y >= MonoTanks.MinWindowSize.Y)
                .ToList();

            foreach (Point resolution in resolutions)
            {
                string description = GetResolutionWithAspectRatio(resolution);

                var button = new Button<Frame>(new Frame()).ApplyStyle(Styles.Settings.SelectorItem);
                button.Clicked += (s, e) => GameSettings.SetResolution(resolution.X, resolution.Y);
                _ = new Text(Styles.Settings.Font, Color.White)
                {
                    Parent = button.Component.InnerContainer,
                    Value = description,
                    TextAlignment = Alignment.Center,
                    TextShrink = TextShrinkMode.HeightAndWidth,
                };

                var item = new Selector<Point>.Item(button, resolution, description);
                selector.AddItem(item);

                button.Clicked += (s, e) => selector.SelectItem(item);
            }
        }

        // Screen type
        {
            var frame = CreateLabelFrame(this.listBox, font, "Labels.ScreenType");
            var selector = CreateSelector<ScreenType>(font, frame.InnerContainer, localized: true);
            selector.Opened += (s, e) => this.UpdateOtherOptions();
            selector.Closed += (s, e) => this.UpdateOtherOptions();
            selector.CurrentItemPredicate = (x) => x == ScreenController.ScreenType;
            selector.ItemSelected += (s, item) =>
            {
                selector.Close();
                selector.InactiveContainer.GetDescendant<LocalizedText>()!.Value = new LocalizedString($"Labels.ScreenType{item?.Name}");
            };

            GameSettings.ScreenTypeChanged += (s, e) => selector.SelectCurrentItem();

            foreach (string screenTypeName in Enum.GetNames(typeof(ScreenType)))
            {
                ScreenType screenType = (ScreenType)Enum.Parse(typeof(ScreenType), screenTypeName);

                var button = new Button<Frame>(new Frame()).ApplyStyle(Styles.Settings.SelectorItem);
                button.Clicked += (s, e) => GameSettings.SetScreenType(screenType);
                _ = new LocalizedText(Styles.Settings.Font, Color.White)
                {
                    Parent = button.Component.InnerContainer,
                    Value = new LocalizedString($"Labels.ScreenType{screenTypeName}"),
                    TextAlignment = Alignment.Center,
                };

                var item = new Selector<ScreenType>.Item(button, screenType, screenTypeName);
                selector.AddItem(item);

                button.Clicked += (s, e) => selector.SelectItem(item);
            }
        }

        this.SetCurrentSettings();

        // Back button
        {
            this.backButton = new Button<Frame>(new Frame())
            {
                Parent = this.BaseComponent,
                Transform =
                {
                    Alignment = Alignment.BottomLeft,
                    RelativeOffset = new Vector2(0.04f, -0.04f),
                    RelativeSize = new Vector2(0.12f, 0.07f),
                },
            }.ApplyStyle(Styles.UI.ButtonStyle);
            this.backButton.Clicked += (s, e) =>
            {
                GameSettings.DiscardChanges();
                this.SetCurrentSettings();

                if (this.IsDisplayedOverlay)
                {
                    HideOverlay<Settings>();
                }
                else
                {
                    ChangeToPreviousOr<MainMenu>();
                }
            };
            this.overlayComponents.Add(this.backButton);
        }

        // Save button
        {
            var button = new Button<Frame>(new Frame())
            {
                Parent = this.BaseComponent,
                Transform =
            {
                Alignment = Alignment.BottomRight,
                RelativeOffset = new Vector2(-0.04f, -0.04f),
                RelativeSize = new Vector2(0.12f, 0.07f),
            },
            }.ApplyStyle(Styles.UI.ButtonStyle);
            button.Clicked += (s, e) =>
            {
                GameSettings.SaveSettings();

                if (this.IsDisplayedOverlay)
                {
                    HideOverlay<Settings>();
                }
                else
                {
                    ChangeToPreviousOr<MainMenu>();
                }
            };
            button.GetDescendant<LocalizedText>()!.Value = new LocalizedString("Buttons.Save");
            this.overlayComponents.Add(button);
        }
    }

    private static Frame CreateLabelFrame(ListBox listBox, ScalableFont font, string localizedIdentificator)
    {
        var frame = new Frame()
        {
            Parent = listBox.ContentContainer,
            Transform =
            {
                RelativePadding = new Vector4(0.01f, 0.0f, 0.0f, 0.0f),
                MinSize = new Point(1, (int)(font.SafeDimensions.Y * 2)),
                MaxSize = new Point(int.MaxValue, (int)(font.SafeDimensions.Y * 2)),
            },
        }.ApplyStyle(Styles.Settings.FrameLabel);
        frame.InnerContainer.GetChild<LocalizedText>()!.Value = new LocalizedString(localizedIdentificator);
        return frame;
    }

    // TODO: Remove localization from this method
    private static Selector<T> CreateSelector<T>(ScalableFont font, IReadOnlyComponent parent, bool localized = false)
    {
        var selector = new Selector<T>(font)
        {
            Parent = parent,
            ElementFixedHeight = (int)(font.SafeDimensions.Y * 1.8f),
            ActiveContainerAlignment = Alignment.Top,
            RelativeHeight = Enum.GetNames(typeof(Language)).Length,
            ListBox =
            {
                Orientation = Orientation.Vertical,
                Spacing = 10,
                ContentContainerRelativeMargin = new Vector4(0.05f, 0.05f, 0.05f, 0.05f),
                ResizeContent = true,
                DrawContentOnMargin = true,
            },
            Transform =
            {
                Alignment = Alignment.Right,
                RelativeSize = new Vector2(0.5f, 1.0f),
            },
            ActiveBackground = new Frame().ApplyStyle(Styles.Settings.SelectorActiveBackground),
            InactiveBackground = new Frame().ApplyStyle(Styles.Settings.SelectorInactiveBackground),
        };

        var text = localized ? new LocalizedText(font, Color.White) : new Text(font, Color.White);
        text.Parent = selector.InactiveContainer;
        text.TextAlignment = Alignment.Center;

        return selector;
    }

    private void SetCurrentSettings()
    {
        this.BaseComponent.GetAllDescendants<ISelector>().ToList().ForEach(x => x.SelectCurrentItem());
    }

    private void UpdateOtherOptions()
    {
        var selected = this.listBox.ContentContainer.GetDescendant<ISelector>(x => x.ActiveContainer.IsEnabled);
        var labels = this.listBox.ContentContainer.GetAllChildren<Frame>();
        foreach (var frame in labels)
        {
            var selector = frame.GetDescendant<ISelector>();
            if (selector != selected && selector is not null)
            {
                var selectedText = selector.InactiveContainer.GetDescendant<Text>()!;
                selectedText.Color = new Color(selectedText.Color, selected is null ? 1f : 0.3f);

                frame.InnerContainer.GetChild<SolidColor>()!.Color = Color.White * (selected is null ? 0.3f : 0.05f);

                var solidColor = selector.InactiveContainer.GetDescendant<SolidColor>()!;
                solidColor.Color = new Color(solidColor.Color, selected is null ? 1f : 0.6f);

                var frame2 = selector.InactiveContainer.GetChild<Frame>()!;
                frame2.Color = new Color(frame2.Color, selected is null ? 1f : 0.2f);

                foreach (var text in frame.InnerContainer.GetAllChildren<Text>())
                {
                    text.Color = new Color(text.Color, selected is null ? 1f : 0.5f);
                }
            }
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Video;
using osu.Framework.Logging;
using osu.Framework.Timing;
using osu.Game.Graphics;
using osu.Game.Tournament.IO;
using osu.Game.Tournament.Localisation;
using osu.Game.Tournament.Models;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    /// <summary>
    /// A background sprite supporting image and video inputs.
    /// </summary>
    public partial class TourneyBackground : CompositeDrawable
    {
        private readonly BackgroundType requestedType;
        private BackgroundInfo info;
        private readonly bool drawFallbackGradient;
        private readonly FillMode fillMode;

        private Sprite? imageSprite;
        private Video? video;
        private ManualClock? manualClock;

        private readonly Container spriteContainer;
        private readonly Box dimBox;

        private readonly SpriteIcon infoIcon;
        private readonly TournamentSpriteText infoText;

        private readonly FillFlowContainer errorFlow;

        private readonly bool skipLadderLookup;
        private bool needDetection;

        [Resolved]
        private TextureStore? textureStore { get; set; }

        [Resolved]
        private TournamentVideoResourceStore? videoStore { get; set; }

        [Resolved]
        private LadderInfo ladder { get; set; } = null!;

        public bool BackgroundAvailable => video != null || imageSprite != null;

        /// <summary>
        /// Fetch background information from cached <see cref="LadderInfo"/> and try to display it.
        /// </summary>
        public TourneyBackground(BackgroundType backgroundType,
                                 bool drawFallbackGradient = true, bool showInfo = false,
                                 FillMode fillMode = FillMode.Fill)
        {
            requestedType = backgroundType;

            this.drawFallbackGradient = drawFallbackGradient;
            this.fillMode = fillMode;

            InternalChildren = new Drawable[]
            {
                spriteContainer = new Container
                {
                    Name = @"Media layer",
                    RelativeSizeAxes = Axes.Both,
                },
                dimBox = new Box
                {
                    Name = @"Dim layer",
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0,
                },
                new Container
                {
                    Name = @"Information layer",
                    RelativeSizeAxes = Axes.Both,
                    Alpha = showInfo ? 1 : 0,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Height = 64,
                            RelativeSizeAxes = Axes.X,
                            Colour = ColourInfo.GradientVertical(Color4.Black.Opacity(0.6f), Color4.Black.Opacity(0)),
                        },
                        new FillFlowContainer
                        {
                            Name = @"Media information",
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(10),
                            X = 20,
                            Y = 20,
                            Children = new Drawable[]
                            {
                                infoIcon = new SpriteIcon
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Icon = FontAwesome.Solid.Play,
                                    Size = new Vector2(20),
                                },
                                infoText = new TournamentSpriteText
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Font = OsuFont.Torus.With(size: 18),
                                },
                            },
                        },
                        errorFlow = new FillFlowContainer
                        {
                            Name = @"Error text",
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(10),
                            Alpha = 0,
                            Children = new Drawable[]
                            {
                                new SpriteIcon
                                {
                                    Size = new Vector2(20),
                                    Icon = FontAwesome.Solid.Unlink,
                                },
                                new TournamentSpriteText
                                {
                                    Font = OsuFont.Torus.With(size: 20),
                                    Text = BaseStrings.BackgroundUnavailable,
                                },
                            },
                        },
                    },
                },
            };
        }

        /// <summary>
        /// Use specified <see cref="BackgroundInfo"/> to lookup and display a background.
        /// </summary>
        /// <remarks>This constructor is for background preview only, and doesn't support ladder-based features.</remarks>
        public TourneyBackground(BackgroundInfo info, bool drawFallbackGradient = true, bool showInfo = false, FillMode fillMode = FillMode.Fill)
            : this(default(BackgroundType), drawFallbackGradient, showInfo, fillMode)
        {
            this.info = info;
            skipLadderLookup = true;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            loadSprites();

            // Subscribe changes (only when fetched from ladder)
            if (!skipLadderLookup)
            {
                ladder.BackgroundMap.BindCollectionChanged((_, _) => loadSprites());
            }
        }

        /// <summary>
        /// Lookup and load required resources for a background.
        /// </summary>
        private void loadSprites()
        {
            if (textureStore == null || videoStore == null) return;

            if (!skipLadderLookup)
            {
                // Only reload when relevant changes were made to the mapping list.
                if (!ladder.BackgroundMap.TryGetBackgroundInfo(requestedType, out var newInfo))
                {
                    if (imageSprite == null && video == null && drawFallbackGradient)
                        loadFallbackGradient();

                    return;
                }

                if (newInfo.FileInfoEquals(info))
                {
                    if (newInfo.Dim != info.Dim)
                        Dim = newInfo.Dim;

                    return;
                }

                info = newInfo;
            }

            infoText.Text = info.Name;
            dimBox.Alpha = info.Dim;

            needDetection = info.Source == BackgroundSource.Auto;
            bool isFaulted = false;

            if (needDetection)
            {
                Logger.Log($"It's not suggested to use automatically defined background type for {requestedType}. Are you using an older version of configuration file?",
                    level: LogLevel.Important);
            }

            if (info.Source == BackgroundSource.Image || needDetection)
            {
                var image = textureStore.Get($"Backgrounds/{info.Name}");

                if (image != null)
                {
                    needDetection = false;
                    spriteContainer.Child = imageSprite = new Sprite
                    {
                        RelativeSizeAxes = Axes.Both,
                        FillMode = fillMode,
                        Texture = image
                    };
                }
                else
                {
#if DEBUG
                    Logger.Log($"Cannot find and load background image \"{info.Name}\" for {requestedType}.",
                        level: LogLevel.Important);
#endif

                    isFaulted = !needDetection;
                }
            }

            if (info.Source == BackgroundSource.Video || needDetection)
            {
                var stream = videoStore.GetStream(info.Name);

                if (stream != null)
                {
                    needDetection = false;
                    spriteContainer.Child = video = new Video(stream, false)
                    {
                        RelativeSizeAxes = Axes.Both,
                        FillMode = fillMode,
                        Clock = new FramedClock(manualClock = new ManualClock()),
                        Loop = loop,
                    };
                }
                else
                {
#if DEBUG
                    Logger.Log($"Cannot find and load background video \"{info.Name}\" for {requestedType}.",
                        level: LogLevel.Important);
#endif

                    isFaulted = true;
                }
            }

            infoIcon.Icon = isFaulted ? FontAwesome.Solid.Unlink : FontAwesome.Solid.Play;

            if (isFaulted)
            {
#if DEBUG
                Logger.Log($"Unable to find available background for {requestedType}. Check your tournament directory and configuration.",
                    level: LogLevel.Important);
#endif

                loadFallbackGradient(isFaulted);
            }
        }

        private void loadFallbackGradient(bool isFaulted = false)
        {
            spriteContainer.Children = new Drawable[]
            {
                new Box
                {
                    Colour = ColourInfo.GradientVertical(OsuColour.Gray(0.3f), OsuColour.Gray(0.6f)),
                    RelativeSizeAxes = Axes.Both,
                    Alpha = drawFallbackGradient ? 1 : 0,
                },
            };

            errorFlow.Alpha = isFaulted ? 1 : 0;
        }

        private bool loop;

        public bool Loop
        {
            set
            {
                loop = value;
                if (video != null)
                    video.Loop = value;
            }
        }

        public float Dim
        {
            get => info.Dim;
            set
            {
                if (info.Dim == value)
                    return;

                info.Dim = value;
                dimBox.FadeTo(value, 300, Easing.OutQuint);
            }
        }

        public void Reset()
        {
            if (manualClock != null)
                manualClock.CurrentTime = 0;
        }

        protected override void Update()
        {
            base.Update();

            if (manualClock != null && Clock.ElapsedFrameTime < 100)
            {
                // we want to avoid seeking as much as possible, because we care about performance, not sync.
                // to avoid seeking completely, we only increment out local clock when in an updating state.
                manualClock.CurrentTime += Clock.ElapsedFrameTime;
            }
        }
    }
}

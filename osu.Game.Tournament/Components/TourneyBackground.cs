// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
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
using osu.Game.Graphics.Containers;
using osu.Game.Tournament.IO;
using osu.Game.Tournament.Models;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    /// <summary>
    /// A background sprite supporting image and video inputs.
    /// </summary>
    public partial class TourneyBackground : CompositeDrawable
    {
        // <summary>
        // Trigger when the video has completed playback.
        // </summary>
        public event Action? OnPlaybackComplete;

        private bool hasTriggeredCompletion;

        private readonly BackgroundType requestedType;
        private BackgroundInfo info;
        private readonly bool drawFallbackGradient;
        private readonly bool showError;
        private readonly FillMode fillMode;

        private Sprite? imageSprite;
        private Video? video;
        private ManualClock? manualClock;
        private readonly Container spriteContainer;
        private OsuTextFlowContainer errorFlow = null!;

        public bool VideoAvailable => video != null;

        private readonly Box dimBox;

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
                                 bool drawFallbackGradient = true, bool showError = false,
                                 FillMode fillMode = FillMode.Fill)
        {
            requestedType = backgroundType;

            this.drawFallbackGradient = drawFallbackGradient;
            this.showError = showError;
            this.fillMode = fillMode;

            InternalChildren = new Drawable[]
            {
                spriteContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                },
                dimBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0,
                },
            };
        }

        /// <summary>
        /// Use specified <see cref="BackgroundInfo"/> to lookup and display a background.
        /// </summary>
        /// <remarks>This constructor is for background preview only, and doesn't support ladder-based features.</remarks>
        public TourneyBackground(BackgroundInfo info, bool drawFallbackGradient = true, bool showError = false, FillMode fillMode = FillMode.Fill)
            : this(default(BackgroundType), drawFallbackGradient, showError, fillMode)
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
                try
                {
                    // Only reload when relevant changes were made to the mapping list.
                    var newInfo = ladder.BackgroundMap.Last(v => v.Key == requestedType).Value;

                    if (newInfo.FileInfoEquals(info))
                    {
                        if (newInfo.Dim != info.Dim)
                            Dim = newInfo.Dim;

                        return;
                    }

                    info = newInfo;
                }
                // Don't let clear changes affect the background.
                catch (InvalidOperationException)
                {
                    return;
                }
            }

            dimBox.FadeTo(info.Dim, 300, Easing.OutQuint);

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

            if (isFaulted)
            {
#if DEBUG
                Logger.Log($"Unable to find available background for {requestedType}. Check your tournament directory and configuration.",
                    level: LogLevel.Important);
#endif

                spriteContainer.Children = new Drawable[]
                {
                    new Box
                    {
                        Colour = ColourInfo.GradientVertical(OsuColour.Gray(0.3f), OsuColour.Gray(0.6f)),
                        RelativeSizeAxes = Axes.Both,
                        Alpha = drawFallbackGradient ? 1 : 0
                    },
                    errorFlow = new OsuTextFlowContainer
                    {
                        Name = @"Error Text",
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoSizeAxes = Axes.Both,
                        Alpha = isFaulted && showError ? 1 : 0
                    }
                };

                errorFlow.AddIcon(FontAwesome.Solid.ExclamationCircle);
                errorFlow.AddText(" Background unavailable!");
            }
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

        public double VideoDuration { get; set; }

        public double CurrentTime { get; private set; }

        protected override void Update()
        {
            base.Update();

            if (manualClock != null && Clock.ElapsedFrameTime < 100)
            {
                // we want to avoid seeking as much as possible, because we care about performance, not sync.
                // to avoid seeking completely, we only increment out local clock when in an updating state.
                manualClock.CurrentTime += Clock.ElapsedFrameTime;
            }

            // Pass the current time to the property to utilize the setter.

            CurrentTime = manualClock?.CurrentTime ?? 0;

            if (!hasTriggeredCompletion && CurrentTime >= VideoDuration)
            {
                hasTriggeredCompletion = true;
                OnPlaybackComplete?.Invoke();
            }
        }
    }
}

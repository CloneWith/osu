// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Video;
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
        private BackgroundSource source;
        private string filename;
        private readonly bool drawFallbackGradient;
        private readonly bool showError;
        private readonly FillMode fillMode;
        private readonly BindableFloat backgroundDim = new BindableFloat();

        private Sprite? imageSprite;
        private Video? video;
        private ManualClock? manualClock;
        private OsuTextFlowContainer errorFlow = null!;
        private Box dimBox = null!;

        private bool needDetection;
        private TextureStore? textureStore;
        private TournamentVideoResourceStore? videoStore;

        public bool BackgroundAvailable => video != null || imageSprite != null;

        private readonly LadderInfo ladder;
        private readonly BackgroundType currentBackgroundType;

        /// <summary>
        /// Fetch background information from specified <paramref name="ladder"/> and try to display it.
        /// </summary>
        public TourneyBackground(BackgroundType backgroundType, LadderInfo ladder,
                                 bool drawFallbackGradient = false, bool showError = false,
                                 FillMode fillMode = FillMode.Fill)
        {
            this.ladder = ladder;
            currentBackgroundType = backgroundType;
            readMapping();

            this.drawFallbackGradient = drawFallbackGradient;
            this.showError = showError;
            this.fillMode = fillMode;
            backgroundDim.BindTo(ladder.BackgroundDim);

            // Subscribe changes
            ladder.BackgroundMap.BindCollectionChanged((_, _) =>
            {
                readMapping();
                reloadBackgroundResources();
            });
        }

        private void readMapping()
        {
            var mapping = ladder.BackgroundMap.LastOrDefault(v => v.Key == currentBackgroundType).Value;
            if (!EqualityComparer<BackgroundInfo>.Default.Equals(mapping, default))
            {
                source = mapping.Source;
                filename = mapping.Name ?? string.Empty;
            }
        }

        [BackgroundDependencyLoader]
        private void load(TournamentVideoResourceStore storage, TextureStore texStore)
        {
            videoStore = storage;
            textureStore = texStore;
            reloadBackgroundResources();

            // Add dim effect
            AddInternal(dimBox = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.Black,
                Alpha = backgroundDim.Value,
            });
            backgroundDim.BindValueChanged(e => dimBox.FadeTo(e.NewValue, 300, Easing.OutQuint));
        }

        /// <summary>
        /// Reload the background based on the current source and filename.
        /// </summary>
        private void reloadBackgroundResources()
        {
            // Clear old resources
            ClearInternal();
            bool isFaulted = false;

            if ((source == BackgroundSource.Image || needDetection) && textureStore != null)
            {
                var image = textureStore.Get($"Backgrounds/{filename}");
                if (image != null)
                {
                    needDetection = false;
                    InternalChild = imageSprite = new Sprite
                    {
                        RelativeSizeAxes = Axes.Both,
                        FillMode = fillMode,
                        Texture = image
                    };
                }
                else
                    isFaulted = !needDetection;
            }
            else if ((source == BackgroundSource.Video || needDetection) && videoStore != null)
            {
                var stream = videoStore.GetStream(filename);
                if (stream != null)
                {
                    needDetection = false;
                    InternalChild = video = new Video(stream, false)
                    {
                        RelativeSizeAxes = Axes.Both,
                        FillMode = fillMode,
                        Clock = new FramedClock(manualClock = new ManualClock()),
                        Loop = loop,
                    };
                }
                else isFaulted = true;
            }

            if (isFaulted || source == BackgroundSource.None)
            {
                InternalChildren = new Drawable[]
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

        /// <summary>
        /// Use specified <see cref="BackgroundInfo"/> to lookup and display a background.
        /// </summary>
        /// <remarks>This constructor is for background preview only, and doesn't support ladder-based features.</remarks>
        public TourneyBackground(BackgroundInfo info, bool drawFallbackGradient = true, bool showError = false, FillMode fillMode = FillMode.Fill)
        {
            source = info.Source;
            filename = info.Name;
            this.drawFallbackGradient = drawFallbackGradient;
            this.showError = showError;
            this.fillMode = fillMode;
        }

        /// <summary>
        /// Get the background with specified <paramref name="filename"/>, and detect the file type automatically.
        /// </summary>
        /// <remarks>This constructor is for background tests only, and doesn't support ladder-based features.</remarks>
        public TourneyBackground(string filename, bool drawFallbackGradient = true, bool showError = false, FillMode fillMode = FillMode.Fill)
        {
            this.filename = filename;
            this.drawFallbackGradient = drawFallbackGradient;
            this.showError = showError;
            this.fillMode = fillMode;
            needDetection = true;
        }
    }
}

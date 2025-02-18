// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
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

namespace osu.Game.Tournament.Components
{
    public partial class TourneyBackground : CompositeDrawable
    {
        private readonly BackgroundSource source;
        private readonly string filename;
        private readonly bool drawFallbackGradient;
        private readonly bool showError;

        private Sprite? imageSprite;
        private Video? video;
        private ManualClock? manualClock;
        private OsuTextFlowContainer errorFlow = null!;

        private bool needDetection;

        public bool BackgroundAvailable => video != null || imageSprite != null;

        public TourneyBackground(BackgroundType backgroundType, LadderInfo ladder,
                                 bool drawFallbackGradient = false, bool showError = false)
        {
            source = ladder.BackgroundMap.LastOrDefault(v => v.Key == backgroundType).Value.Source;
            filename = ladder.BackgroundMap.LastOrDefault(v => v.Key == backgroundType).Value.Name ?? string.Empty;
            this.drawFallbackGradient = drawFallbackGradient;
            this.showError = showError;

            // Reload the background as soon as the background mapping is changed.
            ladder.BackgroundMap.BindCollectionChanged((_, _) => Invalidate(Invalidation.Presence));
        }

        public TourneyBackground(BackgroundInfo info, bool drawFallbackGradient = true, bool showError = false)
        {
            source = info.Source;
            filename = info.Name;
            this.drawFallbackGradient = drawFallbackGradient;
            this.showError = showError;
        }

        public TourneyBackground(string filename, bool drawFallbackGradient = true, bool showError = false)
        {
            this.filename = filename;
            this.drawFallbackGradient = drawFallbackGradient;
            this.showError = showError;
            needDetection = true;
        }

        [BackgroundDependencyLoader]
        private void load(TournamentVideoResourceStore storage, TextureStore textureStore)
        {
            bool isFaulted = false;

            if (source == BackgroundSource.Image || needDetection)
            {
                var image = textureStore.Get($"Backgrounds/{filename}");

                if (image != null)
                {
                    needDetection = false;
                    InternalChild = imageSprite = new Sprite
                    {
                        RelativeSizeAxes = Axes.Both,
                        FillMode = FillMode.Fill,
                        Texture = image
                    };
                }
                else isFaulted = !needDetection;
            }
            else if (source == BackgroundSource.Video || needDetection)
            {
                var stream = storage.GetStream(filename);

                if (stream != null)
                {
                    needDetection = false;
                    InternalChild = video = new Video(stream, false)
                    {
                        RelativeSizeAxes = Axes.Both,
                        FillMode = FillMode.Fill,
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
    }
}

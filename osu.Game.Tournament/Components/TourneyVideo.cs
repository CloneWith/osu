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
using osu.Framework.Graphics.Video;
using osu.Framework.Timing;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Tournament.IO;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Components
{
    public partial class TourneyVideo : CompositeDrawable
    {
        /// <summary>
        /// Trigger when the video has completed playback.
        /// </summary>
        public event Action? OnPlaybackComplete;

        private bool hasTriggeredCompletion;

        private readonly string filename;
        private readonly bool drawFallbackGradient;
        private Video? video;
        private ManualClock? manualClock;
        private OsuTextFlowContainer errorFlow = null!;

        public bool VideoAvailable => video != null;

        public TourneyVideo(BackgroundVideo videoType, LadderInfo ladder, bool drawFallbackGradient = false)
        {
            filename = ladder.BackgroundVideoFiles.Last(v => v.Key == videoType).Value;
            this.drawFallbackGradient = drawFallbackGradient;
        }

        public TourneyVideo(string filename, bool drawFallbackGradient = false)
        {
            this.filename = filename;
            this.drawFallbackGradient = drawFallbackGradient;
        }

        [BackgroundDependencyLoader]
        private void load(TournamentVideoResourceStore storage)
        {
            var stream = storage.GetStream(filename);

            if (stream != null)
            {
                InternalChild = video = new Video(stream, false)
                {
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fit,
                    Clock = new FramedClock(manualClock = new ManualClock()),
                    Loop = loop,
                };
            }
            else
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
                        AutoSizeAxes = Axes.Both
                    }
                };

                errorFlow.AddIcon(FontAwesome.Solid.ExclamationCircle);
                errorFlow.AddText(" Video unavailable!");
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

        public double VideoDuration { get; set; } // In seconds
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

            // Pass the current time to the property to utilize the setter
            CurrentTime = manualClock?.CurrentTime ?? 0;

            if (!hasTriggeredCompletion && CurrentTime >= VideoDuration)
            {
                hasTriggeredCompletion = true;
                OnPlaybackComplete?.Invoke();
            }
        }
    }
}

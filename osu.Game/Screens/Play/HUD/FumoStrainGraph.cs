// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterfaceFumo;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    public partial class FumoStrainGraph : SongProgressBar
    {
        public int SectionGranularity
        {
            set
            {
                if (displayGranularity == value) return;

                displayGranularity = value;
                values = new float[displayGranularity];
                refresh();
            }
        }

        public int HorizontalSpacing
        {
            set
            {
                if (horizontalSpacing == value) return;

                horizontalSpacing = value;
                updateGraph();
            }
        }

        public int VerticalSpacing
        {
            set
            {
                if (verticalSpacing == value) return;

                verticalSpacing = value;
                updateGraph();
            }
        }

        public bool ShowBackground
        {
            set => background.FadeTo(value ? 0.3f : 0, 300, Easing.OutQuint);
        }

        public bool UseBackgroundGradient
        {
            set
            {
                if (useBackgroundGradient == value) return;

                useBackgroundGradient = value;
                background.FadeColour(useBackgroundGradient
                        ? ColourInfo.GradientVertical(Color4.Black.Opacity(0), backgroundColour.Opacity(0.9f))
                        : backgroundColour.Opacity(0.9f),
                    300, Easing.OutQuint);
            }
        }

        public Colour4 BackgroundColour
        {
            set
            {
                if (backgroundColour == value) return;

                backgroundColour = value;
                background.FadeColour(backgroundColour, 300, Easing.OutQuint);
            }
        }

        public Colour4 LineColour
        {
            set
            {
                if (lineColour == value) return;

                lineColour = value;
                drawablePath.FadeColour(lineColour, 300, Easing.OutQuint);
            }
        }

        private int displayGranularity = 10;
        private int horizontalSpacing = 5;
        private int verticalSpacing = 10;

        private bool useBackgroundGradient = true;
        private Colour4 backgroundColour = FumoColours.SeaBlue.Light;
        private Colour4 lineColour = Color4.White;

        private const int highest_point = 5;

        private readonly Box background;
        private readonly SliderPath path = new SliderPath();
        private readonly SmoothPath drawablePath;

        private IEnumerable<HitObject>? objects;
        private float[] values = new float[10];

        public IEnumerable<HitObject> Objects
        {
            set
            {
                objects = value;
                refresh();
            }
        }

        public FumoStrainGraph()
        {
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = useBackgroundGradient
                        ? ColourInfo.GradientVertical(Color4.Black.Opacity(0), backgroundColour.Opacity(0.9f))
                        : backgroundColour.Opacity(0.9f),
                    Alpha = 0,
                    Depth = float.MaxValue,
                },
                drawablePath = new SmoothPath
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    PathRadius = 2,
                    Colour = lineColour,
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            background.FadeTo(0.3f, 200, Easing.In);
        }

        private void refresh()
        {
            if (objects == null || !objects.Any())
                return;

            values = new float[displayGranularity];

            (double firstHit, double lastHit) = BeatmapExtensions.CalculatePlayableBounds(objects);

            if (lastHit == 0)
                lastHit = objects.Last().StartTime;

            double interval = (lastHit - firstHit + 1) / displayGranularity;

            foreach (var h in objects)
            {
                double endTime = h.GetEndTime();

                Debug.Assert(endTime >= h.StartTime);

                int startRange = (int)((h.StartTime - firstHit) / interval);
                int endRange = (int)((endTime - firstHit) / interval);
                for (int i = startRange; i <= endRange; i++)
                    values[i]++;
            }

            standardizeValues();
            updateGraph();
        }

        private void standardizeValues()
        {
            float minValue = values.Min();
            float maxValue = values.Max() - minValue;

            for (int i = 0; i < displayGranularity; i++)
            {
                values[i] = (values[i] - minValue) / maxValue * highest_point;
            }
        }

        private void updateGraph()
        {
            path.ControlPoints.Clear();

            for (int i = 0; i < displayGranularity; i++)
            {
                path.ControlPoints.Add(new PathControlPoint
                {
                    Position = new Vector2(i * horizontalSpacing, -values[i] * verticalSpacing),
                    Type = PathType.BEZIER,
                });
            }

            List<Vector2> vertices = new List<Vector2>();
            path.GetPathToProgress(vertices, 0, 1);

            drawablePath.Vertices = vertices;
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterfaceFumo;
using osu.Game.Rulesets.Objects;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    public partial class FumoSongProgress : SongProgress
    {
        [SettingSource("Show background")]
        public BindableBool ShowBackground { get; } = new BindableBool(true);

        [SettingSource("Enable background gradient")]
        public BindableBool UseBackgroundGradient { get; } = new BindableBool(true);

        [SettingSource("Background colour")]
        public BindableColour4 BackgroundColour { get; } = new BindableColour4(FumoColours.SeaBlue.Light);

        [SettingSource("Line colour")]
        public BindableColour4 LineColour { get; } = new BindableColour4(Color4.White);

        [SettingSource("Horizontal spacing")]
        public BindableInt HorizontalSpacing { get; } = new BindableInt(5)
        {
            MinValue = 3,
            MaxValue = 10,
        };

        [SettingSource("Vertical spacing")]
        public BindableInt VerticalSpacing { get; } = new BindableInt(10)
        {
            MinValue = 5,
            MaxValue = 20,
        };

        [SettingSource("Section granularity")]
        public BindableInt SectionGranularity { get; } = new BindableInt(50)
        {
            MinValue = 10,
            MaxValue = 200,
        };

        private readonly FumoStrainGraph graph;

        public FumoSongProgress()
        {
            AutoSizeAxes = Axes.Both;

            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;
            Masking = true;

            Child = new Container
            {
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Container
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        AutoSizeAxes = Axes.Both,
                        Masking = true,
                        Child = graph = new FumoStrainGraph
                        {
                            Name = "Difficulty graph",
                            Blending = BlendingParameters.Additive,
                            SectionGranularity = SectionGranularity.Value,
                            HorizontalSpacing = HorizontalSpacing.Value,
                            VerticalSpacing = VerticalSpacing.Value,
                            ShowBackground = ShowBackground.Value,
                            UseBackgroundGradient = UseBackgroundGradient.Value,
                            BackgroundColour = BackgroundColour.Value,
                            LineColour = LineColour.Value,
                        },
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            SectionGranularity.BindValueChanged(e => graph.SectionGranularity = e.NewValue);
            HorizontalSpacing.BindValueChanged(e => graph.HorizontalSpacing = e.NewValue);
            VerticalSpacing.BindValueChanged(e => graph.VerticalSpacing = e.NewValue);

            ShowBackground.BindValueChanged(e => graph.ShowBackground = e.NewValue);
            UseBackgroundGradient.BindValueChanged(e => graph.UseBackgroundGradient = e.NewValue);
            BackgroundColour.BindValueChanged(e => graph.BackgroundColour = e.NewValue);
            LineColour.BindValueChanged(e => graph.LineColour = e.NewValue);
        }

        protected override void UpdateObjects(IEnumerable<HitObject> objects)
        {
            graph.Objects = objects;
        }

        protected override void UpdateProgress(double progress, bool isIntro)
        {
        }
    }
}

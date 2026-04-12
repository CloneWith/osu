// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Graphics.UserInterfaceFumo
{
    public partial class FoldableSectionHeader : FillFlowContainer
    {
        protected override Container<Drawable> Content => contentFlow;

        private readonly FillFlowContainer contentFlow;

        private readonly BindableBool sectionShown = new BindableBool(true);

        public FoldableSectionHeader(LocalisableString title, HoverSampleSet sampleSet = HoverSampleSet.Button)
        {
            RelativeSizeAxes = Axes.X;
            Direction = FillDirection.Vertical;
            AutoSizeAxes = Axes.Y;
            AutoSizeEasing = Easing.OutQuint;
            AutoSizeDuration = 300;
            Spacing = new Vector2(5);

            InternalChildren = new Drawable[]
            {
                new GridContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    RowDimensions = [new Dimension(GridSizeMode.AutoSize)],
                    ColumnDimensions =
                    [
                        new Dimension(),
                        new Dimension(GridSizeMode.AutoSize),
                    ],
                    Content = new Drawable[][]
                    {
                        [
                            new SectionHeader(title),
                            new StateSwitchButton(sampleSet: sampleSet)
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Current = { BindTarget = sectionShown },
                                IdleIcon = FontAwesome.Solid.Expand,
                                ActiveIcon = FontAwesome.Solid.Compress,
                            },
                        ]
                    },
                },
                contentFlow = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(5),
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            sectionShown.BindValueChanged(e => contentFlow.FadeTo(e.NewValue ? 1 : 0, 300, Easing.OutQuint));
        }
    }
}

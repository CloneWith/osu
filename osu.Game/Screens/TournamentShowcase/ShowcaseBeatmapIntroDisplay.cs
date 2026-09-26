// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Models;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.TournamentShowcase
{
    /// <summary>
    /// A transient beatmap information panel centred on the screen, shown for a short while before a replay starts,
    /// and in place of the persistent wedge on the results screen of a secret map.
    /// </summary>
    public partial class ShowcaseBeatmapIntroDisplay : CompositeDrawable
    {
        /// <summary>
        /// How long the display takes to fade out.
        /// <br/>Shared with the screen driving this display so that the persistent wedge can take over at exactly the right moment.
        /// </summary>
        public const float CONCEAL_DURATION = 500;

        /// <summary>
        /// The beatmap to display information for.
        /// <br/>Kept in sync with the persistent wedge by <see cref="ShowcaseContainer"/>.
        /// </summary>
        public readonly Bindable<ShowcaseBeatmap?> Target = new Bindable<ShowcaseBeatmap?>();

        /// <summary>
        /// The size of the information panel. Matches the width of the persistent wedge it hands over to.
        /// </summary>
        private const float panel_width = 400;

        /// <summary>
        /// The horizontal distance the information panel slides in from.
        /// </summary>
        private const float slide_distance = 32;

        private const float separator_height = 50;

        private const float spacing = 32;

        private const float scale_from = 0.8f;

        private readonly OverlayColourScheme colourScheme;

        private readonly OverlayColourProvider colourProvider;

        private Container cardColumn = null!;
        private Drawable separator = null!;
        private Drawable detailsColumn = null!;
        private Drawable wedgesContainer = null!;

        public ShowcaseBeatmapIntroDisplay(OverlayColourScheme colourScheme)
        {
            this.colourScheme = colourScheme;

            colourProvider = new OverlayColourProvider(colourScheme);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;

            Alpha = 0;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Name = @"Background mask",
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.Black.Opacity(0.5f),
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Direction = FillDirection.Horizontal,
                    AutoSizeAxes = Axes.Both,
                    Spacing = new Vector2(spacing),
                    LayoutDuration = 500,
                    LayoutEasing = Easing.OutPow10,
                    Children = new[]
                    {
                        cardColumn = new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            AutoSizeAxes = Axes.Both,
                        },
                        separator = new Box
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(2, separator_height),
                            Scale = new Vector2(1, 0),
                            Alpha = 0,
                            Colour = colourProvider.Colour0,
                        },
                        detailsColumn = new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.None,
                            Width = panel_width,
                            AutoSizeAxes = Axes.Y,
                            Scale = new Vector2(scale_from),
                            Alpha = 0,
                            Child = wedgesContainer = new Container
                            {
                                RelativeSizeAxes = Axes.None,
                                Width = panel_width,
                                AutoSizeAxes = Axes.Y,
                                X = slide_distance,
                                Child = new ShowcaseBeatmapWedge
                                {
                                    RelativeSizeAxes = Axes.None,
                                    Width = panel_width,
                                },
                            },
                        },
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Target.BindValueChanged(targetChanged, true);
        }

        /// <summary>
        /// Build the card for the currently showcased beatmap.
        /// </summary>
        private void targetChanged(ValueChangedEvent<ShowcaseBeatmap?> target)
        {
            cardColumn.Clear();

            if (target.NewValue != null)
                cardColumn.Add(new ShowcaseBeatmapCard(target.NewValue, colourScheme));
        }

        /// <summary>
        /// Play the reveal choreography, keeping the display on screen until <see cref="Conceal"/> is called.
        /// </summary>
        public void Reveal()
        {
            separator.Alpha = 0;
            separator.Scale = new Vector2(1, 0);
            detailsColumn.Alpha = 0;
            detailsColumn.Scale = new Vector2(scale_from);
            wedgesContainer.X = slide_distance;

            Show();

            separator.FadeIn(800, Easing.OutPow10);
            separator.ScaleTo(Vector2.One, 1000, Easing.OutPow10);

            detailsColumn.Delay(200).FadeIn(800, Easing.OutPow10);
            detailsColumn.Delay(200).ScaleTo(Vector2.One, 1000, Easing.OutPow10);
            wedgesContainer.Delay(200).MoveToX(0, 1000, Easing.OutPow10);
        }

        /// <summary>
        /// Fade the display back out. Pass a duration of zero to reset it immediately.
        /// </summary>
        public void Conceal(double duration = CONCEAL_DURATION)
        {
            this.FadeOut(duration, Easing.OutQuint);

            wedgesContainer.MoveToX(slide_distance, duration, Easing.OutQuint);
        }
    }
}

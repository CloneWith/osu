// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Tournament.Localisation;
using osu.Game.Tournament.Localisation.Screens;
using osu.Game.Tournament.Models;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    public partial class RoundCounterLine : CompositeDrawable
    {
        public const int HEIGHT = 30;

        private Box counterBackground = null!;

        private TournamentSpriteText roundIndexText = null!;
        private TournamentSpriteText actionText = null!;

        [Resolved]
        private LadderInfo ladder { get; set; } = null!;

        public RoundCounterLine()
        {
            Height = HEIGHT;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new GridContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                ColumnDimensions = new[]
                {
                    new Dimension(),
                    new Dimension(GridSizeMode.Absolute, 120),
                    new Dimension(),
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new TournamentSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Text = BoardStrings.CurrentRound,
                            Font = OsuFont.Torus.With(weight: FontWeight.SemiBold, size: 18),
                            Padding = new MarginPadding { Left = 10 },
                            Shadow = false,
                        },
                        new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            CornerRadius = 5,
                            Children = new Drawable[]
                            {
                                counterBackground = new Box
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.Black,
                                },
                                roundIndexText = new TournamentSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Colour = Color4.White,
                                    Text = @"??",
                                    Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 18),
                                    Shadow = false,
                                },
                            }
                        },
                        actionText = new TournamentSpriteText
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Text = @"-",
                            Font = OsuFont.Torus.With(weight: FontWeight.SemiBold, size: 18),
                            Padding = new MarginPadding { Right = 10 },
                            Shadow = false,
                        }
                    }
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ladder.CurrentMatch.BindValueChanged(_ => updateDisplay(), true);
            ladder.CurrentMatch.Value?.CurrentRoundIndex.BindValueChanged(_ => updateDisplay());
            ladder.CurrentMatch.Value?.PreparationMode.BindValueChanged(_ => updateDisplay());
        }

        private void updateDisplay()
        {
            TournamentMatch? currentMatch = ladder.CurrentMatch.Value;

            FinishTransforms(true);

            if (currentMatch?.PreparationMode.Value != false)
            {
                counterBackground.FadeColour(Color4.Black, 500, Easing.OutQuint);
                roundIndexText.Text = @"??";
                actionText.Text = @"...";
                actionText.FadeColour(Color4.White, 500, Easing.OutQuint);
                return;
            }

            int index = currentMatch.CurrentRoundIndex.Value;
            LocalisableString teamString = TournamentGame.GetTeamString(currentMatch.CurrentTeam);

            counterBackground.FadeColour(TournamentGame.GetTeamColour(currentMatch.CurrentTeam), 500, Easing.OutQuint);
            roundIndexText.Text = index <= 0 ? @"..." : index.ToString();
            actionText.Text = BoardStrings.RoundActionPrompt(teamString,
                index <= 0 ? InstructionsStrings.BanShort : InstructionsStrings.PickShort);

            actionText.Colour = TournamentGame.GetTeamColour(currentMatch.CurrentTeam);
            actionText.FlashColour(Color4.White, 600, Easing.OutQuint);
        }
    }
}

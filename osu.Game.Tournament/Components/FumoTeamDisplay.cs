// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Tournament.Models;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    public partial class FumoTeamDisplay : FillFlowContainer
    {
        private const int loop_anim_duration = 1500;
        private const int loop_delay = 3000;

        private readonly Bindable<string> teamName = new Bindable<string>("Unknown Team");
        private readonly Bindable<string> teamSeed = new Bindable<string>("#?");

        private bool isActive;

        private readonly OsuSpriteText teamNameText;
        private readonly OsuSpriteText teamSeedText;
        private readonly Circle nameHeader;
        private readonly Box colourMask;
        private readonly SpriteIcon activeIcon;

        /// <summary>
        /// Is the specified team in an active state.
        /// </summary>
        public bool IsActive
        {
            get => isActive;
            set
            {
                if (isActive == value) return;

                isActive = value;
                if (isActive) Activate();
                else Deactivate();
            }
        }

        public FumoTeamDisplay(TournamentTeam? team, TeamColour colour)
        {
            if (team != null)
            {
                teamName.BindTo(team.FullName);
                teamSeed.BindTo(team.Seed);
            }

            var anchor = colour == TeamColour.Red ? Anchor.CentreLeft : Anchor.CentreRight;

            AutoSizeAxes = Axes.Both;
            Direction = FillDirection.Horizontal;
            Spacing = new Vector2(10);
            Children = new Drawable[]
            {
                new Container
                {
                    Anchor = anchor,
                    Origin = anchor,
                    Size = new Vector2(75, 54),
                    CornerRadius = 5,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new DrawableTeamFlag(team)
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                        colourMask = new Box
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Colour = TournamentGame.GetTeamColour(colour),
                            Alpha = 0,
                        },
                        activeIcon = new SpriteIcon
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(32),
                            Icon = FontAwesome.Solid.Flag,
                            Alpha = 0,
                        }
                    },
                },
                new FillFlowContainer
                {
                    Anchor = anchor,
                    Origin = anchor,
                    AutoSizeAxes = Axes.Both,
                    AutoSizeEasing = Easing.OutQuint,
                    AutoSizeDuration = 300,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(1),
                    Children = new Drawable[]
                    {
                        nameHeader = new Circle
                        {
                            Anchor = anchor,
                            Origin = anchor,
                            RelativeSizeAxes = Axes.X,
                            Height = 4,
                            Colour = TournamentGame.GetTeamColour(colour),
                        },
                        teamNameText = new OsuSpriteText
                        {
                            Anchor = anchor,
                            Origin = anchor,
                            Font = OsuFont.Torus.With(size: 24, weight: FontWeight.Bold),
                            Colour = TournamentGame.GetTeamColour(colour),
                        },
                        teamSeedText = new OsuSpriteText
                        {
                            Anchor = anchor,
                            Origin = anchor,
                            Font = OsuFont.Torus.With(size: 20, weight: FontWeight.SemiBold),
                        },
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            teamName.BindValueChanged(name => teamNameText.Text = name.NewValue, true);
            teamSeed.BindValueChanged(seed => teamSeedText.Text = seed.NewValue, true);
        }

        protected void Activate()
        {
            // Initialize
            colourMask.FadeOut();
            activeIcon.FadeOut().RotateTo(0).ScaleTo(1.5f);

            // Show at the first time
            nameHeader.FlashColour(Color4.White, 1000, Easing.OutQuint);
            teamNameText.FlashColour(Color4.White, 1000, Easing.OutQuint);
            colourMask.FadeTo(0.75f, 300, Easing.OutQuint);
            activeIcon.FadeIn(300, Easing.OutQuint)
                      .ScaleTo(1, 300, Easing.OutQuint)
                      .RotateTo(15, 500, Easing.OutQuint);

            // Looped animation
            using (BeginDelayedSequence(1500))
            {
                colourMask.FadeTo(0.75f, loop_anim_duration, Easing.OutQuint).Delay(loop_delay)
                          .FadeOut(loop_anim_duration, Easing.OutQuint).Delay(loop_delay).Loop();
                activeIcon.FadeIn(loop_anim_duration, Easing.OutQuint).Delay(loop_delay)
                          .FadeOut(loop_anim_duration, Easing.OutQuint).Delay(loop_delay).Loop();
            }
        }

        protected void Deactivate()
        {
            colourMask.ClearTransforms();
            activeIcon.ClearTransforms();

            colourMask.FadeOut(300, Easing.OutQuint);
            activeIcon.ScaleTo(1.5f, 300, Easing.OutQuint).FadeOut(300, Easing.OutQuint);
        }
    }
}

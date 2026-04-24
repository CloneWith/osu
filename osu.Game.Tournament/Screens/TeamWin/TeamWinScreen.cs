// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceFumo;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Localisation;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Board.Components;
using osu.Game.Tournament.Screens.Gameplay.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.TeamWin
{
    public partial class TeamWinScreen : TournamentMatchScreen
    {
        private Container mainContainer = null!;
        private Container firstStageContainer = null!;

        private readonly Bindable<bool> currentCompleted = new Bindable<bool>();

        private TourneyBackground blueWinBackground = null!;
        private TourneyBackground redWinBackground = null!;
        private TourneyBackground mainBackground = null!;

        private FumoLogo logo = null!;
        private FillFlowContainer matchInfoFlow = null!;
        private MatchRoundDisplay roundDisplay = null!;
        private GridContainer roundLine = null!;
        private FillFlowContainer drawDisplayFlow = null!;
        private RotatingDisplayContainer drawTextDisplay = null!;

        private TeamGradientBackground gradientBackground = null!;
        private Box colourMask = null!;

        private SpriteIcon drawIcon = null!;
        private TrianglesV2 winnerTriangles = null!;

        private Container thanksContainer = null!;
        private TournamentSpriteText thanksText = null!;
        private SpriteIcon heart = null!;

        private ScheduledDelegate? scheduledApplause;
        private Sample? applauseSample;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            RelativeSizeAxes = Axes.Both;

            applauseSample = audio.Samples.Get(@"Results/applause-s");

            InternalChildren = new Drawable[]
            {
                blueWinBackground = new TourneyBackground(BackgroundType.BlueWin)
                {
                    Alpha = 0,
                    RelativeSizeAxes = Axes.Both,
                    Loop = true,
                },
                redWinBackground = new TourneyBackground(BackgroundType.RedWin)
                {
                    Alpha = 0,
                    RelativeSizeAxes = Axes.Both,
                    Loop = true,
                },
                mainBackground = new TourneyBackground(BackgroundType.Draw)
                {
                    Alpha = 1,
                    RelativeSizeAxes = Axes.Both,
                    Loop = true,
                },
                gradientBackground = new TeamGradientBackground(),
                colourMask = new Box
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.3f,
                    Colour = Color4.Transparent,
                    Shear = OsuGame.SHEAR,
                },
                new TrianglesV2
                {
                    RelativeSizeAxes = Axes.Both,
                    ScaleAdjust = 1.5f,
                    Alpha = 0.6f,
                },
                firstStageContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                },
                mainContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                },
                new ControlPanel
                {
                    Children = new Drawable[]
                    {
                        new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = BaseStrings.Refresh,
                            Action = update
                        },
                    },
                },
            };

            currentCompleted.BindValueChanged(_ => ResetSelectStatus());
        }

        protected override void OnFirstSelected()
        {
            base.OnFirstSelected();
            update();
        }

        protected override void CurrentMatchChanged(ValueChangedEvent<TournamentMatch?> match)
        {
            base.CurrentMatchChanged(match);

            currentCompleted.UnbindBindings();

            if (match.NewValue == null)
                return;

            currentCompleted.BindTo(match.NewValue.Completed);
            ResetSelectStatus();
        }

        private bool firstDisplay = true;

        private void update() => Scheduler.AddOnce(() =>
        {
            scheduledApplause?.Cancel();
            var match = CurrentMatch.Value;

            redWinBackground.Alpha = match?.WinnerColour == TeamColour.Red ? 1 : 0;
            blueWinBackground.Alpha = match?.WinnerColour == TeamColour.Blue ? 1 : 0;
            gradientBackground.FadeIn();
            firstStageContainer.FadeOut();
            colourMask.FadeOut();

            firstStageContainer.Children = new Drawable[]
            {
                logo = new FumoLogo("header-logo")
                {
                    Anchor = Anchor.Centre,
                    Scale = new Vector2(0.8f),
                    Triangles = false,
                },
                matchInfoFlow = new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.TopCentre,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(5),
                    Alpha = 0,
                    Children = new Drawable[]
                    {
                        new TournamentSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.Torus.With(size: 42, weight: FontWeight.Bold),
                            Text = LadderInfo.FullName.Value,
                        },
                        roundDisplay = new MatchRoundDisplay
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Alpha = 0,
                            Scale = new Vector2(0.5f),
                        },
                        roundLine = new GridContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            AutoSizeAxes = Axes.Both,
                            Alpha = 0,
                            Scale = new Vector2(1.2f),
                            RowDimensions = [new Dimension(GridSizeMode.AutoSize)],
                            ColumnDimensions =
                            [
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension(GridSizeMode.AutoSize),
                            ],
                            Content = new Drawable[][]
                            {
                                [
                                    new DrawableTeamLine(match?.Team1.Value, TeamColour.Red),
                                    new SpriteIcon
                                    {
                                        Size = new Vector2(24),
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Shadow = false,
                                        Icon = match?.Round.Value?.UseBoard.Value == true
                                            ? FontAwesome.Solid.ChessBoard
                                            : FontAwesome.Solid.Trophy,
                                        Colour = FumoColours.SunshineYellow.Regular,
                                        Margin = new MarginPadding { Horizontal = 20 },
                                    },
                                    new DrawableTeamLine(match?.Team2.Value, TeamColour.Blue),
                                ],
                            },
                        },
                    },
                },
                drawDisplayFlow = new FillFlowContainer
                {
                    Name = @"Draw indicator",
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Y = 100,
                    AutoSizeAxes = Axes.Both,
                    AutoSizeEasing = Easing.OutQuint,
                    AutoSizeDuration = 300,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(10),
                    Alpha = 0,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(24),
                            Children = new Drawable[]
                            {
                                drawIcon = new SpriteIcon
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Size = new Vector2(24),
                                    Icon = FontAwesome.Solid.HourglassHalf,
                                }
                            },
                        },
                        drawTextDisplay = new RotatingDisplayContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            AutoSizeAxes = Axes.Both,
                            CrossAnimation = true,
                            TransformLength = 1000,
                        },
                    },
                }
            };

            using (BeginDelayedSequence(1000))
            {
                firstStageContainer.FadeIn(2000, Easing.OutSine);

                using (BeginDelayedSequence(1500))
                {
                    logo.MoveToY(-100, 1500, Easing.OutQuint);
                    matchInfoFlow.FadeIn(900, Easing.OutQuint)
                                 .MoveToY(150, 1500, Easing.OutQuint);
                    roundDisplay.Delay(150).FadeIn(900, Easing.OutQuint);
                    roundLine.Delay(300).FadeIn(900, Easing.OutQuint);
                }
            }

            if (match?.Winner == null)
            {
                mainContainer.Clear();

                drawTextDisplay.AddLayers([
                    new TournamentSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = "胜负未决 请坐和放宽",
                        Font = OsuFont.Torus.With(size: 22, weight: FontWeight.SemiBold),
                    },
                    new TournamentSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = "Who would win? Sit down and relax...",
                        Font = OsuFont.TorusAlternate.With(size: 22, weight: FontWeight.SemiBold),
                    },
                ]);

                drawTextDisplay.Start(true);

                using (BeginDelayedSequence(3500 + 1000))
                {
                    drawIcon.Delay(800).RotateTo(0)
                            .Then().RotateTo(360 * 5, 5000, Easing.InOutExpo)
                            .Loop(3000);

                    using (BeginDelayedSequence(2000))
                    {
                        drawDisplayFlow.MoveToY(-50, 1500, Easing.OutQuint);
                        drawDisplayFlow.FadeInFromZero(500, Easing.OutQuint);
                    }
                }
            }
            else
            {
                if (firstDisplay)
                {
                    if (match.WinnerColour == TeamColour.Red)
                        redWinBackground.Reset();
                    else
                        blueWinBackground.Reset();
                    firstDisplay = false;
                }

                redWinBackground.Alpha = match.WinnerColour == TeamColour.Red ? 1 : 0;
                blueWinBackground.Alpha = match.WinnerColour == TeamColour.Blue ? 1 : 0;
                mainBackground.Alpha = 1;

                mainContainer.Children = new Drawable[]
                {
                    winnerTriangles = new TrianglesV2
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        ScaleAdjust = 1.1f,
                        Colour = match.WinnerColour == TeamColour.Red ? FumoColours.FlandreRed.Light : FumoColours.SeaBlue.Light,
                        Alpha = 0,
                    },
                    new DrawableTeamFlag(match.Winner)
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Position = new Vector2(-50, 50),
                        Scale = new Vector2(2f),
                        EdgeEffect = new EdgeEffectParameters
                        {
                            Type = EdgeEffectType.Glow,
                            Colour = TournamentExtensions.GetTeamColour(match.WinnerColour).MultiplyAlpha(0.5f),
                            Radius = 10,
                        },
                    },
                    new DrawableTeamTitleWithHeader(match.Winner, match.WinnerColour)
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Position = new Vector2(-50, 150),
                    },
                    new FumoChessBoard
                    {
                        Anchor = Anchor.BottomRight,
                        Origin = Anchor.BottomRight,
                        Position = new Vector2(-50, -50),
                        Scale = new Vector2(0.5f),
                    },
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopLeft,
                        Position = new Vector2(50, 50),
                        Children = new Drawable[]
                        {
                            new RoundDisplay(match)
                            {
                                Margin = new MarginPadding { Bottom = 30 },
                            },
                            new TournamentSpriteText
                            {
                                Text = "Winner",
                                Font = OsuFont.KaushanScript.With(size: 80),
                                Margin = new MarginPadding { Bottom = 30 },
                            },
                            new DrawableTeamWithPlayers(match.Winner, match.WinnerColour,
                                autoAdjust: false, hideHeader: true),
                        },
                    },
                    thanksContainer = new Container
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Position = new Vector2(50, 100),
                        AutoSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = 10,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = new OsuColour().Pink3.Opacity(0.6f),
                            },
                            new FillFlowContainer
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                AutoSizeAxes = Axes.Both,
                                AutoSizeEasing = Easing.OutExpo,
                                AutoSizeDuration = 900,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(10),
                                Margin = new MarginPadding { Horizontal = 15, Vertical = 10 },
                                Children = new Drawable[]
                                {
                                    new Container
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Size = new Vector2(24),
                                        Child = heart = new SpriteIcon
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Size = new Vector2(20),
                                            Icon = FontAwesome.Solid.Heart,
                                            Colour = new OsuColour().Pink,
                                        }
                                    },
                                    thanksText = new TournamentSpriteText
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Alpha = 0,
                                        Text = BaseStrings.Thanks,
                                        Font = OsuFont.Torus.With(size: 20, weight: FontWeight.SemiBold),
                                    },
                                },
                            },
                        },
                    },
                };

                mainContainer.FadeOut();

                scheduledApplause = Scheduler.AddDelayed(() =>
                {
                    applauseSample?.Play();
                }, 6000);

                using (BeginDelayedSequence(3500 + 1500))
                {
                    firstStageContainer.FadeOut(1500, Easing.OutQuint);

                    using (BeginDelayedSequence(1500))
                    {
                        gradientBackground.FadeOut(500, Easing.OutQuint);
                        mainBackground.FadeOut(1000, Easing.OutQuint);
                        if (match.WinnerColour == TeamColour.Red)
                            redWinBackground.FadeIn(1000, Easing.OutQuint);
                        else
                            blueWinBackground.FadeIn(1000, Easing.OutQuint);
                        mainContainer.FadeIn(1600, Easing.OutQuint);
                        colourMask.FadeTo(0.6f, 1500, Easing.OutQuint);
                        Color4 targetColour = TournamentExtensions.GetTeamColour(match.WinnerColour);
                        colourMask.FadeColour(ColourInfo.GradientHorizontal(targetColour,
                            targetColour.Opacity(0)), 2000, Easing.OutQuint);
                        winnerTriangles.Delay(1000).FadeTo(0.6f, 2000, Easing.OutQuint);

                        using (BeginDelayedSequence(3000))
                        {
                            heart.ScaleTo(1.1f, 100, Easing.InExpo).Then().ScaleTo(0.9f, 900, Easing.OutQuint)
                                 .Loop();
                            thanksContainer.MoveToY(-20, 1500, Easing.OutQuint);
                            thanksText.Delay(1000).FadeIn(1000, Easing.OutSine);
                        }
                    }
                }
            }
        });
    }
}

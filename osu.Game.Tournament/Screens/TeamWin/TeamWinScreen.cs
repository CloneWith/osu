// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Board.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.TeamWin
{
    public partial class TeamWinScreen : TournamentMatchScreen
    {
        private Container mainContainer = null!;
        private Container transferContainer = null!;
        private Container altContainer = null!;

        private readonly Bindable<bool> currentCompleted = new Bindable<bool>();

        private TourneyBackground blueWinBackground = null!;
        private TourneyBackground redWinBackground = null!;
        private TourneyBackground mainBackground = null!;

        private Container? symbolContainer;

        private TournamentSpriteText captionMainText = null!;
        private TournamentSpriteText captionMainCaption = null!;
        private TournamentSpriteText captionSubText = null!;
        private TournamentSpriteText captionSubCaption = null!;

        private TournamentSpriteText winMainText = null!;
        private TournamentSpriteText winSubText = null!;

        private EmptyBox colourMask = null!;
        private EmptyBox flash = null!;

        private SpriteIcon redIcon = null!;
        private SpriteIcon drawIcon = null!;
        private SpriteIcon blueIcon = null!;

        private Sprite spinner = null!;

        private Triangles winnerTriangles = null!;

        private SimpleAnimatedBoard? animatedBoard;

        private Sprite? banner;

        private TextureStore textureStore = null!;

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            textureStore = textures;
            RelativeSizeAxes = Axes.Both;

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
                colourMask = new EmptyBox
                {
                    RelativeSizeAxes = Axes.Both,
                    BoxColour = Color4.White,
                    Alpha = 0.4f,
                },
                new TrianglesV2
                {
                    RelativeSizeAxes = Axes.Both,
                    ScaleAdjust = 1.5f,
                    Alpha = 0.6f,
                },
                transferContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        winMainText = new TournamentSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = "胜负已定...",
                            X = -250,
                            Y = 250 - 25,
                            Font = OsuFont.HarmonyOSSans.With(size: 64, weight: FontWeight.Bold),
                            Alpha = 0,
                        },
                        winSubText = new TournamentSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = "...最后的获胜队是...",
                            X = 250,
                            Y = 250 + 25,
                            Font = OsuFont.HarmonyOSSans.With(size: 64, weight: FontWeight.Bold),
                            Alpha = 0,
                        }
                    },
                    Alpha = 0,
                },
                altContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                },
                mainContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                },
                flash = new EmptyBox
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.White,
                    Alpha = 0,
                },
                new ControlPanel
                {
                    Children = new Drawable[]
                    {
                        new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Refresh",
                            Action = update
                        },
                    },
                },
            };

            currentCompleted.BindValueChanged(_ => update());
            update();
        }

        protected override void CurrentMatchChanged(ValueChangedEvent<TournamentMatch?> match)
        {
            base.CurrentMatchChanged(match);

            currentCompleted.UnbindBindings();

            if (match.NewValue == null)
                return;

            currentCompleted.BindTo(match.NewValue.Completed);
            update();
        }

        private bool firstDisplay = true;

        private void update() => Scheduler.AddOnce(() =>
        {
            var match = CurrentMatch.Value;

            redWinBackground.Alpha = match?.WinnerColour == TeamColour.Red ? 1 : 0;
            blueWinBackground.Alpha = match?.WinnerColour == TeamColour.Blue ? 1 : 0;
            colourMask.FadeOut();

            animatedBoard?.Expire();

            if (match?.Winner == null)
            {
                mainContainer.Clear();

                altContainer.Children = new Drawable[]
                {
                    symbolContainer = new Container
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        // Y = 100,
                        AutoSizeAxes = Axes.Both,
                        Alpha = 0,
                        Children = new Drawable[]
                        {
                            redIcon = new SpriteIcon
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                X = -250,
                                Icon = FontAwesome.Solid.Trophy,
                                Colour = new OsuColour().Pink1,
                                Size = new Vector2(70),
                                Alpha = 0,
                            },
                            drawIcon = new SpriteIcon
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Icon = FontAwesome.Solid.Question,
                                Size = new Vector2(52),
                                Alpha = 0,
                            },
                            blueIcon = new SpriteIcon
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                X = 250,
                                Icon = FontAwesome.Solid.Trophy,
                                Colour = Color4.SkyBlue,
                                Size = new Vector2(70),
                                Alpha = 0,
                            },
                            new DrawableTeamFlag(CurrentMatch.Value?.Team1.Value)
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Position = new Vector2(-300, 10),
                                Scale = new Vector2(2f),
                                EdgeEffect = new EdgeEffectParameters
                                {
                                    Type = EdgeEffectType.Glow,
                                    Colour = new OsuColour().Pink1.Opacity(0.5f),
                                    Radius = 10,
                                },
                            },
                            new DrawableTeamFlag(CurrentMatch.Value?.Team2.Value)
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Position = new Vector2(300, 10),
                                Scale = new Vector2(2f),
                                EdgeEffect = new EdgeEffectParameters
                                {
                                    Type = EdgeEffectType.Glow,
                                    Colour = Color4.SkyBlue.Opacity(0.5f),
                                    Radius = 10,
                                },
                            },
                        }
                    },
                    new FillFlowContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Name = @"Draw State Caption",
                        Y = 200,
                        AutoSizeAxes = Axes.Both,
                        AutoSizeEasing = Easing.OutQuint,
                        AutoSizeDuration = 300,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(10),
                        Children = new Drawable[]
                        {
                            captionMainText = new TournamentSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = "胜负未决",
                                Font = OsuFont.HarmonyOSSans.With(size: 70, weight: FontWeight.Bold),
                                Alpha = 0,
                            },
                            captionMainCaption = new TournamentSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = "Who would win?",
                                Font = OsuFont.TorusAlternate.With(size: 64, weight: FontWeight.Bold),
                                Alpha = 0,
                            },
                            captionSubText = new TournamentSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = "请坐和放宽",
                                Font = OsuFont.HarmonyOSSans.With(size: 60, weight: FontWeight.Regular),
                                Alpha = 0,
                            },
                            captionSubCaption = new TournamentSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = "Sit down and relax...",
                                Font = OsuFont.TorusAlternate.With(size: 55, weight: FontWeight.Regular),
                                Alpha = 0,
                            },
                        }
                    },
                    banner = new Sprite
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Y = 250,
                        Texture = textureStore.Get("Icons/usercard-default"),
                        Scale = new Vector2(0.5f),
                        Alpha = 0,
                    }
                };

                using (BeginDelayedSequence(1500))
                {
                    redIcon.FadeInFromZero(500, Easing.OutQuint);
                    blueIcon.FadeInFromZero(500, Easing.OutQuint);
                    redIcon.MoveToX(-100, 2000, Easing.OutQuint);
                    blueIcon.MoveToX(100, 2000, Easing.OutQuint);
                    drawIcon.Delay(800).FadeInFromZero(1000, Easing.OutQuint);
                    symbolContainer.FadeInFromZero(500, Easing.OutQuint);
                    symbolContainer.MoveToY(100, 100, Easing.OutQuint);

                    using (BeginDelayedSequence(2000))
                    {
                        captionMainText.FadeInFromZero(500, Easing.OutQuint);
                        captionMainCaption.Delay(500).FadeInFromZero(500, Easing.OutQuint);
                        captionSubText.FadeInFromZero(500, Easing.OutQuint);
                        captionSubCaption.Delay(500).FadeInFromZero(500, Easing.OutQuint);

                        using (BeginDelayedSequence(1000))
                        {
                            banner.FadeInFromZero(3000, Easing.OutQuint);
                        }
                    }
                }
            }
            else
            {
                altContainer.Clear();
                symbolContainer?.Clear();
                banner?.FadeOut();

                AddInternal(animatedBoard = new SimpleAnimatedBoard(CurrentMatch)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                });

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

                transferContainer.Show();

                mainContainer.Children = new Drawable[]
                {
                    winnerTriangles = new Triangles
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        TriangleScale = 0.9f,
                        ColourDark = match.WinnerColour == TeamColour.Red ? new OsuColour().Pink1 : Color4.DeepSkyBlue,
                        ColourLight = match.WinnerColour == TeamColour.Red ? new OsuColour().Pink2 : Color4.SkyBlue,
                        Alpha = 0,
                    },
                    spinner = new Sprite
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Position = new Vector2(-300, 10),
                        Texture = textureStore.Get(@"MedalSplash/disc-spin"),
                        Colour = match.WinnerColour == TeamColour.Red ? new OsuColour().Pink1 : Color4.SkyBlue,
                        Alpha = 0.6f,
                    },
                    new DrawableTeamFlag(match.Winner)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Position = new Vector2(-300, 10),
                        Scale = new Vector2(2f),
                        EdgeEffect = new EdgeEffectParameters
                        {
                            Type = EdgeEffectType.Glow,
                            Colour = (match.WinnerColour == TeamColour.Red
                                ? new OsuColour().Pink1
                                : (match.WinnerColour == TeamColour.Blue ? Color4.SkyBlue : Color4.Yellow)).Opacity(0.5f),
                            Radius = 10,
                        },
                    },
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        X = 260,
                        Children = new Drawable[]
                        {
                            new RoundDisplay(match)
                            {
                                Margin = new MarginPadding { Bottom = 30 },
                            },
                            new TournamentSpriteText
                            {
                                Text = "WINNER",
                                Font = OsuFont.Torus.With(size: 100, weight: FontWeight.Bold),
                                Margin = new MarginPadding { Bottom = 50 },
                            },
                            new DrawableTeamWithPlayers(match.Winner, match.WinnerColour, autoAdjust: false)
                        }
                    },
                };

                mainContainer.FadeOut();

                using (BeginDelayedSequence(4500))
                {
                    animatedBoard.ScaleTo(0.85f, 1000, Easing.OutQuint);
                    animatedBoard.MoveToY(-100, 2500, Easing.OutQuint).Delay(4000).FadeOut(500, Easing.OutQuint);

                    using (BeginDelayedSequence(2000))
                    {
                        winMainText.FadeInFromZero(500, Easing.OutQuint);
                        winMainText.MoveToX(-150, 3000, Easing.OutQuint).Then().FadeOut(500, Easing.OutQuint);
                        winSubText.FadeInFromZero(500, Easing.OutQuint);
                        winSubText.MoveToX(150, 3000, Easing.OutQuint);
                        winSubText.Delay(1000).FadeColour(match.WinnerColour == TeamColour.Red ? new OsuColour().Pink1 : Color4.SkyBlue, 2000, Easing.OutQuint)
                                  .Then().FadeOut(500, Easing.OutQuint);
                    }

                    using (BeginDelayedSequence(5000))
                    {
                        flash.FadeOutFromOne(6000, Easing.OutQuint);
                        mainBackground.FadeOut(1000, Easing.OutQuint);
                        if (match.WinnerColour == TeamColour.Red)
                            redWinBackground.FadeIn(1000, Easing.OutQuint);
                        else
                            blueWinBackground.FadeIn(1000, Easing.OutQuint);
                        mainContainer.FadeIn(1600, Easing.OutQuint);
                        colourMask.FadeTo(0.4f, 1500, Easing.OutQuint);
                        colourMask.FadeColour(match.WinnerColour == TeamColour.Red ? new OsuColour().Pink1
                            : match.WinnerColour == TeamColour.Blue ? Color4.SkyBlue : Color4.White, 2000, Easing.OutQuint);
                        spinner.Spin(36000, RotationDirection.Clockwise);
                        winnerTriangles.Delay(1000).FadeTo(0.6f, 2000, Easing.OutQuint);
                    }
                }
            }
        });
    }
}

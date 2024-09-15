// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Testing;
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Screens;
using osu.Game.Tournament.Screens.Drawings;
using osu.Game.Tournament.Screens.Editors;
using osu.Game.Tournament.Screens.Gameplay;
using osu.Game.Tournament.Screens.Ladder;
using osu.Game.Tournament.Screens.MapPool;
using osu.Game.Tournament.Screens.Schedule;
using osu.Game.Tournament.Screens.Setup;
using osu.Game.Tournament.Screens.Showcase;
using osu.Game.Tournament.Screens.TeamIntro;
using osu.Game.Tournament.Screens.TeamWin;
using osu.Game.Tournament.Screens.Board;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;
using osu.Game.Tournament.Models;
using osu.Framework.Bindables;

namespace osu.Game.Tournament
{
    [Cached]
    public partial class TournamentSceneManager : CompositeDrawable
    {
        private Container screens = null!;
        private TourneyVideo video = null!;
        private BindableList<IAnimation> animationQueue = new BindableList<IAnimation>();

        private IAnimation? currentAnimation;

        public const int CONTROL_AREA_WIDTH = 200;

        public const int STREAM_AREA_WIDTH = 1366;
        public const int STREAM_AREA_HEIGHT = (int)(STREAM_AREA_WIDTH / ASPECT_RATIO);

        public const float ASPECT_RATIO = 16 / 9f;

        public const int REQUIRED_WIDTH = CONTROL_AREA_WIDTH * 2 + STREAM_AREA_WIDTH;

        public bool IsChatShown = true;

        [Cached]
        private TournamentMatchChatDisplay chat = new TournamentMatchChatDisplay(RelativeSizeY: true);

        private Container chatContainer = new Container
        {
            Anchor = Anchor.TopLeft,
            Origin = Anchor.TopLeft,
            RelativeSizeAxes = Axes.None,
            Width = STREAM_AREA_WIDTH,
            Height = 480,
        };

        private FillFlowContainer buttons = null!;

        private TournamentScreen middle = null!;

        public TournamentSceneManager()
        {
            RelativeSizeAxes = Axes.Both;

            animationQueue.BindCollectionChanged((_, arg) =>
            {
                if (!animationQueue.Any())
                    return;

                if (currentAnimation == null || currentAnimation.Status == AnimationStatus.Complete)
                {
                    var animation = animationQueue.First();
                    startAnimation(animation);
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Y,
                    X = CONTROL_AREA_WIDTH,
                    FillMode = FillMode.Fit,
                    FillAspectRatio = ASPECT_RATIO,
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Width = STREAM_AREA_WIDTH,
                    //Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = new Color4(20, 20, 20, 255),
                            Anchor = Anchor.TopRight,
                            RelativeSizeAxes = Axes.Both,
                            Width = 10,
                        },
                        video = new TourneyVideo("main", true)
                        {
                            Loop = true,
                            RelativeSizeAxes = Axes.Both,
                        },
                        screens = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new SetupScreen(),
                                new ScheduleScreen(),
                                new LadderScreen(),
                                new LadderEditorScreen(),
                                new TeamEditorScreen(),
                                new RoundEditorScreen(),
                                new ShowcaseScreen(),
                                new MapPoolScreen(),
                                new TeamIntroScreen(),
                                new SeedingScreen(),
                                new DrawingsScreen(),
                                new GameplayScreen(),
                                new TeamWinScreen(),
                                middle = new BoardScreen(),
                                new EXBoardScreen()
                            }
                        },
                        chatContainer = new Container
                        {
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            RelativeSizeAxes = Axes.None,
                            Width = STREAM_AREA_WIDTH,
                            Height = 480,
                            Child = chat
                        },
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = CONTROL_AREA_WIDTH,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = Color4.Black,
                            RelativeSizeAxes = Axes.Both,
                        },
                        buttons = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(5),
                            Padding = new MarginPadding(5),
                            Children = new Drawable[]
                            {
                                new ScreenButton(typeof(SetupScreen)) { Text = "Setup", RequestSelection = SetScreen },
                                new Separator(),
                                new ScreenButton(typeof(TeamEditorScreen)) { Text = "Team Editor", RequestSelection = SetScreen },
                                new ScreenButton(typeof(RoundEditorScreen)) { Text = "Rounds Editor", RequestSelection = SetScreen },
                                new ScreenButton(typeof(LadderEditorScreen)) { Text = "Bracket Editor", RequestSelection = SetScreen },
                                new Separator(),
                                new ScreenButton(typeof(ScheduleScreen), Key.S) { Text = "Schedule", RequestSelection = SetScreen },
                                new ScreenButton(typeof(LadderScreen), Key.R) { Text = "Bracket", RequestSelection = SetScreen },
                                new Separator(),
                                new ScreenButton(typeof(TeamIntroScreen), Key.I) { Text = "Team Intro", RequestSelection = SetScreen },
                                new ScreenButton(typeof(SeedingScreen), Key.D) { Text = "Seeding", RequestSelection = SetScreen },
                                new Separator(),
                                /* Append new screen "board" for board display */
                                new ScreenButton(typeof(BoardScreen), Key.B) { Text = "Board", RequestSelection = SetScreen },
                                new ScreenButton(typeof(EXBoardScreen), Key.E) { Text = "EX Stage", RequestSelection = SetScreen },
                                new ScreenButton(typeof(MapPoolScreen), Key.M) { Text = "Map Pool", RequestSelection = SetScreen },
                                new ScreenButton(typeof(GameplayScreen), Key.G) { Text = "Gameplay", RequestSelection = SetScreen },
                                new Separator(),
                                new ScreenButton(typeof(TeamWinScreen), Key.W) { Text = "Win", RequestSelection = SetScreen },
                                new Separator(),
                                new ScreenButton(typeof(DrawingsScreen)) { Text = "Drawings", RequestSelection = SetScreen },
                                new ScreenButton(typeof(ShowcaseScreen)) { Text = "Showcase", RequestSelection = SetScreen },
                            }
                        },
                    },
                },
            };

            foreach (var drawable in screens)
                drawable.Hide();

            SetScreen(typeof(SetupScreen));
        }

        private float depth;

        private Drawable? currentScreen;
        private ScheduledDelegate? scheduledHide;

        private Drawable? temporaryScreen;

        public void SetScreen(Drawable screen)
        {
            currentScreen?.Hide();
            currentScreen = null;

            screens.Add(temporaryScreen = screen);
        }

        public void SetScreen(Type screenType)
        {
            temporaryScreen?.Expire();

            var target = screens.FirstOrDefault(s => s.GetType() == screenType);

            if (target == null || currentScreen == target) return;

            if (scheduledHide?.Completed == false)
            {
                scheduledHide.RunTask();
                scheduledHide.Cancel(); // see https://github.com/ppy/osu-framework/issues/2967
                scheduledHide = null;
            }

            var lastScreen = currentScreen;
            currentScreen = target;

            if (currentScreen.ChildrenOfType<TourneyVideo>().FirstOrDefault()?.VideoAvailable == true)
            {
                video.FadeOut(200);

                // delay the hide to avoid a double-fade transition.
                scheduledHide = Scheduler.AddDelayed(() => lastScreen?.Hide(), TournamentScreen.FADE_DELAY);
            }
            else
            {
                lastScreen?.Hide();
                video.Show();
            }

            screens.ChangeChildDepth(currentScreen, depth--);
            currentScreen.Show();

            var team1List = new DrawableTeamPlayerList(middle.LadderInfo.CurrentMatch.Value?.Team1.Value);

            switch (currentScreen)
            {
                case MapPoolScreen:
                    chatContainer.FadeIn(TournamentScreen.FADE_DELAY);
                    chatContainer.ResizeWidthTo(STREAM_AREA_WIDTH, 500, Easing.OutQuint);
                    chatContainer.ResizeHeightTo(144, 500, Easing.OutQuint);
                    chatContainer.MoveTo(new Vector2(0, STREAM_AREA_HEIGHT - 144), 500, Easing.OutQuint);
                    chat.ChangeRadius(0);
                    break;

                case GameplayScreen:
                    chatContainer.FadeIn(TournamentScreen.FADE_DELAY);
                    chatContainer.ResizeWidthTo(STREAM_AREA_WIDTH / 2, 500, Easing.OutQuint);
                    chatContainer.ResizeHeightTo(144, 500, Easing.OutQuint);
                    chatContainer.MoveTo(new Vector2(0, IsChatShown ? STREAM_AREA_HEIGHT - 144 : STREAM_AREA_HEIGHT + 200), 500, Easing.OutQuint);
                    chat.ChangeRadius(0);
                    break;

                case BoardScreen or EXBoardScreen:
                    chatContainer.FadeIn(TournamentScreen.FADE_DELAY);
                    chatContainer.MoveTo(new Vector2(30, team1List.GetHeight() + 100), 500, Easing.OutQuint);
                    chatContainer.ResizeWidthTo(300, 500, Easing.OutQuint);
                    chatContainer.ResizeHeightTo(660 - team1List.GetHeight() - 5, 500, Easing.OutQuint);
                    chat.ChangeRadius(10);
                    break;

                default:
                    chatContainer.FadeOut(TournamentScreen.FADE_DELAY);
                    break;
            }

            foreach (var s in buttons.OfType<ScreenButton>())
                s.IsSelected = screenType == s.Type;
        }

        private partial class Separator : CompositeDrawable
        {
            public Separator()
            {
                RelativeSizeAxes = Axes.X;
                Height = 5;
            }
        }

        private partial class ScreenButton : TourneyButton
        {
            public readonly Type Type;

            private readonly Key? shortcutKey;

            public ScreenButton(Type type, Key? shortcutKey = null)
            {
                this.shortcutKey = shortcutKey;

                Type = type;

                BackgroundColour = OsuColour.Gray(0.2f);
                Action = () => RequestSelection?.Invoke(type);

                RelativeSizeAxes = Axes.X;

                if (shortcutKey != null)
                {
                    Add(new CircularContainer
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Size = new Vector2(24),
                        Margin = new MarginPadding(5),
                        Masking = true,
                        Alpha = 0.5f,
                        Blending = BlendingParameters.Additive,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                Colour = OsuColour.Gray(0.1f),
                                RelativeSizeAxes = Axes.Both,
                            },
                            new OsuSpriteText
                            {
                                Font = OsuFont.Default.With(size: 24),
                                Y = -2,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = shortcutKey.Value.ToString(),
                            }
                        }
                    });
                }
            }

            protected override bool OnKeyDown(KeyDownEvent e)
            {
                if (e.Key == shortcutKey)
                {
                    TriggerClick();
                    return true;
                }

                return base.OnKeyDown(e);
            }

            private bool isSelected;

            public Action<Type>? RequestSelection;

            public bool IsSelected
            {
                get => isSelected;
                set
                {
                    if (value == isSelected)
                        return;

                    isSelected = value;
                    BackgroundColour = isSelected ? Color4.SkyBlue : OsuColour.Gray(0.2f);
                    SpriteText.Colour = isSelected ? Color4.Black : Color4.White;
                }
            }
        }

        public void UpdateChatState(bool isShown)
        {
            switch (currentScreen)
            {
                case GameplayScreen:
                    chatContainer.MoveToY(isShown ? STREAM_AREA_HEIGHT - 144 : STREAM_AREA_HEIGHT + 200, 500, Easing.OutQuint);
                    break;

                default:
                    return;
            }
        }

        public void HideShowChat(int duration) =>
            chatContainer.Delay(1500).FadeTo(0.6f, duration, Easing.OutQuint)
                .Then().Delay(5700).FadeIn(duration, Easing.OutQuint);

        public void ShowChat(int duration) => chatContainer.FadeIn(duration, Easing.OutQuint);

        public void ShowMapIntro(RoundBeatmap map, TeamColour colour = TeamColour.Neutral, TrapInfo? trap = null) => queueAnimation(new TournamentIntro(map, colour, trap)
        {
            Anchor = Anchor.CentreLeft,
            Origin = Anchor.CentreLeft,
            X = CONTROL_AREA_WIDTH + STREAM_AREA_WIDTH / 2,
        });

        public void ShowWinAnimation(TournamentTeam? team, TeamColour colour = TeamColour.Neutral) => queueAnimation(new RoundAnimation(team, colour)
        {
            Anchor = Anchor.CentreLeft,
            Origin = Anchor.CentreLeft,
            // X = CONTROL_AREA_WIDTH + STREAM_AREA_WIDTH / 2,
        });

        private void startAnimation(IAnimation animation)
        {
            AddInternal((Drawable)(currentAnimation = animation));

            animation.Fire();
            animation.OnAnimationComplete += () =>
            {
                animationQueue.Remove(animation);
            };
        }

        private void queueAnimation(IAnimation d)
        {
            animationQueue.Add(d);
        }
    }
}

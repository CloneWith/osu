// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Threading;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays.Settings;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Localisation.Screens;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Board;
using osu.Game.Tournament.Screens.Gameplay.Components;
using osu.Game.Tournament.Screens.MapPool;
using osu.Game.Tournament.Screens.TeamWin;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Gameplay
{
    public partial class GameplayScreen : BeatmapInfoScreen
    {
        private readonly BindableBool warmup = new BindableBool();
        private readonly Bindable<TourneyState> state = new Bindable<TourneyState>();

        private bool isChatShown;
        private bool chatEnforcing;

        private bool isUsingBoard => CurrentMatch.Value?.Round.Value?.UseBoard.Value == true;

        [Resolved]
        private TournamentSceneManager? sceneManager { get; set; }

        [Resolved]
        private MatchIPCInfo ipc { get; set; } = null!;

        private LabelledSwitchButton warmupToggle = null!;
        private Drawable chroma = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            AddRangeInternal(new Drawable[]
            {
                new TourneyBackground(BackgroundType.Gameplay)
                {
                    Loop = true,
                    RelativeSizeAxes = Axes.Both,
                },
                header = new MatchHeader
                {
                    ShowLogo = false,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Y = 110,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Children = new[]
                    {
                        chroma = new Container
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Height = 512,
                            Width = 1366,
                            Children = new Drawable[]
                            {
                                new PlayerArea(TeamColour.Red)
                                {
                                    Name = "Left PlayerArea",
                                    RelativeSizeAxes = Axes.Both,
                                    Width = 0.5f,
                                },
                                new PlayerArea(TeamColour.Blue)
                                {
                                    Name = "Right PlayerArea",
                                    RelativeSizeAxes = Axes.Both,
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Width = 0.5f,
                                }
                            }
                        },
                    }
                },
                scoreDisplay = new TournamentMatchScoreDisplay
                {
                    Y = -147,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.TopCentre,
                },
                chatBackground = new EmptyBox
                {
                    Name = "chat Background",
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    Width = 0.5f,
                    Height = 144,
                },
                new ControlPanel(true)
                {
                    Children = new Drawable[]
                    {
                        warmupToggle = new LabelledSwitchButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Label = GameplayScreenStrings.WarmupStage,
                            Current = warmup,
                        },
                        new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = GameplayScreenStrings.ToggleChat,
                            Action = () =>
                            {
                                chatEnforcing = true;

                                if (isChatShown)
                                {
                                    isChatShown = false;
                                    expand();
                                }
                                else
                                {
                                    isChatShown = true;
                                    contract();
                                }
                            }
                        },
                        new LabelledSwitchButton
                        {
                            Label = GameplayScreenStrings.BlueChroma,
                            Current = LadderInfo.UseBlueChroma,
                        },
                        new SettingsSlider<int>
                        {
                            LabelText = $"{(OperatingSystem.IsWindows() ? "Player Area" : "Chroma")} width",
                            Current = LadderInfo.ChromaKeyWidth,
                            KeyboardStep = 1,
                        },
                        OperatingSystem.IsWindows()
                            ? new SettingsSlider<int>
                            {
                                LabelText = "Frame rate",
                                Current = LadderInfo.FrameRate,
                                KeyboardStep = 1,
                            }
                            : Empty(),
                        new SettingsSlider<int>
                        {
                            LabelText = GameplayScreenStrings.PlayersPerTeam,
                            Current = LadderInfo.PlayersPerTeam,
                            KeyboardStep = 1,
                        },
                    }
                }
            });

            LadderInfo.ChromaKeyWidth.BindValueChanged(width => chroma.Width = width.NewValue, true);

            warmup.BindValueChanged(w =>
            {
                header.ShowScores = !w.NewValue;
            }, true);
        }

        private void updateWarmup()
        {
            warmup.Value = warmupToggle.Current.Value;
            updateState();
            warmupToggle.Current.Value = warmup.Value;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            warmupToggle.Current.BindValueChanged(_ => updateWarmup(), true);

            state.BindTo(ipc.State);
            state.BindValueChanged(e => updateState(e), true);
        }

        protected override void CurrentMatchChanged(ValueChangedEvent<TournamentMatch?> match)
        {
            base.CurrentMatchChanged(match);

            if (match.NewValue == null)
                return;

            warmup.Value = match.NewValue.Team1Score.Value + match.NewValue.Team2Score.Value == 0;
            sceneManager?.CancelScreenChange();
        }

        private ScheduledDelegate? scheduledContract;

        private TournamentMatchScoreDisplay scoreDisplay = null!;

        private TourneyState lastState;
        private MatchHeader header = null!;
        private EmptyBox chatBackground = null!;

        private void contract()
        {
            if (!IsLoaded)
                return;

            scheduledContract?.Cancel();

            SongBar.Expanded = false;
            scoreDisplay.FadeOut(100);

            if (sceneManager != null)
                sceneManager.ShowChat = true;
            chatBackground.MoveToY(0, 500, Easing.OutQuint);
        }

        private void expand()
        {
            if (!IsLoaded)
                return;

            scheduledContract?.Cancel();

            if (sceneManager != null)
                sceneManager.ShowChat = false;
            chatBackground.MoveToY(200, 500, Easing.OutQuint);

            using (BeginDelayedSequence(300))
            {
                scoreDisplay.FadeIn(100);
                SongBar.Expanded = true;
            }
        }

        private void updateState(ValueChangedEvent<TourneyState>? e = null)
        {
            try
            {
                sceneManager?.CancelScreenChange();

                switch (state.Value)
                {
                    case TourneyState.Idle:
                        if (!chatEnforcing || lastState == TourneyState.Ranking)
                        {
                            chatEnforcing = false;
                            isChatShown = true;
                            contract();
                        }

                        break;

                    case TourneyState.Ranking:
                        const int delay_before_progression = 25000;

                        if (CurrentMatch.Value != null && !isUsingBoard && !warmup.Value)
                        {
                            if (ipc.Score1.Value > ipc.Score2.Value)
                                CurrentMatch.Value.Team1Score.Value++;
                            else
                                CurrentMatch.Value.Team2Score.Value++;
                        }

                        if (LadderInfo.AutoProgressScreens.Value)
                        {
                            // if we've gone to ranking and the last screen was playing
                            // we should automatically proceed after a short delay
                            // It's shit code to wait for an Idle state
                            if ((e?.OldValue == TourneyState.Playing || lastState == TourneyState.Playing) && !warmup.Value)
                            {
                                switch (CurrentMatch.Value?.Completed.Value)
                                {
                                    case true:
                                        sceneManager?.ScheduleScreenChange(typeof(TeamWinScreen), delay_before_progression);
                                        break;

                                    case false:
                                        sceneManager?.ScheduleScreenChange(isUsingBoard ? typeof(BoardScreen) : typeof(MapPoolScreen), delay_before_progression);
                                        break;
                                }
                            }
                        }

                        scheduledContract = Scheduler.AddDelayed(contract, 20000);
                        break;

                    default:
                        if (e == null || !chatEnforcing)
                        {
                            isChatShown = false;
                            expand();
                        }

                        break;
                }
            }
            finally
            {
                lastState = e?.NewValue ?? state.Value;
            }
        }

        public override void Hide()
        {
            sceneManager?.CancelScreenChange();
            base.Hide();
        }

        public override void Show()
        {
            updateState();
            base.Show();
        }

        private partial class PlayerArea : CompositeDrawable
        {
            [Resolved]
            private LadderInfo ladder { get; set; } = null!;

            private readonly Color4 chromaGreen = new Color4(0, 255, 0, 255);
            private readonly Color4 chromaBlue = new Color4(0, 0, 255, 255);

            private readonly TeamColour teamColour;

            // 到底什么样的弱智会往前面加个空格啊
            // 对，就是ppy
            private const string tournament_client_name = @" Tournament Client ";

            public PlayerArea(TeamColour teamColour)
            {
                this.teamColour = teamColour;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                // chroma key area for stable gameplay
                ladder.UseBlueChroma.BindValueChanged(e =>
                    this.FadeColour(e.NewValue ? chromaBlue : chromaGreen, 300, Easing.OutQuint), true);

                ladder.PlayersPerTeam.BindValueChanged(performLayout, true);
                ladder.NativeTourneyWindowCapturing.BindValueChanged(_ => ladder.PlayersPerTeam.TriggerChange());
            }

            private void performLayout(ValueChangedEvent<int> playerCount)
            {
                if (!ladder.NativeTourneyWindowCapturing.Value || !OperatingSystem.IsWindows())
                {
                    switch (playerCount.NewValue)
                    {
                        case 3:
                            InternalChildren = new Drawable[]
                            {
                                new ChromaBox
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Width = 0.5f,
                                    Height = 0.5f,
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                },
                                new ChromaBox
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Height = 0.5f,
                                },
                            };
                            break;

                        default:
                            InternalChild = new ChromaBox
                            {
                                RelativeSizeAxes = Axes.Both,
                            };
                            break;
                    }

                    return;
                }

                int clientIndex = teamColour == TeamColour.Red ? 0 : playerCount.NewValue;

                switch (playerCount.NewValue)
                {
                    case 1:
                        InternalChildren = new Drawable[]
                        {
                            new CapturedWindowSprite($"{tournament_client_name}{clientIndex}")
                            {
                                RelativeSizeAxes = Axes.Both,
                            }
                        };
                        break;

                    case 2:
                        InternalChildren = new Drawable[]
                        {
                            new CapturedWindowSprite($"{tournament_client_name}{clientIndex++}")
                            {
                                RelativeSizeAxes = Axes.Both,
                                Height = 0.5f,
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                            },
                            new CapturedWindowSprite($"{tournament_client_name}{clientIndex}")
                            {
                                RelativeSizeAxes = Axes.Both,
                                Height = 0.5f,
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.BottomCentre,
                            }
                        };
                        break;

                    case 3:
                        InternalChildren = new Drawable[]
                        {
                            new CapturedWindowSprite($"{tournament_client_name}{clientIndex++}")
                            {
                                RelativeSizeAxes = Axes.Both,
                                Width = 0.5f,
                                Height = 0.5f,
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                            },
                            new CapturedWindowSprite($"{tournament_client_name}{clientIndex++}")
                            {
                                RelativeSizeAxes = Axes.Both,
                                Width = 0.5f,
                                Height = 0.5f,
                                Anchor = Anchor.BottomLeft,
                                Origin = Anchor.BottomLeft,
                            },
                            new CapturedWindowSprite($"{tournament_client_name}{clientIndex}")
                            {
                                RelativeSizeAxes = Axes.Both,
                                Width = 0.5f,
                                Height = 0.5f,
                                Anchor = Anchor.BottomRight,
                                Origin = Anchor.BottomRight,
                            },
                        };
                        break;

                    case 4:
                        InternalChildren = new Drawable[]
                        {
                            new CapturedWindowSprite($"{tournament_client_name}{clientIndex++}")
                            {
                                RelativeSizeAxes = Axes.Both,
                                Width = 0.5f,
                                Height = 0.5f,
                                Anchor = Anchor.TopLeft,
                                Origin = Anchor.TopLeft,
                            },
                            new CapturedWindowSprite($"{tournament_client_name}{clientIndex++}")
                            {
                                RelativeSizeAxes = Axes.Both,
                                Width = 0.5f,
                                Height = 0.5f,
                                Anchor = Anchor.TopRight,
                                Origin = Anchor.TopRight,
                            },
                            new CapturedWindowSprite($"{tournament_client_name}{clientIndex++}")
                            {
                                RelativeSizeAxes = Axes.Both,
                                Width = 0.5f,
                                Height = 0.5f,
                                Anchor = Anchor.BottomLeft,
                                Origin = Anchor.BottomLeft,
                            },
                            new CapturedWindowSprite($"{tournament_client_name}{clientIndex}")
                            {
                                RelativeSizeAxes = Axes.Both,
                                Width = 0.5f,
                                Height = 0.5f,
                                Anchor = Anchor.BottomRight,
                                Origin = Anchor.BottomRight,
                            },
                        };
                        break;

                    default:
                        throw new ArgumentException("Not Support this player count");
                }
            }
        }
    }
}

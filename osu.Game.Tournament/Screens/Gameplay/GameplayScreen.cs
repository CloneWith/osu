// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Logging;
using osu.Framework.Threading;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays.Settings;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Localisation.Screens;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Gameplay.Components;
using osu.Game.Tournament.Screens.MapPool;
using osu.Game.Tournament.Screens.TeamWin;
using osu.Game.TournamentIpc;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Gameplay
{
    public partial class GameplayScreen : BeatmapInfoScreen
    {
        private readonly BindableBool warmup = new BindableBool();

        public readonly Bindable<TourneyState> State = new Bindable<TourneyState>();
        public readonly Bindable<LegacyTourneyState> LegacyState = new Bindable<LegacyTourneyState>();
        public readonly Bindable<TourneyState> LazerState = new Bindable<TourneyState>();
        private OsuCheckbox matchCompleteOverride = null!;
        private LegacyMatchIPCInfo legacyIpc = null!;
        private MatchIPCInfo lazerIpc = null!;

        [Resolved]
        private TournamentSceneManager? sceneManager { get; set; }

        private LabelledSwitchButton warmupToggle = null!;
        private Drawable chroma = null!;

        [BackgroundDependencyLoader]
        private void load(LegacyMatchIPCInfo legacyIpc, MatchIPCInfo lazerIpc)
        {
            this.legacyIpc = legacyIpc;
            this.lazerIpc = lazerIpc;

            LabelledSwitchButton chatToggle;

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
                    ShowMatchRound = false,
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
                                new ChromaArea
                                {
                                    Name = "Left PlayerArea",
                                    RelativeSizeAxes = Axes.Both,
                                    Width = 0.5f,
                                },
                                new ChromaArea
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
                        chatToggle = new LabelledSwitchButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Label = GameplayScreenStrings.ToggleChat,
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
                        new SettingsSlider<int>
                        {
                            LabelText = GameplayScreenStrings.PlayersPerTeam,
                            Current = LadderInfo.PlayersPerTeam,
                            KeyboardStep = 1,
                        },
                        matchCompleteOverride = new OsuCheckbox
                        {
                            LabelText = "match complete?",
                        },
                    }
                }
            });

            State.BindValueChanged(state => chatToggle.Current.Value = State.Value == TourneyState.Lobby, true);
            chatToggle.Current.BindValueChanged(v => State.Value = v.NewValue ? TourneyState.Lobby : TourneyState.Playing);

            LadderInfo.ChromaKeyWidth.BindValueChanged(width => chroma.Width = width.NewValue, true);

            warmup.BindValueChanged(w =>
            {
                header.ShowScores = !w.NewValue;
            }, true);
        }

        private void updateWarmup()
        {
            warmup.Value = warmupToggle.Current.Value;
            updateStateLazer();
            warmupToggle.Current.Value = warmup.Value;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            LadderInfo.UseLazerIpc.BindValueChanged(vce =>
            {
                LegacyState.UnbindAll();
                LazerState.UnbindAll();

                if (vce.NewValue)
                {
                    LazerState.BindTo(lazerIpc.State);
                    LazerState.BindValueChanged(_ => updateStateLazer(), true);
                    return;
                }

                LegacyState.BindTo(legacyIpc.State);
                LegacyState.BindValueChanged(_ => updateStateLegacy(), true);
            }, true);

            warmupToggle.Current.BindValueChanged(_ => updateWarmup(), true);
        }

        protected override void CurrentMatchChanged(ValueChangedEvent<TournamentMatch?> match)
        {
            base.CurrentMatchChanged(match);

            if (match.NewValue == null)
                return;

            warmup.Value = match.NewValue.Team1Score.Value + match.NewValue.Team2Score.Value == 0;

            if (match.OldValue != null)
                matchCompleteOverride.Current.UnbindFrom(match.OldValue.Completed);
            matchCompleteOverride.Current.BindTo(match.NewValue.Completed);
            sceneManager?.CancelScreenChange();
        }

        private ScheduledDelegate? scheduledScreenChange;
        private ScheduledDelegate? scheduledContract;

        private TournamentMatchScoreDisplay scoreDisplay = null!;

        private LegacyTourneyState lastLegacyState;
        private TourneyState lastLazerState;
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

        private void advanceAfterRanking(float delayBeforeProgression)
        {
            if (CurrentMatch.Value?.Completed.Value == true)
                scheduledScreenChange = Scheduler.AddDelayed(() => { sceneManager?.SetScreen(typeof(TeamWinScreen)); }, delayBeforeProgression);
            else if (CurrentMatch.Value?.Completed.Value == false)
                scheduledScreenChange = Scheduler.AddDelayed(() => { sceneManager?.SetScreen(typeof(MapPoolScreen)); }, delayBeforeProgression);
        }

        // kind of an ugly copy and paste from updateStateLegacy
        private void updateStateLazer()
        {
            Logger.Log($"lazer ipc state changed: {LazerState.Value}");

            try
            {
                sceneManager?.CancelScreenChange();

                if (LazerState.Value == TourneyState.Ranking)
                {
                    if (warmup.Value || CurrentMatch.Value == null) return;

                    if (LadderInfo.CumulativeScore.Value)
                    {
                        int mapId = lazerIpc.Beatmap.Value?.OnlineID ?? 0;

                        if (mapId > 0)
                        {
                            var roundMap = CurrentMatch.Value.Round.Value?.Beatmaps.FirstOrDefault(b => b.ID == mapId);

                            if (roundMap != null)
                            {
                                CurrentMatch.Value.MapScores[roundMap.SlotName] = new Tuple<long, long>(lazerIpc.Score1.Value, lazerIpc.Score2.Value);

                                // The following is ugly, but it will do for now.
                                var currentSet = MatchSet.FindSetByMapId(CurrentMatch.Value, mapId);

                                if (currentSet != null)
                                {
                                    if ((currentSet.IsTiebreaker == false && mapId == currentSet.Map2Id.Value) || mapId == currentSet.Map3Id.Value)
                                    {
                                        // add point to match, the set is complete
                                        var scores = currentSet.GetSetScores(CurrentMatch.Value);

                                        if (scores != null)
                                        {
                                            if (scores.Item1 > scores.Item2)
                                                CurrentMatch.Value.Team1Score.Value++;
                                            else
                                                CurrentMatch.Value.Team2Score.Value++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (lazerIpc.Score1.Value > lazerIpc.Score2.Value)
                            CurrentMatch.Value.Team1Score.Value++;
                        else
                            CurrentMatch.Value.Team2Score.Value++;
                    }
                }

                switch (LazerState.Value)
                {
                    case TourneyState.Lobby:
                        contract();

                        if (LadderInfo.AutoProgressScreens.Value
                            && lastLazerState == TourneyState.Ranking
                            && !warmup.Value)
                        {
                            // if we've returned to idle and the last screen was ranking
                            // we should automatically proceed after a short delay
                            advanceAfterRanking(50);
                        }

                        break;

                    case TourneyState.Ranking:
                        scheduledContract = Scheduler.AddDelayed(contract, 10_000);
                        break;

                    default:
                        expand();
                        break;
                }
            }
            finally
            {
                lastLazerState = LazerState.Value;
            }
        }

        private void updateStateLegacy()
        {
            try
            {
                scheduledScreenChange?.Cancel();

                if (LegacyState.Value == LegacyTourneyState.Ranking)
                {
                    if (warmup.Value || CurrentMatch.Value == null) return;

                    if (legacyIpc.Score1.Value > legacyIpc.Score2.Value)
                        CurrentMatch.Value.Team1Score.Value++;
                    else
                        CurrentMatch.Value.Team2Score.Value++;
                }

                switch (LegacyState.Value)
                {
                    case LegacyTourneyState.Idle:
                        contract();

                        if (LadderInfo.AutoProgressScreens.Value
                            && lastLegacyState == LegacyTourneyState.Ranking
                            && !warmup.Value)
                        {
                            // if we've returned to idle and the last screen was ranking
                            // we should automatically proceed after a short delay
                            advanceAfterRanking(4000);
                        }

                        break;

                    case LegacyTourneyState.Ranking:
                        scheduledContract = Scheduler.AddDelayed(contract, 10000);
                        break;

                    default:
                        expand();
                        break;
                }
            }
            finally
            {
                lastLegacyState = LegacyState.Value;
            }
        }

        public override void Hide()
        {
            sceneManager?.CancelScreenChange();
            base.Hide();
        }

        public override void Show()
        {
            if (LadderInfo.UseLazerIpc.Value)
                updateStateLazer();
            else
                updateStateLegacy();

            base.Show();
        }

        private partial class ChromaArea : CompositeDrawable
        {
            [Resolved]
            private LadderInfo ladder { get; set; } = null!;

            private readonly Color4 chromaGreen = new Color4(0, 255, 0, 255);
            private readonly Color4 chromaBlue = new Color4(0, 0, 255, 255);

            [BackgroundDependencyLoader]
            private void load()
            {
                // chroma key area for stable gameplay
                ladder.UseBlueChroma.BindValueChanged(e =>
                    this.FadeColour(e.NewValue ? chromaBlue : chromaGreen, 300, Easing.OutQuint), true);

                ladder.PlayersPerTeam.BindValueChanged(performLayout, true);
            }

            private void performLayout(ValueChangedEvent<int> playerCount)
            {
                switch (playerCount.NewValue)
                {
                    case 3:
                        InternalChildren = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Width = 0.5f,
                                Height = 0.5f,
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                            },
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Anchor = Anchor.BottomLeft,
                                Origin = Anchor.BottomLeft,
                                Height = 0.5f,
                            },
                        };
                        break;

                    default:
                        InternalChild = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        };
                        break;
                }
            }
        }
    }
}

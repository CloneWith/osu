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
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Ladder.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    public partial class TournamentSetPanel : CompositeDrawable
    {
        private partial class SetMapScoreCounter : DrawableMatchTeam.MatchTeamCumulativeScoreCounter
        {
            protected override double RollingDuration => 350;

            protected override OsuSpriteText CreateSpriteText() => base.CreateSpriteText().With(s =>
            {
                DisplayedSpriteText = s;
                DisplayedSpriteText.Font = OsuFont.Torus.With(size: 16);
            });
        }

        public partial class SetMapResultDisplay : CompositeDrawable
        {
            public event Action? ResultChanged;

            // todo: make these private?
            public readonly Bindable<long?> ScoreRed = new Bindable<long?>();
            public readonly Bindable<long?> ScoreBlue = new Bindable<long?>();

            private TournamentSpriteText slotText = null!;
            private long mapId = 0;

            public long MapID
            {
                get => mapId;
                set
                {
                    mapId = value;

                    updateSlotName();
                }
            }

            private SetMapScoreCounter scoreCounterRed = null!;
            private SetMapScoreCounter scoreCounterBlue = null!;

            [Resolved]
            protected LadderInfo LadderInfo { get; private set; } = null!;

            protected readonly Bindable<TournamentMatch?> CurrentMatch = new Bindable<TournamentMatch?>();

            protected override void LoadComplete()
            {
                base.LoadComplete();

                CurrentMatch.BindTo(LadderInfo.CurrentMatch);
                CurrentMatch.BindValueChanged(_ => updateSlotName(), true);
            }

            // lookup slot name from ladder
            private void updateSlotName()
            {
                if (CurrentMatch.Value == null)
                    return;

                if (mapId == 0)
                {
                    slotText.Text = string.Empty;
                    ScoreRed.Value = null;
                    ScoreBlue.Value = null;
                    ResultChanged?.Invoke();
                    return;
                }

                var poolMap = CurrentMatch.Value.Round.Value?.Beatmaps.FirstOrDefault(bm => bm.ID == mapId);

                slotText.Text = poolMap?.SlotName ?? "??";

                Logger.Log($"Setting mapid for {TestName}: {poolMap}");

                CurrentMatch.Value.MapScores.BindCollectionChanged((_, _) =>
                {
                    string key = slotText.Text.ToString();

                    Logger.Log($"updating scores for slot {key}");

                    if (CurrentMatch.Value.MapScores.TryGetValue(key, out var value))
                    {
                        Logger.Log($"slot {key} was found in pool: {value.Item1} - {value.Item2}");

                        ScoreRed.Value = value.Item1;
                        ScoreBlue.Value = value.Item2;

                        ResultChanged?.Invoke();
                        return;
                    }

                    Logger.Log($"slot {key} was not found in pool");

                    if (ScoreRed.Value == null)
                        return;

                    ScoreRed.Value = null;
                    ScoreBlue.Value = null;
                    ResultChanged?.Invoke();
                }, true);
            }

            public readonly string TestName = "";

            public SetMapResultDisplay(string? name = null)
            {
                Height = 32;
                Anchor = Anchor.CentreLeft;
                Origin = Anchor.CentreLeft;

                if (name != null)
                    TestName = name;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChildren = new Drawable[]
                {
                    new Container
                    {
                        Width = 0.3f,
                        RelativeSizeAxes = Axes.Both,
                        Child = slotText = new TournamentSpriteText
                        {
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            Text = "",
                            Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 22),
                        },
                    },
                    new Container
                    {
                        Masking = true,
                        Width = 0.35f,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.Both,
                        RelativePositionAxes = Axes.X,
                        X = 0.3f,
                        Children = new Drawable[]
                        {
                            scoreCounterRed = new SetMapScoreCounter
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                            },
                        }
                    },
                    new Container
                    {
                        Masking = true,
                        Width = 0.35f,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.Both,
                        RelativePositionAxes = Axes.X,
                        X = 0.65f,
                        Children = new Drawable[]
                        {
                            scoreCounterBlue = new SetMapScoreCounter
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                            },
                        }
                    }
                };

                ScoreRed.BindValueChanged(val => updatePlayerScore(scoreCounterRed, val), true);
                ScoreBlue.BindValueChanged(val => updatePlayerScore(scoreCounterBlue, val), true);
                return;

                void updatePlayerScore(SetMapScoreCounter counter, ValueChangedEvent<long?> changeEvent)
                {
                    Logger.Log($"updating score to ({changeEvent.NewValue}) for {TestName} - {counter.DisplayedSpriteText.Text} - {counter.Current.Value}");

                    switch (changeEvent.NewValue)
                    {
                        case 0:
                        case null:
                            counter.Current.Value = 0;
                            counter.DisplayedSpriteText.Hide();
                            break;

                        default:
                            counter.Current.Value = 0;
                            counter.DisplayedSpriteText.Show();
                            counter.Current.Value = (double)changeEvent.NewValue;
                            break;
                    }
                }
            }
        }

        public MatchSet Model { get; init; }

        public const float HEIGHT = TournamentBeatmapPanel.HEIGHT;
        public const float WIDTH = TournamentBeatmapPanel.WIDTH;

        private TeamColour? winnerColour = null;

        public TeamColour? Winner
        {
            get => winnerColour;
            set
            {
                winnerColour = value;
                updateWinState();
            }
        }

        public readonly BindableList<TournamentBeatmapPanel> BeatmapPanels = new BindableList<TournamentBeatmapPanel>();

        private SetMapResultDisplay map1ResultDisplay = null!;
        private SetMapResultDisplay map2ResultDisplay = null!;
        private SetMapResultDisplay? map3ResultDisplay;
        private FillFlowContainer mainFlow = null!;

        public TournamentSetPanel(MatchSet set)
        {
            Model = set;

            Width = WIDTH;
            Height = HEIGHT;
            Masking = true;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddRangeInternal(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.35f,
                },
                mainFlow = new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0),
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding(5),
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Children = new Drawable[]
                            {
                                map1ResultDisplay = new SetMapResultDisplay("slot1")
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Width = 0.5f
                                },
                                map2ResultDisplay = new SetMapResultDisplay("slot2")
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Width = 0.5f,
                                }
                            },
                        },
                    }
                },
            });

            if (Model.IsTiebreaker)
            {
                mainFlow.Add(map3ResultDisplay = new SetMapResultDisplay("slot3")
                {
                    RelativeSizeAxes = Axes.X,
                });
            }

            Model.Map1Id.BindValueChanged(vce =>
            {
                map1ResultDisplay.MapID = vce.NewValue;
            }, true);
            Model.Map2Id.BindValueChanged(vce =>
            {
                map2ResultDisplay.MapID = vce.NewValue;
            }, true);
            Model.Map3Id.BindValueChanged(vce =>
            {
                if (map3ResultDisplay != null)
                    map3ResultDisplay.MapID = vce.NewValue;
            }, true);

            map1ResultDisplay.ResultChanged += checkWinState;
            map2ResultDisplay.ResultChanged += checkWinState;
            if (map3ResultDisplay != null)
                map3ResultDisplay.ResultChanged += checkWinState;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            BeatmapPanels.BindCollectionChanged((sender, args) =>
            {
                Logger.Log($"TournamentSetPanel beatmap panels collection changed: {args.NewItems}");
            });
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            map1ResultDisplay.ResultChanged -= checkWinState;
            map2ResultDisplay.ResultChanged -= checkWinState;
            if (map3ResultDisplay != null)
                map3ResultDisplay.ResultChanged -= checkWinState;
        }

        /// <summary>
        /// Checks the score(s) of the set and set a winner if appropriate
        /// </summary>
        private void checkWinState()
        {
            if (map1ResultDisplay.ScoreRed.Value == null || map2ResultDisplay.ScoreRed.Value == null || (Model.IsTiebreaker && map3ResultDisplay?.ScoreRed.Value == null))
            {
                Winner = null;
                return;
            }

            long redScore = (map1ResultDisplay.ScoreRed.Value ?? 0) + (map2ResultDisplay.ScoreRed.Value ?? 0) + (map3ResultDisplay?.ScoreRed.Value ?? 0);
            long blueScore = (map1ResultDisplay.ScoreBlue.Value ?? 0) + (map2ResultDisplay.ScoreBlue.Value ?? 0) + (map3ResultDisplay?.ScoreBlue.Value ?? 0);

            if (redScore < blueScore)
            {
                Winner = TeamColour.Blue;
                return;
            }

            Winner = TeamColour.Red;
        }

        /// <summary>
        /// Add/remove colored border depending on winner
        /// </summary>
        private void updateWinState()
        {
            if (winnerColour != null)
            {
                BorderThickness = 6;

                BorderColour = TournamentGame.GetTeamColour((TeamColour)winnerColour);
            }
            else
            {
                Colour = Color4.White;
                BorderThickness = 0;
                Alpha = 1;
            }
        }
    }
}

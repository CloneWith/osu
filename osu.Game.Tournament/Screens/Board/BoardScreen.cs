// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Framework.Threading;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Components.Dialogs;
using osu.Game.Tournament.Localisation;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Board.Components;
using osu.Game.Tournament.Screens.Gameplay;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Tournament.Screens.Board
{
    public partial class BoardScreen : TournamentMatchScreen
    {
        private readonly List<BoardBeatmapPanel> boardMapList = new List<BoardBeatmapPanel>();

        [Resolved]
        private TournamentSceneManager? sceneManager { get; set; }

        private Container warningContainer = null!;

        private readonly Bindable<TournamentMatch?> currentMatch = new Bindable<TournamentMatch?>();

        private TeamColour pickTeam;
        private ChoiceType pickType;

        private OsuButton buttonRedBan = null!;
        private OsuButton buttonBlueBan = null!;
        private OsuButton buttonRedPick = null!;
        private OsuButton buttonBluePick = null!;

        private OsuButton buttonRedWin = null!;
        private OsuButton buttonBlueWin = null!;

        private OsuButton buttonIndicator = null!;

        private DialogOverlay dialogOverlay = null!;

        private const int side_list_height = 660;

        private ScheduledDelegate? scheduledScreenChange;

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            currentMatch.BindValueChanged(matchChanged);
            currentMatch.BindTo(LadderInfo.CurrentMatch);

            InternalChildren = new Drawable[]
            {
                new TourneyBackground(BackgroundType.Board)
                {
                    Loop = true,
                    RelativeSizeAxes = Axes.Both,
                },
                new FumoMatchHeader(),

                warningContainer = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                },
                new ControlPanel(true)
                {
                    Children = new Drawable[]
                    {
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 40,
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    buttonRedBan = new TourneyButton
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Text = "Red Ban",
                                        BackgroundColour = TournamentGame.COLOUR_RED,
                                        Action = () => setMode(TeamColour.Red, ChoiceType.Ban)
                                    },
                                    buttonBlueBan = new TourneyButton
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Text = "Blue Ban",
                                        BackgroundColour = TournamentGame.COLOUR_BLUE,
                                        Action = () => setMode(TeamColour.Blue, ChoiceType.Ban)
                                    },
                                }
                            },
                        },
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 40,
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    buttonRedPick = new TourneyButton
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Text = "Red Pick",
                                        BackgroundColour = TournamentGame.COLOUR_RED,
                                        Action = () => setMode(TeamColour.Red, ChoiceType.Pick)
                                    },
                                    buttonBluePick = new TourneyButton
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Text = "Blue Pick",
                                        BackgroundColour = TournamentGame.COLOUR_BLUE,
                                        Action = () => setMode(TeamColour.Blue, ChoiceType.Pick)
                                    },
                                }
                            },
                        },
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 40,
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    buttonRedWin = new TourneyButton
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Text = "Red Win",
                                        BackgroundColour = TournamentGame.COLOUR_RED,
                                        Action = () => setMode(TeamColour.Red, ChoiceType.RedWin)
                                    },
                                    buttonBlueWin = new TourneyButton
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Text = "Blue Win",
                                        BackgroundColour = TournamentGame.COLOUR_BLUE,
                                        Action = () => setMode(TeamColour.Blue, ChoiceType.BlueWin)
                                    },
                                }
                            },
                        },
                        new ControlPanel.Spacer(),
                        buttonIndicator = new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "TB Indicator",
                            BackgroundColour = Color4.Purple,
                            Colour = Color4.Gray,
                            Action = () => setMode(TeamColour.Neutral, ChoiceType.Neutral)
                        },
                        //new TourneyButton
                        //{
                        //    RelativeSizeAxes = Axes.X,
                        //    Text = BaseStrings.Refresh,
                        //    BackgroundColour = Color4.Orange,
                        //    Action =
                        //},
                        new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = BaseStrings.Reset,
                            BackgroundColour = Color4.DeepPink,
                            Action = () =>
                            {
                                dialogOverlay.Push(new ResetBoardDialog(reset));
                            },
                        },
                    },
                },
                dialogOverlay = new DialogOverlay(),
            };
        }

        private void matchChanged(ValueChangedEvent<TournamentMatch?> match)
        {
            if (match.NewValue != null)
            {
                if (!IsLoaded)
                    return;
            }
        }

        private void setMode(TeamColour colour, ChoiceType choiceType)
        {
            pickTeam = colour;
            pickType = choiceType;

            buttonRedBan.Colour = setColour(pickTeam == TeamColour.Red && pickType == ChoiceType.Ban);
            buttonBlueBan.Colour = setColour(pickTeam == TeamColour.Blue && pickType == ChoiceType.Ban);
            buttonRedPick.Colour = setColour(pickTeam == TeamColour.Red && pickType == ChoiceType.Pick);
            buttonBluePick.Colour = setColour(pickTeam == TeamColour.Blue && pickType == ChoiceType.Pick);
            buttonRedWin.Colour = setColour(pickTeam == TeamColour.Red && pickType == ChoiceType.RedWin);
            buttonBlueWin.Colour = setColour(pickTeam == TeamColour.Blue && pickType == ChoiceType.BlueWin);

            static Color4 setColour(bool active) => active ? Color4.White : Color4.Gray;
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            var map = boardMapList.FirstOrDefault(m => m.ReceivePositionalInputAt(e.ScreenSpaceMousePosition));

            if (map != null)
            {
                if (e.Button == MouseButton.Left && map.Beatmap?.OnlineID > 0)
                {
                    // Handle updating status to Red/Blue Win
                    if (isPickWin)
                    {
                        updateWinStatusForBeatmap(map.Beatmap.OnlineID);
                    }
                    else
                    {
                        addForBeatmap(map.Beatmap.OnlineID);
                    }
                }
                else if (e.Button == MouseButton.Right)
                {
                    var existing = CurrentMatch.Value?.PicksBans.LastOrDefault(p => p.BeatmapID == map.Beatmap?.OnlineID);

                    if (existing != null)
                    {
                        CurrentMatch.Value?.PicksBans.Remove(existing);
                    }
                }

                return true;
            }

            return base.OnMouseDown(e);
        }

        private void updateWinStatusForBeatmap(int beatmapId)
        {
            var existing = CurrentMatch.Value?.PicksBans.FirstOrDefault(p => p.BeatmapID == beatmapId && (p.Type == ChoiceType.RedWin || p.Type == ChoiceType.BlueWin));

            if (existing != null)
            {
                CurrentMatch.Value?.PicksBans.Remove(existing);
            }

            CurrentMatch.Value?.PicksBans.Add(new BeatmapChoice
            {
                Team = pickType == ChoiceType.RedWin ? TeamColour.Red : TeamColour.Blue,
                Type = pickType,
                BeatmapID = beatmapId,
            });
        }

        private void reset()
        {
            // Clear map marking lists
            CurrentMatch.Value?.PicksBans.Clear();
            CurrentMatch.Value?.Round.Value?.IsFinalStage.BindTo(new BindableBool());

            if (CurrentMatch.Value != null)
            {
                CurrentMatch.Value.Completed.Value = false;
                CurrentMatch.Value.Team1Score.Value = 0;
                CurrentMatch.Value.Team2Score.Value = 0;
            }

            // Reset button group
            buttonBlueBan.Colour = Color4.White;
            buttonBluePick.Colour = Color4.White;
            buttonBlueWin.Colour = Color4.White;
            buttonRedBan.Colour = Color4.White;
            buttonRedPick.Colour = Color4.White;
            buttonRedWin.Colour = Color4.White;
            buttonIndicator.Colour = Color4.Gray;

            pickTeam = TeamColour.None;
            pickType = ChoiceType.Neutral;
        }

        private bool isPickWin => pickType == ChoiceType.RedWin || pickType == ChoiceType.BlueWin;

        private void addForBeatmap(int beatmapId)
        {
            bool isPickBan = pickType == ChoiceType.Pick || pickType == ChoiceType.Ban || isPickWin;

            if (pickType == ChoiceType.Neutral || pickTeam == TeamColour.None || pickTeam == TeamColour.None)
                return;

            if (CurrentMatch.Value?.Round.Value == null)
                return;

            if (CurrentMatch.Value.Round.Value.Beatmaps.All(b => b.Beatmap?.OnlineID != beatmapId))
                // don't attempt to add if the beatmap isn't in our pool
                return;

            if (!isPickWin && CurrentMatch.Value.PicksBans.Any(p => p.BeatmapID == beatmapId
                                                                    && (p.Type == ChoiceType.Ban || p.Type == ChoiceType.RedWin || p.Type == ChoiceType.BlueWin)))
                // don't attempt to add if already banned / won, and it's not a win type.
                return;

            if (pickType == ChoiceType.Pick)
            {
                var introMap = CurrentMatch.Value.Round.Value.Beatmaps.FirstOrDefault(b => b.Beatmap?.OnlineID == beatmapId);

                if (introMap != null)
                    sceneManager?.ShowMapIntro(introMap, pickTeam);
            }

            if (isPickBan && !CurrentMatch.Value.PicksBans.Any(p => p.BeatmapID == beatmapId && p.Type == pickType))
            {
                CurrentMatch.Value.PicksBans.Add(new BeatmapChoice
                {
                    Team = pickTeam,
                    Type = pickType,
                    BeatmapID = beatmapId,
                });
            }

            // setNextMode(); // Uncomment if you still want to automatically set the next mode

            if (LadderInfo.AutoProgressScreens.Value)
            {
                if (pickType == ChoiceType.Pick && CurrentMatch.Value.PicksBans.Any(i => i.Type == ChoiceType.Pick))
                {
                    scheduledScreenChange?.Cancel();
                    scheduledScreenChange = Scheduler.AddDelayed(() => { sceneManager?.SetScreen(typeof(GameplayScreen)); }, 10000);
                }
            }
        }

        public override void Hide()
        {
            scheduledScreenChange?.Cancel();
            base.Hide();
        }
    }
}

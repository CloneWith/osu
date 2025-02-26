// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Framework.Threading;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Overlays.Toolbar;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Components.Dialogs;
using osu.Game.Tournament.Localisation;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Board.Components;
using osu.Game.Tournament.Screens.Gameplay;
using osu.Game.Tournament.Screens.Gameplay.Components;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Tournament.Screens.Board
{
    public partial class BoardScreen : TournamentMatchScreen
    {
        private Container boardContainer = null!;
        private readonly List<BoardBeatmapPanel> boardMapList = new List<BoardBeatmapPanel>();

        [Resolved]
        private TournamentSceneManager? sceneManager { get; set; }

        private Container warningContainer = null!;

        private readonly Bindable<TournamentMatch?> currentMatch = new Bindable<TournamentMatch?>();

        private TeamColour pickTeam;
        private ChoiceType pickType;

        private TeamColour teamWinner = TeamColour.None;

        private OsuButton buttonRedBan = null!;
        private OsuButton buttonBlueBan = null!;
        private OsuButton buttonRedPick = null!;
        private OsuButton buttonBluePick = null!;

        private OsuButton buttonRedWin = null!;
        private OsuButton buttonBlueWin = null!;

        private OsuButton buttonIndicator = null!;

        private bool isInTieBreaker;

        private Container informationDisplayContainer = null!;

        private DrawableTeamPlayerList team1List = null!;
        private DrawableTeamPlayerList team2List = null!;
        private EmptyBox extCommentBox = null!;

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
                new MatchHeader
                {
                    ShowScores = false,
                    ShowRound = false,
                },

                // Box for trap type / display of other info.
                new EmptyBox(cornerRadius: 10)
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.None,
                    Width = 650,
                    Height = 100,
                    Margin = new MarginPadding { Bottom = 12 },
                    Colour = Color4.Black,
                    Alpha = 0.7f,
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    RelativeSizeAxes = Axes.None,
                    Position = new Vector2(40, 100),
                    Width = 320,
                    Height = side_list_height,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        team1List = new DrawableTeamPlayerList(LadderInfo.CurrentMatch.Value?.Team1.Value)
                        {
                            RelativeSizeAxes = Axes.None,
                            Width = 300,
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                        },
                    },
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    RelativeSizeAxes = Axes.None,
                    Position = new Vector2(-40, 100),
                    Width = 320,
                    Height = side_list_height,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        team2List = new DrawableTeamPlayerList(LadderInfo.CurrentMatch.Value?.Team2.Value)
                        {
                            RelativeSizeAxes = Axes.None,
                            Width = 300,
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                        },
                        // A single Box for livestream comments.
                        // Wrapped in a container for round corners.
                        extCommentBox = new EmptyBox(cornerRadius: 10)
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            RelativeSizeAxes = Axes.None,
                            Width = 300,
                            Height = side_list_height - team2List.GetHeight() - 5,
                            Colour = Color4.Black,
                            Alpha = 0.7f,
                        },
                    },
                },
                boardContainer = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    CornerRadius = 10,
                },
                informationDisplayContainer = new Container
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomLeft,
                    Position = new Vector2(-300, 7),
                    Height = 100,
                    Width = 500,
                    Child = new InstructionDisplay(),
                },
                new Sprite
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomRight,
                    Position = new Vector2(300, -20),
                    Size = new Vector2(85),
                    Texture = textures.Get("Icons/additional-icon"),
                },
                new ToolbarClock
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    RelativeSizeAxes = Axes.None,
                    Height = 50,
                    Position = new Vector2(-40, -10),
                },
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
                        new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = BaseStrings.Refresh,
                            BackgroundColour = Color4.Orange,
                            Action = updateDisplay
                        },
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

                if (match.NewValue.Team1.Value != null) team1List.ReloadWithTeam(match.NewValue.Team1.Value);

                if (match.NewValue.Team2.Value != null)
                {
                    team2List.ReloadWithTeam(match.NewValue.Team2.Value);
                    extCommentBox.ResizeHeightTo(Height = side_list_height - team2List.GetHeight() - 5, 500, Easing.OutCubic);
                }
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
            updateBottomDisplay();
        }

        private void updateBottomDisplay(ValueChangedEvent<bool>? _ = null, bool bottomOnly = true, bool refresh = true)
        {
            if (CurrentMatch.Value == null) return;

            Drawable oldDisplay = informationDisplayContainer.Child;

            var color = pickTeam;
            RoundStep state = RoundStep.Default;

            if (DetectTieBreaker())
            {
                state = RoundStep.TieBreaker;
            }
            else if (DetectWin())
            {
                state = RoundStep.FinalWin;
                color = teamWinner;
            }
            else
            {
                switch (pickType)
                {
                    case ChoiceType.Pick:
                        state = RoundStep.Pick;
                        break;

                    case ChoiceType.Ban:
                        state = RoundStep.Ban;
                        break;

                    case ChoiceType.RedWin or ChoiceType.BlueWin:
                        state = RoundStep.Win;
                        break;
                }
            }

            Drawable newDisplay = new InstructionDisplay(team: color, roundStep: state);

            if (oldDisplay != newDisplay && refresh)
            {
                informationDisplayContainer.Child = newDisplay;
                informationDisplayContainer.FadeInFromZero(duration: 200, easing: Easing.InCubic);
                CurrentMatch.Value.Round.Value?.IsFinalStage.BindTo(new BindableBool(color == TeamColour.Neutral));

                if (state == RoundStep.FinalWin && !bottomOnly)
                {
                    sceneManager?.ShowWinAnimation(teamWinner == TeamColour.Red ? CurrentMatch.Value.Team1.Value
                        : teamWinner == TeamColour.Blue ? CurrentMatch.Value.Team2.Value
                        : null, teamWinner);
                }
            }
            else
            {
                CurrentMatch.Value.Round.Value?.IsFinalStage.BindTo(new BindableBool());
            }
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

                // Automatically detect special conditions
                if (CurrentMatch.Value != null)
                {
                    buttonIndicator.Colour = DetectWin() ? Color4.Orange : (DetectTieBreaker() ? Color4.White : Color4.Gray);

                    // Restore to the last state
                    updateBottomDisplay(bottomOnly: e.Button != MouseButton.Left);
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

            // Reset bottom display
            informationDisplayContainer.Child = new InstructionDisplay();

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

        private void addForBeatmap(string modId)
        {
            var map = CurrentMatch.Value?.Round.Value?.Beatmaps.FirstOrDefault(b => b.Mods + b.ModIndex == modId);

            if (map != null)
                addForBeatmap(map.ID);
        }

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

            // Remove the latest win state for Reverse Trap
            if (pickType == ChoiceType.Pick && CurrentMatch.Value.PicksBans.Any(p => p.BeatmapID == beatmapId
                                                                                     && (p.Type == ChoiceType.RedWin || p.Type == ChoiceType.BlueWin)))
            {
                var latestWin = CurrentMatch.Value.PicksBans.LastOrDefault(p => p.BeatmapID == beatmapId && (p.Type == ChoiceType.RedWin || p.Type == ChoiceType.BlueWin));
                if (latestWin != null) CurrentMatch.Value.PicksBans.Remove(latestWin);
            }

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

        protected override void CurrentMatchChanged(ValueChangedEvent<TournamentMatch?> match)
        {
            base.CurrentMatchChanged(match);
            updateDisplay();
        }

        /// <summary>
        /// Calculate the corresponding angle from two board blocks.
        /// </summary>
        /// <param name="x1">The X value of the first block.</param>
        /// <param name="x2">The X value of the first block.</param>
        /// <param name="y1">The Y value of the second block.</param>
        /// <param name="y2">The Y value of the second block.</param>
        /// <returns>An angle in degree.</returns>
        protected static float GetAngle(double x1, double x2, double y1, double y2)
        {
            if (x1 == x2) return y1 > y2 ? 90 : -90;

            return (float)(Math.Atan((y2 - y1) / (x2 - x1)) * 180 / Math.PI);
        }

        /// <summary>
        /// Detects if someone has won the match.
        /// </summary>
        /// <returns>true if someone has, otherwise false</returns>
        public bool DetectWin()
        {
            // Don't detect if not defining board coordinates
            if (CurrentMatch.Value?.Round.Value?.Beatmaps == null) return false;
            if (!CurrentMatch.Value.Round.Value.UseBoard.Value) return false;

            List<TeamColour> winColours =
            [
                isWin(1, 1, 1, 4),
                isWin(2, 1, 2, 4),
                isWin(3, 1, 3, 4),
                isWin(4, 1, 4, 4),
                isWin(1, 1, 4, 1),
                isWin(1, 2, 4, 2),
                isWin(1, 3, 4, 3),
                isWin(1, 4, 4, 4),
                isWin(1, 1, 4, 4),
                isWin(1, 4, 4, 1)
            ];

            TeamColour winner = winColours.Contains(TeamColour.Red)
                ? winColours.Contains(TeamColour.Blue)
                    ? TeamColour.Neutral
                    : TeamColour.Red
                : winColours.Contains(TeamColour.Blue)
                    ? TeamColour.Blue
                    : TeamColour.None;

            teamWinner = winner;

            if (winner == TeamColour.Neutral || winner == TeamColour.None)
            {
                // Reset team scores
                CurrentMatch.Value.Team1Score.Value = 0;
                CurrentMatch.Value.Team2Score.Value = 0;

                return winner == TeamColour.Neutral;
            }
            else
            {
                CurrentMatch.Value.Team1Score.Value = winner == TeamColour.Red ? 6 : 0;
                CurrentMatch.Value.Team2Score.Value = winner == TeamColour.Blue ? 6 : 0;

                return true;
            }
        }

        /// <summary>
        /// Get all beatmaps on a specified line.
        /// </summary>
        /// <param name="startX">The start point of the line, X value.</param>
        /// <param name="startY">The start point of the line, Y value.</param>
        /// <param name="endX">The end point of the line, X value.</param>
        /// <param name="endY">The end point of the line, Y value.</param>
        /// <returns>A <see langword="List"/> of <see cref="RoundBeatmap"/>.</returns>
        private List<RoundBeatmap> getMapLine(int startX, int startY, int endX, int endY)
        {
            List<RoundBeatmap> mapLine = new List<RoundBeatmap>();

            // Reject null matches
            if (CurrentMatch.Value == null) return mapLine;

            // Vertical Lines
            if (startX == endX)
            {
                for (int i = startY; i <= endY; i++)
                {
                    var map = getBoardMap(startX, i);
                    if (map != null) mapLine.Add(map);
                }
            }
            // Horizontal line
            else if (startY == endY)
            {
                for (int i = startX; i <= endX; i++)
                {
                    var map = getBoardMap(i, startY);
                    if (map != null) mapLine.Add(map);
                }
            }
            // Diagonal line
            else
            {
                int stepX = endX > startX ? 1 : -1;
                int stepY = endY > startY ? 1 : -1;

                for (int i = 0; i <= 3; i++)
                {
                    var map = getBoardMap(startX + i * stepX, startY + i * stepY);
                    if (map != null) mapLine.Add(map);
                }
            }

            return mapLine;
        }

        /// <summary>
        /// Detects if either team has won.
        ///
        /// <br></br>The given line should be either a straight line or a diagonal line.
        /// </summary>
        /// <param name="startX">The start point of the line, X value.</param>
        /// <param name="startY">The start point of the line, Y value.</param>
        /// <param name="endX">The end point of the line, X value.</param>
        /// <param name="endY">The end point of the line, Y value.</param>
        /// <returns>the winner team's colour, or <see cref="TeamColour.Neutral"/> if there isn't one</returns>
        private TeamColour isWin(int startY, int startX, int endY, int endX)
        {
            // Currently limited to 4x4 use only
            if ((endX - startX) % 3 != 0 || (endY - startY) % 3 != 0) return TeamColour.None;

            // Reject null matches
            if (CurrentMatch.Value == null) return TeamColour.None;

            var mapLine = getMapLine(startY, startX, endY, endX);

            var result = mapLine.Select(m => CurrentMatch.Value.PicksBans.FirstOrDefault(p => p.BeatmapID == m.Beatmap?.OnlineID && p.Type != ChoiceType.Pick))
                                .GroupBy(p => p?.Type);

            if (result.FirstOrDefault(g => g.Key == ChoiceType.BlueWin)?.Count() == mapLine.Count)
            {
                return TeamColour.Blue;
            }

            if (result.FirstOrDefault(g => g.Key == ChoiceType.RedWin)?.Count() == mapLine.Count)
            {
                return TeamColour.Red;
            }

            return TeamColour.None;
        }

        /// <summary>
        /// Detects if the board satisfies the conditions to enter the EX stage.
        /// </summary>
        /// <returns>true if satisfies, otherwise false.</returns>
        public bool DetectTieBreaker()
        {
            if (CurrentMatch.Value?.Round.Value?.Beatmaps == null) return false;
            if (!CurrentMatch.Value.Round.Value.UseBoard.Value) return false;

            // Manba out
            // TODO: Rewrite based on new rules
            bool isRowAvailable = canWin(1, 1, 1, 4) || canWin(2, 1, 2, 4) || canWin(3, 1, 3, 4) || canWin(4, 1, 4, 4);
            bool isColumnAvailable = canWin(1, 1, 4, 1) || canWin(1, 2, 4, 2) || canWin(1, 3, 4, 3) || canWin(1, 4, 4, 4);
            bool isDiagonalAvailable = canWin(1, 1, 4, 4) || canWin(1, 4, 4, 1);

            isInTieBreaker = !isDiagonalAvailable && !isRowAvailable && !isColumnAvailable;
            return isInTieBreaker;
        }

        /// <summary>
        /// Detects if either team could use the given line to win.
        ///
        /// <br></br>The given line should be either a straight line or a diagonal line.
        /// </summary>
        /// <param name="startX">The start point of the line, X value.</param>
        /// <param name="startY">The start point of the line, Y value.</param>
        /// <param name="endX">The end point of the line, X value.</param>
        /// <param name="endY">The end point of the line, Y value.</param>
        /// <returns>true if either team can, otherwise false</returns>
        private bool canWin(int startY, int startX, int endY, int endX)
        {
            // Currently limited to 4x4 use only
            if ((endX - startX) % 3 != 0 || (endY - startY) % 3 != 0) return false;

            // Reject null matches
            if (CurrentMatch.Value == null) return false;

            List<RoundBeatmap> mapLine = getMapLine(startX, startY, endX, endY);
            TeamColour thisColour = TeamColour.Neutral;

            foreach (RoundBeatmap b in mapLine)
            {
                // Get the coloured map
                var pickedMap = CurrentMatch.Value.PicksBans.FirstOrDefault(p =>
                    (p.BeatmapID == b.Beatmap?.OnlineID && (p.Type == ChoiceType.RedWin || p.Type == ChoiceType.BlueWin)));

                // Have banned maps: Cannot win
                if (CurrentMatch.Value.PicksBans.Any(p => (p.BeatmapID == b.Beatmap?.OnlineID && p.Type == ChoiceType.Ban))) return false;

                if (pickedMap != null)
                {
                    // Set the default colour
                    if (thisColour == TeamColour.Neutral) { thisColour = pickedMap.Team; }
                    // Different mark colour: Cannot win
                    else
                    {
                        if (thisColour != pickedMap.Team) return false;
                    }
                }
            }

            // Finally: Can win
            return true;
        }

        /// <summary>
        /// Get a beatmap placed on a specific point on the board.
        /// </summary>
        /// <param name="x">The X coordinate value of the beatmap.</param>
        /// <param name="y">The Y coordinate value of the beatmap.</param>
        /// <returns>A <see cref="RoundBeatmap"/>, pointing to the corresponding beatmap.</returns>
        private RoundBeatmap? getBoardMap(int x, int y)
        {
            BoardBeatmapPanel? dMap = boardMapList.FirstOrDefault(p => p.RealX == x && p.RealY == y && p.Mod != "TB");
            return CurrentMatch.Value?.Round.Value?.Beatmaps.FirstOrDefault(p => p.Beatmap?.OnlineID == dMap?.Beatmap?.OnlineID && p.Mods != "TB");
        }

        private Vector2 getBlockPosition(int x, int y)
        {
            return new Vector2(-400 + x * 160, -450 + y * 160);
        }

        private void updateDisplay()
        {
            boardContainer.Clear();
            boardMapList.Clear();
            sceneManager?.ReloadChat();

            if (CurrentMatch.Value == null)
            {
                warningContainer.Child = new WarningBox(BaseStrings.MatchUnavailableWarning);
                warningContainer.FadeIn(duration: 200, easing: Easing.OutCubic);
                return;
            }

            if (CurrentMatch.Value.Round.Value != null)
            {
                // Use predefined Board coordinate
                if (CurrentMatch.Value.Round.Value.UseBoard.Value)
                {
                    warningContainer.FadeOut(duration: 200, easing: Easing.OutCubic);

                    for (int i = 1; i <= 4; i++)
                    {
                        for (int j = 1; j <= 4; j++)
                        {
                            var nextMap = CurrentMatch.Value.Round.Value.Beatmaps.FirstOrDefault(p => p.Mods != "TB" && p.BoardX == j && p.BoardY == i);

                            if (nextMap != null)
                            {
                                Vector2 position = getBlockPosition(j, i);
                                var mapDrawable = new BoardBeatmapPanel(nextMap.Beatmap, nextMap.Mods, nextMap.ModIndex, j, i)
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    X = position.X,
                                    Y = position.Y,
                                };
                                boardContainer.Add(mapDrawable);
                                boardMapList.Add(mapDrawable);
                            }
                            else
                            {
                                // TODO: Do we need to add a placeholder here?
                            }
                        }
                    }
                }
                else
                {
                    warningContainer.Child = new WarningBox(BaseStrings.BoardModeUnsetWarning);
                    warningContainer.FadeIn(duration: 200, easing: Easing.OutCubic);
                }
            }
        }
    }
}

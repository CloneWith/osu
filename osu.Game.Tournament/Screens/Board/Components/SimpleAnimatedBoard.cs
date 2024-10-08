// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Framework.Threading;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Screens.Board.Components
{
    public partial class SimpleAnimatedBoard : CompositeDrawable
    {
        private Container boardContainer = null!;
        private List<SimpleBoardBeatmapPanel> boardMapList = new List<SimpleBoardBeatmapPanel>();

        private WarningBox warning = null!;

        private readonly Bindable<TournamentMatch?> currentMatch = new Bindable<TournamentMatch?>();

        private bool useEX = false;
        private bool hasTrap = false;

        private ScheduledDelegate? scheduledScreenChange;

        public SimpleAnimatedBoard(Bindable<TournamentMatch?> currentMatch)
        {
            this.currentMatch.BindTo(currentMatch);
            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            InternalChildren = new Drawable[]
            {
                boardContainer = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    CornerRadius = 10,
                },
            };
            updateDisplay();
        }

        private void revertSwaps()
        {
            if (currentMatch.Value == null)
                return;

            var swaps = currentMatch.Value.SwapRecords;

            if (swaps.Count == 0)
                return;

            // Revert in Reversed order 0.0
            foreach (var rec in swaps.Reverse())
            {
                // TODO: Use a queue for swap animations
                if (rec.Key.Beatmap != null && rec.Value.Beatmap != null)
                    SwapMap(rec.Key.Beatmap.OnlineID, rec.Value.Beatmap.OnlineID);
                swaps.Remove(rec);
            }
        }

        public override void Hide()
        {
            scheduledScreenChange?.Cancel();
            base.Hide();
        }

        protected void SwapMap(int sourceMapID, int targetMapID)
        {
            var sourceDrawable = boardMapList.FirstOrDefault(p => p.Beatmap?.OnlineID == sourceMapID);
            var targetDrawable = boardMapList.FirstOrDefault(p => p.Beatmap?.OnlineID == targetMapID);

            // Already detected null here, no need to do again
            if (sourceDrawable != null && targetDrawable != null)
            {
                if (currentMatch.Value?.Round.Value?.UseBoard.Value == false) return;

                int middleX = sourceDrawable.RealX;
                int middleY = sourceDrawable.RealY;
                float middleDX = sourceDrawable.X;
                float middleDY = sourceDrawable.Y;

                sourceDrawable.RealX = targetDrawable.RealX;
                sourceDrawable.RealY = targetDrawable.RealY;

                targetDrawable.RealX = middleX;
                targetDrawable.RealY = middleY;

                sourceDrawable.Flash();
                targetDrawable.Flash();

                sourceDrawable.Delay(200).Then().MoveTo(new Vector2(targetDrawable.X, targetDrawable.Y), 500, Easing.OutCubic);
                targetDrawable.Delay(200).Then().MoveTo(new Vector2(middleDX, middleDY), 500, Easing.OutCubic);
            }
            else
            {
                // Rare, but may happen
                throw new InvalidOperationException("Cannot get the corresponding maps.");
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
            if (currentMatch.Value == null) return mapLine;

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
            if (currentMatch.Value == null) return TeamColour.None;

            var mapLine = getMapLine(startY, startX, endY, endX);

            var result = mapLine.Select(m => currentMatch.Value.PicksBans.FirstOrDefault(p => p.BeatmapID == m.Beatmap?.OnlineID && p.Type != ChoiceType.Pick))
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
        /// Detects if either team could use the given line to win.
        ///
        /// <br></br>The given line should be either a straight line or a diagonal line.
        /// </summary>
        /// <param name="startX">The start point of the line, X value.</param>
        /// <param name="startY">The start point of the line, Y value.</param>
        /// <param name="endX">The end point of the line, X value.</param>
        /// <param name="endY">The end point of the line, Y value.</param>
        /// <returns>true if can, otherwise false</returns>
        private bool canWin(int startY, int startX, int endY, int endX)
        {
            List<RoundBeatmap> mapLine = new List<RoundBeatmap>();
            TeamColour thisColour = TeamColour.Neutral;

            // Currently limited to 4x4 use only
            if ((endX - startX) % 3 != 0 || (endY - startY) % 3 != 0) return false;

            // Reject null matches
            if (currentMatch.Value == null) return false;

            mapLine = getMapLine(startX, startY, endX, endY);

            foreach (RoundBeatmap b in mapLine)
            {
                // Get the coloured map
                var pickedMap = currentMatch.Value.PicksBans.FirstOrDefault(p =>
                    (p.BeatmapID == b.Beatmap?.OnlineID &&
                     (p.Type == ChoiceType.RedWin
                      || p.Type == ChoiceType.BlueWin)));

                // Have banned maps: Cannot win
                if (currentMatch.Value.PicksBans.Any(p => (p.BeatmapID == b.Beatmap?.OnlineID && p.Type == ChoiceType.Ban))) return false;

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
                var drawableMap = boardMapList.FirstOrDefault(p => p.Beatmap?.OnlineID == b.Beatmap?.OnlineID);
                drawableMap?.Flash();
            }

            // Finally: Can win
            return true;
        }

        /// <summary>
        /// Get a beatmap placed on a specific point on the board.
        /// </summary>
        /// <param name="X">The X coordinate value of the beatmap.</param>
        /// <param name="Y">The Y coordinate value of the beatmap.</param>
        /// <returns>A <see cref="RoundBeatmap"/>, pointing to the corresponding beatmap.</returns>

        private RoundBeatmap? getBoardMap(int X, int Y)
        {
            SimpleBoardBeatmapPanel? dMap = boardMapList.FirstOrDefault(p => p.RealX == X && p.RealY == Y && p.Mod != "EX");
            return currentMatch.Value?.Round.Value?.Beatmaps.FirstOrDefault(p => p.Beatmap?.OnlineID == dMap?.Beatmap?.OnlineID && p.Mods != "EX");
        }

        private void updateDisplay()
        {
            boardContainer.Clear();
            boardMapList.Clear();

            if (currentMatch.Value == null)
            {
                AddInternal(warning = new WarningBox("Cannot access current match, sorry ;w;"));
                return;
            }

            if (currentMatch.Value.Round.Value != null)
            {
                // Use predefined Board coodinate
                if (currentMatch.Value.Round.Value.UseBoard.Value)
                {
                    warning?.FadeOut(duration: 200, easing: Easing.OutCubic);

                    for (int i = 1; i <= 4; i++)
                    {
                        for (int j = 1; j <= 4; j++)
                        {
                            var nextMap = currentMatch.Value.Round.Value.Beatmaps.FirstOrDefault(p => (p.Mods != "EX" && p.BoardX == j && p.BoardY == i));
                            if (nextMap != null)
                            {
                                var hasSwappedMap = currentMatch.Value.PendingSwaps.FirstOrDefault(p => p.BeatmapID == nextMap.Beatmap?.OnlineID);
                                var mapDrawable = new SimpleBoardBeatmapPanel(nextMap.Beatmap, nextMap.Mods, nextMap.ModIndex, j, i)
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    X = -400 + j * 160,
                                    Y = -400 + i * 160,
                                };
                                boardContainer.Add(mapDrawable);
                                boardMapList.Add(mapDrawable);
                                mapDrawable.InitAnimate((i + j) * 250);
                                mapDrawable.AddStateAnimate((i + j) * 250 + 250);
                            }
                            else
                            {
                                // TODO: Do we need to add a placeholder here?
                            }
                        }
                    }

                    if (currentMatch.Value.SwapRecords.Count > 0)
                    {
                        foreach (var i in currentMatch.Value.SwapRecords)
                        {
                            if (i.Key.Beatmap != null && i.Value.Beatmap != null)
                                SwapMap(i.Key.Beatmap.OnlineID, i.Value.Beatmap.OnlineID);
                        }
                    }
                }
                else
                {
                    AddInternal(warning = new WarningBox("This round isn't set up for board view..."));
                    return;
                }
            }
        }
    }
}

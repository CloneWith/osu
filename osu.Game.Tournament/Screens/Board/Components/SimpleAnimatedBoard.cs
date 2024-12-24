// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Screens.Board.Components
{
    public partial class SimpleAnimatedBoard : CompositeDrawable
    {
        private Container boardContainer = null!;
        private readonly List<SimpleBoardBeatmapPanel> boardMapList = new List<SimpleBoardBeatmapPanel>();

        private WarningBox warning = null!;

        private readonly Bindable<TournamentMatch?> currentMatch = new Bindable<TournamentMatch?>();

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
                float middleDx = sourceDrawable.X;
                float middleDy = sourceDrawable.Y;

                sourceDrawable.RealX = targetDrawable.RealX;
                sourceDrawable.RealY = targetDrawable.RealY;

                targetDrawable.RealX = middleX;
                targetDrawable.RealY = middleY;

                sourceDrawable.Flash();
                targetDrawable.Flash();

                sourceDrawable.Delay(200).Then().MoveTo(new Vector2(targetDrawable.X, targetDrawable.Y), 500, Easing.OutCubic);
                targetDrawable.Delay(200).Then().MoveTo(new Vector2(middleDx, middleDy), 500, Easing.OutCubic);
            }
            else
            {
                // Rare, but may happen
                throw new InvalidOperationException("Cannot get the corresponding maps.");
            }
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
                // Use predefined Board coordinate
                if (currentMatch.Value.Round.Value.UseBoard.Value)
                {
                    warning.FadeOut(duration: 200, easing: Easing.OutCubic);

                    for (int i = 1; i <= 4; i++)
                    {
                        for (int j = 1; j <= 4; j++)
                        {
                            var nextMap = currentMatch.Value.Round.Value.Beatmaps.FirstOrDefault(p => (p.Mods != "EX" && p.BoardX == j && p.BoardY == i));

                            if (nextMap != null)
                            {
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
                }
            }
        }
    }
}

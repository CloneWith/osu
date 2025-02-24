// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Screens.Board.Components
{
    public partial class SimpleAnimatedBoard : CompositeDrawable
    {
        private Container boardContainer = null!;
        private readonly List<SimpleBoardBeatmapPanel> boardMapList = new List<SimpleBoardBeatmapPanel>();

        private Container warningContainer = null!;

        private readonly Bindable<TournamentMatch?> currentMatch = new Bindable<TournamentMatch?>();

        public SimpleAnimatedBoard(Bindable<TournamentMatch?> currentMatch)
        {
            this.currentMatch.BindTo(currentMatch);
            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                boardContainer = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    CornerRadius = 10,
                },
                warningContainer = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                },
            };
            updateDisplay();
        }

        private void updateDisplay()
        {
            boardContainer.Clear();
            boardMapList.Clear();

            if (currentMatch.Value == null)
            {
                warningContainer.Child = new WarningBox("Cannot access current match, sorry ;w;");
                warningContainer.FadeIn(duration: 200, easing: Easing.OutCubic);
                return;
            }

            if (currentMatch.Value.Round.Value != null)
            {
                // Use predefined Board coordinate
                if (currentMatch.Value.Round.Value.UseBoard.Value)
                {
                    warningContainer.FadeOut(duration: 200, easing: Easing.OutCubic);

                    for (int i = 1; i <= 4; i++)
                    {
                        for (int j = 1; j <= 4; j++)
                        {
                            var nextMap = currentMatch.Value.Round.Value.Beatmaps.FirstOrDefault(p => p.Mods != "TB" && p.BoardX == j && p.BoardY == i);

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
                }
                else
                {
                    warningContainer.Child = new WarningBox("This round isn't set up for board view...");
                    warningContainer.FadeIn(duration: 200, easing: Easing.OutCubic);
                }
            }
        }
    }
}

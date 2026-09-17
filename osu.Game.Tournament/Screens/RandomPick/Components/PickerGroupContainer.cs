// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Screens.RandomPick.Components
{
    /// <summary>
    /// The boxes of every team taking part in the random phase, arranged in two rows which grow towards the
    /// middle of the screen.
    /// </summary>
    public partial class PickerGroupContainer : Container
    {
        private const float group_spacing = 7;

        private readonly List<PickerGroup> groups = new List<PickerGroup>();
        private readonly FillFlowContainer topGroups;
        private readonly FillFlowContainer bottomGroups;

        public PickerGroupContainer()
        {
            Children = new Drawable[]
            {
                topGroups = new FillFlowContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,

                    AutoSizeAxes = Axes.Both,

                    Spacing = new Vector2(group_spacing, 0)
                },
                bottomGroups = new FillFlowContainer
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,

                    AutoSizeAxes = Axes.Both,

                    Spacing = new Vector2(group_spacing, 0)
                }
            };
        }

        /// <summary>
        /// The boxes on screen, in team order — which is also the order a draw looks for room in.
        /// </summary>
        public IReadOnlyList<PickerGroup> Groups => groups;

        /// <summary>
        /// (Re)build one box per team, the first half along the top and the rest along the bottom.
        /// </summary>
        public void SetGroups(IReadOnlyList<TournamentTeam> teams)
        {
            topGroups.Clear();
            bottomGroups.Clear();
            groups.Clear();

            int half = (int)MathF.Ceiling(teams.Count / 2f);

            for (int i = 0; i < teams.Count; i++)
            {
                var group = new PickerGroup(teams[i]);

                groups.Add(group);

                if (i < half)
                    topGroups.Add(group);
                else
                    bottomGroups.Add(group);
            }
        }

        /// <summary>
        /// The box standing for <paramref name="team"/>, or <c>null</c> if it is not on screen.
        /// </summary>
        public PickerGroup? GroupFor(TournamentTeam team) => groups.FirstOrDefault(group => group.Team == team);

        /// <summary>
        /// Where <paramref name="group"/> sits in the rotation.
        /// </summary>
        public int IndexOf(PickerGroup group) => groups.IndexOf(group);
    }
}

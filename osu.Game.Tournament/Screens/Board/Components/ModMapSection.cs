// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceFumo;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Screens.Board.Components
{
    /// <summary>
    /// A section showing all beatmaps of a specific mod category.
    /// </summary>
    public partial class ModMapSection : FillFlowContainer
    {
        private const int width = 250;

        public IReadOnlyList<FumoBeatmapPanel> Cards => mapFlow.Children;

        [Resolved]
        private LadderInfo ladder { get; set; } = null!;

        private readonly string modAcronym;
        private readonly string modName;

        private readonly ModColourScheme colourScheme;

        private FillFlowContainer<FumoBeatmapPanel> mapFlow = null!;

        /// <inheritdoc cref="ModMapSection"/>
        /// <param name="acronym">the acronym of the mod.</param>
        /// <param name="name">the title of the section's header.</param>
        public ModMapSection(string acronym, string? name = null)
        {
            modAcronym = acronym;
            modName = name ?? acronym;
            colourScheme = ModColours.FromModString(acronym);
            Width = width;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;
            Spacing = new Vector2(10);
            Children = new Drawable[]
            {
                new Container
                {
                    Name = @"Header",
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Child = new FumoSectionHeader
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Scale = new Vector2(0.75f),
                        AccentColour = colourScheme.Accent,
                        Icon = TournamentExtensions.GetModIcon(modAcronym),
                        IconSize = 30,
                        Text = modName,
                    },
                },
                mapFlow = new FillFlowContainer<FumoBeatmapPanel>
                {
                    Name = @"Map pool content",
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Full,
                    Spacing = new Vector2(8, 15),
                    Padding = new MarginPadding { Horizontal = 10 },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ladder.CurrentMatch.BindValueChanged(matchChanged, true);
        }

        private void matchChanged(ValueChangedEvent<TournamentMatch?> e)
        {
            if (e.NewValue != null)
            {
                e.NewValue.Round.BindValueChanged(_ => updateList());
                e.NewValue.Round.Value?.Beatmaps.BindCollectionChanged((_, _) => updateList());
            }

            updateList();
        }

        private void updateList()
        {
            var mapList = ladder.CurrentMatch.Value?.Round.Value?.Beatmaps.Where(b =>
                b.Mods.Equals(modAcronym, StringComparison.OrdinalIgnoreCase)).ToList();

            if (mapList != null && mapList.Count != 0)
            {
                this.FadeIn(300, Easing.OutQuint);
                mapFlow.ChildrenEnumerable = mapList.Select(m => new FumoBeatmapPanel(m));
            }
            else
            {
                this.FadeOut(300, Easing.OutQuint);
                mapFlow.Clear();
                mapFlow.Hide();
            }
        }
    }
}

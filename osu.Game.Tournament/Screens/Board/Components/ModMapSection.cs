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
        /// <summary>
        /// The number of beatmap panels laid out per row, each of equal width.
        /// </summary>
        private const int panels_per_row = 3;

        public IEnumerable<FumoBeatmapPanel> Cards => mapFlow.Children.Select(c => (FumoBeatmapPanel)c.Child);

        [Resolved]
        private LadderInfo ladder { get; set; } = null!;

        private readonly string modAcronym;
        private readonly string modName;

        private readonly ModColourScheme colourScheme;

        private FillFlowContainer<Container> mapFlow = null!;

        /// <inheritdoc cref="ModMapSection"/>
        /// <param name="acronym">the acronym of the mod.</param>
        /// <param name="name">the title of the section's header.</param>
        public ModMapSection(string acronym, string? name = null)
        {
            modAcronym = acronym;
            modName = name ?? acronym;
            colourScheme = ModColours.FromModString(acronym);
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
                mapFlow = new FillFlowContainer<Container>
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
                mapFlow.Show();
                mapFlow.ChildrenEnumerable = mapList.Select(m => new Container
                {
                    Name = @"Panel cell",
                    Child = new FumoBeatmapPanel(m),
                });
            }
            else
            {
                this.FadeOut(300, Easing.OutQuint);
                mapFlow.Clear();
                mapFlow.Hide();
            }
        }

        protected override void Update()
        {
            base.Update();

            if (mapFlow.Children.Count == 0)
                return;

            // Split the flow's content width (minus horizontal padding and inner gaps) evenly across each row.
            float contentWidth = mapFlow.DrawWidth - mapFlow.Padding.TotalHorizontal;

            if (contentWidth <= 0)
                return;

            // Shave a tiny epsilon so rounding can't push the third cell of a row onto the next line.
            float cellWidth = (contentWidth - mapFlow.Spacing.X * (panels_per_row - 1)) / panels_per_row - 0.5f;
            float panelScale = cellWidth / FumoBeatmapPanel.WIDTH;
            var cellSize = new Vector2(cellWidth, FumoBeatmapPanel.HEIGHT * panelScale);

            foreach (var child in mapFlow.Children)
            {
                if (child.Size != cellSize)
                    child.Size = cellSize;

                var panel = (FumoBeatmapPanel)child.Child;
                if (panel.Scale != new Vector2(panelScale))
                    panel.Scale = new Vector2(panelScale);
            }
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterfaceFumo;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Localisation;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Screens.Board.Components
{
    /// <summary>
    /// A section showing all beatmaps of a specific mod category.
    /// </summary>
    public partial class ModMapSection : FillFlowContainer
    {
        public const int WIDTH = 250;

        public readonly string ModAcronym;
        public readonly string ModName;

        public IReadOnlyList<FumoBeatmapPanel> Cards => mapFlow.Children;

        [Resolved]
        private LadderInfo ladder { get; set; } = null!;

        private readonly ModColourScheme colourScheme;

        private FillFlowContainer<FumoBeatmapPanel> mapFlow = null!;
        private FillFlowContainer placeholderFlow = null!;

        /// <inheritdoc cref="ModMapSection"/>
        /// <param name="acronym">the acronym of the mod.</param>
        /// <param name="name">the title of the section's header.</param>
        public ModMapSection(string acronym, string? name = null)
        {
            ModAcronym = acronym;
            ModName = name ?? acronym;
            colourScheme = ModColours.FromModString(acronym);
            Width = WIDTH;
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
                        Icon = TournamentGame.GetModIcon(ModAcronym),
                        IconSize = 30,
                        Text = ModName,
                    },
                },
                mapFlow = new FillFlowContainer<FumoBeatmapPanel>
                {
                    Name = @"Map pool content",
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(10),
                    Padding = new MarginPadding { Horizontal = 10 },
                },
                placeholderFlow = new FillFlowContainer
                {
                    Name = @"Placeholder",
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(10),
                    Padding = new MarginPadding { Horizontal = 10 },
                    Children = new Drawable[]
                    {
                        new SpriteIcon
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Icon = FontAwesome.Solid.ExclamationCircle,
                            Size = new Vector2(24),
                        },
                        new TournamentSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = BaseStrings.NoBeatmapAvailable,
                            Font = OsuFont.Torus.With(weight: FontWeight.SemiBold, size: 24),
                        },
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ladder.CurrentMatch.BindValueChanged(matchChanged);
            ladder.CurrentMatch.Value?.Round.BindValueChanged(_ => updateList());
            ladder.CurrentMatch.Value?.Round.Value?.Beatmaps.BindCollectionChanged((_, _) => updateList());

            updateList();
        }

        private void matchChanged(ValueChangedEvent<TournamentMatch?> e)
        {
            if (e.OldValue != null)
            {
                e.OldValue.ChessPlacements.CollectionChanged -= placementChanged;
            }

            if (e.NewValue != null)
            {
                e.NewValue.ChessPlacements.CollectionChanged += placementChanged;
                e.NewValue.Round.BindValueChanged(_ => updateList());
                e.NewValue.Round.Value?.Beatmaps.BindCollectionChanged((_, _) => updateList());
            }

            updateList();
        }

        private void placementChanged(object? _, NotifyCollectionChangedEventArgs __)
            => Scheduler.AddOnce(updateList);

        private void updateList()
        {
            var mapList = ladder.CurrentMatch.Value?.Round.Value?.Beatmaps.Where(b =>
                b.Mods.Equals(ModAcronym, StringComparison.OrdinalIgnoreCase));

            mapFlow.Direction = mapList != null && mapList.Any() ? FillDirection.Full : FillDirection.Horizontal;

            if (mapList != null && mapList.Any())
            {
                mapFlow.Show();
                placeholderFlow.Hide();

                mapFlow.ChildrenEnumerable = mapList.Select(m => new FumoBeatmapPanel(m)
                {
                    Scale = new Vector2(0.8f),
                });
            }
            else
            {
                mapFlow.Clear();
                mapFlow.Hide();
                placeholderFlow.Show();
            }
        }
    }
}

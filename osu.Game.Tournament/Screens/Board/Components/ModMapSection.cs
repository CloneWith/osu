// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterfaceFumo;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Screens.Board.Components
{
    public partial class ModMapSection : FillFlowContainer
    {
        public const int WIDTH = 250;

        public readonly string ModAcronym;
        public readonly string ModName;

        [Resolved]
        private LadderInfo ladder { get; set; } = null!;

        private readonly ModColourScheme colourScheme;

        private FillFlowContainer mapFlow = null!;

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
            Spacing = new Vector2(20);
            Children = new Drawable[]
            {
                new Container
                {
                    Name = @"Header",
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        new FumoSectionHeader
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            AccentColour = colourScheme.Accent,
                            Icon = TournamentGame.GetModIcon(ModAcronym),
                            IconSize = 30,
                            Text = ModName,
                        },
                        new FillFlowContainer
                        {
                            Name = @"Remaining maps display",
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(3),
                            Children = new Drawable[]
                            {
                                new TournamentSpriteText
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Text = "Remaining: ",
                                },
                                // TODO: Integrate with LadderInfo
                                new TournamentSpriteText
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Text = "1 3 4 5",
                                },
                            },
                        },
                    },
                },
                mapFlow = new FillFlowContainer
                {
                    Name = @"Map pool content",
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(10),
                    Padding = new MarginPadding { Horizontal = 15 },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ladder.CurrentMatch.BindValueChanged(_ => updateList());
            ladder.CurrentMatch.Value?.Round.BindValueChanged(_ => updateList());
            ladder.CurrentMatch.Value?.Round.Value?.Beatmaps.BindCollectionChanged((_, _) => updateList());

            updateList();
        }

        private void updateList()
        {
            var mapList = ladder.CurrentMatch.Value?.Round.Value?.Beatmaps.Where(b =>
                b.Mods.Equals(ModAcronym, StringComparison.OrdinalIgnoreCase));

            mapFlow.Direction = mapList != null ? FillDirection.Full : FillDirection.Vertical;

            if (mapList != null)
            {
                mapFlow.ChildrenEnumerable = mapList.Select(m => new FumoBeatmapPanel(m));
            }
            else
            {
                mapFlow.Children = new Drawable[]
                {
                    new SpriteIcon
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Icon = FontAwesome.Solid.ExclamationCircle,
                        Size = new Vector2(32),
                    },
                    new TournamentSpriteText
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Text = @"No map available!",
                        Font = OsuFont.Torus.With(weight: FontWeight.SemiBold, size: 24),
                    },
                };
            }
        }
    }
}

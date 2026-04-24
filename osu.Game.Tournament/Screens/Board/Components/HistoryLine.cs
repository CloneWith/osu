// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceFumo;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Localisation;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Screens.Board.Components
{
    public partial class HistoryLine : FillFlowContainer
    {
        public History History { get; }

        private SpriteIcon icon = null!;
        private TextFlowContainer descriptionText = null!;

        private readonly Action<SpriteText> textFormat = t =>
            t.Font = OsuFont.Torus.With(size: 18, weight: FontWeight.SemiBold);

        public HistoryLine(History history)
        {
            History = history;
            Spacing = new Vector2(10);
        }

        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder)
        {
            Direction = FillDirection.Vertical;
            InternalChildren = new Drawable[]
            {
                new GridContainer
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 25,
                    ColumnDimensions =
                    [
                        new Dimension(GridSizeMode.Absolute, 18),
                        new Dimension(),
                        new Dimension(GridSizeMode.Absolute, 32),
                    ],
                    Content = new Drawable[][]
                    {
                        [
                            icon = new SpriteIcon
                            {
                                Name = @"Symbolic icon",
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Size = new Vector2(18),
                                Icon = FontAwesome.Solid.Adjust,
                            },
                            descriptionText = new OsuTextFlowContainer(textFormat)
                            {
                                Name = @"Record description",
                                RelativeSizeAxes = Axes.Both,
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                TextAnchor = Anchor.CentreLeft,
                                Margin = new MarginPadding { Left = 10 },
                            },
                            new Container
                            {
                                Name = @"Position display",
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Alpha = History.OriginalType is ChoiceType.Pick ? 1 : 0,
                                Margin = new MarginPadding { Left = 5 },
                                Children = new Drawable[]
                                {
                                    new Circle
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = TournamentExtensions.GetTeamColour(History.Team),
                                    },
                                    new TournamentSpriteText
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Text = $"{(char)('A' + History.Column - 1)}{History.Row}",
                                        Font = OsuFont.Torus.With(size: 19, weight: FontWeight.SemiBold),
                                        Margin = new MarginPadding { Horizontal = 5 },
                                    },
                                },
                            },
                        ],
                    },
                },
            };

            icon.Icon = History.Type switch
            {
                HistoryType.Normal => History.OriginalType switch
                {
                    ChoiceType.Ban => FontAwesome.Solid.Ban,
                    ChoiceType.Pick => FontAwesome.Solid.Check,
                    ChoiceType.RedWin or ChoiceType.BlueWin => FontAwesome.Solid.Trophy,
                    _ => FontAwesome.Solid.Fire,
                },
                HistoryType.ShiroPlacement => FontAwesome.Regular.Circle,
                HistoryType.OwnerUpdate => FontAwesome.Solid.ArrowUp,
                _ => FontAwesome.Solid.Fire,
            };

            icon.Colour = TournamentExtensions.GetTeamColour(History.Team);

            descriptionText.AddText(TournamentExtensions.GetTeamString(History.Team, true), t =>
            {
                textFormat.Invoke(t);
                t.Colour = TournamentExtensions.GetTeamColour(History.Team);
            });

            switch (History.Type)
            {
                case HistoryType.Normal:
                    descriptionText.AddText(History.OriginalType switch
                    {
                        ChoiceType.Ban => HistoryStrings.Banned,
                        ChoiceType.Pick => HistoryStrings.Picked,
                        ChoiceType.RedWin or ChoiceType.BlueWin => HistoryStrings.Won,
                        _ => "",
                    });

                    if (History.Mod != null && History.ModIndex != null)
                    {
                        var beatmap = ladder.CurrentMatch.Value?.Round.Value?.Beatmaps.FirstOrDefault(b =>
                            b.Mods == History.Mod && b.ModIndex == History.ModIndex);

                        if (History.OriginalType is (ChoiceType.Ban or ChoiceType.Pick) && beatmap != null)
                        {
                            AddInternal(new TournamentBeatmapPanel(beatmap.Beatmap, History.Mod, History.ModIndex)
                            {
                                RelativeSizeAxes = Axes.X,
                                Width = 1,
                            });
                        }
                    }

                    break;

                case HistoryType.ShiroPlacement:
                    descriptionText.AddText(HistoryStrings.PlacedShiro);
                    break;

                case HistoryType.OwnerUpdate:
                    string usedPieceText = string.Empty;
                    History.UsedPieces.ForEach(p => usedPieceText += $" {p.mod}{p.modIndex}");
                    descriptionText.AddText(HistoryStrings.UpdatedWinner(usedPieceText));
                    break;
            }

            descriptionText.AddText($"{History.Mod}{History.ModIndex}", t =>
            {
                textFormat.Invoke(t);
                t.Colour = ModColours.FromModString(History.Mod ?? "").Accent;
            });
        }
    }
}

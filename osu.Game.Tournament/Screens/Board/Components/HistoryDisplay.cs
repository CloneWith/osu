// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceFumo;
using osu.Game.Overlays.Chat;
using osu.Game.Tournament.Localisation;
using osu.Game.Tournament.Models;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Board.Components
{
    public partial class HistoryDisplay : GridContainer
    {
        [Resolved]
        private LadderInfo ladderInfo { get; set; } = null!;

        private readonly Bindable<TournamentMatch?> currentMatch = new Bindable<TournamentMatch?>();

        private FillFlowContainer<HistoryLine> innerContent = null!;
        private TournamentSpriteText currentStatusText = null!;
        private SpriteIcon winnerIcon = null!;
        private LoadingSpinner topSpinner = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            RowDimensions =
            [
                new Dimension(GridSizeMode.AutoSize),
                new Dimension(),
            ];

            Content = new Drawable[][]
            {
                [
                    new FumoSectionHeader
                    {
                        Icon = FontAwesome.Solid.History,
                        AccentColour = FumoColours.DeepPurple.Regular,
                        Text = HistoryStrings.MatchHistory,
                        Margin = new MarginPadding { Bottom = 10 },
                    },
                ],
                [
                    new ChannelScrollContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        ScrollbarVisible = false,
                        Padding = new MarginPadding { Horizontal = 10 },
                        Child = new FillFlowContainer
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(10),
                            Children = new Drawable[]
                            {
                                innerContent = new FillFlowContainer<HistoryLine>
                                {
                                    Name = "Main flow",
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(5),
                                },
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(10),
                                    Children = new Drawable[]
                                    {
                                        new Container
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            Size = new Vector2(18),
                                            Children = new Drawable[]
                                            {
                                                topSpinner = new LoadingSpinner
                                                {
                                                    Anchor = Anchor.CentreLeft,
                                                    Origin = Anchor.CentreLeft,
                                                    Size = new Vector2(18),
                                                },
                                                winnerIcon = new SpriteIcon
                                                {
                                                    Anchor = Anchor.CentreLeft,
                                                    Origin = Anchor.CentreLeft,
                                                    Size = new Vector2(18),
                                                    Icon = FontAwesome.Solid.Trophy,
                                                    Alpha = 0,
                                                }
                                            }
                                        },
                                        currentStatusText = new TournamentSpriteText
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            Text = HistoryStrings.MatchInProgress,
                                            Font = OsuFont.Torus.With(size: 18, weight: FontWeight.SemiBold),
                                        },
                                    },
                                },
                            },
                        },
                    },
                ],
            };

            topSpinner.Show();
        }

        private void historyCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldItems != null && e.OldStartingIndex != -1)
                    {
                        foreach (object item in e.OldItems)
                        {
                            var target = innerContent.Children.Where(line => line.Placement == (ChessPlacement)item!)
                                                     .ToList();

                            foreach (HistoryLine line in target)
                                innerContent.Remove(line, true);
                        }
                    }

                    if (e.NewItems != null && e.NewStartingIndex != -1)
                    {
                        for (int i = 0; i < e.NewItems.Count; i++)
                        {
                            var placement = (ChessPlacement)e.NewItems[i]!;
                            if (!placement.IsViewable) continue;

                            innerContent.Insert(e.NewStartingIndex + i, new HistoryLine(placement)
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                            });
                        }
                    }

                    break;

                case NotifyCollectionChangedAction.Reset:
                    innerContent.Clear();
                    break;
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            currentMatch.BindTo(ladderInfo.CurrentMatch);

            currentMatch.BindValueChanged(e =>
            {
                if (e.OldValue != null)
                {
                    innerContent.Clear();
                    e.OldValue.ChessPlacements.CollectionChanged -= historyCollectionChanged;
                }

                if (e.NewValue != null)
                {
                    innerContent.ChildrenEnumerable = e.NewValue.ChessPlacements
                                                       .Where(p => p.IsViewable)
                                                       .Select(p => new HistoryLine(p)
                                                       {
                                                           RelativeSizeAxes = Axes.X, AutoSizeAxes = Axes.Y,
                                                       });
                    e.NewValue.ChessPlacements.CollectionChanged += historyCollectionChanged;
                }
            }, true);

            currentMatch.Value?.IsFinalStage.BindValueChanged(e =>
            {
                if (e.NewValue)
                {
                    topSpinner.FadeIn(500, Easing.OutQuint);
                    winnerIcon.FadeOut(500, Easing.OutQuint);
                    topSpinner.FadeColour(Color4.Orange, 500, Easing.OutQuint);
                    currentStatusText.FadeColour(Color4.Orange, 500, Easing.OutQuint);
                    currentStatusText.Text = HistoryStrings.MatchTiebreaker;
                }
                else
                {
                    currentMatch.Value?.Completed.TriggerChange();
                }
            });

            currentMatch.Value?.Completed.BindValueChanged(e =>
            {
                topSpinner.FadeColour(Color4.White, 500, Easing.OutQuint);
                topSpinner.FadeTo(e.NewValue ? 0 : 1, 500, Easing.OutQuint);
                winnerIcon.FadeTo(e.NewValue ? 1 : 0, 500, Easing.OutQuint);
                currentStatusText.FadeColour(e.NewValue ? TournamentExtensions.GetTeamColour(currentMatch.Value.WinnerColour) : Color4.White, 500, Easing.OutQuint);
                currentStatusText.Text = e.NewValue
                    ? HistoryStrings.MatchEnded(currentMatch.Value.WinnerColour)
                    : HistoryStrings.MatchInProgress;

                if (e.NewValue)
                    winnerIcon.FadeColour(TournamentExtensions.GetTeamColour(currentMatch.Value.WinnerColour), 500, Easing.OutQuint);
            }, true);
        }
    }
}

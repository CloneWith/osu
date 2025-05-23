// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceFumo;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Localisation;
using osu.Game.Tournament.Localisation.Screens;
using osu.Game.Tournament.Models;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Board.Components
{
    public partial class ChessMapPool : Container
    {
        public IEnumerable<FumoBeatmapPanel> MapPanels => sectionFlow.Children.SelectMany(s => s.Cards);

        private FillFlowContainer<ModMapSection> sectionFlow = null!;
        private TournamentSpriteTextWithBackground shiroStatusText = null!;

        [Resolved]
        private LadderInfo ladder { get; set; } = null!;

        public ChessMapPool()
        {
            RelativeSizeAxes = Axes.Both;
            Masking = true;
            CornerRadius = 10;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                new Box
                {
                    Name = @"Background box",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.5f,
                },
                new GridContainer
                {
                    Name = @"Content grid",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Horizontal = 10, Vertical = 5 },
                    RowDimensions =
                    [
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(),
                        new Dimension(GridSizeMode.AutoSize),
                    ],
                    ColumnDimensions =
                    [
                        new Dimension(),
                    ],
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new TournamentSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = ScreenStrings.MapPool,
                                Font = OsuFont.Torus.With(size: 18, weight: FontWeight.SemiBold),
                                Margin = new MarginPadding { Vertical = 5 },
                            },
                        },
                        new Drawable[]
                        {
                            new OsuScrollContainer
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                ScrollbarVisible = false,
                                Padding = new MarginPadding { Vertical = 5 },
                                Child = sectionFlow = new FillFlowContainer<ModMapSection>
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(15),
                                    ChildrenEnumerable = TournamentGame.MODS.Select(kv => new ModMapSection(kv.Key, kv.Value)
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Width = 1,
                                    }),
                                },
                            },
                        },
                        new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(5),
                                Margin = new MarginPadding { Vertical = 5 },
                                Children = new Drawable[]
                                {
                                    new SpriteIcon
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Size = new Vector2(24),
                                        Icon = FontAwesome.Regular.Circle,
                                        Margin = new MarginPadding { Horizontal = 5 },
                                    },
                                    new TournamentSpriteText
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Text = BoardStrings.ShiroStatus,
                                        Font = OsuFont.Torus.With(size: 18, weight: FontWeight.SemiBold),
                                    },
                                    shiroStatusText = new TournamentSpriteTextWithBackground
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Text = @"???",
                                        Font = OsuFont.Torus.With(size: 18, weight: FontWeight.SemiBold),
                                        Masking = true,
                                        CornerRadius = 5,
                                        InnerPadding = new MarginPadding { Horizontal = 10, Vertical = 5 },
                                    },
                                },
                            },
                        },
                    },
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
            if (e.OldValue != null)
            {
                e.OldValue.ChessPlacements.CollectionChanged -= placementChanged;
            }

            if (e.NewValue != null)
            {
                e.NewValue.ChessPlacements.CollectionChanged += placementChanged;
            }

            updateShiroStatus();
        }

        private void placementChanged(object? _, NotifyCollectionChangedEventArgs __)
            => updateShiroStatus();

        private void updateShiroStatus()
        {
            if (ladder.CurrentMatch.Value == null)
            {
                shiroStatusText.Text = @"???";
                shiroStatusText.TransformTo(nameof(shiroStatusText.BackgroundColour), Color4.White, 500, Easing.OutQuint);
                shiroStatusText.TransformTo(nameof(shiroStatusText.TextColour), Color4.Black, 500, Easing.OutQuint);

                return;
            }

            var shiroRecord = ladder.CurrentMatch.Value.ChessPlacements.LastOrDefault(c => c.BeatmapID == TournamentGame.RESERVED_BEATMAP_ID);

            if (shiroRecord == null)
            {
                shiroStatusText.Text = BoardStrings.ShiroAvailable;
                shiroStatusText.TransformTo(nameof(shiroStatusText.BackgroundColour), Color4.White, 500, Easing.OutQuint);
                shiroStatusText.TransformTo(nameof(shiroStatusText.TextColour), Color4.Black, 500, Easing.OutQuint);

                return;
            }

            Color4 backgroundColour = Color4.White;
            Color4 textColour = Color4.Black;

            switch (shiroRecord.CurrentType)
            {
                case ChoiceType.Pick:
                    shiroStatusText.Text = BoardStrings.ShiroNeedsActivation;
                    backgroundColour = FumoColours.DeepPurple.Regular;
                    textColour = Color4.White;
                    break;

                case ChoiceType.RedWin:
                    shiroStatusText.Text = BoardStrings.ShiroHeldBy(BaseStrings.TeamRed);
                    backgroundColour = TournamentGame.COLOUR_RED;
                    textColour = Color4.White;
                    break;

                case ChoiceType.BlueWin:
                    shiroStatusText.Text = BoardStrings.ShiroHeldBy(BaseStrings.TeamBlue);
                    backgroundColour = TournamentGame.COLOUR_BLUE;
                    textColour = Color4.White;
                    break;

                case ChoiceType.Consumed:
                    shiroStatusText.Text = BoardStrings.ShiroConsumed;
                    backgroundColour = Color4.Gray;
                    textColour = Color4.White;
                    break;
            }

            shiroStatusText.TransformTo(nameof(shiroStatusText.BackgroundColour), backgroundColour, 500, Easing.OutQuint);
            shiroStatusText.TransformTo(nameof(shiroStatusText.TextColour), textColour, 500, Easing.OutQuint);
        }
    }
}

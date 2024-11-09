// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ShowcaseConfigScreen : OsuScreen
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        private OsuScrollContainer scroll = null!;
        private FillFlowContainer innerFlow = null!;

        public ShowcaseConfigScreen()
        {
            Alpha = 0;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                scroll = new OsuScrollContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.75f,
                    Height = 0.8f,
                    Child = innerFlow = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Spacing = new Vector2(10),
                        Direction = FillDirection.Full,
                        Children = new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Spacing = new Vector2(10),
                                Direction = FillDirection.Vertical,
                                Children = new Drawable[]
                                {
                                    new SectionHeader(@"Tournament Information"),
                                    new FormTextBox
                                    {
                                        Caption = "Name",
                                        PlaceholderText = "Tournament series name (e.g. osu! World Cup)",
                                        HintText = "This would be shown at the intro screen.",
                                    },
                                    new FormTextBox
                                    {
                                        Caption = "Round",
                                        PlaceholderText = "Tournament round (e.g. Semifinals)",
                                    },
                                    new FormTextBox
                                    {
                                        Caption = "Date and Time",
                                        PlaceholderText = "2024/11/4 5:14:19:191 UTC+8",
                                        HintText = "This would stay the same in the showcase. So use your own preferred format!",
                                    },
                                    new FormTextBox
                                    {
                                        Caption = "Comment",
                                        PlaceholderText = "Welcome to osu!",
                                        HintText = "In fact you can write anything here.\nThis is also part of the intro screen.",
                                    },
                                }
                            },
                            new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Spacing = new Vector2(10),
                                Direction = FillDirection.Vertical,
                                Children = new Drawable[]
                                {
                                    new SectionHeader(@"Showcase Settings"),
                                    new FormTextBox(),
                                    new FormTextBox(),
                                }
                            },
                            new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Spacing = new Vector2(10),
                                Direction = FillDirection.Vertical,
                                Children = new Drawable[]
                                {
                                    new SectionHeader(@"Team List"),
                                    new FormTextBox(),
                                    new FormTextBox(),
                                }
                            },
                            new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Spacing = new Vector2(10),
                                Direction = FillDirection.Vertical,
                                Children = new Drawable[]
                                {
                                    new SectionHeader(@"Beatmap Queue"),
                                    new FormTextBox(),
                                    new FormTextBox(),
                                }
                            }
                        }
                    },
                },
                new RoundedButton
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.X,
                    Width = 0.25f,
                    Y = -30,
                    Text = "Show",
                }
            };

            foreach (var f in innerFlow.Children)
            {
                f.Width = 0.49f;
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            this.FadeInFromZero(500, Easing.OutQuint);
        }
    }
}

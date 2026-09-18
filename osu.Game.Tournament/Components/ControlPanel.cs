// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using osu.Game.Tournament.Localisation;
using osuTK;

namespace osu.Game.Tournament.Components
{
    /// <summary>
    /// An element anchored to the right-hand area of a screen that provides streamer level controls.
    /// Should be off-screen.
    /// </summary>
    public partial class ControlPanel : Container
    {
        private readonly Action<bool>? refetchAction;
        private readonly Action? saveAction;
        private readonly bool needSaving;

        private readonly FillFlowContainer buttons;
        private SettingsNote statusNote = null!;

        /// <summary>
        /// The optional status information to show as <see cref="SettingsNote.Data"/> at the bottom of the panel.
        /// </summary>
        public readonly Bindable<SettingsNote.Data?> Status = new Bindable<SettingsNote.Data?>();

        protected override Container<Drawable> Content => buttons;

        /// <param name="needSaving">Whether the panel offers the save button for the bracket.</param>
        /// <param name="refetchAction">The action of the refetch button, if the panel needs one.</param>
        /// <param name="saveAction">
        /// Run alongside the bracket save, for state which is not part of the bracket.
        /// </param>
        public ControlPanel(bool needSaving = false, Action<bool>? refetchAction = null, Action? saveAction = null)
        {
            Name = @"Control Panel Sidebar";
            RelativeSizeAxes = Axes.Y;
            AlwaysPresent = true;
            Width = TournamentSceneManager.CONTROL_AREA_WIDTH;
            Anchor = Anchor.TopRight;

            this.needSaving = needSaving;
            this.refetchAction = refetchAction;
            this.saveAction = saveAction;

            buttons = new FillFlowContainer
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding(5),
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 5f),
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background6,
                },
                new GridContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.Both,
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(),
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(GridSizeMode.AutoSize),
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Horizontal,
                                Padding = new MarginPadding { Vertical = 10 },
                                Spacing = new Vector2(5),
                                Children = new Drawable[]
                                {
                                    new SpriteIcon
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Icon = OsuIcon.Settings,
                                        Size = new Vector2(20),
                                    },
                                    new TournamentSpriteText
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Text = BaseStrings.ControlPanel,
                                        Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 24),
                                    },
                                }
                            },
                        },
                        new Drawable[]
                        {
                            new OsuScrollContainer
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.Both,
                                ScrollbarVisible = false,
                                Child = buttons,
                            },
                        },
                        new Drawable[]
                        {
                            // SettingsNote doesn't support customized padding.
                            new Container
                            {
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.BottomCentre,
                                RelativeSizeAxes = Axes.X,
                                Padding = new MarginPadding(5),
                                Child = statusNote = new SettingsNote
                                {
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.BottomCentre,
                                    RelativeSizeAxes = Axes.X,
                                },
                            }
                        },
                        new[]
                        {
                            refetchAction != null
                                ? new FetchDataButton(refetchAction)
                                {
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.BottomCentre,
                                    Padding = new MarginPadding(5),
                                }
                                : Empty(),
                        },
                        new[]
                        {
                            needSaving
                                ? new SaveChangesButton(saveAction)
                                {
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.BottomCentre,
                                    Padding = new MarginPadding(5),
                                }
                                : Empty(),
                        },
                    }
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            statusNote.Current.BindTo(Status);
        }

        public partial class Spacer : CompositeDrawable
        {
            public Spacer(float height = 20)
            {
                RelativeSizeAxes = Axes.X;
                Height = height;
                AlwaysPresent = true;
            }
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays.Settings;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Localisation;
using osu.Game.Tournament.Localisation.Screens;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Screens.Editors
{
    public partial class PunishmentEditorScreen : TournamentEditorScreen<PunishmentEditorScreen.PunishmentRow, PunishmentEntry>
    {
        protected override BindableList<PunishmentEntry> Storage => LadderInfo.Punishments;

        public partial class PunishmentRow : CompositeDrawable, IModelBacked<PunishmentEntry>
        {
            public PunishmentEntry Model { get; }

            private readonly Bindable<string> playerId = new Bindable<string>();

            public PunishmentRow(PunishmentEntry entry, LadderInfo ladder)
            {
                Model = entry;

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

                Masking = true;
                CornerRadius = 10;

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        Colour = OsuColour.Gray(0.2f),
                        RelativeSizeAxes = Axes.Both,
                    },
                    new FillFlowContainer
                    {
                        Margin = new MarginPadding(5),
                        Padding = new MarginPadding { Right = 60 },
                        Spacing = new Vector2(5),
                        Direction = FillDirection.Full,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            new FormNumberBox
                            {
                                Caption = BaseStrings.UserID,
                                RelativeSizeAxes = Axes.None,
                                Width = 0.32f,
                                Current = playerId,
                            },
                            new FormTextBox
                            {
                                Caption = PunishmentEditorStrings.Reason,
                                RelativeSizeAxes = Axes.None,
                                Width = 0.32f,
                                Current = Model.Reason,
                            },
                            new FormSliderBar<int>
                            {
                                Caption = PunishmentEditorStrings.Penalty,
                                RelativeSizeAxes = Axes.None,
                                Width = 0.32f,
                                Current = Model.Penalty,
                            },
                            new FormEnumDropdown<PunishmentType>
                            {
                                Caption = PunishmentEditorStrings.PunishmentType,
                                RelativeSizeAxes = Axes.None,
                                Width = 0.32f,
                                Current = Model.Type,
                            },
                            new DateTextBox
                            {
                                Caption = PunishmentEditorStrings.RecordTime,
                                RelativeSizeAxes = Axes.None,
                                Width = 0.32f,
                                Current = Model.RecordTime,
                            },
                            new DateTextBox
                            {
                                Caption = PunishmentEditorStrings.ExpireTime,
                                RelativeSizeAxes = Axes.None,
                                Width = 0.32f,
                                Current = Model.ExpireTime,
                            },
                        },
                    },
                    new DangerousSettingsButton
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        RelativeSizeAxes = Axes.None,
                        Width = 150,
                        Text = BaseStrings.Remove,
                        Action = () =>
                        {
                            Expire();
                            ladder.Punishments.Remove(entry);
                        },
                    },
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                playerId.BindValueChanged(e =>
                {
                    if (int.TryParse(e.NewValue, out int val))
                    {
                        Model.UserID.Value = val;
                    }
                    else
                    {
                        playerId.Value = Model.UserID.ToString();
                    }
                }, true);
            }
        }

        protected override PunishmentRow CreateDrawable(PunishmentEntry model) => new PunishmentRow(model, LadderInfo);
    }
}

﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using osu.Game.Tournament.Localisation;
using osu.Game.Tournament.Localisation.Screens;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Editors.Components;
using osu.Game.Tournament.Screens.Drawings.Components;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tournament.Screens.Editors
{
    public partial class TeamEditorScreen : TournamentEditorScreen<TeamEditorScreen.TeamRow, TournamentTeam>
    {
        protected override BindableList<TournamentTeam> Storage => LadderInfo.Teams;

        [Resolved]
        private IDialogOverlay? dialogOverlay { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            ControlPanel.Add(new DangerousSettingsButton
            {
                RelativeSizeAxes = Axes.X,
                Text = TeamEditorStrings.AddAllCountries,
                Action = () => dialogOverlay?.Push(new AddAllDialog(() =>
                {
                    Expire();
                    addAllCountries();
                }))
            });
        }

        protected override TeamRow CreateDrawable(TournamentTeam model) => new TeamRow(model, this);

        private void addAllCountries()
        {
            var countries = new List<TournamentTeam>();

            foreach (var country in Enum.GetValues<CountryCode>().Skip(1))
            {
                countries.Add(new TournamentTeam
                {
                    FlagName = { Value = country.ToString() },
                    FullName = { Value = country.GetDescription() },
                    Acronym = { Value = country.GetAcronym() },
                });
            }

            foreach (var c in countries)
                Storage.Add(c);
        }

        public partial class TeamRow : CompositeDrawable, IModelBacked<TournamentTeam>
        {
            public TournamentTeam Model { get; }

            [Resolved]
            private TournamentSceneManager? sceneManager { get; set; }

            [Resolved]
            private IDialogOverlay? dialogOverlay { get; set; }

            [Resolved]
            private LadderInfo ladderInfo { get; set; } = null!;

            public TeamRow(TournamentTeam team, TournamentScreen parent)
            {
                Model = team;

                Model.FullName.Default = Model.FullName.Value;
                Model.Acronym.Default = Model.Acronym.Value;
                Model.FlagName.Default = Model.FlagName.Value;
                Model.LastYearPlacing.Default = Model.LastYearPlacing.Value;
                Model.Seed.Default = Model.Seed.Value;

                Masking = true;
                CornerRadius = 10;

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

                PlayerEditor playerEditor = new PlayerEditor(Model);

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        Colour = OsuColour.Gray(0.1f),
                        RelativeSizeAxes = Axes.Both,
                    },
                    new GroupTeam(team)
                    {
                        Margin = new MarginPadding(16),
                        Scale = new Vector2(2),
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                    },
                    new FillFlowContainer
                    {
                        Spacing = new Vector2(5),
                        Padding = new MarginPadding(10),
                        Direction = FillDirection.Full,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            new SectionHeader(TeamEditorStrings.TeamInfoHeader),
                            new FormTextBox
                            {
                                Caption = TeamEditorStrings.TeamName,
                                Width = 0.2f,
                                Current = Model.FullName,
                            },
                            new FormTextBox
                            {
                                Caption = TeamEditorStrings.TeamAcronym,
                                Width = 0.2f,
                                Current = Model.Acronym,
                            },
                            new FormTextBox
                            {
                                Caption = TeamEditorStrings.TeamFlag,
                                Width = 0.2f,
                                Current = Model.FlagName,
                            },
                            new FormTextBox
                            {
                                Caption = TeamEditorStrings.TeamSeed,
                                Width = 0.2f,
                                Current = Model.Seed,
                            },
                            new DangerousSettingsButton
                            {
                                Width = 0.2f,
                                Text = TeamEditorStrings.DeleteTeam,
                                Action = () => dialogOverlay?.Push(new DeleteTeamDialog(Model, () =>
                                {
                                    Expire();
                                    ladderInfo.Teams.Remove(Model);
                                })),
                            },
                            new FormSliderBar<int>
                            {
                                Caption = TeamEditorStrings.LastYearPlacement,
                                Width = 0.33f,
                                Current = Model.LastYearPlacing,
                                TransferValueOnCommit = true,
                                TabbableContentContainer = this,
                            },
                            new SettingsButton
                            {
                                Width = 0.2f,
                                Margin = new MarginPadding { Left = 10 },
                                Text = TeamEditorStrings.EditSeedingResults,
                                Action = () =>
                                {
                                    sceneManager?.SetScreen(new SeedingEditorScreen(team, parent));
                                },
                            },
                            playerEditor,
                            new SettingsButton
                            {
                                Text = TeamEditorStrings.AddPlayer,
                                Margin = new MarginPadding { Top = 10, Bottom = 10 },
                                Action = () => playerEditor.CreateNew(),
                            },
                        }
                    },
                };
            }

            public partial class PlayerEditor : CompositeDrawable
            {
                private readonly TournamentTeam team;
                private readonly FillFlowContainer flow;

                public PlayerEditor(TournamentTeam team)
                {
                    this.team = team;

                    RelativeSizeAxes = Axes.X;
                    AutoSizeAxes = Axes.Y;

                    InternalChild = flow = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Full,
                        Padding = new MarginPadding(5),
                        Spacing = new Vector2(5),
                        Child = new SectionHeader(TeamEditorStrings.PlayerListHeader),
                    };

                    flow.AddRange(team.Players.Select(p => new PlayerRow(team, p)));
                }

                public void CreateNew()
                {
                    var player = new TournamentUser();
                    team.Players.Add(player);
                    flow.Add(new PlayerRow(team, player));
                }

                public partial class PlayerRow : CompositeDrawable
                {
                    private readonly TournamentUser user;

                    [Resolved]
                    private TournamentGameBase game { get; set; } = null!;

                    [Resolved]
                    private IDialogOverlay? dialogOverlay { get; set; }

                    private readonly Bindable<int?> playerId = new Bindable<int?>();

                    private readonly Container userPanelContainer;

                    public PlayerRow(TournamentTeam team, TournamentUser user)
                    {
                        this.user = user;

                        RelativeSizeAxes = Axes.X;
                        Width = 0.49f;
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
                                Direction = FillDirection.Horizontal,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Children = new Drawable[]
                                {
                                    new SettingsNumberBox
                                    {
                                        LabelText = BaseStrings.UserID,
                                        Width = 0.25f,
                                        Current = playerId,
                                    },
                                    userPanelContainer = new Container
                                    {
                                        Width = 350,
                                        RelativeSizeAxes = Axes.Y,
                                    },
                                }
                            },
                            new DangerousSettingsButton
                            {
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                RelativeSizeAxes = Axes.None,
                                Width = 150,
                                Text = BaseStrings.Remove,
                                Action = () => dialogOverlay?.Push(new DeletePlayerDialog(user, () =>
                                {
                                    Expire();
                                    team.Players.Remove(user);
                                }))
                            }
                        };
                    }

                    [BackgroundDependencyLoader]
                    private void load()
                    {
                        playerId.Default = playerId.Value = user.OnlineID;
                        playerId.BindValueChanged(id =>
                        {
                            user.OnlineID = id.NewValue ?? 0;

                            if (id.NewValue != id.OldValue)
                                user.Username = string.Empty;

                            if (!string.IsNullOrEmpty(user.Username))
                            {
                                updatePanel();
                                return;
                            }

                            game.PopulatePlayer(user, updatePanel, updatePanel);
                        }, true);
                    }

                    private void updatePanel() => Scheduler.AddOnce(() =>
                    {
                        userPanelContainer.Child = new UserListPanel(user.ToAPIUser(), 60, mode: ListDisplayMode.Statistics)
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Scale = new Vector2(1f),
                        };
                    });
                }
            }
        }
    }
}

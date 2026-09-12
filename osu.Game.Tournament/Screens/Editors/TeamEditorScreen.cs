// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
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
        private TournamentGameBase? tournamentGame { get; set; }

        public TeamEditorScreen()
        {
            FetchAction = fetchAll => Task.Run(() => tournamentGame?.AddPlayers(fetchAll))
                                          .ContinueWith(_ => Scheduler.Add(RefreshFlow));
        }

        protected override TeamRow CreateDrawable(TournamentTeam model) => new TeamRow(model, this);

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
                            new FormButton
                            {
                                Width = 0.2f,
                                Caption = TeamEditorStrings.EditSeedingResults,
                                Action = () =>
                                {
                                    sceneManager?.SetScreen(new SeedingEditorScreen(team, parent));
                                },
                            },
                            new FormTextBox
                            {
                                Caption = TeamEditorStrings.LastYearPlacement,
                                Width = 0.33f,
                                Current = Model.LastYearPlacing,
                                TabbableContentContainer = this,
                            },
                            new FormButton
                            {
                                Width = 0.2f,
                                Caption = TeamEditorStrings.DeleteTeam,
                                ButtonIcon = FontAwesome.Solid.Trash,
                                BackgroundColour = new OsuColour().DangerousButtonColour,
                                Action = () => dialogOverlay?.Push(new DeleteTeamDialog(Model, () =>
                                {
                                    Expire();
                                    ladderInfo.Teams.Remove(Model);
                                })),
                            },
                            playerEditor,
                            new SettingsButton
                            {
                                Text = TeamEditorStrings.AddPlayer,
                                Margin = new MarginPadding { Top = 10, Bottom = 10 },
                                Action = playerEditor.CreateNew,
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
                    private readonly Bindable<UserRole> role = new Bindable<UserRole>();

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
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(5),
                                Margin = new MarginPadding(5),
                                // Leave space for the remove button
                                Padding = new MarginPadding { Right = 150 },
                                Children = new Drawable[]
                                {
                                    new GridContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        RowDimensions = [new Dimension(GridSizeMode.AutoSize)],
                                        ColumnDimensions =
                                        [
                                            new Dimension(),
                                            new Dimension(GridSizeMode.Absolute, 350),
                                        ],
                                        Content = new Drawable[][]
                                        {
                                            [
                                                new SettingsNumberBox
                                                {
                                                    LabelText = BaseStrings.UserID,
                                                    Current = playerId,
                                                },
                                                userPanelContainer = new Container
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                },
                                            ],
                                        }
                                    },
                                    new FormEnumDropdown<UserRole>
                                    {
                                        Caption = BaseStrings.UserRole,
                                        Current = role,
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
                        role.Default = role.Value = user.Role;
                        playerId.Default = playerId.Value = user.OnlineID;

                        role.BindValueChanged(r => user.Role = r.NewValue);
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
                        userPanelContainer.Child = new UserListPanel(user.ToAPIUser())
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                        };
                    });
                }
            }
        }
    }
}

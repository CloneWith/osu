// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Localisation;
using osu.Game.Tournament.Localisation.Screens;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Editors.Components;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tournament.Screens.Editors
{
    public partial class RoundEditorScreen : TournamentEditorScreen<RoundEditorScreen.RoundRow, TournamentRound>
    {
        protected override BindableList<TournamentRound> Storage => LadderInfo.Rounds;

        public partial class RoundRow : CompositeDrawable, IModelBacked<TournamentRound>
        {
            public TournamentRound Model { get; }

            [Resolved]
            private LadderInfo ladderInfo { get; set; } = null!;

            [Resolved]
            private IDialogOverlay? dialogOverlay { get; set; }

            public RoundRow(TournamentRound round)
            {
                Model = round;

                Model.Name.Default = Model.Name.Value;
                Model.Description.Default = Model.Description.Value;
                Model.StartDate.Default = Model.StartDate.Value;
                Model.UseBoard.Default = Model.UseBoard.Value;
                Model.BanCount.Default = Model.BanCount.Value;
                Model.BestOf.Default = Model.BestOf.Value;

                Masking = true;
                CornerRadius = 10;

                RoundBeatmapEditor beatmapEditor = new RoundBeatmapEditor(round)
                {
                    Width = 0.98f
                };

                RoundRefereeEditor refereeEditor = new RoundRefereeEditor(round)
                {
                    Width = 0.98f
                };

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        Colour = OsuColour.Gray(0.1f),
                        RelativeSizeAxes = Axes.Both,
                    },
                    new FillFlowContainer
                    {
                        Margin = new MarginPadding(5),
                        Spacing = new Vector2(10),
                        Direction = FillDirection.Full,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            new SectionHeader(RoundEditorStrings.RoundInfoHeader),
                            new FormTextBox
                            {
                                Caption = RoundEditorStrings.RoundName,
                                Width = 0.32f,
                                Current = Model.Name,
                            },
                            new FormTextBox
                            {
                                Caption = RoundEditorStrings.RoundDescription,
                                Width = 0.32f,
                                Current = Model.Description,
                            },
                            new DateTextBox
                            {
                                Caption = RoundEditorStrings.StartTime,
                                Width = 0.32f,
                                Current = Model.StartDate,
                            },
                            new FormSliderBar<int>
                            {
                                Caption = RoundEditorStrings.NumOfBans,
                                Width = 0.48f,
                                Current = Model.BanCount,
                            },
                            new FormSliderBar<int>
                            {
                                Caption = RoundEditorStrings.BestOf,
                                Width = 0.48f,
                                Current = Model.BestOf,
                            },
                            new FormCheckBox
                            {
                                Caption = RoundEditorStrings.BoardMode,
                                Width = 0.48f,
                                Current = Model.UseBoard,
                            },
                            new DangerousSettingsButton
                            {
                                Width = 0.2f,
                                Text = RoundEditorStrings.DeleteRound,
                                Action = () => dialogOverlay?.Push(new DeleteRoundDialog(Model, () =>
                                {
                                    Expire();
                                    ladderInfo.Rounds.Remove(Model);
                                })),
                            },
                            refereeEditor,
                            new SettingsButton
                            {
                                Text = RoundEditorStrings.AddReferee,
                                Margin = new MarginPadding { Top = 10, Bottom = 10 },
                                Action = () => refereeEditor.CreateNew(),
                            },
                            beatmapEditor,
                            new SettingsButton
                            {
                                Text = BaseStrings.AddBeatmap,
                                Margin = new MarginPadding { Top = 10, Bottom = 10 },
                                Action = () => beatmapEditor.CreateNew(),
                            },
                        },
                    },
                };

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
            }

            public partial class RoundRefereeEditor : CompositeDrawable
            {
                private readonly TournamentRound round;
                private readonly FillFlowContainer flow;

                public RoundRefereeEditor(TournamentRound round)
                {
                    this.round = round;

                    RelativeSizeAxes = Axes.X;
                    AutoSizeAxes = Axes.Y;

                    InternalChild = flow = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Padding = new MarginPadding(5),
                        Spacing = new Vector2(5),
                        Child = new SectionHeader(RoundEditorStrings.RefereeList),
                    };

                    flow.AddRange(round.Referees.Select(p => new RefereeRow(round, p)));
                }

                public void CreateNew()
                {
                    var player = new TournamentUser();
                    round.Referees.Add(player);
                    flow.Add(new RefereeRow(round, player));
                }

                public partial class RefereeRow : CompositeDrawable
                {
                    private readonly TournamentUser user;

                    [Resolved]
                    private TournamentGameBase game { get; set; } = null!;

                    [Resolved]
                    private IDialogOverlay? dialogOverlay { get; set; }

                    private readonly Bindable<int?> playerId = new Bindable<int?>();

                    private readonly Container userPanelContainer;

                    public RefereeRow(TournamentRound round, TournamentUser user)
                    {
                        this.user = user;

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
                                Direction = FillDirection.Horizontal,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Children = new Drawable[]
                                {
                                    new SettingsNumberBox
                                    {
                                        LabelText = BaseStrings.UserID,
                                        RelativeSizeAxes = Axes.None,
                                        Width = 200,
                                        Current = playerId,
                                    },
                                    userPanelContainer = new Container
                                    {
                                        Width = 400,
                                        RelativeSizeAxes = Axes.Y,
                                    },
                                },
                            },
                            new DangerousSettingsButton
                            {
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                RelativeSizeAxes = Axes.None,
                                Width = 150,
                                Text = RoundEditorStrings.DeleteReferee,
                                Action = () => dialogOverlay?.Push(new DeleteRefereeDialog(user, () =>
                                {
                                    Expire();
                                    round.Referees.Remove(user);
                                })),
                            },
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
                        userPanelContainer.Child = new UserListPanel(user.ToAPIUser(), mode: ListDisplayMode.Statistics)
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Scale = new Vector2(1f),
                        };
                    });
                }
            }

            public partial class RoundBeatmapEditor : CompositeDrawable
            {
                private readonly TournamentRound round;
                private readonly FillFlowContainer flow;

                public RoundBeatmapEditor(TournamentRound round)
                {
                    this.round = round;

                    RelativeSizeAxes = Axes.X;
                    AutoSizeAxes = Axes.Y;

                    InternalChild = flow = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Child = new SectionHeader(RoundEditorStrings.RoundBeatmapsHeader),
                    };

                    flow.AddRange(round.Beatmaps.Select(p => new RoundBeatmapRow(round, p)));
                }

                public void CreateNew()
                {
                    var b = new RoundBeatmap();

                    round.Beatmaps.Add(b);

                    flow.Add(new RoundBeatmapRow(round, b));
                }

                public partial class RoundBeatmapRow : CompositeDrawable
                {
                    public RoundBeatmap Model { get; }

                    [Resolved]
                    protected IAPIProvider API { get; private set; } = null!;

                    [Resolved]
                    private IDialogOverlay? dialogOverlay { get; set; }

                    private readonly Bindable<int?> beatmapId = new Bindable<int?>();

                    private readonly Bindable<string> modIndex = new Bindable<string>(string.Empty);

                    private readonly Bindable<string> mods = new Bindable<string>(string.Empty);

                    private readonly Bindable<int?> boardX = new Bindable<int?>();
                    private readonly Bindable<int?> boardY = new Bindable<int?>();

                    private readonly Container drawableContainer;

                    public RoundBeatmapRow(TournamentRound team, RoundBeatmap beatmap)
                    {
                        Model = beatmap;

                        Margin = new MarginPadding(10);

                        RelativeSizeAxes = Axes.X;
                        AutoSizeAxes = Axes.Y;

                        Masking = true;
                        CornerRadius = 5;

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
                                Padding = new MarginPadding { Right = 10 },
                                Spacing = new Vector2(5),
                                Direction = FillDirection.Horizontal,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Children = new Drawable[]
                                {
                                    new SettingsNumberBox
                                    {
                                        LabelText = BaseStrings.BeatmapID,
                                        RelativeSizeAxes = Axes.None,
                                        Width = 125,
                                        Current = beatmapId,
                                    },
                                    new SettingsTextBox
                                    {
                                        LabelText = BaseStrings.BeatmapMod,
                                        Width = 0.1f,
                                        Current = mods,
                                    },
                                    new SettingsTextBox
                                    {
                                        LabelText = RoundEditorStrings.ModIndex,
                                        Width = 0.1f,
                                        Current = modIndex,
                                    },
                                    new SettingsNumberBox
                                    {
                                        LabelText = "Row",
                                        RelativeSizeAxes = Axes.None,
                                        Width = 100,
                                        Current = boardY,
                                    },
                                    new SettingsNumberBox
                                    {
                                        LabelText = "Column",
                                        RelativeSizeAxes = Axes.None,
                                        Width = 100,
                                        Current = boardX,
                                    },
                                    drawableContainer = new Container
                                    {
                                        Size = new Vector2(100, 70),
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
                                Action = () => dialogOverlay?.Push(new DeleteBeatmapDialog(Model, () =>
                                {
                                    Expire();
                                    team.Beatmaps.Remove(beatmap);
                                })),
                            }
                        };
                    }

                    [BackgroundDependencyLoader]
                    private void load()
                    {
                        beatmapId.Default = beatmapId.Value = Model.ID;

                        beatmapId.BindValueChanged(id =>
                        {
                            Model.ID = id.NewValue ?? 0;

                            if (id.NewValue != id.OldValue)
                                Model.Beatmap = null;

                            if (Model.Beatmap != null)
                            {
                                updatePanel();
                                return;
                            }

                            var req = new GetBeatmapRequest(new APIBeatmap { OnlineID = Model.ID });

                            req.Success += res =>
                            {
                                Model.Beatmap = new TournamentBeatmap(res);
                                updatePanel();
                            };

                            req.Failure += _ =>
                            {
                                Model.Beatmap = null;
                                updatePanel();
                            };

                            API.Queue(req);
                        }, true);

                        mods.Default = mods.Value = Model.Mods;
                        mods.BindValueChanged(modString => Model.Mods = modString.NewValue);

                        modIndex.Default = modIndex.Value = Model.ModIndex;
                        modIndex.BindValueChanged(newIndex => Model.ModIndex = newIndex.NewValue);

                        boardX.Default = boardX.Value = Model.BoardX;
                        boardX.BindValueChanged(newX => { Model.BoardX = newX.NewValue ?? 0; });

                        boardY.Default = boardY.Value = Model.BoardY;
                        boardY.BindValueChanged(newY => { Model.BoardY = newY.NewValue ?? 0; });
                    }

                    private void updatePanel() => Schedule(() =>
                    {
                        drawableContainer.Clear();

                        if (Model.Beatmap != null)
                        {
                            drawableContainer.Child = new TournamentBeatmapPanel(Model.Beatmap, Model.Mods, Model.ModIndex)
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Width = 500
                            };
                        }
                    });
                }
            }
        }

        protected override RoundRow CreateDrawable(TournamentRound model) => new RoundRow(model);
    }
}

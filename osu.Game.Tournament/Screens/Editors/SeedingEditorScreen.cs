// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Settings;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Localisation;
using osu.Game.Tournament.Localisation.Screens;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Screens.Editors
{
    public partial class SeedingEditorScreen : TournamentEditorScreen<SeedingEditorScreen.SeedingResultRow, SeedingResult>
    {
        private readonly TournamentTeam team;

        protected override BindableList<SeedingResult> Storage => team.SeedingResults;

        private const float shared_relative_width = 0.15f;

        public SeedingEditorScreen(TournamentTeam team, TournamentScreen parentScreen)
            : base(parentScreen)
        {
            this.team = team;
        }

        public partial class SeedingResultRow : CompositeDrawable, IModelBacked<SeedingResult>
        {
            public SeedingResult Model { get; }

            public SeedingResultRow(TournamentTeam team, SeedingResult round)
            {
                Model = round;

                Masking = true;
                CornerRadius = 10;

                SeedingBeatmapEditor beatmapEditor = new SeedingBeatmapEditor(round)
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
                            new SectionHeader(TeamEditorStrings.SeedingEntryHeader),
                            new FormTextBox
                            {
                                Caption = BaseStrings.BeatmapMod,
                                Width = 0.33f,
                                Current = Model.Mod,
                            },
                            new FormSliderBar<int>
                            {
                                Caption = BaseStrings.Seed,
                                Width = 0.33f,
                                Current = Model.Seed,
                            },
                            new SettingsButton
                            {
                                Width = 0.2f,
                                Margin = new MarginPadding(10),
                                Text = BaseStrings.AddBeatmap,
                                Action = () => beatmapEditor.CreateNew(),
                            },
                            beatmapEditor,
                        }
                    },
                    new IconButton
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        RelativeSizeAxes = Axes.None,
                        Icon = FontAwesome.Solid.TimesCircle,
                        IconScale = new Vector2(1.75f),
                        Size = new Vector2(60),
                        TooltipText = BaseStrings.Remove,
                        Action = () =>
                        {
                            Expire();
                            team.SeedingResults.Remove(Model);
                        },
                    },
                };

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
            }

            public partial class SeedingBeatmapEditor : CompositeDrawable
            {
                private readonly SeedingResult round;
                private readonly FillFlowContainer flow;

                public SeedingBeatmapEditor(SeedingResult round)
                {
                    this.round = round;

                    RelativeSizeAxes = Axes.X;
                    AutoSizeAxes = Axes.Y;

                    InternalChild = flow = new FillFlowContainer
                    {
                        Margin = new MarginPadding(5),
                        Spacing = new Vector2(10),
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Child = new SectionHeader(TeamEditorStrings.SeedingBeatmapsHeader),
                    };

                    flow.AddRange(round.Beatmaps.Select(p => new SeedingBeatmapRow(round, p)));
                }

                public void CreateNew()
                {
                    var user = new SeedingBeatmap();
                    round.Beatmaps.Add(user);
                    flow.Add(new SeedingBeatmapRow(round, user));
                }

                public partial class SeedingBeatmapRow : CompositeDrawable
                {
                    private readonly SeedingResult result;
                    public SeedingBeatmap Model { get; }

                    [Resolved]
                    protected IAPIProvider API { get; private set; } = null!;

                    private readonly Bindable<int?> beatmapId = new Bindable<int?>();

                    private readonly Bindable<string> score = new Bindable<string>(string.Empty);

                    private readonly Container mapCardContainer;

                    public SeedingBeatmapRow(SeedingResult result, SeedingBeatmap beatmap)
                    {
                        this.result = result;
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
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Margin = new MarginPadding(5),
                                // Padding = new MarginPadding { Right = 160 },
                                Spacing = new Vector2(5),
                                Direction = FillDirection.Horizontal,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Children = new Drawable[]
                                {
                                    new SettingsNumberBox
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        LabelText = BaseStrings.BeatmapID,
                                        Width = shared_relative_width,
                                        Current = beatmapId,
                                    },
                                    new SettingsSlider<int>
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        LabelText = BaseStrings.Seed,
                                        Width = shared_relative_width,
                                        Current = beatmap.Seed,
                                    },
                                    new SettingsTextBox
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        LabelText = BaseStrings.Score,
                                        Width = shared_relative_width,
                                        Current = score,
                                    },
                                    mapCardContainer = new Container
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Size = new Vector2(1.5f, 1f),
                                    },
                                }
                            },
                            new IconButton
                            {
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                RelativeSizeAxes = Axes.None,
                                Icon = FontAwesome.Solid.TimesCircle,
                                IconScale = new Vector2(1.25f),
                                Size = new Vector2(45),
                                Margin = new MarginPadding { Right = 10 },
                                Action = () =>
                                {
                                    Expire();
                                    result.Beatmaps.Remove(beatmap);
                                },
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

                            req.Success += res => Schedule(() =>
                            {
                                Model.Beatmap = new TournamentBeatmap(res);
                                updatePanel();
                            });

                            req.Failure += _ => Schedule(() =>
                            {
                                Model.Beatmap = null;
                                updatePanel();
                            });

                            API.Queue(req);
                        }, true);

                        score.Default = score.Value = Model.Score.ToString();
                        score.BindValueChanged(str => long.TryParse(str.NewValue, out Model.Score));
                    }

                    private void updatePanel()
                    {
                        mapCardContainer.Clear();

                        if (Model.Beatmap != null)
                        {
                            mapCardContainer.Child = new TournamentBeatmapPanel(Model.Beatmap, result.Mod.Value)
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                RelativeSizeAxes = Axes.Both
                            };
                        }
                    }
                }
            }
        }

        protected override SeedingResultRow CreateDrawable(SeedingResult model) => new SeedingResultRow(team, model);
    }
}

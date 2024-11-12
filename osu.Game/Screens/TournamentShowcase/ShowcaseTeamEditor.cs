// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Models;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets;
using osuTK;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ShowcaseTeamEditor : FillFlowContainer
    {
        private readonly Bindable<ShowcaseConfig> config = new Bindable<ShowcaseConfig>();

        private readonly BindableList<ShowcaseTeam> teams = new BindableList<ShowcaseTeam>();

        private readonly OsuAnimatedButton addButton;

        private OsuTextFlowContainer textFlow = null!;

        public ShowcaseTeamEditor(Bindable<ShowcaseConfig> config)
        {
            this.config.BindTo(config);
            teams.BindTo(config.Value.Teams);

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;
            Spacing = new Vector2(10);
            Children = new Drawable[]
            {
                new SectionHeader(@"Team List"),
                addButton = new OsuAnimatedButton
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Action = () =>
                    {
                        var addedTeam = new ShowcaseTeam();
                        config.Value.Teams.Add(addedTeam);
                        Add(new TeamRow(addedTeam, config.Value));
                    },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            addButton.Add(textFlow = new OsuTextFlowContainer(cp => cp.Font = cp.Font.With(size: 20))
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Margin = new MarginPadding(5)
            });

            textFlow.AddIcon(FontAwesome.Solid.PlusCircle, i =>
            {
                i.Padding = new MarginPadding { Right = 10 };
            });

            textFlow.AddText(@"Add Team");

            // Read and attach existing teams to the container
            AddRange(teams.Select(t => new TeamRow(t, config.Value)));
        }
    }

    public partial class TeamRow : FillFlowContainer
    {
        public ShowcaseTeam Team { get; }

        private ShowcaseConfig config { get; set; }

        public TeamRow(ShowcaseTeam team, ShowcaseConfig config)
        {
            Team = team;
            this.config = config;

            Masking = true;
            CornerRadius = 10;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            PlayerEditor playerEditor = new PlayerEditor(Team, config.Ruleset.Value);

            Spacing = new Vector2(5);
            Padding = new MarginPadding(10);
            Direction = FillDirection.Full;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Children = new Drawable[]
            {
                new FormTextBox
                {
                    Caption = @"Name",
                    Width = 1f,
                    Current = Team.FullName
                },
                new FormTextBox
                {
                    Caption = @"Acronym",
                    Width = 0.29f,
                    Current = Team.Acronym
                },
                new FormTextBox
                {
                    Caption = @"Flag",
                    HintText = @"The name of the flag image file, without extension.",
                    Width = 0.29f,
                    Current = Team.FlagName
                },
                new FormTextBox
                {
                    Caption = @"Seed",
                    Width = 0.29f,
                    Current = Team.Seed
                },
                new IconButton
                {
                    RelativeSizeAxes = Axes.X,
                    Width = 0.1f,
                    Icon = FontAwesome.Solid.TimesCircle,
                    TooltipText = @"Delete this team",
                    Action = () =>
                    {
                        Expire();
                        this.config.Teams.Remove(Team);
                    },
                },
                playerEditor,
                new SettingsButton
                {
                    Text = @"Add player",
                    Action = () => playerEditor.CreateNew()
                },
            };
        }

        public partial class PlayerEditor : CompositeDrawable
        {
            private readonly ShowcaseTeam team;
            private readonly FillFlowContainer flow;
            private readonly RulesetInfo? ruleset;

            public PlayerEditor(ShowcaseTeam team, RulesetInfo? ruleset)
            {
                this.team = team;
                this.ruleset = ruleset;

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

                InternalChild = flow = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding(5),
                    Spacing = new Vector2(5),
                    ChildrenEnumerable = team.Members.Select(p => new PlayerRow(team, p, ruleset))
                };
            }

            public void CreateNew()
            {
                var player = new ShowcaseUser();
                team.Members.Add(player);
                flow.Add(new PlayerRow(team, player, ruleset));
            }
        }
    }
}

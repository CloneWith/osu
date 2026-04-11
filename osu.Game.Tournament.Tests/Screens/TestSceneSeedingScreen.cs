// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Ladder.Components;
using osu.Game.Tournament.Screens.TeamIntro;

namespace osu.Game.Tournament.Tests.Screens
{
    public partial class TestSceneSeedingScreen : TournamentScreenTestScene
    {
        [Cached]
        private readonly LadderInfo ladder = new LadderInfo
        {
            Teams =
            {
                new TournamentTeam
                {
                    FullName = { Value = @"Japan" },
                    Acronym = { Value = "JPN" },
                    Seed = { Value = "#28" },
                    LastYearPlacing = { Value = "#17-24" },
                    SeedingResults =
                    {
                        new SeedingResult
                        {
                            // Mod intentionally left blank.
                            Seed = { Value = 1 }
                        },
                        new SeedingResult
                        {
                            Mod = { Value = "DT" },
                            Seed = { Value = 2 }
                        }
                    }
                },
                new TournamentTeam
                {
                    FullName = { Value = @"bbbbb" },
                    Acronym = { Value = "BBB" },
                    Seed = { Value = @"2" },
                    SeedingResults =
                    {
                        new SeedingResult
                        {
                            // Mod intentionally left blank.
                            Seed = { Value = 2 }
                        },
                        new SeedingResult
                        {
                            Mod = { Value = "DT" },
                            Seed = { Value = 3 }
                        }
                    }
                },
                new TournamentTeam
                {
                    FullName = { Value = @"CCC" },
                    Acronym = { Value = "C" },
                    Seed = { Value = @"3" },
                    SeedingResults =
                    {
                        new SeedingResult
                        {
                            // Mod intentionally left blank.
                            Seed = { Value = 3 }
                        },
                        new SeedingResult
                        {
                            Mod = { Value = "DT" },
                            Seed = { Value = 4 }
                        }
                    }
                },
                new TournamentTeam
                {
                    Acronym = { Value = "USA" },
                    FlagName = { Value = "US" },
                    FullName = { Value = "United States" },
                }
            }
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(new SeedingScreen
            {
                FillMode = FillMode.Fit,
                FillAspectRatio = 16 / 9f
            });
        }

        [Test]
        public void TestBasic()
        {
            AddStep("create seeding screen", () => Add(new SeedingScreen
            {
                FillMode = FillMode.Fit,
                FillAspectRatio = 16 / 9f
            }));

            AddStep("set team to Japan", () => this.ChildrenOfType<SettingsTeamDropdown>().Single().Current.Value = ladder.Teams.First());
        }

        [Test]
        public void TestNoSeed()
        {
            AddStep("set team to USA", () =>
                this.ChildrenOfType<SettingsTeamDropdown>().Single().Current.Value = ladder.Teams.Single(t => t.FullName.Value == "United States"));
        }
    }
}

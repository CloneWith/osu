// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Platform;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Tests;
using osu.Game.Users;

namespace osu.Game.Tournament.Tests.NonVisual
{
    public partial class DataLoadTest : TournamentHostTest
    {
        private const string bracket_with_players = @"{
            ""Ruleset"": {
                ""ShortName"": ""osu"",
                ""Name"": ""osu!"",
                ""InstantiationInfo"": ""osu.Game.Rulesets.Osu.OsuRuleset, osu.Game.Rulesets.Osu"",
                ""Available"": true
            },
            ""Teams"": [
                {
                    ""FullName"": ""Team One"",
                    ""Acronym"": ""ONE"",
                    ""Players"": [
                        { ""id"": 2 },
                        { ""id"": 3 }
                    ]
                }
            ]
        }";

        [Test]
        public void TestRulesetGetsValidOnlineID()
        {
            using HeadlessGameHost host = new CleanRunHeadlessGameHost();

            try
            {
                var osu = new TestTournament(runOnLoadComplete: () =>
                {
                    var storage = host.Storage.GetStorageForDirectory(Path.Combine("tournaments", "default"));

                    using var stream = storage.CreateFileSafely("bracket.json");
                    using var writer = new StreamWriter(stream);

                    writer.Write(@"{
                        ""Ruleset"": {
                            ""ShortName"": ""taiko"",
                            ""OnlineID"": -1,
                            ""Name"": ""osu!taiko"",
                            ""InstantiationInfo"": ""osu.Game.Rulesets.OsuTaiko.TaikoRuleset, osu.Game.Rulesets.Taiko"",
                            ""Available"": true
                        } }");
                });

                LoadTournament(host, osu);
                osu.BracketLoadTask.WaitSafely();

                Assert.That(osu.Dependencies.Get<IBindable<RulesetInfo>>().Value.OnlineID, Is.EqualTo(1));
            }
            finally
            {
                host.Exit();
            }
        }

        [Test]
        public void TestUnavailableRuleset()
        {
            using HeadlessGameHost host = new CleanRunHeadlessGameHost();

            try
            {
                var osu = new TestTournament(true);

                LoadTournament(host, osu);
                var storage = osu.Dependencies.Get<Storage>();

                Assert.That(storage.GetFullPath("."), Is.EqualTo(Path.Combine(host.Storage.GetFullPath("."), "tournaments", "default")));
            }
            finally
            {
                host.Exit();
            }
        }

        /// <summary>
        /// Startup population of the bracket happens on a thread pool thread, where the framework's waiting
        /// helpers deliberately refuse to block. Waiting there anyway fails the entire bracket load, so the
        /// population has to be awaited from the load instead of being made to block.
        /// </summary>
        [Test]
        public void TestPlayersPopulatedAtStartup()
        {
            using HeadlessGameHost host = new CleanRunHeadlessGameHost();

            try
            {
                var osu = new TestTournament(runOnLoadComplete: () =>
                {
                    var storage = host.Storage.GetStorageForDirectory(Path.Combine("tournaments", "default"));

                    using var stream = storage.CreateFileSafely("bracket.json");
                    using var writer = new StreamWriter(stream);
                    writer.Write(bracket_with_players);
                });

                osu.API.HandleRequest = request =>
                {
                    if (request is not GetUsersRequest usersRequest)
                        return false;

                    // The request is held open so that the profiles are still missing when the lookup is
                    // waited on. Replying instantly would let a blocking wait pass on an already completed
                    // task, which would hide exactly the failure this test exists to catch.
                    Thread.Sleep(100);

                    usersRequest.TriggerSuccess(new GetUsersResponse
                    {
                        Users = usersRequest.UserIds.Select(id => new APIUser
                        {
                            Id = id,
                            Username = $@"User {id}",
                            CountryCode = CountryCode.JP,
                            RulesetsStatistics = new Dictionary<string, UserStatistics>
                            {
                                [@"osu"] = new UserStatistics
                                {
                                    GlobalRank = 1000 + id,
                                    PP = id * 10,
                                },
                            },
                        }).ToList(),
                    });

                    return true;
                };

                LoadTournament(host, osu);

                // Throws when the bracket failed to load, which is how a blocking wait at startup surfaces.
                osu.BracketLoadTask.WaitSafely();

                // The bracket is written back once the profiles have been fetched, so reading it covers both
                // the population and the I/O which must not happen before the wait has actually finished.
                var bracket = JObject.Parse(File.ReadAllText(host.Storage.GetFullPath(Path.Combine("tournaments", "default", "bracket.json"))));
                var players = bracket[@"Teams"]![0]![@"Players"]!.Children<JObject>().ToList();

                Assert.That(players.Select(p => (int)p[@"id"]!), Is.EqualTo([2, 3]));
                Assert.That(players.Select(p => (string)p[@"Username"]!), Is.EqualTo([@"User 2", @"User 3"]));
                Assert.That(players.Select(p => (int)p[@"Rank"]!), Is.EqualTo([1002, 1003]));
                Assert.That(players.Select(p => (decimal)p[@"PP"]!), Is.EqualTo([20m, 30m]));
                Assert.That(players.Select(p => (string)p[@"country_code"]!), Is.EqualTo([@"JP", @"JP"]));
                Assert.That(players.Select(p => p[@"ProfileFetchedAt"]), Has.All.Not.Null);
            }
            finally
            {
                host.Exit();
            }
        }

        public partial class TestTournament : TournamentGameBase
        {
            private readonly bool resetRuleset;
            private readonly Action? runOnLoadComplete;

            public new Task BracketLoadTask => base.BracketLoadTask;

            public new DummyAPIAccess API => (DummyAPIAccess)base.API;

            public TestTournament(bool resetRuleset = false, [InstantHandle] Action? runOnLoadComplete = null)
            {
                this.resetRuleset = resetRuleset;
                this.runOnLoadComplete = runOnLoadComplete;

                // Requests must never leave the machine during a test.
                base.API = new DummyAPIAccess();
            }

            protected override void LoadComplete()
            {
                runOnLoadComplete?.Invoke();
                base.LoadComplete();
                if (resetRuleset)
                    Ruleset.Value = new RulesetInfo(); // not available
            }
        }
    }
}

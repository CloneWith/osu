// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input;
using osu.Framework.IO.Stores;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Localisation;
using osu.Game.Online;
using osu.Game.Online.API.Requests;
using osu.Game.Tournament.IO;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Localisation;
using osu.Game.Tournament.Models;
using osu.Game.Users;
using osuTK.Input;

namespace osu.Game.Tournament
{
    [Cached(typeof(TournamentGameBase))]
    public partial class TournamentGameBase : OsuGameBase
    {
        public const string BRACKET_FILENAME = @"bracket.json";
        public const string BACKGROUND_MAPPING_FILENAME = @"backgrounds.json";

        private LadderInfo ladder = new LadderInfo();
        private TournamentStorage storage = null!;
        private DependencyContainer dependencies = null!;
        private FileBasedIPC ipc = null!;
        private BeatmapLookupCache beatmapCache = null!;

        private Bindable<string> frameworkLocale = null!;
        private IBindable<LocalisationParameters> localisationParameters = null!;

        protected Task BracketLoadTask => bracketLoadTaskCompletionSource.Task;

        private readonly TaskCompletionSource<bool> bracketLoadTaskCompletionSource = new TaskCompletionSource<bool>();

        private void updateLanguage() => CurrentLanguage.Value = LanguageExtensions.GetLanguageFor(frameworkLocale.Value, localisationParameters.Value);

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            return dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
        }

        public override EndpointConfiguration CreateEndpoints()
        {
            if (UseDevelopmentServer)
                return base.CreateEndpoints();

            return new ProductionEndpointConfiguration();
        }

        private TournamentSpriteText initialisationText = null!;

        [BackgroundDependencyLoader]
        private void load(Storage baseStorage, FrameworkConfigManager frameworkConfig)
        {
            Add(initialisationText = new TournamentSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Font = OsuFont.Torus.With(size: 32),
            });

            Resources.AddStore(new DllResourceStore(typeof(TournamentGameBase).Assembly));

            dependencies.CacheAs<Storage>(storage = new TournamentStorage(baseStorage));
            dependencies.CacheAs(storage);

            dependencies.Cache(new TournamentVideoResourceStore(storage));

            Textures.AddTextureSource(new TextureLoaderStore(new StorageBackedResourceStore(storage)));

            dependencies.CacheAs(new StableInfo(storage));

            beatmapCache = dependencies.Get<BeatmapLookupCache>();

            frameworkLocale = frameworkConfig.GetBindable<string>(FrameworkSetting.Locale);
            frameworkLocale.BindValueChanged(_ => updateLanguage());

            localisationParameters = Localisation.CurrentParameters.GetBoundCopy();
            localisationParameters.BindValueChanged(_ => updateLanguage(), true);

            CurrentLanguage.BindValueChanged(val => frameworkLocale.Value = val.NewValue.ToCultureCode());
        }

        protected override void LoadComplete()
        {
            GlobalCursorDisplay.MenuCursor.AlwaysPresent = true; // required for tooltip display

            // we don't want to show the menu cursor as it would appear on stream output.
            GlobalCursorDisplay.MenuCursor.Alpha = 0;

            base.LoadComplete();

            #region Localisation Initialization

            // These code is directly taken from OsuGame.
            var languages = Enum.GetValues<Language>();

            var mappings = languages.Select(language =>
            {
#if DEBUG
                if (language == Language.debug)
                    return new LocaleMapping("debug", new DebugLocalisationStore());
#endif

                string cultureCode = language.ToCultureCode();

                try
                {
                    return new LocaleMapping(new ResourceManagerLocalisationStore(cultureCode));
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"Could not load localisations for language \"{cultureCode}\"");
                    return null;
                }
            }).Where(m => m != null);

            Localisation.AddLocaleMappings(mappings!);

            #endregion

            Task.Run(readBracket);
        }

        private async Task readBracket()
        {
            // Try to read and parse main bracket file
            try
            {
                if (storage.Exists(BRACKET_FILENAME))
                {
                    using (Stream stream = storage.GetStream(BRACKET_FILENAME, FileAccess.Read, FileMode.Open))
                    using (var sr = new StreamReader(stream))
                    {
                        ladder = JsonConvert.DeserializeObject<LadderInfo>(await sr.ReadToEndAsync().ConfigureAwait(false), new JsonPointConverter()) ?? ladder;
                    }
                }

                var resolvedRuleset = ladder.Ruleset.Value != null
                    ? RulesetStore.GetRuleset(ladder.Ruleset.Value.ShortName)
                    : RulesetStore.AvailableRulesets.First();

                // Must set to null initially to avoid the following re-fetch hitting `ShortName` based equality check.
                ladder.Ruleset.Value = null;
                ladder.Ruleset.Value = resolvedRuleset;

                bool addedInfo = false;

                // assign teams
                foreach (var match in ladder.Matches)
                {
                    match.Team1.Value = ladder.Teams.FirstOrDefault(t => t.Acronym.Value == match.Team1Acronym);
                    match.Team2.Value = ladder.Teams.FirstOrDefault(t => t.Acronym.Value == match.Team2Acronym);

                    foreach (var conditional in match.ConditionalMatches)
                    {
                        conditional.Team1.Value = ladder.Teams.FirstOrDefault(t => t.Acronym.Value == conditional.Team1Acronym);
                        conditional.Team2.Value = ladder.Teams.FirstOrDefault(t => t.Acronym.Value == conditional.Team2Acronym);
                        conditional.Round.Value = match.Round.Value;
                    }
                }

                // assign progressions
                foreach (var pair in ladder.Progressions)
                {
                    var src = ladder.Matches.FirstOrDefault(p => p.ID == pair.SourceID);
                    var dest = ladder.Matches.FirstOrDefault(p => p.ID == pair.TargetID);

                    if (src == null)
                        continue;

                    if (dest != null)
                    {
                        if (pair.Losers)
                            src.LosersProgression.Value = dest;
                        else
                            src.Progression.Value = dest;
                    }
                }

                // link matches to rounds
                foreach (var round in ladder.Rounds)
                {
                    foreach (int id in round.Matches)
                    {
                        var found = ladder.Matches.FirstOrDefault(p => p.ID == id);

                        if (found != null)
                        {
                            found.Round.Value = round;
                            if (round.StartDate.Value > found.Date.Value)
                                found.Date.Value = round.StartDate.Value;
                        }
                    }
                }

                addedInfo |= addPlayers();
                addedInfo |= await addRoundBeatmaps().ConfigureAwait(false);
                addedInfo |= await addSeedingBeatmaps().ConfigureAwait(false);

                if (addedInfo)
                    saveChanges();

                ladder.CurrentMatch.Value = ladder.Matches.FirstOrDefault(p => p.Current.Value);

                ladder.Ruleset.BindValueChanged(r =>
                {
                    // Refetch player rank data on next startup as the ruleset has changed.
                    foreach (var team in ladder.Teams)
                    {
                        foreach (var player in team.Players)
                            player.Rank = null;
                    }

                    SaveChanges();
                });
            }
            catch (Exception e)
            {
                bracketLoadTaskCompletionSource.SetException(e);
                return;
            }

            // Try to read and parse background mapping file
            try
            {
                if (storage.Exists(BACKGROUND_MAPPING_FILENAME))
                {
                    using (Stream stream = storage.GetStream(BACKGROUND_MAPPING_FILENAME, FileAccess.Read, FileMode.Open))
                    using (var sr = new StreamReader(stream))
                    {
                        ladder.BackgroundMap =
                            JsonConvert.DeserializeObject<BindableList<KeyValuePair<BackgroundType, BackgroundInfo>>>(await sr.ReadToEndAsync().ConfigureAwait(false), new JsonPointConverter())
                            ?? ladder.BackgroundMap;
                    }
                }
                else
                {
                    Logger.Log("Unable to find background mapping file. Is it included in the main bracket file?", level: LogLevel.Important);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to parse background mapping information, falling back to default values.");
            }

            Schedule(() =>
            {
                Ruleset.BindTo(ladder.Ruleset);

                dependencies.Cache(ladder);
                dependencies.CacheAs<MatchIPCInfo>(ipc = new FileBasedIPC());
                Add(ipc);

                bracketLoadTaskCompletionSource.SetResult(true);

                initialisationText.Expire();
            });
        }

        /// <summary>
        /// Add missing player info based on user IDs.
        /// </summary>
        private bool addPlayers()
        {
            var playersRequiringPopulation = ladder.Teams
                                                   .SelectMany(t => t.Players)
                                                   .Where(p => string.IsNullOrEmpty(p.Username)
                                                               || p.CountryCode == CountryCode.Unknown
                                                               || p.Rank == null).ToList();

            if (playersRequiringPopulation.Count == 0)
                return false;

            for (int i = 0; i < playersRequiringPopulation.Count; i++)
            {
                var p = playersRequiringPopulation[i];
                PopulatePlayer(p, immediate: true);
                updateLoadProgressMessage(BaseStrings.PopulatingUserStats(i, playersRequiringPopulation.Count));
            }

            return true;
        }

        /// <summary>
        /// Add missing beatmap info based on beatmap IDs
        /// </summary>
        private async Task<bool> addRoundBeatmaps()
        {
            var beatmapsRequiringPopulation = ladder.Rounds
                                                    .SelectMany(r => r.Beatmaps)
                                                    .Where(b => (b.Beatmap == null || b.Beatmap?.OnlineID == 0) && b.ID > 0).ToList();

            if (beatmapsRequiringPopulation.Count == 0)
                return false;

            for (int i = 0; i < beatmapsRequiringPopulation.Count; i++)
            {
                var b = beatmapsRequiringPopulation[i];

                var populated = await beatmapCache.GetBeatmapAsync(b.ID).ConfigureAwait(false);
                if (populated != null)
                    b.Beatmap = new TournamentBeatmap(populated);

                updateLoadProgressMessage(BaseStrings.PopulatingRoundBeatmaps(i, beatmapsRequiringPopulation.Count));
            }

            return true;
        }

        /// <summary>
        /// Add missing beatmap info based on beatmap IDs
        /// </summary>
        private async Task<bool> addSeedingBeatmaps()
        {
            var beatmapsRequiringPopulation = ladder.Teams
                                                    .SelectMany(r => r.SeedingResults)
                                                    .SelectMany(r => r.Beatmaps)
                                                    .Where(b => (b.Beatmap == null || b.Beatmap.OnlineID == 0) && b.ID > 0).ToList();

            if (beatmapsRequiringPopulation.Count == 0)
                return false;

            for (int i = 0; i < beatmapsRequiringPopulation.Count; i++)
            {
                var b = beatmapsRequiringPopulation[i];

                var populated = await beatmapCache.GetBeatmapAsync(b.ID).ConfigureAwait(false);
                if (populated != null)
                    b.Beatmap = new TournamentBeatmap(populated);

                updateLoadProgressMessage(BaseStrings.PopulatingSeedingBeatmaps(i, beatmapsRequiringPopulation.Count));
            }

            return true;
        }

        private void updateLoadProgressMessage(LocalisableString s) => Schedule(() => initialisationText.Text = s);

        public void PopulatePlayer(TournamentUser user, Action? success = null, Action? failure = null, bool immediate = false)
        {
            var req = new GetUserRequest(user.OnlineID, ladder.Ruleset.Value);

            if (immediate)
            {
                API.Perform(req);
                populate();
            }
            else
            {
                req.Success += _ => { populate(); };
                req.Failure += _ =>
                {
                    user.OnlineID = 1;
                    failure?.Invoke();
                };

                API.Queue(req);
            }

            void populate()
            {
                var res = req.Response;

                if (res == null)
                    return;

                user.OnlineID = res.Id;

                user.Username = res.Username;
                user.CoverUrl = res.CoverUrl;
                user.CountryCode = res.CountryCode;
                user.Rank = res.Statistics?.GlobalRank;

                success?.Invoke();
            }
        }

        public void SaveChanges()
        {
            if (!bracketLoadTaskCompletionSource.Task.IsCompletedSuccessfully)
            {
                Logger.Log("Inhibiting bracket save as bracket parsing failed");
                return;
            }

            saveChanges();
        }

        private void saveChanges()
        {
            // Serialise before opening stream for writing, so if there's a failure it will leave the file in the previous state.
            string serialisedLadder = GetSerialisedLadder(false);
            using (var stream = storage.CreateFileSafely(BRACKET_FILENAME))
            using (var sw = new StreamWriter(stream))
                sw.Write(serialisedLadder);

            string serialisedBackgroundMapping = JsonConvert.SerializeObject(ladder.BackgroundMap, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
            });

            using (var stream = storage.CreateFileSafely(BACKGROUND_MAPPING_FILENAME))
            using (var sw = new StreamWriter(stream))
                sw.Write(serialisedBackgroundMapping);
        }

        public string GetSerialisedLadder(bool includeAllParts = true)
        {
            ladder.SkipBackgroundMapSerialization = !includeAllParts;

            foreach (var r in ladder.Rounds)
                r.Matches = ladder.Matches.Where(p => p.Round.Value == r).Select(p => p.ID).ToList();

            ladder.Progressions = ladder.Matches.Where(p => p.Progression.Value != null).Select(p => new TournamentProgression(p.ID, p.Progression.Value.AsNonNull().ID)).Concat(
                                            ladder.Matches.Where(p => p.LosersProgression.Value != null).Select(p => new TournamentProgression(p.ID, p.LosersProgression.Value.AsNonNull().ID, true)))
                                        .ToList();

            return JsonConvert.SerializeObject(ladder, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Converters =
                {
                    new JsonPointConverter(),
                },
            });
        }

        protected override void InitialiseFonts()
        {
            AddFont(Resources, @"Fonts/osuFont");

            AddFont(Resources, @"Fonts/Torus/Torus-Regular");
            AddFont(Resources, @"Fonts/Torus/Torus-Light");
            AddFont(Resources, @"Fonts/Torus/Torus-SemiBold");
            AddFont(Resources, @"Fonts/Torus/Torus-Bold");

            AddFont(Resources, @"Fonts/Torus-Alternate/Torus-Alternate-Regular");
            AddFont(Resources, @"Fonts/Torus-Alternate/Torus-Alternate-Light");
            AddFont(Resources, @"Fonts/Torus-Alternate/Torus-Alternate-SemiBold");
            AddFont(Resources, @"Fonts/Torus-Alternate/Torus-Alternate-Bold");

            AddFont(Resources, @"Fonts/HarmonyTorus/HarmonyTorus-Regular");
            AddFont(Resources, @"Fonts/HarmonyTorus/HarmonyTorus-SemiBold");
            AddFont(Resources, @"Fonts/HarmonyTorus/HarmonyTorus-Bold");

            AddFont(Resources, @"Fonts/Noto/Noto-Basic");
            AddFont(Resources, @"Fonts/Noto/Noto-Hangul");
            AddFont(Resources, @"Fonts/Noto/Noto-CJK-Basic");
            AddFont(Resources, @"Fonts/Noto/Noto-CJK-Compatibility");
            AddFont(Resources, @"Fonts/Noto/Noto-Thai");

            Fonts.AddStore(new OsuIcon.OsuIconStore(Textures));
        }

        protected override UserInputManager CreateUserInputManager() => new TournamentInputManager();

        private partial class TournamentInputManager : UserInputManager
        {
            protected override MouseButtonEventManager CreateButtonEventManagerFor(MouseButton button)
            {
                switch (button)
                {
                    case MouseButton.Right:
                        return new RightMouseManager(button);
                }

                return base.CreateButtonEventManagerFor(button);
            }

            private class RightMouseManager : MouseButtonEventManager
            {
                public RightMouseManager(MouseButton button)
                    : base(button)
                {
                }

                public override bool EnableDrag => true; // allow right-mouse dragging for absolute scroll in scroll containers.
                public override bool EnableClick => true;
                public override bool ChangeFocusOnClick => false;
            }
        }
    }
}

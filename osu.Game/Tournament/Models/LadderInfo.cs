// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Game.Rulesets;

namespace osu.Game.Tournament.Models
{
    /// <summary>
    /// Holds the complete data required to operate the tournament system.
    /// </summary>
    [Serializable]
    public class LadderInfo
    {
        public Bindable<string> Version = new Bindable<string>(@"astra");

        public Bindable<string> FullName = new Bindable<string>();

        public Bindable<RulesetInfo?> Ruleset = new Bindable<RulesetInfo?>();

        public BindableList<TournamentMatch> Matches = new BindableList<TournamentMatch>();
        public BindableList<TournamentRound> Rounds = new BindableList<TournamentRound>();
        public BindableList<TournamentTeam> Teams = new BindableList<TournamentTeam>();
        public BindableList<PunishmentEntry> Punishments = new BindableList<PunishmentEntry>();

        // only used for serialisation
        public List<TournamentProgression> Progressions = new List<TournamentProgression>();

        [JsonIgnore] // updated manually in TournamentGameBase
        public Bindable<TournamentMatch?> CurrentMatch = new Bindable<TournamentMatch?>();

        public Bindable<int> ChromaKeyWidth = new BindableInt(1024)
        {
            MinValue = 640,
            MaxValue = 1366,
        };

        public Bindable<int> ShowcaseChromaWidth = new BindableInt(1366)
        {
            MinValue = 480,
            MaxValue = 1366,
        };

        public Bindable<int> ShowcaseChromaHeight = new BindableInt(TournamentExtensions.STREAM_AREA_HEIGHT - (int)TournamentExtensions.SONGBAR_HEIGHT)
        {
            MinValue = 270,
            MaxValue = TournamentExtensions.STREAM_AREA_HEIGHT - (int)TournamentExtensions.SONGBAR_HEIGHT
        };

        public Bindable<int> ShowcaseChromaVerticalOffset = new BindableInt
        {
            MinValue = 0,
            MaxValue = TournamentExtensions.STREAM_AREA_HEIGHT - (int)TournamentExtensions.SONGBAR_HEIGHT - 270
        };

        public Bindable<int> PlayersPerTeam = new BindableInt(4)
        {
            MinValue = 1,
            MaxValue = 4,
        };

        public Bindable<bool> UseUtcTime = new BindableBool();

        public Bindable<bool> AutoProgressRound = new BindableBool();

        public Bindable<bool> AutoProgressScreens = new BindableBool(true);

        public Bindable<bool> UseLazerIpc = new Bindable<bool>(true);

        public Bindable<bool> Use1V1Mode = new Bindable<bool>();

        public Bindable<bool> SplitMapPoolByMods = new BindableBool(true);

        public Bindable<bool> DisplayTeamSeeds = new BindableBool();

        public Bindable<bool> UseBlueChroma = new BindableBool(true);

        public BindableList<KeyValuePair<BackgroundType, BackgroundInfo>> BackgroundMap = new BindableList<KeyValuePair<BackgroundType, BackgroundInfo>>();

        [JsonIgnore]
        public bool SkipBackgroundMapSerialization = false;

        public bool ShouldSerializeBackgroundMap() => !SkipBackgroundMapSerialization;

        /// <summary>
        /// Now used for set cumulative scoring
        /// </summary>
        public Bindable<bool> CumulativeScore = new BindableBool();
    }
}

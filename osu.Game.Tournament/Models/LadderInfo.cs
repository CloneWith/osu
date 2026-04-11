// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets;
using osu.Game.Tournament.Components;

namespace osu.Game.Tournament.Models
{
    /// <summary>
    /// Holds the complete data required to operate the tournament system.
    /// </summary>
    [Serializable]
    public class LadderInfo
    {
        public Bindable<RulesetInfo?> Ruleset = new Bindable<RulesetInfo?>();

        public BindableList<TournamentMatch> Matches = new BindableList<TournamentMatch>();
        public BindableList<TournamentRound> Rounds = new BindableList<TournamentRound>();
        public BindableList<TournamentTeam> Teams = new BindableList<TournamentTeam>();

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

        public Bindable<int> ShowcaseChromaHeight = new BindableInt(TournamentSceneManager.STREAM_AREA_HEIGHT - (int)SongBar.HEIGHT)
        {
            MinValue = 270,
            MaxValue = TournamentSceneManager.STREAM_AREA_HEIGHT - (int)SongBar.HEIGHT
        };

        public Bindable<int> ShowcaseChromaVerticalOffset = new BindableInt()
        {
            MinValue = 0,
            MaxValue = TournamentSceneManager.STREAM_AREA_HEIGHT - (int)SongBar.HEIGHT - 270
        };

        public Bindable<int> PlayersPerTeam = new BindableInt(4)
        {
            MinValue = 3,
            MaxValue = 4,
        };

        public Bindable<bool> AutoProgressScreens = new BindableBool(true);

        public Bindable<bool> UseLazerIpc = new Bindable<bool>(true);

        public Bindable<bool> Use1V1Mode = new Bindable<bool>(false);

        public Bindable<bool> SplitMapPoolByMods = new BindableBool(true);

        public Bindable<bool> DisplayTeamSeeds = new BindableBool();

        /// <summary>
        /// Now used for set cumulative scoring
        /// </summary>
        public Bindable<bool> CumulativeScore = new BindableBool();

        public Bindable<Colour4> TextForegroundColour = new Bindable<Colour4>(Colour4.White);
    }
}

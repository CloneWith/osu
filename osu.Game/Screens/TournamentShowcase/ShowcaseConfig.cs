// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Game.Models;
using osu.Game.Rulesets;

namespace osu.Game.Screens.TournamentShowcase
{
    [Serializable]
    public class ShowcaseConfig
    {
        public Bindable<RulesetInfo?> Ruleset = new Bindable<RulesetInfo?>();

        public Bindable<string> TournamentName = new Bindable<string>();
        public Bindable<string> RoundName = new Bindable<string>();
        public Bindable<string> DateTime = new Bindable<string>();
        public Bindable<string> Comment = new Bindable<string>();

        public BindableInt TransformDuration = new BindableInt(1000)
        {
            MinValue = 250,
            MaxValue = 3000,
        };

        public BindableList<ShowcaseTeam> Teams = new BindableList<ShowcaseTeam>();

        public BindableList<ShowcaseStaff> Staffs = new BindableList<ShowcaseStaff>();

        public BindableList<ShowcaseBeatmap> Beatmaps = new BindableList<ShowcaseBeatmap>();

        public Bindable<bool> ShowStaffList = new Bindable<bool>();

        public Bindable<bool> SplitMapPoolByMods = new BindableBool(true);

        public Bindable<bool> DisplayTeamSeeds = new BindableBool();
    }
}

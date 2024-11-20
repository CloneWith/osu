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

        public BindableInt ShowcaseCountdown = new BindableInt(3)
        {
            MinValue = 0,
            MaxValue = 30,
        };

        public BindableList<ShowcaseTeam> Teams = new BindableList<ShowcaseTeam>();
        public BindableList<ShowcaseStaff> Staffs = new BindableList<ShowcaseStaff>();
        public BindableList<ShowcaseBeatmap> Beatmaps = new BindableList<ShowcaseBeatmap>();

        public Bindable<ShowcaseLayout> Layout = new Bindable<ShowcaseLayout>();

        public BindableBool UseCustomIntroBeatmap = new BindableBool();

        public Bindable<ShowcaseBeatmap> IntroBeatmap = new Bindable<ShowcaseBeatmap>();
        public Bindable<TimeSpan> IntroTimestamp = new Bindable<TimeSpan>();

        public BindableBool ShowMapPool = new BindableBool(true);
        public BindableBool ShowBoardPool = new BindableBool();
        public BindableBool ShowStaffList = new BindableBool(true);
        public BindableBool SplitMapPoolByMods = new BindableBool(true);
    }
}

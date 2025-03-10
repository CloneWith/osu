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
        /// <summary>
        /// A fallback ruleset for the showcase.
        /// <br/>When unable to find a specific ruleset using the ID provided by a <see cref="ShowcaseBeatmap"/>,
        /// we can turn to use this.
        /// </summary>
        public Bindable<RulesetInfo> FallbackRuleset = new Bindable<RulesetInfo>();

        public Bindable<string> TournamentName = new Bindable<string>();
        public Bindable<string> RoundName = new Bindable<string>();
        public Bindable<string> DateTime = new Bindable<string>();
        public Bindable<string> Comment = new Bindable<string>();

        public BindableInt StartCountdown = new BindableInt(5000)
        {
            MinValue = 3000,
            MaxValue = 60000,
            Precision = 1000
        };

        public BindableInt TransformDuration = new BindableInt(1000)
        {
            MinValue = 250,
            MaxValue = 3000,
        };

        public BindableInt PauseCountdown = new BindableInt(3000)
        {
            MinValue = 1000,
            MaxValue = 5000,
            Precision = 500
        };

        public BindableFloat AspectRatio = new BindableFloat(1)
        {
            MinValue = 0.6f,
            MaxValue = 3f,
            Precision = 0.1f
        };

        public BindableList<ShowcaseBeatmap> Beatmaps = new BindableList<ShowcaseBeatmap>();

        public Bindable<ShowcaseLayout> Layout = new Bindable<ShowcaseLayout>();

        public BindableBool UseCustomIntroBeatmap = new BindableBool();
        public Bindable<ShowcaseBeatmap> IntroBeatmap = new Bindable<ShowcaseBeatmap>();

        public BindableBool ShowMapPool = new BindableBool(true);
        public BindableBool SplitMapPoolByMods = new BindableBool(true);

        public Bindable<string> OutroTitle = new Bindable<string>();
        public Bindable<string> OutroSubtitle = new Bindable<string>();
    }
}

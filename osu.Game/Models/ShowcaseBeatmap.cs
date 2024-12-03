// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Screens.TournamentShowcase;

namespace osu.Game.Models
{
    [Serializable]
    public class ShowcaseBeatmap
    {
        // The BeatmapInfo structure itself introduces looped reference, so ignoring it.
        [JsonIgnore]
        public BeatmapInfo BeatmapInfo = new BeatmapInfo();

        public int BeatmapId;
        public Guid BeatmapGuid = Guid.Empty;
        public int RulesetId;
        public Bindable<BeatmapType> ModType = new Bindable<BeatmapType>();
        public string ModString = string.Empty;
        public Bindable<string> ModIndex = new Bindable<string>();
        public IEnumerable<Mod> RequiredMods { get; set; } = Enumerable.Empty<Mod>();

        public Bindable<ShowcaseUser> Selector = new Bindable<ShowcaseUser>();
        public BindableBool IsOriginal = new BindableBool();
        public Bindable<string> BeatmapArea = new Bindable<string>();
        public Bindable<string> BeatmapComment = new Bindable<string>();

        [JsonIgnore]
        public ScoreInfo? ShowcaseScore;

        public string ScoreHash = string.Empty;

        public ShowcaseBeatmap()
        {
        }

        public ShowcaseBeatmap(BeatmapInfo? beatmapInfo)
        {
            BeatmapInfo = beatmapInfo ?? new BeatmapInfo();
            BeatmapId = BeatmapInfo.OnlineID;
            BeatmapGuid = BeatmapInfo.ID;
        }
    }
}

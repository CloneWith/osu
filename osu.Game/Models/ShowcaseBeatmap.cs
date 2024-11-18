// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Replays;

namespace osu.Game.Models
{
    [Serializable]
    public class ShowcaseBeatmap
    {
        // The BeatmapInfo structure itself introduces looped reference, so ignoring it.
        [JsonIgnore]
        public readonly BeatmapInfo BeatmapInfo;

        public readonly int BeatmapId;
        public readonly Guid BeatmapGuid;

        public readonly BindableBool IsOriginal = new BindableBool();
        public readonly Bindable<string> BeatmapArea = new Bindable<string>();
        public readonly Bindable<string> BeatmapComment = new Bindable<string>();

        public Replay? ShowcaseReplay;

        public ShowcaseBeatmap(BeatmapInfo? beatmapInfo)
        {
            BeatmapInfo = beatmapInfo ?? new BeatmapInfo();
            BeatmapId = BeatmapInfo.OnlineID;
            BeatmapGuid = BeatmapInfo.ID;
        }
    }
}

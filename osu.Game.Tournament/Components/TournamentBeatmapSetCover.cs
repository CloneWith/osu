// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Tournament.Caching;

namespace osu.Game.Tournament.Components
{
    /// <summary>
    /// A beatmap set cover which reads through <see cref="OnlineAssetCache"/>.
    /// </summary>
    [LongRunningLoad]
    public partial class TournamentBeatmapSetCover : Sprite
    {
        private readonly IBeatmapSetOnlineInfo set;
        private readonly BeatmapSetCoverType type;

        public TournamentBeatmapSetCover(IBeatmapSetOnlineInfo set, BeatmapSetCoverType type = BeatmapSetCoverType.Cover)
        {
            ArgumentNullException.ThrowIfNull(set);

            this.set = set;
            this.type = type;
        }

        [BackgroundDependencyLoader]
        private void load(OnlineAssetCache onlineAssets)
        {
            string? resource = type switch
            {
                BeatmapSetCoverType.Card => set.Covers.Card,
                BeatmapSetCoverType.List => set.Covers.List,
                _ => set.Covers.Cover,
            };

            if (resource == null)
                return;

            Texture = onlineAssets.Get(resource);
        }
    }
}

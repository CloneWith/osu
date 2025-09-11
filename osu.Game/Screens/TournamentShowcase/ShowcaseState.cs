// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Screens.TournamentShowcase
{
    public enum ShowcaseState
    {
        /// <summary>
        /// Getting things ready.
        /// </summary>
        Initialization,
        Intro,
        MapPool,

        /// <summary>
        /// A beatmap replay is being shown.
        /// </summary>
        BeatmapShow,

        /// <summary>
        /// Switching between beatmaps.
        /// </summary>
        BeatmapTransition,
        Ending,

        /// <summary>
        /// The showcase has come to a conclusion.
        /// </summary>
        Ended,
    }
}

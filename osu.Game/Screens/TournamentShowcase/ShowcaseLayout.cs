// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace osu.Game.Screens.TournamentShowcase
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ShowcaseLayout
    {
        [Description("Immersive Mode (Fullscreen)")]
        Immersive,

        [Description("Simple Control")]
        SimpleControl,

        [Description("Detailed Control")]
        DetailedControl,
    }
}

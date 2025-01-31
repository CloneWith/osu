// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Overlays.Settings;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Components
{
    public partial class BackgroundTypeDropdown : SettingsDropdown<BackgroundType>
    {
        public BackgroundTypeDropdown()
        {
            Items = Enum.GetValues(typeof(BackgroundType)).Cast<BackgroundType>().ToArray();
        }
    }
}

﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Overlays.Settings;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Screens.Board.Components
{
    public partial class TrapTypeDropdown : SettingsDropdown<LocalisableString>
    {
        public TrapTypeDropdown()
        {
            ShowsDefaultIndicator = false;

            add(new TrapInfo(type: TrapType.Swap).Name);
            add(new TrapInfo(type: TrapType.Reverse).Name);
        }

        private void add(LocalisableString typeName)
        {
            Control.AddDropdownItem(typeName);
        }
    }
}

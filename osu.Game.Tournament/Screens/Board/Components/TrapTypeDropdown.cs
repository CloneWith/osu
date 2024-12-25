﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Screens.Board.Components
{
    public partial class TrapTypeDropdown : FormDropdown<LocalisableString>
    {
        public TrapTypeDropdown()
        {
            Items = new List<LocalisableString>
            {
                new TrapInfo(type: TrapType.Swap).Name,
                new TrapInfo(type: TrapType.Reverse).Name
            };
        }
    }
}

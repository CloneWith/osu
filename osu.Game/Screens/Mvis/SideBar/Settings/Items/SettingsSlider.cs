// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;

namespace osu.Game.Screens.Mvis.SideBar.Settings.Items
{
    [Obsolete("原Mvis播放器现已移动至LLin(osu.Game.Screens.LLin)")]
    public class SettingsSlider<T> : osu.Game.Screens.LLin.SideBar.Settings.Items.SettingsSlider<T>
        where T : struct, IEquatable<T>, IComparable<T>, IConvertible
    {
    }
}

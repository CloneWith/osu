// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class LabelledColourPicker : LabelledComponent<OsuColourPicker, Colour4>
    {
        public LabelledColourPicker()
            : base(true)
        {
        }

        protected override OsuColourPicker CreateComponent() => new OsuColourPicker();
    }
}

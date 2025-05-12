// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Graphics.UserInterface
{
    public partial class OsuDecimalBox : OsuTextBox
    {
        public OsuDecimalBox()
        {
            SelectAllOnFocus = true;
        }

        protected override bool CanAddCharacter(char character)
            => char.IsAsciiDigit(character) || (character == '.' && !Text.Contains('.'));
    }
}

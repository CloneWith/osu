// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ProfileCheckFailedDialog : PopupDialog
    {
        /// <summary>
        /// Construct a new dialog to prompt the profile filled by user is invalid.
        /// </summary>
        public ProfileCheckFailedDialog()
        {
            HeaderText = @"Oops...";
            BodyText = @"Fill in the tournament title, round and ruleset at least!";

            Icon = FontAwesome.Solid.ExclamationTriangle;

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton()
            };
        }
    }
}

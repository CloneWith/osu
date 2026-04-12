// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Overlays.Dialog;
using osu.Game.Tournament.Localisation;

namespace osu.Game.Tournament.Components.Dialogs
{
    public partial class ActionNotPermittedDialog : PopupDialog
    {
        public ActionNotPermittedDialog(LocalisableString bodyText)
        {
            Icon = FontAwesome.Solid.Ban;
            HeaderText = BaseStrings.ActionNotPermitted;
            BodyText = bodyText;
            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = BaseStrings.Okay,
                },
            };
        }
    }
}

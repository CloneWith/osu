// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;
using osu.Game.Tournament.Localisation.Screens;

namespace osu.Game.Tournament.Components.Dialogs
{
    public partial class BackgroundResetDialog : PopupDialog
    {
        public BackgroundResetDialog(Action resetOneAction, Action resetAllAction)
        {
            HeaderText = BackgroundSelectStrings.ResetBackgroundTitle;
            BodyText = BackgroundSelectStrings.ResetBackgroundText;
            Icon = FontAwesome.Solid.Undo;
            Buttons = new PopupDialogButton[]
            {
                new PopupDialogDangerousButton
                {
                    Text = BackgroundSelectStrings.DialogResetOne,
                    Action = resetOneAction,
                },
                new PopupDialogDangerousButton
                {
                    Text = BackgroundSelectStrings.DialogResetAll,
                    Action = resetAllAction,
                },
                new PopupDialogCancelButton
                {
                    Text = BackgroundSelectStrings.DialogCancel,
                },
            };
        }
    }
}

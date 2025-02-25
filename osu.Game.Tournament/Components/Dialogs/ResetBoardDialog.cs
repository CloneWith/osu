// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Tournament.Components.Dialogs
{
    public partial class ResetBoardDialog : PopupDialog
    {
        public ResetBoardDialog(Action resetAction)
        {
            HeaderText = @"Warning: Reset";
            BodyText = @"This would reset the board to the initial state. Are you sure?";
            Icon = FontAwesome.Solid.Undo;
            Buttons = new PopupDialogButton[]
            {
                new PopupDialogDangerousButton
                {
                    Text = @"Yes, reset to the initial state.",
                    Action = resetAction,
                },
                new PopupDialogCancelButton
                {
                    Text = @"I'd rather stay the same.",
                },
            };
        }
    }
}

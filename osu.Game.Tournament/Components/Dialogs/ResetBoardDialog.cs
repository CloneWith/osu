// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;
using osu.Game.Tournament.Localisation.Screens;

namespace osu.Game.Tournament.Components.Dialogs
{
    public partial class ResetBoardDialog : PopupDialog
    {
        public ResetBoardDialog(Action resetAction)
        {
            HeaderText = BoardStrings.ResetBoardTitle;
            BodyText = BoardStrings.ResetBoardDescription;
            Icon = FontAwesome.Solid.Undo;
            Buttons = new PopupDialogButton[]
            {
                new PopupDialogDangerousButton
                {
                    Text = BoardStrings.AgreeReset,
                    Action = resetAction,
                },
                new PopupDialogCancelButton
                {
                    Text = BoardStrings.KeepCurrentState,
                },
            };
        }
    }
}

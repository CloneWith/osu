// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Tournament.Components.Dialogs
{
    public partial class FetchDataDialog : PopupDialog
    {
        public FetchDataDialog(Action<bool> fetchAction)
        {
            HeaderText = @"Fetch Data";
            BodyText = @"";
            Icon = FontAwesome.Solid.Download;
            Buttons = new PopupDialogButton[]
            {
                new PopupDialogButton
                {
                    Text = @"Fetch data for unknown entries only.",
                    Action = () => fetchAction.Invoke(false),
                },
                new PopupDialogButton
                {
                    Text = @"Fetch data for all entries.",
                    Action = () => fetchAction.Invoke(true),
                },
                new PopupDialogCancelButton
                {
                    Text = @"No.",
                },
            };
        }
    }
}

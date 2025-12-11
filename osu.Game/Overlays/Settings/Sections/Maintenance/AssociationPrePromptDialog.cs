// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public partial class AssociationPrePromptDialog : PopupDialog
    {
        public AssociationPrePromptDialog(Action setAssociation)
        {
            HeaderText = DebugSettingsStrings.AssociationDialogHeader;
            BodyText = DebugSettingsStrings.AssociationDialogText;
            Icon = FontAwesome.Solid.InfoCircle;

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = DialogStrings.Confirm,
                    Action = setAssociation,
                },
                new PopupDialogCancelButton
                {
                    Text = DialogStrings.Cancel,
                },
            };
        }
    }
}

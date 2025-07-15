// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Graphics;
using osu.Game.Localisation;
using osu.Game.Overlays.Dialog;
using CommonStrings = osu.Game.Resources.Localisation.Web.CommonStrings;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ScoreMissingDialog : ConfirmDialog
    {
        public ScoreMissingDialog(int missingNumber, Action onConfirm)
            : base("", onConfirm)
        {
            HeaderText = TournamentShowcaseStrings.ScoreMissingDialogTitle;
            BodyText = TournamentShowcaseStrings.ScoreMissingDialogText(missingNumber);

            Icon = OsuIcon.Search;

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = TournamentShowcaseStrings.ScoreMissingProceed,
                    Action = onConfirm,
                },
                new PopupDialogCancelButton
                {
                    Text = CommonStrings.ButtonsCancel,
                },
            };
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Tournament.Localisation;

namespace osu.Game.Tournament
{
    internal partial class SaveChangesButton : OsuButton, IKeyBindingHandler<PlatformAction>
    {
        [Resolved]
        private TournamentGame? tournamentGame { get; set; }

        private string? lastSerialisedLadder;
        private static bool ladderUnchanged = true;

        public SaveChangesButton()
        {
            RelativeSizeAxes = Axes.X;
            Height = 48;
            Text = BaseStrings.SaveChanges;
            Action = saveChanges;
            Enabled.Value = false;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            scheduleNextCheck();
        }

        private async Task checkForChanges()
        {
            if (tournamentGame == null)
            {
                Enabled.Value = false;
                return;
            }

            string serialisedLadder = await Task.Run(() => tournamentGame.GetSerialisedLadder()).ConfigureAwait(true);

            // If a save hasn't been triggered by the user yet, populate the initial value
            lastSerialisedLadder ??= serialisedLadder;
            ladderUnchanged &= lastSerialisedLadder == serialisedLadder;
            Enabled.Value = !ladderUnchanged;

            scheduleNextCheck();
        }

        public bool OnPressed(KeyBindingPressEvent<PlatformAction> e)
        {
            if (e.Action == PlatformAction.Save && !e.Repeat)
            {
                TriggerClick();
                return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<PlatformAction> e)
        {
        }

        private void scheduleNextCheck() => Scheduler.AddDelayed(() => checkForChanges().FireAndForget(), 1000);

        private void saveChanges()
        {
            tournamentGame?.SaveChanges();
            lastSerialisedLadder = tournamentGame?.GetSerialisedLadder();
            ladderUnchanged = true;

            Enabled.Value = false;
        }
    }
}

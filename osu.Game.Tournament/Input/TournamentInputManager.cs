// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input;
using osuTK.Input;

namespace osu.Game.Tournament.Input
{
    /// <summary>
    /// The input manager the tournament host runs.
    /// </summary>
    /// <remarks>
    /// The same as the one osu! runs, apart from the right mouse button, which the tournament gives clicks to;
    /// see <see cref="TournamentMouseButtonManagers"/> for why. The tests drive a manual input manager built
    /// from the same managers, so that what the right button does here is what it does in a test.
    /// </remarks>
    public partial class TournamentInputManager : UserInputManager
    {
        protected override MouseButtonEventManager CreateButtonEventManagerFor(MouseButton button)
            => TournamentMouseButtonManagers.CreateFor(button) ?? base.CreateButtonEventManagerFor(button);
    }
}

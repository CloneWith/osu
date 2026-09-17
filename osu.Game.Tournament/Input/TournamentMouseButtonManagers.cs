// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input;
using osuTK.Input;

namespace osu.Game.Tournament.Input
{
    /// <summary>
    /// The mouse button event managers the tournament hands to the framework, kept in one place so the host and
    /// the test suite cannot drift apart.
    /// </summary>
    public static class TournamentMouseButtonManagers
    {
        /// <summary>
        /// The manager to handle <paramref name="button"/> with, or <c>null</c> to leave the button to the framework.
        /// </summary>
        public static MouseButtonEventManager? CreateFor(MouseButton button)
            => button == MouseButton.Right ? new RightMouseManager(button) : null;

        /// <summary>
        /// The framework keeps clicks off for every button but the left one, so that a right click can be told
        /// apart from a left one by which events arrive.
        /// </summary>
        private class RightMouseManager : MouseButtonEventManager
        {
            public RightMouseManager(MouseButton button)
                : base(button)
            {
            }

            public override bool EnableDrag => true; // allow right-mouse dragging for absolute scroll in scroll containers.
            public override bool EnableClick => true;
            public override bool ChangeFocusOnClick => false;
        }
    }
}

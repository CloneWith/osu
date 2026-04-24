// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Utils
{
    /// <summary>
    /// Log silently without showing an error notification to the user.
    /// </summary>
    internal class DiagnosticsException : Exception
    {
        public DiagnosticsException(string message)
            : base(message)
        {
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;

namespace osu.Game.Tournament.Components
{
    public interface IAnimation : IDrawable
    {
        /// <summary>
        /// Triggered when the animation has completed.
        /// </summary>
        event Action? OnAnimationComplete;

        /// <summary>
        /// Start the animation.
        /// </summary>
        void Fire();

        /// <summary>
        /// The status of the animation.
        /// </summary>
        AnimationStatus Status { get; }
    }

    public enum AnimationStatus
    {
        Loading,
        Running,
        Complete
    }
}

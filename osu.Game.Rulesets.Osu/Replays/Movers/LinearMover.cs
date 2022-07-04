﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osuTK;

namespace osu.Game.Rulesets.Osu.Replays.Movers
{
    public class LinearMover : Mover
    {
        protected readonly bool WaitForPreempt = MConfigManager.Instance.Get<bool>(MSetting.WaitForPreempt);
        protected new double StartTime;

        protected double GetReactionTime(double timeInstant) => ApplyModsToRate(timeInstant, 100);

        protected double ApplyModsToRate(double time, double rate)
        {
            foreach (var mod in TimeAffectingMods)
                rate = mod.ApplyToRate(time, rate);
            return rate;
        }

        public override Vector2 Update(double time)
        {
            double waitTime = End.StartTime - Math.Max(0.0, End.TimePreempt - GetReactionTime(EndTime - End.TimePreempt));

            if (WaitForPreempt && waitTime > time)
            {
                StartTime = waitTime;
                return StartPos;
            }

            return Interpolation.ValueAt(time, StartPos, EndPos, StartTime, EndTime, Easing.Out);
        }

        public override void OnObjChange() => StartTime = base.StartTime;
    }
}

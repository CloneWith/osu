// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Graphics.Sprites;
using System.Numerics;

namespace osu.Game.Graphics.UserInterface
{
    public abstract partial class CommaSeparatedScoreCounter<T> : RollingCounter<T> where T : struct, INumber<T>, IConvertible
    {
        protected override double RollingDuration => 1000;
        protected override Easing RollingEasing => Easing.Out;

        protected override double GetProportionalDuration(T currentValue, T newValue) =>
            Convert.ToDouble(currentValue > newValue ? currentValue - newValue : newValue - currentValue);

        protected override LocalisableString FormatCount(T count) => count.ToLocalisableString(@"N0");

        protected override OsuSpriteText CreateSpriteText()
            => base.CreateSpriteText().With(s => s.Font = s.Font.With(fixedWidth: true));
    }

    public abstract partial class CommaSeparatedScoreCounter : CommaSeparatedScoreCounter<double>;
}

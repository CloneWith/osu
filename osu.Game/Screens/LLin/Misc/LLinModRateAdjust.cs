// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Screens.LLin.Misc
{
    internal class LLinModRateAdjust : ModRateAdjust
    {
        public override string Name => ToString();
        public override string Acronym => "RA";
        public override LocalisableString Description => "missingno";
        public override double ScoreMultiplier => 0;

        public override BindableNumber<double> SpeedChange { get; } = new BindableDouble
        {
            Value = 1,
            MaxValue = 2,
            MinValue = 0.1f
        };

        public override void ApplyToTrack(IAdjustableAudioComponent track)
        {
            //不要应用到音轨，我们只希望这个Mod影响故事版Sample
        }
    }
}

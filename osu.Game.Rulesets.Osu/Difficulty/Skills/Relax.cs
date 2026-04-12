// This file is originally created by GooGuTeam.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to aim patterns when the Relax mod is enabled.
    /// </summary>
    public class Relax : OsuStrainSkill
    {
        public readonly bool IncludeSliders;

        public Relax(Mod[] mods, bool includeSliders)
            : base(mods)
        {
            IncludeSliders = includeSliders;
        }

        private const double skill_multiplier = 24.16;
        private const double strain_decay_base = 0.15;

        private double currentStrain;

        private readonly List<double> sliderStrains = new List<double>();

        private double strainDecay(double ms) => Math.Pow(strain_decay_base, ms / 1000);

        protected override double CalculateInitialStrain(double time, DifficultyHitObject current) => currentStrain * strainDecay(time - current.Previous(0).StartTime);

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            var osuCurrent = (OsuDifficultyHitObject)current;

            currentStrain *= strainDecay(osuCurrent.DeltaTime);
            currentStrain += RelaxAimEvaluator.EvaluateDifficultyOf(osuCurrent, IncludeSliders) * skill_multiplier;

            if (current.BaseObject is Slider)
                sliderStrains.Add(currentStrain);

            return currentStrain;
        }

        public double GetDifficultSliders()
        {
            if (sliderStrains.Count == 0)
                return 0;

            double maxSliderStrain = sliderStrains.Max();
            if (maxSliderStrain == 0)
                return 0;

            return sliderStrains.Sum(strain => 1.0 / (1.0 + Math.Exp(-(strain / maxSliderStrain * 12.0 - 6.0))));
        }
    }
}

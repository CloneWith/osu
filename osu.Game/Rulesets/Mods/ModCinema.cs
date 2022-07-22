// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModCinema<T> : ModCinema, IApplicableToDrawableRuleset<T>
        where T : HitObject
    {
        public virtual void ApplyToDrawableRuleset(DrawableRuleset<T> drawableRuleset)
        {
            // AlwaysPresent required for hitsounds
            drawableRuleset.AlwaysPresent = true;
            drawableRuleset.Hide();
        }
    }

    public class ModCinema : ModAutoplay
    {
        public override string Name => "Cinema";
        public override string Acronym => "CN";
        public override IconUsage? Icon => OsuIcon.ModCinema;
        public override string Description => "Watch the video without visual distractions.";

        public override void ApplyToHUD(HUDOverlay overlay)
        {
            overlay.ShowHud.Value = false;
            overlay.ShowHud.Disabled = true;
        }

        public override void ApplyToPlayer(Player player)
        {
            player.ApplyToBackground(b => b.IgnoreUserSettings.Value = true);
            player.DimmableStoryboard.IgnoreUserSettings.Value = true;

            player.BreakOverlay.Hide();
        }
    }
}

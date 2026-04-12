// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Overlays;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterfaceFumo
{
    public partial class FlatActionButton : FlatButton, IFilterable
    {
        [BackgroundDependencyLoader(true)]
        private void load(OverlayColourProvider? overlayColourProvider, OsuColour colours)
        {
            // Many buttons have local colours, but this provides a sane default for all other cases.
            DefaultBackgroundColour = overlayColourProvider?.Colour3 ?? colours.Blue3;
        }

        protected override bool OnHover(HoverEvent e)
        {
            Background.FadeColour(BackgroundColour.Opacity(0.3f), 300, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            Background.FadeColour(Color4.White.Opacity(0), 300, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        public virtual IEnumerable<LocalisableString> FilterTerms => new[] { Text };

        public bool MatchingFilter
        {
            set => this.FadeTo(value ? 1 : 0);
        }

        public bool FilteringActive { get; set; }
    }
}

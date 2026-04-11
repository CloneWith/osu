// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Tournament.Components;
using osu.Framework.Graphics.Shapes;
using osu.Game.Overlays.Settings;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Ladder.Components;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Showcase
{
    public partial class ShowcaseScreen : BeatmapInfoScreen
    {
        private SettingsDropdown<TournamentRound?> roundDropdown = null!;
        private Drawable chroma = null!;

        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder)
        {
            AddRangeInternal(new Drawable[]
            {
                new TournamentLogo(),
                new TourneyVideo("showcase")
                {
                    Loop = true,
                    RelativeSizeAxes = Axes.Both,
                },
                new Container
                {
                    Padding = new MarginPadding { Bottom = SongBar.HEIGHT },
                    RelativeSizeAxes = Axes.Both,
                    Child = chroma = new Box
                    {
                        // chroma key area for stable gameplay
                        Name = "chroma",
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Height = 512,
                        Y = 0,
                        Colour = new Color4(0, 255, 0, 255),
                    }
                },
                new ControlPanel
                {
                    Children = new Drawable[]
                    {
                        roundDropdown = new LadderEditorSettings.SettingsRoundDropdown(ladder.Rounds) { LabelText = "Round" },
                        new SettingsSlider<int>
                        {
                            LabelText = "Chroma width",
                            Current = LadderInfo.ShowcaseChromaWidth,
                            KeyboardStep = 1,
                        },
                        new SettingsSlider<int>
                        {
                            LabelText = "Chroma height",
                            Current = LadderInfo.ShowcaseChromaHeight,
                            KeyboardStep = 1,
                        },
                        new SettingsSlider<int>
                        {
                            LabelText = "Chroma offset",
                            Current = LadderInfo.ShowcaseChromaVerticalOffset,
                            KeyboardStep = 1,
                        },
                        new TourneyButton
                        {
                            Text = "Set 16:9 ratio from width",
                            Action = () => LadderInfo.ShowcaseChromaHeight.Value = LadderInfo.ShowcaseChromaWidth.Value / 16 * 9
                        },
                        new TourneyButton
                        {
                            Text = "Set 16:9 ratio from height",
                            Action = () => LadderInfo.ShowcaseChromaWidth.Value = LadderInfo.ShowcaseChromaHeight.Value * 16 / 9
                        }
                    },
                }
            });

            roundDropdown.Current.BindValueChanged(@event =>
            {
                SongBar.Pool = @event.NewValue?.Beatmaps.ToList() ?? [];
            });

            LadderInfo.ShowcaseChromaWidth.BindValueChanged(width => chroma.Width = width.NewValue, true);
            LadderInfo.ShowcaseChromaHeight.BindValueChanged(height => chroma.Height = height.NewValue, true);
            LadderInfo.ShowcaseChromaVerticalOffset.BindValueChanged(offset => chroma.Y = -offset.NewValue, true);
        }

        protected override void CurrentMatchChanged(ValueChangedEvent<TournamentMatch?> match)
        {
            // showcase screen doesn't care about a match being selected.
            // base call intentionally omitted to not show match warning.
        }
    }
}

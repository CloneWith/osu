// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets;
using osu.Game.Tournament.Models;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    /// <summary>
    /// A mod icon with an expandable text mod badge.
    /// </summary>
    public partial class TournamentFumoModIcon : CompositeDrawable
    {
        private readonly string modAcronym;
        private readonly string modIndex;
        private readonly Box mainBackground;
        private readonly Box badgeBackground;
        private readonly OsuSpriteText badgeText;
        private readonly Sprite imageSprite;
        private readonly SpriteIcon iconSprite;

        /// <summary>
        /// The currently displaying texture (as icon) of this mod icon.
        /// </summary>
        public Texture? Texture
        {
            get => imageSprite.Texture;
            set
            {
                if (value == null) return;

                imageSprite.Texture = value;
                iconSprite.Hide();
                imageSprite.Show();
                imageSprite.ScaleTo(1.25f).Then().ScaleTo(1f, 300, Easing.OutQuint);
            }
        }

        /// <summary>
        /// The currently displaying icon of this mod icon.
        /// </summary>
        public IconUsage? Icon
        {
            get => iconSprite.Icon;
            set
            {
                if (value == null) return;

                iconSprite.Icon = value.Value;
                imageSprite.Hide();
                iconSprite.Show();
                iconSprite.ScaleTo(0.8f).Then().ScaleTo(0.75f, 300, Easing.OutQuint);
            }
        }

        public Color4 MainBackgroundColour
        {
            get => mainBackgroundColour;
            set
            {
                mainBackgroundColour = value;
                mainBackground.FadeColour(value, 300, Easing.OutQuint);
            }
        }

        public Color4 BadgeBackgroundColour
        {
            get => badgeBackgroundColour;
            set
            {
                badgeBackgroundColour = value;
                badgeBackground.FadeColour(value, 300, Easing.OutQuint);
            }
        }

        public Color4 BadgeTextColour
        {
            get => badgeTextColour;
            set
            {
                badgeTextColour = value;
                badgeText.FadeColour(value, 300, Easing.OutQuint);
            }
        }

        public Color4 MainIconColour
        {
            get => mainIconColour;
            set
            {
                mainIconColour = value;
                iconSprite.FadeColour(value, 300, Easing.OutQuint);
            }
        }

        private Color4 mainBackgroundColour = Color4Extensions.FromHex("#535353");
        private Color4 badgeBackgroundColour = Color4Extensions.FromHex("#535353");
        private Color4 badgeTextColour = Color4.White;
        private Color4 mainIconColour = Color4.White;

        [Resolved]
        private IRulesetStore rulesets { get; set; } = null!;

        public TournamentFumoModIcon(string modAcronym, string modIndex)
        {
            AutoSizeAxes = Axes.Both;

            this.modAcronym = modAcronym;
            this.modIndex = modIndex;

            ModColour? colorScheme = ModIconColours.ColourMap.FirstOrDefault(kv => kv.Key == modAcronym).Value;

            if (colorScheme != null)
            {
                mainBackgroundColour = colorScheme.Value.MainBackground;
                badgeBackgroundColour = colorScheme.Value.BadgeBackground;
                badgeTextColour = colorScheme.Value.BadgeText;
                mainIconColour = colorScheme.Value.MainIcon;
            }

            InternalChildren = new Drawable[]
            {
                new CircularContainer
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Size = new Vector2(36),
                    Masking = true,
                    Children = new Drawable[]
                    {
                        mainBackground = new Box
                        {
                            RelativeSizeAxes = Axes.Both, Colour = mainBackgroundColour
                        },
                        imageSprite = new Sprite
                        {
                            FillMode = FillMode.Fit,
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Alpha = 0
                        },
                        iconSprite = new SpriteIcon
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Colour = mainIconColour,
                            Size = new Vector2(36),
                            Scale = new Vector2(0.75f),
                            Alpha = 0
                        }
                    }
                },
                new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    X = 12,
                    Y = 15f,
                    AutoSizeAxes = Axes.Both,
                    AutoSizeDuration = 300,
                    AutoSizeEasing = Easing.OutQuint,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        badgeBackground = new Box
                        {
                            RelativeSizeAxes = Axes.Both, Colour = badgeBackgroundColour
                        },
                        badgeText = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = modAcronym + modIndex,
                            Colour = badgeTextColour,
                            Font = OsuFont.Torus.With(size: 11, weight: FontWeight.SemiBold),
                            Padding = new MarginPadding
                            {
                                Horizontal = 5, Vertical = 1
                            },
                            ShadowColour = Color4.Black.Opacity(0.3f),
                            ShadowOffset = new Vector2(0, 0.08f),
                            Shadow = true,
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures, LadderInfo ladderInfo)
        {
            if (modAcronym.IsNull()) return;

            var customTexture = textures.Get($"Mods/{modAcronym}");

            if (customTexture != null)
            {
                imageSprite.Show();
                imageSprite.Texture = customTexture;
                return;
            }

            var ruleset = rulesets.GetRuleset(ladderInfo.Ruleset.Value?.OnlineID ?? 0);
            var modIcon = ruleset?.CreateInstance().CreateModFromAcronym(modAcronym);

            iconSprite.Show();
            iconSprite.Icon = modIcon?.Icon ?? FontAwesome.Solid.Question;
        }

        /// <summary>
        /// Expand the mod text badge.
        /// </summary>
        public void Expand()
        {
            badgeText.Text = modAcronym + modIndex;
        }

        /// <summary>
        /// Collapse the mod text badge.
        /// </summary>
        public void Collapse()
        {
            badgeText.Text = modIndex;
        }
    }

    /// <summary>
    /// A colour scheme struct for <see cref="TournamentFumoModIcon"/>.
    /// </summary>
    public struct ModColour
    {
        public Color4 MainBackground;
        public Color4 BadgeBackground;
        public Color4 BadgeText;
        public Color4 MainIcon;
    }

    /// <summary>
    /// A dummy static class for colour schemes of <see cref="TournamentFumoModIcon"/>.
    /// </summary>
    public static class ModIconColours
    {
        /// <summary>
        /// A list of common colour schemes for <see cref="TournamentFumoModIcon"/>.
        /// </summary>
        public static List<KeyValuePair<string, ModColour>> ColourMap = new List<KeyValuePair<string, ModColour>>
        {
            new KeyValuePair<string, ModColour>("NM", new ModColour
            {
                MainBackground = Color4Extensions.FromHex("#535353"),
                BadgeBackground = Color4Extensions.FromHex("#828282"),
                BadgeText = Color4Extensions.FromHex("#FFE675"),
                MainIcon = Color4Extensions.FromHex("#FFE675")
            }),
            new KeyValuePair<string, ModColour>("FM", new ModColour
            {
                MainBackground = Color4Extensions.FromHex("#24EECB"),
                BadgeBackground = Color4Extensions.FromHex("#23A68F"),
                BadgeText = Color4.White,
                MainIcon = Color4.White
            }),
            new KeyValuePair<string, ModColour>("HR", new ModColour
            {
                MainBackground = Color4Extensions.FromHex("#F76363"),
                BadgeBackground = Color4Extensions.FromHex("#6C3335"),
                BadgeText = Color4Extensions.FromHex("#F97777"),
                MainIcon = Color4Extensions.FromHex("#3D4247")
            }),
            new KeyValuePair<string, ModColour>("DT", new ModColour
            {
                MainBackground = Color4Extensions.FromHex("#66CCFF"),
                BadgeBackground = Color4Extensions.FromHex("#25445E"),
                BadgeText = Color4Extensions.FromHex("#5BE6C8"),
                MainIcon = Color4Extensions.FromHex("#535353")
            }),
            new KeyValuePair<string, ModColour>("HD", new ModColour
            {
                MainBackground = Color4Extensions.FromHex("#FDC300"),
                BadgeBackground = Color4Extensions.FromHex("#EBC547"),
                BadgeText = Color4Extensions.FromHex("#666666"),
                MainIcon = Color4Extensions.FromHex("#535353")
            }),
        };
    }
}

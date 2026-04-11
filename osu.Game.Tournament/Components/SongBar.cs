// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Models;
using osu.Game.Online.API;
using osu.Game.Rulesets;
using osu.Game.Screens.Menu;
using osu.Game.Tournament.Models;
using osu.Game.Utils;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    public partial class SongBar : CompositeDrawable
    {
        private IBeatmapInfo? beatmap;
        private IBeatmapInfo? oldBeatmap;
        private bool beatmapChanged;

        private TournamentBeatmapPanel beatmapPanel = null!;

        private DiffPiece diffPiece1 = null!;
        private DiffPiece diffPiece2 = null!;
        private DiffPiece diffPiece3 = null!;
        private DiffPiece diffPiece4 = null!;

        public const float HEIGHT = 145 / 2f;
        private const int slot_text_duration = 250;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        public IBeatmapInfo? Beatmap
        {
            set
            {
                if (beatmap == value)
                    return;

                oldBeatmap = beatmap;
                beatmap = value;

                beatmapChanged = true;

                refreshContent();
            }
        }

        private LegacyMods mods;

        public LegacyMods Mods
        {
            get => mods;
            set
            {
                mods = value;
                lazerMods.Clear();
                refreshContent();
            }
        }

        private List<APIMod> lazerMods = new List<APIMod>();

        public List<APIMod> LazerMods
        {
            get => lazerMods;
            set
            {
                mods = 0;
                lazerMods = value;
                refreshContent();
            }
        }

        // private readonly Dictionary<int, string> poolSlotsText = new Dictionary<int, string>();
        /// <summary>
        /// Dict:
        /// - key: tuple(onlineId, Md5)
        /// - value: slot name
        /// </summary>
        private readonly Dictionary<(int, string), string> poolSlotsText = new Dictionary<(int, string), string>();

        // TODO: use SlotName in ladder (new addition) instead of this
        public List<RoundBeatmap> Pool
        {
            set
            {
                poolSlotsText.Clear();

                string? currentMods = null;
                int modSlotIndex = 1;

                foreach (var b in value)
                {
                    if (currentMods != b.Mods)
                    {
                        currentMods = b.Mods;
                        modSlotIndex = 1;
                    }

                    poolSlotsText[(b.ID, b.MD5)] = currentMods == "TB" ? currentMods : $"{currentMods}{modSlotIndex}";
                    modSlotIndex++;
                }

                refreshContent();
            }
        }

        private FillFlowContainer flow = null!;
        private FillFlowContainer<FillFlowContainer<GlowingSpriteText>> poolSlots = null!;

        private bool expanded;

        public bool Expanded
        {
            get => expanded;
            set
            {
                expanded = value;
                flow.Direction = expanded ? FillDirection.Full : FillDirection.Vertical;
            }
        }

        // Todo: This is a hack for https://github.com/ppy/osu-framework/issues/3617 since this container is at the very edge of the screen and potentially initially masked away.
        protected override bool ComputeIsMaskedAway(RectangleF maskingBounds) => false;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Masking = true;
            CornerRadius = 5;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = colours.Gray3,
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.4f,
                },
                flow = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Full,
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,

                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = HEIGHT,
                            Width = 0.5f,
                            Anchor = Anchor.BottomRight,
                            Origin = Anchor.BottomRight,

                            Children = new Drawable[]
                            {
                                new GridContainer
                                {
                                    RelativeSizeAxes = Axes.Both,

                                    ColumnDimensions = new[]
                                    {
                                        new Dimension(),
                                        new Dimension(),
                                        new Dimension(GridSizeMode.AutoSize),
                                        new Dimension(GridSizeMode.Absolute, size: HEIGHT)
                                    },

                                    Content = new[]
                                    {
                                        new Drawable[]
                                        {
                                            new FillFlowContainer
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                Direction = FillDirection.Vertical,
                                                Children = new Drawable[]
                                                {
                                                    diffPiece1 = new DiffPiece(),
                                                    diffPiece2 = new DiffPiece()
                                                }
                                            },
                                            new FillFlowContainer
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                Direction = FillDirection.Vertical,
                                                Children = new Drawable[]
                                                {
                                                    diffPiece3 = new DiffPiece(),
                                                    diffPiece4 = new DiffPiece(),
                                                }
                                            },
                                            poolSlots = new FillFlowContainer<FillFlowContainer<GlowingSpriteText>>
                                            {
                                                AutoSizeAxes = Axes.Both,
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                Direction = FillDirection.Vertical,
                                                Margin = new MarginPadding { Horizontal = 16 },
                                            },
                                            new Container
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Children = new Drawable[]
                                                {
                                                    new Box
                                                    {
                                                        Colour = Color4.Black,
                                                        RelativeSizeAxes = Axes.Both,
                                                        Alpha = 0.1f,
                                                    },
                                                    new OsuLogo
                                                    {
                                                        Triangles = false,
                                                        Scale = new Vector2(0.08f),
                                                        Margin = new MarginPadding(50),
                                                        X = -10,
                                                        Anchor = Anchor.CentreRight,
                                                        Origin = Anchor.CentreRight,
                                                    },
                                                }
                                            },
                                        },
                                    }
                                }
                            }
                        },
                        beatmapPanel = new TournamentBeatmapPanel(beatmap)
                        {
                            RelativeSizeAxes = Axes.X,
                            Width = 0.5f,
                            Height = HEIGHT,
                            Anchor = Anchor.BottomRight,
                            Origin = Anchor.BottomRight,
                        }
                    },
                }
            };

            Expanded = true;
        }

        private void refreshContent()
        {
            flow.Remove(beatmapPanel, true);

            beatmap ??= new BeatmapInfo
            {
                Metadata = new BeatmapMetadata
                {
                    Artist = "unknown",
                    Title = "no beatmap selected",
                    Author = new RealmUser { Username = "unknown" },
                },
                DifficultyName = "unknown",
                BeatmapSet = new BeatmapSetInfo(),
                StarRating = 0,
                Difficulty = new BeatmapDifficulty
                {
                    CircleSize = 0,
                    DrainRate = 0,
                    OverallDifficulty = 0,
                    ApproachRate = 0,
                },
            };

            flow.Add(beatmapPanel = new TournamentBeatmapPanel(beatmap)
            {
                RelativeSizeAxes = Axes.X,
                Width = 0.5f,
                Height = HEIGHT,
                Anchor = Anchor.BottomRight,
                Origin = Anchor.BottomRight,
            });

            var rulesetInstance = ruleset.Value.CreateInstance();

            var convertedMods = rulesetInstance.ConvertFromLegacyMods(mods).ToList();
            var adjustedDifficulty = rulesetInstance.GetAdjustedDisplayDifficulty(beatmap, convertedMods);

            double rate = ModUtils.CalculateRateWithMods(convertedMods);
            double bpm = FormatUtils.RoundBPM(beatmap.BPM, rate);
            double length = beatmap.Length / rate;

            string srExtra = "";

            if ((mods & LegacyMods.HardRock) > 0 || lazerMods.Any(mod => mod.Acronym is "HR"))
            {
                srExtra = "*";
            }

            var lazerDoubleTime = lazerMods.FirstOrDefault(mod => mod.Acronym is "DT");

            if (lazerDoubleTime != null)
            {
                // not so temporary local calculation (taken from straight above)
                double speedChange = 1.5;

                var speedSetting = lazerDoubleTime.Settings.FirstOrDefault(s => s.Key == "speed_change");
                if (speedSetting.Value != null)
                    double.TryParse(speedSetting.Value.ToString(), out speedChange);

                bpm *= speedChange;
                length /= speedChange;
                srExtra = "*";
            }

            (string heading, string content)[] stats;

            switch (ruleset.Value.OnlineID)
            {
                default:
                    stats = new (string heading, string content)[]
                    {
                        ("CS", $"{adjustedDifficulty.CircleSize:0.#}"),
                        ("AR", $"{adjustedDifficulty.ApproachRate:0.#}"),
                        ("OD", $"{adjustedDifficulty.OverallDifficulty:0.#}"),
                    };
                    break;

                case 1:
                case 3:
                    stats = new (string heading, string content)[]
                    {
                        ("OD", $"{adjustedDifficulty.OverallDifficulty:0.#}"),
                        ("HP", $"{adjustedDifficulty.DrainRate:0.#}")
                    };
                    break;

                case 2:
                    stats = new (string heading, string content)[]
                    {
                        ("CS", $"{adjustedDifficulty.CircleSize:0.#}"),
                        ("AR", $"{adjustedDifficulty.ApproachRate:0.#}"),
                    };
                    break;
            }

            diffPiece1.Stats = stats;
            diffPiece2.Stats = [("Star Rating", $"{beatmap.StarRating.FormatStarRating()}{srExtra}")];
            diffPiece3.Stats = [("Length", length.ToFormattedDuration().ToString())];
            diffPiece4.Stats = [("BPM", $"{bpm:0.#}")];

            // update mod slots display
            updateSlotDisplay();
        }

        private void updateSlotDisplay()
        {
            // a little workaround to handle having beatmap being updated (to update background image IPC) without actually changing beaatmap
            {
                bool isSameBeatmap = false;

                // Check if the beatmap has a valid OnlineID and if it matches the old beatmap's OnlineID
                if (beatmap?.OnlineID > 0 &&
                    beatmap?.OnlineID != null &&
                    beatmap?.OnlineID == oldBeatmap?.OnlineID)
                {
                    isSameBeatmap = true;
                }

                // Check if the beatmap has the same MD5 hash as the old beatmap and ensure the MD5 hash is not null or empty
                else if (beatmap?.MD5Hash == oldBeatmap?.MD5Hash &&
                         !string.IsNullOrEmpty(beatmap?.MD5Hash))
                {
                    isSameBeatmap = true;
                }

                if (isSameBeatmap)
                {
                    beatmapChanged = false;
                    return;
                }
            }

            poolSlots.Clear();

            int newlineThreshold = 0;

            if (poolSlotsText.Count > 2)
            {
                newlineThreshold = Math.Max(4, poolSlotsText.Count / 2);
            }

            FillFlowContainer<GlowingSpriteText>? currentFlow = null;

            foreach (var key in poolSlotsText.Keys)
            {
                (int mapId, string md5) = key;

                if (currentFlow == null || currentFlow.Children.Count > newlineThreshold)
                {
                    currentFlow = new FillFlowContainer<GlowingSpriteText>
                    {
                        AutoSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(10, 0)
                    };
                    poolSlots.Add(currentFlow);
                }

                var glowText = new GlowingSpriteText
                {
                    Text = poolSlotsText[key],
                    Font = OsuFont.GetFont(weight: FontWeight.SemiBold),
                    GlowColour = Color4Extensions.FromHex("#FFFFFF").Opacity(0.2f),
                };
                currentFlow.Add(glowText);

                if ((mapId > 0 && mapId == beatmap?.OnlineID) || md5 == beatmap?.MD5Hash)
                {
                    glowText.TransformTo(nameof(glowText.GlowColour), (ColourInfo)Color4Extensions.FromHex("#fff8e5"), slot_text_duration);
                    glowText.TransformTo(nameof(glowText.Colour), (ColourInfo)Color4Extensions.FromHex("#faf79f"), slot_text_duration);
                }

                // move glow to new slot
                if (beatmapChanged)
                {
                    if ((mapId > 0 && mapId == oldBeatmap?.OnlineID) || md5 == oldBeatmap?.MD5Hash)
                    {
                        glowText.GlowColour = Color4Extensions.FromHex("#fff8e5");
                        glowText.Colour = Color4Extensions.FromHex("#faf79f");
                        glowText.TransformTo(nameof(glowText.GlowColour), (ColourInfo)Color4Extensions.FromHex("#FFFFFF").Opacity(0.2f), slot_text_duration);
                        glowText.TransformTo(nameof(glowText.Colour), (ColourInfo)Color4Extensions.FromHex("#FFFFFF"), slot_text_duration);
                    }
                }
            }

            beatmapChanged = false;
        }

        public partial class DiffPiece : TextFlowContainer
        {
            private (string heading, string content)[] statsParts = [];

            public (string heading, string content)[] Stats
            {
                get => statsParts;
                set
                {
                    if (statsParts == value)
                        return;

                    statsParts = value;
                    redrawText();
                }
            }

            public DiffPiece()
            {
                Margin = new MarginPadding { Horizontal = 15, Vertical = 1 };
                AutoSizeAxes = Axes.Both;
            }

            private static void cp(SpriteText s, bool bold)
            {
                s.Font = OsuFont.Torus.With(weight: bold ? FontWeight.Bold : FontWeight.Regular, size: 15);
            }

            private void redrawText()
            {
                Clear();

                for (int i = 0; i < statsParts.Length; i++)
                {
                    (string heading, string content) = statsParts[i];

                    if (i > 0)
                    {
                        AddText(" / ", s =>
                        {
                            cp(s, false);
                            s.Spacing = new Vector2(-2, 0);
                        });
                    }

                    AddText(new TournamentSpriteText { Text = heading }, s => cp(s, false));
                    AddText(" ", s => cp(s, false));
                    AddText(new TournamentSpriteText { Text = content }, s => cp(s, true));
                }
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                redrawText();
            }
        }
    }
}

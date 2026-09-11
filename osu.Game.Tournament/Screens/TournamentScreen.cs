// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Game.Graphics.Cursor;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Screens
{
    /// <summary>
    /// A generalized "screen" with <see cref="LadderInfo"/> and <see cref="TournamentSceneManager"/> injected,
    /// and also with <see cref="OsuContextMenuContainer"/> and <see cref="PopoverContainer"/> support.
    /// </summary>
    /// <remarks>
    /// Don't use <see cref="CompositeDrawable.InternalChildren"/> to directly mutate the component
    /// since this breaks the container support above. Use <see cref="Container{T}.Children"/> instead.
    /// </remarks>
    public abstract partial class TournamentScreen : Container
    {
        public const double FADE_DELAY = 200;

        public bool HadBeenSelected { get; protected set; }

        [Resolved]
        protected LadderInfo LadderInfo { get; private set; } = null!;

        [Resolved]
        protected TournamentSceneManager? SceneManager { get; private set; }

        private readonly Container content;
        protected override Container<Drawable> Content => content;

        protected TournamentScreen()
        {
            RelativeSizeAxes = Axes.Both;

            FillMode = FillMode.Fit;
            FillAspectRatio = 16 / 9f;

            InternalChildren = new Drawable[]
            {
                new OsuContextMenuContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = new PopoverContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = content = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                    },
                },
            };
        }

        /// <summary>
        /// Called when the screen is selected the first time in this session.
        /// </summary>
        protected virtual void OnFirstSelected()
        {
        }

        public void ResetSelectStatus() => HadBeenSelected = false;

        public override void Hide() => this.FadeOut(FADE_DELAY);

        public override void Show()
        {
            this.FadeIn(FADE_DELAY);
            if (HadBeenSelected) return;

            HadBeenSelected = true;
            OnFirstSelected();
        }
    }
}

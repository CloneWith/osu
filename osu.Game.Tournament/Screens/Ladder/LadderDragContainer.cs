// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input.Events;
using osuTK;

namespace osu.Game.Tournament.Screens.Ladder
{
    public partial class LadderDragContainer : Container
    {
        protected override bool OnDragStart(DragStartEvent e) => true;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

         public Action? TargetChanged;
        public Action? ScaleChanged;

        private Vector2 targetPosition;

        public Vector2 TargetPosition
        {
            get => targetPosition;
            private set
            {
                if (targetPosition == value)
                    return;

                targetPosition = value;
                TargetChanged?.Invoke();
            }
        }

        private float targetScale = 1;

        public float TargetScale
        {
            get => targetScale;
            private set
            {
                if (targetScale == value)
                    return;

                targetScale = value;
                ScaleChanged?.Invoke();
            }
        }

        protected override bool ComputeIsMaskedAway(RectangleF maskingBounds) => false;

        public override bool UpdateSubTreeMasking() => false;

        protected override void OnDrag(DragEvent e)
        {
            this.MoveTo(TargetPosition += e.Delta, 1000, Easing.OutQuint);
        }

        public void SetPosition(Vector2 position, float duration = 1000, Easing easing = Easing.OutQuint)
        {
            this.MoveTo(TargetPosition = position, duration, easing);
        }

        public void AdjustPosition(Vector2 delta, float duration = 1000, Easing easing = Easing.OutQuint)
        {
            this.MoveTo(TargetPosition += delta, duration, easing);
        }

        public void SetScale(float newScale, float duration = 1000, Easing easing = Easing.OutQuint)
        {
            newScale = Math.Clamp(newScale, min_scale, max_scale);

            SetPosition(TargetPosition - Parent!.DrawSize / 2f * (newScale - TargetScale));
            this.ScaleTo(TargetScale = newScale, duration, easing);
        }

        public void AdjustScale(float scaleDelta, float duration = 1000, Easing easing = Easing.OutQuint)
        {
            SetScale(TargetScale + scaleDelta, duration, easing);
        }

        private const float min_scale = 0.3f;
        private const float max_scale = 1.4f;

        protected override bool OnScroll(ScrollEvent e)
        {
            float newScale = Math.Clamp(TargetScale + e.ScrollDelta.Y / 15 * TargetScale, min_scale, max_scale);

            this.MoveTo(TargetPosition -= e.MousePosition * (newScale - TargetScale), 1000, Easing.OutQuint);
            this.ScaleTo(TargetScale = newScale, 1000, Easing.OutQuint);

            return true;
        }
    }
}

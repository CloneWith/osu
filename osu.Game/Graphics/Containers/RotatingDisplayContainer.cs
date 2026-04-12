// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Graphics.Containers
{
    /// <summary>
    /// A container that supports rotating display of multiple layers.
    /// </summary>
    public partial class RotatingDisplayContainer : Container
    {
        /// <summary>
        /// A list of <see cref="Drawable"/>, storing layers of content to show at different time.
        /// </summary>
        protected readonly List<Drawable> Layers = new List<Drawable>();

        /// <summary>
        /// The index of the layer currently shown.
        /// </summary>
        public int CurrentIndex { get; protected set; } = -1;

        /// <summary>
        /// Whether the rotating sequence is playing on progress.
        /// </summary>
        public bool Playing { get; protected set; }

        /// <summary>
        /// How long a layer should keep showing.
        /// </summary>
        public double DisplayLength = 5000;

        /// <summary>
        /// How long the easing in and out animations should take.
        /// </summary>
        /// <remarks>Only works with <see cref="CrossAnimation"/> set to false.</remarks>
        public double TransformLength = 500;

        /// <summary>
        /// The extra duration to wait after the easing out animation, and before the easing in one.
        /// </summary>
        public double Interval = 0;

        /// <summary>
        /// Whether to wait for the easing out animation to complete before the easing in animation begins.
        /// <c>true</c> if not, otherwise <c>false</c>.
        /// </summary>
        public bool CrossAnimation = false;

        /// <summary>
        /// Whether layers should be shown in random or sequential order.
        /// </summary>
        public bool Random = false;

        /// <summary>
        /// Whether layers should be shown repeatedly.
        /// If not, the container would show the last layer at the end.
        /// </summary>
        /// <remarks>This only works with <see cref="Random"/> set to <c>false</c>.</remarks>
        public bool Looped = true;

        /// <summary>
        /// The easing in animation of a layer.
        /// </summary>
        public Action<Drawable, double> InAnimation = (d, len) => d.FadeIn(len, Easing.OutQuint);

        /// <summary>
        /// The easing out animation of a layer.
        /// </summary>
        public Action<Drawable, double> OutAnimation = (d, len) => d.FadeOut(len, Easing.OutQuint);

        /// <summary>
        /// The event triggered when the current display sequence is completed or stopped manually.
        /// </summary>
        /// <remarks>This event could be only triggered with <see cref="Looped"/> set to <c>false</c>.</remarks>
        public event Action? OnComplete;

        private readonly Random random = new Random();

        private double realTransformLength => CrossAnimation ? 0 : TransformLength;
        private DateTimeOffset nextTransformTime = DateTimeOffset.Now;

        /// <summary>
        /// Add a new layer to the display sequence.
        /// </summary>
        /// <param name="drawable">The <see cref="Drawable"/> to be added.</param>
        /// <param name="index">The index to insert the layer at.</param>
        /// <param name="showInstantly">Whether to immediately switch to the newly added layer.</param>
        public void AddLayer(Drawable drawable, int? index = null, bool showInstantly = false)
        {
            if (index != null)
                Layers.Insert(index.Value, drawable);
            else
                Layers.Add(drawable);

            drawable.FadeOut();
            AddInternal(drawable);

            if (showInstantly)
            {
                ShowIndex(index ?? Layers.Count - 1);
            }
        }

        /// <summary>
        /// Add a set of <see cref="Drawable"/>s in order to the display sequence.
        /// </summary>
        /// <param name="drawables">The <see cref="Drawable"/>s to he added.</param>
        public void AddLayers(IEnumerable<Drawable> drawables)
            => drawables.ForEach(d => AddLayer(d));

        /// <summary>
        /// Show the next layer if possible.
        /// </summary>
        public void ShowNext()
        {
            if (CurrentIndex == Layers.Count - 1)
            {
                if (!Looped)
                    Pause();
                else
                    ShowIndex(0);

                return;
            }

            ShowIndex(Random ? random.Next(0, Layers.Count) : CurrentIndex + 1);
        }

        /// <summary>
        /// Show a layer with specified index.
        /// </summary>
        /// <param name="index">The index of the layer to show.</param>
        public void ShowIndex(int index)
        {
            int targetIndex = Math.Clamp(index, 0, Layers.Count - 1);

            if (CurrentIndex >= 0 && CurrentIndex < Layers.Count)
                OutAnimation.Invoke(Layers[CurrentIndex], TransformLength);
            Scheduler.AddDelayed(() => InAnimation.Invoke(Layers[targetIndex], TransformLength), realTransformLength + Interval);

            CurrentIndex = targetIndex;
        }

        /// <summary>
        /// Reset the <see cref="OnComplete"/> event trigger.
        /// </summary>
        public void ResetCompleteTrigger() => OnComplete = null;

        /// <summary>
        /// Start the rotating display sequence.
        /// </summary>
        /// <param name="startFromBeginning">whether to start from the first layer.</param>
        /// <remarks>This will always show the next layer when called.</remarks>
        public void Start(bool startFromBeginning = false)
        {
            if (Layers.Count == 0)
                return;

            if (startFromBeginning)
            {
                if (CurrentIndex >= 0 && CurrentIndex < Layers.Count)
                    OutAnimation.Invoke(Layers[CurrentIndex], TransformLength);
                CurrentIndex = -1;
            }

            Playing = true;
        }

        /// <summary>
        /// Stop the rotating display sequence.
        /// </summary>
        public void Pause()
        {
            Playing = false;
            OnComplete?.Invoke();
        }

        protected override void Update()
        {
            base.Update();

            if (Playing && nextTransformTime < DateTimeOffset.Now)
            {
                nextTransformTime = DateTimeOffset.Now.AddMilliseconds(DisplayLength + realTransformLength + Interval);
                ShowNext();
            }
        }
    }
}

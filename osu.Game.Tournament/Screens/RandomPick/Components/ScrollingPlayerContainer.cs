// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Tournament.Models;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.RandomPick.Components
{
    /// <summary>
    /// The strip of players which scrolls across the middle of the screen until it is stopped, at which point
    /// whoever sits closest to the center is the one drawn.
    /// </summary>
    public partial class ScrollingPlayerContainer : Container
    {
        public event Action? OnScrollStarted;
        public event Action<TournamentUser>? OnSelected;

        private readonly List<TournamentUser> availablePlayers = new List<TournamentUser>();

        private readonly Container tracker;

#pragma warning disable 649
        // set via reflection.
        private float speed;
#pragma warning restore 649

        private int expiredCount;

        private float offset;
        private float timeOffset;
        private float leftPos => offset + timeOffset + expiredCount * ScrollingPlayer.WIDTH;

        private double lastTime;

        private ScheduledDelegate? delayedStateChangeDelegate;

        public ScrollingPlayerContainer()
        {
            AutoSizeAxes = Axes.Y;

            Children = new Drawable[]
            {
                tracker = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    AutoSizeAxes = Axes.Both,
                    Colour = OsuColour.Gray(0.33f),

                    Masking = true,
                    CornerRadius = 10f,
                    Alpha = 0,

                    Children = new[]
                    {
                        new Box
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.BottomCentre,
                            Size = new Vector2(2, 55),

                            Colour = ColourInfo.GradientVertical(Color4.Transparent, Color4.White)
                        },
                        new Box
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.TopCentre,
                            Size = new Vector2(2, 55),

                            Colour = ColourInfo.GradientVertical(Color4.White, Color4.Transparent)
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Whether the strip is moving, i.e. a draw is under way or coming to a stop.
        /// </summary>
        public bool Scrolling => scrollState is ScrollState.Scrolling or ScrollState.Stopping;

        /// <summary>
        /// How many players the strip is currently cycling through.
        /// </summary>
        public int PlayerCount => availablePlayers.Count;

        private ScrollState scrollState;

        private void setScrollState(ScrollState newState)
        {
            if (scrollState == newState)
                return;

            delayedStateChangeDelegate?.Cancel();

            switch (scrollState = newState)
            {
                case ScrollState.Scrolling:
                    resetSelected();

                    OnScrollStarted?.Invoke();

                    speedTo(1000f, 200);
                    tracker.FadeOut(100);
                    break;

                case ScrollState.Stopping:
                    speedTo(0f, 2000);
                    tracker.FadeIn(200);

                    delayedStateChangeDelegate = Scheduler.AddDelayed(() => setScrollState(ScrollState.Stopped), 2300);
                    break;

                case ScrollState.Stopped:
                    // Find closest to center
                    if (!Children.Any())
                        break;

                    ScrollingPlayer? closest = null;

                    foreach (var c in Children)
                    {
                        if (c is not ScrollingPlayer spc)
                            continue;

                        if (closest == null)
                        {
                            closest = spc;
                            continue;
                        }

                        float o = Math.Abs(c.Position.X + c.DrawWidth / 2f - DrawWidth / 2f);
                        float lastOffset = Math.Abs(closest.Position.X + closest.DrawWidth / 2f - DrawWidth / 2f);

                        if (o < lastOffset)
                            closest = spc;
                    }

                    Debug.Assert(closest != null, "closest != null");

                    offset += DrawWidth / 2f - (closest.Position.X + closest.DrawWidth / 2f);

                    ScrollingPlayer sp = closest;

                    sp.Selected = true;
                    OnSelected?.Invoke(sp.Player.AsNonNull());

                    delayedStateChangeDelegate = Scheduler.AddDelayed(() => setScrollState(ScrollState.Idle), 10000);
                    break;

                case ScrollState.Idle:
                    resetSelected();

                    OnScrollStarted?.Invoke();

                    speedTo(40f, 200);
                    tracker.FadeOut(100);
                    break;
            }
        }

        /// <summary>
        /// Show <paramref name="players"/> in the strip, replacing what was there.
        /// </summary>
        public void SetPlayers(IEnumerable<TournamentUser> players)
        {
            Children.OfType<ScrollingPlayer>().ToList().ForEach(p => p.Expire());

            availablePlayers.Clear();
            availablePlayers.AddRange(players);

            setScrollState(ScrollState.Idle);
        }

        /// <summary>
        /// Take the drawn player out of the strip, once the screen has found them a place.
        /// </summary>
        public void RemovePlayer(TournamentUser player)
        {
            availablePlayers.Remove(player);

            foreach (var c in Children)
            {
                if (c is ScrollingPlayer sp && sp.Player == player)
                {
                    sp.FadeOut(200);
                    sp.Expire();
                }
            }
        }

        public void StartScrolling()
        {
            if (availablePlayers.Count == 0)
                return;

            setScrollState(ScrollState.Scrolling);
        }

        /// <summary>
        /// Abandon a draw in progress without landing it. The strip keeps cycling and nobody is drawn.
        /// </summary>
        public void CancelScrolling() => setScrollState(ScrollState.Idle);

        public void StopScrolling()
        {
            if (availablePlayers.Count == 0)
                return;

            switch (scrollState)
            {
                case ScrollState.Stopped:
                case ScrollState.Idle:
                    return;
            }

            setScrollState(ScrollState.Stopping);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            setScrollState(ScrollState.Idle);
        }

        protected override void UpdateAfterChildren()
        {
            timeOffset -= (float)(Time.Current - lastTime) / 1000 * speed;
            lastTime = Time.Current;

            if (availablePlayers.Count > 0)
            {
                // Fill more than required to account for transformation + scrolling speed
                while (Children.Count(c => c is ScrollingPlayer) < DrawWidth * 2 / ScrollingPlayer.WIDTH)
                    addPlayers();
            }

            float pos = leftPos;

            foreach (var c in Children)
            {
                if (c is not ScrollingPlayer)
                    continue;

                if (c.Position.X + c.DrawWidth < 0)
                {
                    c.ClearTransforms();
                    c.Expire();
                    expiredCount++;
                }
                else
                {
                    c.MoveToX(pos, 100);
                    c.FadeTo(1.0f - Math.Abs(pos - DrawWidth / 2f) / (DrawWidth / 2.5f), 100);
                }

                pos += ScrollingPlayer.WIDTH;
            }
        }

        private void addPlayers()
        {
            foreach (TournamentUser player in availablePlayers)
            {
                Add(new ScrollingPlayer(player)
                {
                    X = leftPos + DrawWidth
                });
            }
        }

        private void resetSelected()
        {
            foreach (var c in Children)
            {
                if (c is ScrollingPlayer sp)
                    sp.Selected = false;
            }
        }

        private void speedTo(float value, double duration = 0, Easing easing = Easing.None) =>
            this.TransformTo(nameof(speed), value, duration, easing);

        protected enum ScrollState
        {
            None,
            Idle,
            Stopping,
            Stopped,
            Scrolling
        }
    }
}

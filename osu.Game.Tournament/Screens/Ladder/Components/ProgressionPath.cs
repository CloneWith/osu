// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Lines;
using osuTK;

namespace osu.Game.Tournament.Screens.Ladder.Components
{
    public partial class ProgressionPath : Path
    {
        public DrawableTournamentMatch Source { get; }
        public DrawableTournamentMatch Destination { get; }

        public ProgressionPath(DrawableTournamentMatch source, DrawableTournamentMatch destination)
        {
            Source = source;
            Destination = destination;

            PathRadius = 3;
            BypassAutoSizeAxes = Axes.Both;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            static Vector2 getCenteredVector(Vector2 top, Vector2 bottom) => new Vector2(top.X, top.Y + (bottom.Y - top.Y) / 2);

            var q1 = Source.ScreenSpaceDrawQuad;
            var q2 = Destination.ScreenSpaceDrawQuad;

            float padding = q1.Width / 20;

            bool progressionToRight = q2.Centre.X > q1.Centre.X;

            // Check for vertical alignment - are they roughly stacked?
            // Calculate horizontal overlap between the matches
            float minRightX = Math.Min(q1.TopRight.X, q2.TopRight.X);
            float maxLeftX = Math.Max(q1.TopLeft.X, q2.TopLeft.X);
            float horizontalOverlap = minRightX - maxLeftX;

            bool isVerticallyStacked = horizontalOverlap > q1.Width * 0.5; // Significant overlap

            Vector2 sourcePoint, destPoint;

            if (isVerticallyStacked)
            {
                // For vertically stacked matches, use the same side for both
                if (progressionToRight)
                {
                    sourcePoint = getCenteredVector(q1.TopRight, q1.BottomRight) + new Vector2(padding, 0);
                    destPoint = getCenteredVector(q2.TopRight, q2.BottomRight) + new Vector2(padding, 0);
                }
                else
                {
                    sourcePoint = getCenteredVector(q1.TopLeft, q1.BottomLeft) - new Vector2(padding, 0);
                    destPoint = getCenteredVector(q2.TopLeft, q2.BottomLeft) - new Vector2(padding, 0);
                }
            }
            else
            {
                // Normal case, source exit depends on relative position
                if (progressionToRight)
                {
                    sourcePoint = getCenteredVector(q1.TopRight, q1.BottomRight) + new Vector2(padding, 0);
                    destPoint = getCenteredVector(q2.TopLeft, q2.BottomLeft) - new Vector2(padding, 0);
                }
                else
                {
                    sourcePoint = getCenteredVector(q1.TopLeft, q1.BottomLeft) - new Vector2(padding, 0);
                    destPoint = getCenteredVector(q2.TopRight, q2.BottomRight) + new Vector2(padding, 0);
                }
            }

            List<Vector2> pathPoints = new List<Vector2> { sourcePoint };

            // use a U-shaped path for vertically stacked matches
            if (isVerticallyStacked)
            {
                pathPoints.Add(new Vector2(destPoint.X + (progressionToRight ? padding : -padding), sourcePoint.Y));
                pathPoints.Add(new Vector2(destPoint.X + (progressionToRight ? padding : -padding), destPoint.Y));
            }
            else // regular path
            {
                float horizontalSpace = Math.Abs(destPoint.X - sourcePoint.X);

                if (horizontalSpace >= 2 * padding)
                {
                    float elbowX = progressionToRight
                                       ? sourcePoint.X + padding
                                       : sourcePoint.X - padding;

                    pathPoints.Add(new Vector2(elbowX, sourcePoint.Y));
                    pathPoints.Add(new Vector2(elbowX, destPoint.Y));
                }
                else
                {
                    // Limited horizontal space - use a mid-point for the vertical segment
                    float midX = (sourcePoint.X + destPoint.X) / 2;

                    pathPoints.Add(new Vector2(midX, sourcePoint.Y));
                    pathPoints.Add(new Vector2(midX, destPoint.Y));
                }
            }

            pathPoints.Add(destPoint);

            var points = pathPoints.ToArray();

            float minX = points.Min(p => p.X);
            float minY = points.Min(p => p.Y);

            var topLeft = new Vector2(minX, minY);

            OriginPosition = new Vector2(PathRadius);
            Position = Parent!.ToLocalSpace(topLeft);
            Vertices = points.Select(p => Parent!.ToLocalSpace(p) - Parent!.ToLocalSpace(topLeft)).ToList();
        }
    }
}

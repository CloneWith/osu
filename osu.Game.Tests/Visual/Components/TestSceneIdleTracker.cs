// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Configuration;
using osu.Game.Input;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Components
{
    [TestFixture]
    public class TestSceneIdleTracker : OsuManualInputManagerTestScene
    {
        private IdleTrackingBox box1;
        private IdleTrackingBox box2;
        private IdleTrackingBox box3;
        private IdleTrackingBox box4;

        private IdleTrackingBox[] boxes;

        private SessionStatics sessionStatics;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            sessionStatics = new SessionStatics();
            InputManager.MoveMouseTo(Vector2.Zero);

            Children = boxes = new[]
            {
                box1 = new IdleTrackingBox(2000, sessionStatics)
                {
                    Name = "TopLeft",
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Red,
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                },
                box2 = new IdleTrackingBox(4000, sessionStatics)
                {
                    Name = "TopRight",
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Green,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                },
                box3 = new IdleTrackingBox(6000, sessionStatics)
                {
                    Name = "BottomLeft",
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Blue,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                },
                box4 = new IdleTrackingBox(8000, sessionStatics)
                {
                    Name = "BottomRight",
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Orange,
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                },
            };
        });

        [Test]
        public void TestNudge()
        {
            AddStep("move to top left", () => InputManager.MoveMouseTo(box1));

            waitForAllIdle();

            AddStep("nudge mouse", () => InputManager.MoveMouseTo(box1.ScreenSpaceDrawQuad.Centre + new Vector2(1)));

            checkIdleStatus(1, false);
            checkIdleStatus(2, true);
            checkIdleStatus(3, true);
            checkIdleStatus(4, true);
        }

        [Test]
        public void TestMovement()
        {
            checkIdleStatus(1, false);
            checkIdleStatus(2, false);
            checkIdleStatus(3, false);
            checkIdleStatus(4, false);

            waitForAllIdle();

            AddStep("move to top right", () => InputManager.MoveMouseTo(box2));

            checkIdleStatus(1, true);
            checkIdleStatus(2, false);
            checkIdleStatus(3, true);
            checkIdleStatus(4, true);

            AddStep("move to bottom left", () => InputManager.MoveMouseTo(box3));
            AddStep("move to bottom right", () => InputManager.MoveMouseTo(box4));

            checkIdleStatus(1, true);
            checkIdleStatus(2, false);
            checkIdleStatus(3, false);
            checkIdleStatus(4, false);

            waitForAllIdle();
        }

        [Test]
        public void TestTimings()
        {
            waitForAllIdle();

            AddStep("move to centre", () => InputManager.MoveMouseTo(Content));

            checkIdleStatus(1, false);
            checkIdleStatus(2, false);
            checkIdleStatus(3, false);
            checkIdleStatus(4, false);

            AddUntilStep("Wait for idle", () => box1.IsIdle);

            checkIdleStatus(1, true);
            checkIdleStatus(2, false);
            checkIdleStatus(3, false);
            checkIdleStatus(4, false);

            AddUntilStep("Wait for idle", () => box2.IsIdle);

            checkIdleStatus(1, true);
            checkIdleStatus(2, true);
            checkIdleStatus(3, false);
            checkIdleStatus(4, false);

            AddUntilStep("Wait for idle", () => box3.IsIdle);

            checkIdleStatus(1, true);
            checkIdleStatus(2, true);
            checkIdleStatus(3, true);
            checkIdleStatus(4, false);

            waitForAllIdle();
        }

        [Test]
        public void TestSessionStaticsReset()
        {
            AddStep("move to top left", () => InputManager.MoveMouseTo(box1));
            AddStep("set session statics", () =>
            {
                sessionStatics.SetValue(Static.LoginOverlayDisplayed, true);
                sessionStatics.SetValue(Static.MutedAudioNotificationShownOnce, true);
                sessionStatics.SetValue(Static.LowBatteryNotificationShownOnce, true);
                sessionStatics.SetValue(Static.LastHoverSoundPlaybackTime, (double?)1d);
            });

            AddStep("move away from box1", () => InputManager.MoveMouseTo(box4));
            AddUntilStep("Wait for idle", () => box1.IsIdle);
            AddAssert("LoginOverlayDisplayed is default", () => sessionStatics.Get<bool>(Static.LoginOverlayDisplayed) == false);
            AddAssert("MutedAudioNotificationShownOnce is default", () => sessionStatics.Get<bool>(Static.MutedAudioNotificationShownOnce) == false);
            AddAssert("LowBatteryNotificationShownOnce is default", () => sessionStatics.Get<bool>(Static.LowBatteryNotificationShownOnce) == false);
            AddAssert("LastHoverSoundPlaybackTime is default", () => sessionStatics.Get<double?>(Static.LastHoverSoundPlaybackTime) == null);
        }

        private void checkIdleStatus(int box, bool expectedIdle)
        {
            AddAssert($"box {box} is {(expectedIdle ? "idle" : "active")}", () => boxes[box - 1].IsIdle == expectedIdle);
        }

        private void waitForAllIdle()
        {
            AddUntilStep("wait for all idle", () => box1.IsIdle && box2.IsIdle && box3.IsIdle && box4.IsIdle);
        }

        private class IdleTrackingBox : CompositeDrawable
        {
            private readonly IdleTracker idleTracker;

            public bool IsIdle => idleTracker.IsIdle.Value;

            public IdleTrackingBox(int timeToIdle, SessionStatics statics)
            {

                Box box;

                Alpha = 0.6f;
                Scale = new Vector2(0.6f);

                InternalChildren = new Drawable[]
                {
                    idleTracker = new GameIdleTracker(timeToIdle),
                    box = new Box
                    {
                        Colour = Color4.White,
                        RelativeSizeAxes = Axes.Both,
                    },
                };

                idleTracker.IsIdle.BindValueChanged(idle =>
                {
                    box.Colour = idle.NewValue ? Color4.White : Color4.Black;
                    if (idle.NewValue) statics.ResetValues();
                }, true);
            }
        }
    }
}

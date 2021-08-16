// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Configuration;
using osu.Framework.Platform;
using osu.Game.Screens.Mvis.SideBar.Tabs;

namespace osu.Game.Configuration
{
    public class MConfigManager : IniConfigManager<MSetting>
    {
        protected override string Filename => "mf.ini";
        public static MConfigManager Instance { get; private set; }

        public MConfigManager(Storage storage)
            : base(storage)
        {
            Instance = this;
        }

        protected override void InitialiseDefaults()
        {
            base.InitialiseDefaults();

            //Other Settings
            SetDefault(MSetting.UseSayobot, true);

            //UI Settings
            SetDefault(MSetting.TrianglesEnabled, true);

            //Gameplay Settings
            SetDefault(MSetting.SamplePlaybackGain, 1f, 0f, 20f);

            //Mvis Settings
            SetDefault(MSetting.MvisContentAlpha, 1f, 0f, 1f);
            SetDefault(MSetting.MvisBgBlur, 0.1f, 0f, 1f);
            SetDefault(MSetting.MvisStoryboardProxy, true);
            SetDefault(MSetting.MvisIdleBgDim, 0.6f, 0f, 1f);
            SetDefault(MSetting.MvisEnableBgTriangles, true);
            SetDefault(MSetting.MvisAdjustMusicWithFreq, false);
            SetDefault(MSetting.MvisMusicSpeed, 1.0, 0.1, 2.0);
            SetDefault(MSetting.MvisEnableNightcoreBeat, false);
            SetDefault(MSetting.MvisInterfaceRed, 0, 0, 255f);
            SetDefault(MSetting.MvisInterfaceGreen, 119f, 0, 255f);
            SetDefault(MSetting.MvisInterfaceBlue, 255f, 0, 255f);
            SetDefault(MSetting.MvisTabControlPosition, TabControlPosition.Right);
            SetDefault(MSetting.MvisCurrentAudioProvider, "a@b");
            SetDefault(MSetting.MvisCurrentFunctionBar, "LegacyBottomBar@Mvis.Plugin.BottomBar");

            //Dance settings
            SetDefault(MSetting.DanceMover, OsuDanceMover.Momentum);
            SetDefault(MSetting.ReplayFramerate, 120f, 15f, 240f, 1f);
            SetDefault(MSetting.SpinnerRadiusStart, 235f, 10f, 350f, 1f);
            SetDefault(MSetting.SpinnerRadiusEnd, 15f, 10f, 250f, 1f);
            SetDefault(MSetting.AngleOffset, 0.45f, 0f, 2f, 0.01f);
            SetDefault(MSetting.JumpMult, 0.6f, 0f, 2f, 0.01f);
            SetDefault(MSetting.NextJumpMult, 0.25f, 0f, 2f, 0.01f);
            SetDefault(MSetting.SkipStackAngles, false);
            SetDefault(MSetting.SliderDance, true);
            SetDefault(MSetting.BorderBounce, true);
            SetDefault(MSetting.PippiSpinner, false);
            SetDefault(MSetting.PippiStream, false);

            //Momentum mover settings
            SetDefault(MSetting.DurationTrigger, 500f, 0f, 5000f, 1f);
            SetDefault(MSetting.DurationMult, 2f, 0f, 50f, 0.1f);
            SetDefault(MSetting.StreamMult, 0.7f, 0f, 50f, 0.1f);
            SetDefault(MSetting.RestrictAngle, 90f, 1f, 180f);
            SetDefault(MSetting.RestrictArea, 40f, 1f, 180f);
            SetDefault(MSetting.StreamRestrict, false);
            SetDefault(MSetting.RestrictInvert, true);

            //Bezier mover settings
            SetDefault(MSetting.BezierAggressiveness, 60f, 1f, 180f);
            SetDefault(MSetting.BezierSliderAggressiveness, 3f, 1f, 20f);

            //Cursor settings
            SetDefault(MSetting.CursorTrailHueOverride, false);
            SetDefault(MSetting.CursorTrailHueShift, false);
            SetDefault(MSetting.CursorTrailHue, .5f, 0.1f, 1.0f, 0.05f);
            SetDefault(MSetting.CursorTrailHueSpeed, 10f, 5f, 50.0f, 1f);
            SetDefault(MSetting.CursorTrailFadeDuration, 500, 100, 5000, 50);
            SetDefault(MSetting.CursorTrailSize, 1f, 1f, 15.0f, 0.5f);
            SetDefault(MSetting.CursorTrailDensity, 1f, 0.5f, 10.0f, 0.5f);
        }
    }

    public enum MSetting
    {
        //Other Settings
        TrianglesEnabled,
        UseSayobot,

        //Mvis Settings
        MvisBgBlur,
        MvisIdleBgDim,
        MvisContentAlpha,
        MvisTabControlPosition,
        MvisEnableBgTriangles,
        MvisStoryboardProxy,
        MvisMusicSpeed,
        MvisAdjustMusicWithFreq,
        MvisEnableNightcoreBeat,
        MvisInterfaceRed,
        MvisInterfaceGreen,
        MvisInterfaceBlue,
        MvisCurrentFunctionBar,
        MvisCurrentAudioProvider,
        SamplePlaybackGain,

        //Dance settings
        DanceMover,
        ReplayFramerate,
        SpinnerRadiusStart,
        SpinnerRadiusEnd,
        AngleOffset,
        JumpMult,
        NextJumpMult,
        BorderBounce,
        SkipStackAngles,
        PippiSpinner,
        PippiStream,
        SliderDance,

        //Momentum mover settings
        StreamMult,
        RestrictInvert,
        RestrictArea,
        RestrictAngle,
        StreamRestrict,
        DurationTrigger,
        DurationMult,

        //Bezier mover settings
        BezierAggressiveness,
        BezierSliderAggressiveness,

        //Cursor settings
        CursorTrailDensity,
        CursorTrailHueShift,
        CursorTrailHueSpeed,
        CursorTrailFadeDuration,
        CursorTrailSize,
        CursorTrailHue,
        CursorTrailHueOverride
    }

    public enum OsuDanceMover
    {
        AxisAligned,
        Aggresive,
        Bezier,
        Flower,
        HalfCircle,
        Pippi,
        Momentum
    }
}

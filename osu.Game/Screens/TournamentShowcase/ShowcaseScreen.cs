using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ShowcaseScreen : OsuScreen, IHasSubScreenStack
    {
        private const float fade_duration = 500;

        [Cached]
        protected readonly OverlayColourProvider ColourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        public IScreen CurrentSubScreen => screenStack.CurrentScreen;

        public override bool ShowFooter => true;

        [Cached]
        private readonly ScreenStack screenStack = new OsuScreenStack { RelativeSizeAxes = Axes.Both };

        public ShowcaseScreen()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
            Padding = new MarginPadding { Horizontal = -HORIZONTAL_OVERFLOW_PADDING };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                screenStack,
                new Header(TournamentShowcaseStrings.ShowcaseTitle, screenStack),
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            screenStack.Push(new ShowcaseListScreen());
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            this.FadeInFromZero(fade_duration, Easing.OutQuint);
            base.OnEntering(e);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            this.FadeOut(fade_duration, Easing.OutQuint);
            return base.OnExiting(e);
        }

        public override bool OnBackButton()
        {
            if (screenStack.CurrentScreen is not ISubScreenWithTitle subScreen)
                return false;

            if (((Drawable)subScreen).IsLoaded && subScreen.AllowUserExit && subScreen.OnBackButton())
                return true;

            if (screenStack.CurrentScreen is null or ShowcaseListScreen)
                return false;

            screenStack.Exit();
            return true;
        }

        ScreenStack IHasSubScreenStack.SubScreenStack => screenStack;
    }
}

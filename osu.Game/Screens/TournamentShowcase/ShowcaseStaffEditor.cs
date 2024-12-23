// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Models;
using osuTK;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ShowcaseStaffEditor : FillFlowContainer
    {
        private readonly Bindable<ShowcaseConfig> config = new Bindable<ShowcaseConfig>();

        private readonly FillFlowContainer staffContainer;

        public ShowcaseStaffEditor(Bindable<ShowcaseConfig> config)
        {
            this.config.BindTo(config);

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            AutoSizeEasing = Easing.OutQuint;
            AutoSizeDuration = 200;
            Direction = FillDirection.Vertical;
            Spacing = new Vector2(10);
            Children = new Drawable[]
            {
                new SectionHeader(@"Staff List"),
                new FormCheckBox
                {
                    Caption = @"Show staff list in the showcase",
                    HintText = @"List all staffs at the end of the showcase.",
                    Current = config.Value.ShowStaffList
                },
                new ShowcaseAddButton(@"Add staff", () =>
                {
                    var addedStaff = new ShowcaseStaff();
                    config.Value.Staffs.Add(addedStaff);
                    Add(new StaffRow(addedStaff, config.Value));
                }),
                staffContainer = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(5),
                    ChildrenEnumerable = config.Value.Staffs.Select(t => new StaffRow(t, config.Value))
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            config.BindValueChanged(conf =>
            {
                staffContainer.ChildrenEnumerable = conf.NewValue.Staffs.Select(t => new StaffRow(t, config.Value));
            });
        }
    }
}

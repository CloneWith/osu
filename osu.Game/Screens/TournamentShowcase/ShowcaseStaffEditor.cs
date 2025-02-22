// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Models;
using osuTK;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ShowcaseStaffEditor : FillFlowContainer
    {
        private readonly Bindable<ShowcaseConfig> config = new Bindable<ShowcaseConfig>();

        private readonly FillFlowContainer staffContainer;
        private readonly FormCheckBox showListCheckBox;

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
                new SectionHeader(TournamentShowcaseStrings.StaffListHeader),
                showListCheckBox = new FormCheckBox
                {
                    Caption = TournamentShowcaseStrings.ShowStaffListInShowcase,
                    HintText = TournamentShowcaseStrings.ShowStaffListInShowcaseDescription,
                    Current = this.config.Value.ShowStaffList,
                },
                new ShowcaseAddButton(TournamentShowcaseStrings.AddStaff, () =>
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
                    ChildrenEnumerable = config.Value.Staffs.Select(t => new StaffRow(t, config.Value)),
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            config.BindValueChanged(conf =>
            {
                showListCheckBox.Current = conf.NewValue.ShowStaffList;
                staffContainer.ChildrenEnumerable = conf.NewValue.Staffs.Select(t => new StaffRow(t, config.Value));
            });
        }
    }
}

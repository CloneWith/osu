// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Overlays.Chat;
using osu.Game.Tests.Visual;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Tests.Components
{
    public partial class TestSceneTournamentMatchChatDisplay : OsuTestScene
    {
        private readonly Channel testChannel = new Channel();
        private readonly Channel testChannel2 = new Channel();

        private readonly APIUser admin = new APIUser
        {
            Username = "HappyStick",
            Id = 2,
            Colour = "f2ca34",
        };

        private readonly TournamentUser carbonReferee = new TournamentUser
        {
            Username = "Sh0rtD011y",
            OnlineID = 114514,
        };

        private readonly TournamentUser cyberReferee = new TournamentUser
        {
            Username = "Juroe",
            OnlineID = 1919810,
        };

        private readonly TournamentUser redUser = new TournamentUser
        {
            Username = "BanchoBot",
            OnlineID = 3,
        };

        private readonly TournamentUser blueUser = new TournamentUser
        {
            Username = "Zallius",
            OnlineID = 4,
        };

        private readonly TournamentUser blueUserWithCustomColour = new TournamentUser
        {
            Username = "nekodex",
            OnlineID = 5,
        };

        private readonly TournamentUser redUserWithLongName = new TournamentUser
        {
            Username = "SplendidSummerLight",
            OnlineID = 6,
        };

        [Cached]
        private LadderInfo ladderInfo = new LadderInfo();

        [Cached]
        private LegacyMatchIPCInfo legacyMatchInfo = new LegacyMatchIPCInfo(); // hide parent

        private readonly TournamentMatchChatDisplay chatDisplay;

        public TestSceneTournamentMatchChatDisplay()
        {
            Add(chatDisplay = new TournamentMatchChatDisplay
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddStep("set up API", () =>
            {
                ((DummyAPIAccess)API).HandleRequest = req =>
                {
                    switch (req)
                    {
                        case JoinChannelRequest joinChannelRequest:
                            joinChannelRequest.TriggerSuccess();
                            return true;

                        case LeaveChannelRequest leaveChannelRequest:
                            leaveChannelRequest.TriggerSuccess();
                            return true;

                        default:
                            return false;
                    }
                };
            });
            AddStep("set channel", () => chatDisplay.Channel.Value = testChannel);

            AddStep("message from admin", () => testChannel.AddNewMessages(new Message(nextMessageId())
            {
                Sender = admin,
                Content = "I am a wang!"
            }));

            AddStep("set current match", () => ladderInfo.CurrentMatch.Value = new TournamentMatch
            {
                Team1 =
                {
                    Value = new TournamentTeam { Players = { redUser, redUserWithLongName } }
                },
                Team2 =
                {
                    Value = new TournamentTeam { Players = { blueUser, blueUserWithCustomColour } }
                },
                Round =
                {
                    Value = new TournamentRound
                    {
                        Referees = { carbonReferee, cyberReferee }
                    }
                }
            });

            AddStep("message from team red", () => testChannel.AddNewMessages(new Message(nextMessageId())
            {
                Sender = redUser.ToAPIUser(),
                Content = "I am team red."
            }));

            AddAssert("message from team red is red color", () =>
                this.ChildrenOfType<DrawableChatUsername>().Last().AccentColour, () => Is.EqualTo(TournamentExtensions.COLOUR_RED));

            AddStep("message from team red", () => testChannel.AddNewMessages(new Message(nextMessageId())
            {
                Sender = redUser.ToAPIUser(),
                Content = "I plan to win!"
            }));

            AddStep("message from team blue", () => testChannel.AddNewMessages(new Message(nextMessageId())
            {
                Sender = blueUser.ToAPIUser(),
                Content = "Not on my watch. Prepare to eat saaaaaaaaaand. Lots and lots of saaaaaaand."
            }));

            AddAssert("message from team blue is blue color", () =>
                this.ChildrenOfType<DrawableChatUsername>().Last().AccentColour, () => Is.EqualTo(TournamentExtensions.COLOUR_BLUE));

            var userWithCustomColour = blueUserWithCustomColour.ToAPIUser();
            userWithCustomColour.Colour = "#e45678";

            AddStep("message from team blue with custom colour", () => testChannel.AddNewMessages(new Message(nextMessageId())
            {
                Sender = userWithCustomColour,
                Content = "Not on my watch. Prepare to eat saaaaaaaaaand. Lots and lots of saaaaaaand."
            }));

            AddAssert("message from team blue is blue color", () =>
                this.ChildrenOfType<DrawableChatUsername>().Last().AccentColour, () => Is.EqualTo(TournamentExtensions.COLOUR_BLUE));

            AddAssert("message from user with custom colour is inverted", () =>
                this.ChildrenOfType<DrawableChatUsername>().Last().Inverted, () => Is.EqualTo(true));

            AddStep("message with a long username", () => testChannel.AddNewMessages(new Message(nextMessageId())
            {
                Sender = redUserWithLongName.ToAPIUser(),
                Content = "Let's see... wow wonderful chessboard artwork owo",
            }));

            AddStep("really long message", () => testChannel.AddNewMessages(new Message(nextMessageId())
            {
                Sender = userWithCustomColour,
                Content = "依赖尽头诠释冷静逆境自信无限打破困局质疑逆转磨炼极限止步假象结果终将问鼎冷静挣脱限制领域 "
                          + "绝巅过去诀别回忆束缚破镜零碎绝劣缺口细节天赋突破诠释天赋专注冷静细节手法摆脱心态上限破局"
                          + "止步压制挣脱冷静逃离看破逆转何为注定打破希望承担绝望质疑坠入实力黄昏终究状态问鼎沉溺磨练"
                          + "心冷自信破局假象约定尽头巅峰答案 离别责任保持期盼诠释余光关于手法依赖重拾失眠止步破碎情绪"
                          + "改变坦诚极限 宿命回忆痛楚我能细节吻别追逐释怀 缠绕雨落失落绝对枷锁奇迹糕手放弃限制结果情绪"
                          + "终将回眸真诚幻境沉沦击败冰冷心绪天赋轨迹缘分道歉随意突破手法绝境自我证明巅峰枷锁答案依赖尽头诠释冷静逆境自信无限左右",
            }));

            AddStep("message from admin", () => testChannel.AddNewMessages(new Message(nextMessageId())
            {
                Sender = admin,
                Content = "Okay okay, calm down guys. Let's do this!"
            }));

            AddStep("multiple messages", () => testChannel.AddNewMessages(
                new Message(nextMessageId())
                {
                    Sender = admin,
                    Content = "I spam you!"
                },
                new Message(nextMessageId())
                {
                    Sender = admin,
                    Content = "I spam you!!!1"
                },
                new Message(nextMessageId())
                {
                    Sender = admin,
                    Content = "I spam you!1!1"
                }));

            AddStep("change channel to 2", () => chatDisplay.Channel.Value = testChannel2);

            AddStep("referee messages", () => testChannel2.AddNewMessages(new Message(nextMessageId())
            {
                Sender = cyberReferee.ToAPIUser(),
                Content = "大家好啊，我是说的道理",
            }));

            AddStep("referee commands", () => testChannel2.AddNewMessages(new Message(nextMessageId())
            {
                Sender = carbonReferee.ToAPIUser(),
                Content = "[*] 比赛时间已到，请各位选手启动原神",
            }));

            AddStep("non-referee commands", () => testChannel2.AddNewMessages(new Message(nextMessageId())
            {
                Sender = blueUser.ToAPIUser(),
                Content = "[*] 大家好啊，我是说的老鲤",
            }));

            AddStep("change channel to 1", () => chatDisplay.Channel.Value = testChannel);

            AddStep("!mp message (shouldn't display)", () => testChannel.AddNewMessages(new Message(nextMessageId())
            {
                Sender = redUser.ToAPIUser(),
                Content = "!mp wangs"
            }));

            AddStep("resize container to 500x500 with animation", () =>
            {
                chatDisplay.RelativeSizeAxes = Axes.None;
                chatDisplay.ResizeTo(new Vector2(500, 500), 1000, Easing.InOutQuint);
            });

            AddAssert("chat display don't use relative size", () =>
                chatDisplay.RelativeSizeAxes == Axes.None, () => Is.EqualTo(true));
        }

        private int messageId;

        private long? nextMessageId() => messageId++;
    }
}

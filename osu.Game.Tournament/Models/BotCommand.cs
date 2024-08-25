﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace osu.Game.Tournament.Models
{
    /// <summary>
    /// Handle special commands from IRC chat.
    /// </summary>
    [Serializable]
    public class BotCommand
    {
        private Regex finalRegex = new Regex("\\[\\*\\] (.+)获胜");
        private Regex banRegex = new Regex("\\[\\*\\] 执行(.*)禁用(.*) - 已完成");
        private Regex winRegex = new Regex("\\[\\*\\] 执行将(.*)设为(.*)获胜 - 已完成");
        private Regex protectRegex = new Regex("\\[\\*\\] 执行将(.*)设为(.*)的保护图 - 已完成");
        private Regex pickRegex = new Regex("\\[\\*\\] 执行(.*)选取(.*) - 已完成");
        private Regex stateRegex = new Regex("\\[\\*\\] 检查棋盘: 进入(.*)");
        private Regex pickEXRegex = new Regex("\\[\\*\\] 执行强制选取EX(.*) - 已完成");
        private Regex panicRegex = new Regex("\\[\\*\\] 收到异常信号, 启动通知进程");

        public Commands Command;
        public TeamColour Team;
        public string MapMod;

        public BotCommand(Commands command = Commands.Unknown, TeamColour team = TeamColour.Neutral,
            string map = "")
        {
            Command = command;
            Team = team;
            MapMod = map;
        }

        public BotCommand ParseFromText(string line)
        {
            GroupCollection obj;
            if (panicRegex.Match(line).Success)
            {
                return new BotCommand(Commands.Panic);
            }
            if (banRegex.Match(line).Success)
            {
                obj = banRegex.Match(line).Groups;
                return new BotCommand(Commands.Ban, team: TranslateFromTeamName(obj[1].Value), map: obj[2].Value);
            }
            if (protectRegex.Match(line).Success)
            {
                obj = protectRegex.Match(line).Groups;
                return new BotCommand(Commands.Protect, team: TranslateFromTeamName(obj[2].Value), map: obj[1].Value);
            }
            if (pickRegex.Match(line).Success)
            {
                obj = pickRegex.Match(line).Groups;
                return new BotCommand(Commands.Pick, team: TranslateFromTeamName(obj[1].Value), map: obj[2].Value);
            }
            if (winRegex.Match(line).Success)
            {
                obj = winRegex.Match(line).Groups;
                return new BotCommand(Commands.MarkWin, team: TranslateFromTeamName(obj[2].Value), map: obj[1].Value);
            }
            if (finalRegex.Match(line).Success)
            {
                obj = finalRegex.Match(line).Groups;
                return new BotCommand(Commands.SetWin, TranslateFromTeamName(obj[1].Value));
            }
            if (pickEXRegex.Match(line).Success)
            {
                obj = pickEXRegex.Match(line).Groups;
                return new BotCommand(Commands.PickEX, map: "EX" + obj[1].Value);
            }
            if (stateRegex.Match(line).Success)
            {
                obj = stateRegex.Match(line).Groups;
                if (obj[1].Value == "EX图池")
                {
                    return new BotCommand(Commands.EnterEX);
                }
            }
            return new BotCommand(Commands.Unknown);
        }

        public TeamColour TranslateFromTeamName(string name)
        {
            switch (name.ToLowerInvariant())
            {
                case "红队":
                case "红":
                case "红方":
                case "red":
                    return TeamColour.Red;

                case "蓝队":
                case "蓝":
                case "蓝方":
                case "blue":
                    return TeamColour.Blue;

                default:
                    return TeamColour.Neutral;
            }
        }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Commands
    {
        Ban,
        Protect,
        Pick,
        SetWin,
        EnterEX,
        PickEX,
        MarkEXWin,
        MarkWin,
        Panic,
        Unknown
    }
}

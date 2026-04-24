// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;

namespace osu.Game.Utils
{
    public static class ExceptionUtils
    {
        public static bool IsLocalUserConnectivityException(Exception exception)
        {
            switch (exception)
            {
                case TimeoutException te:
                    return te.Message.Contains(@"elapsed without receiving a message from the server");

                case WebException we:
                    // more statuses may need to be blocked as we come across them.
                    return we.Status == WebExceptionStatus.Timeout;

                case WebSocketException:
                case SocketException:
                    return true;
            }

            return false;
        }
    }
}

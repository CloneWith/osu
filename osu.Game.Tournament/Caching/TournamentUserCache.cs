// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Game.Database;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Tournament.Caching
{
    /// <summary>
    /// A shared, in-memory cache of user profiles for the tournament client.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The client asks for the same handful of users from several places, and each of those used to issue its own
    /// <see cref="GetUserRequest"/>. Routing them all through one cache means a profile is fetched once per
    /// session no matter how many places display it, and that concurrent requests for a large team list are
    /// folded into batched lookups of up to 50 users.
    /// </para>
    /// <para>
    /// <see cref="OnlineLookupCache{TLookup,TValue,TRequest}"/> is used with <see cref="GetUsersRequest"/>
    /// rather than <see cref="LookupUsersRequest"/> because the tournament needs per-ruleset statistics: a
    /// player card shows pp and global rank for the ruleset the ladder is currently set to, and that data is
    /// only present in this response.
    /// </para>
    /// <para>
    /// Nothing here survives a restart. Persisting profiles is the bracket's job, via <see cref="Models.TournamentUser"/>.
    /// </para>
    /// </remarks>
    public partial class TournamentUserCache : OnlineLookupCache<int, APIUser, GetUsersRequest>
    {
        // A failed or unknown user is retried on the next request rather than being remembered as absent,
        // so a player which appears later is picked up without a restart.
        protected override bool CacheNullValues => false;

        /// <summary>
        /// Retrieves a user's profile.
        /// </summary>
        /// <param name="userId">The user to look up.</param>
        /// <param name="token">An optional cancellation token.</param>
        /// <returns>The profile, or <c>null</c> if the user could not be retrieved.</returns>
        public Task<APIUser?> GetUserAsync(int userId, CancellationToken token = default) => LookupAsync(userId, token);

        /// <summary>
        /// Retrieves several users' profiles at once.
        /// </summary>
        /// <param name="userIds">The users to look up.</param>
        /// <param name="token">An optional cancellation token.</param>
        /// <returns>The profiles. May contain <c>null</c> for users which could not be retrieved.</returns>
        public Task<APIUser?[]> GetUsersAsync(int[] userIds, CancellationToken token = default) => LookupAsync(userIds, token);

        protected override GetUsersRequest CreateRequest(IEnumerable<int> ids) => new GetUsersRequest(ids.ToArray());

        protected override IEnumerable<APIUser>? RetrieveResults(GetUsersRequest request) => request.Response?.Users;
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Tournament.Caching;
using osu.Game.Users;

namespace osu.Game.Tournament.Components
{
    /// <summary>
    /// A simple avatar which reads through <see cref="OnlineAssetCache"/>.
    /// </summary>
    public partial class TournamentAvatar : Container
    {
        private readonly IUser? user;

        /// <summary>
        /// Construct a new tournament avatar.
        /// </summary>
        /// <param name="user">The user to display. A null value, or a user without a real ID, gets a placeholder avatar.</param>
        public TournamentAvatar(IUser? user)
        {
            this.user = user;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            // The sprite is reached through the wrapper so that its loader runs on the load thread. A cold
            // cache reaches out to the network, and player cards are added straight to the tree, so loading
            // the avatar inline would stall the update thread for as long as the request takes.
            InternalChild = new DelayedLoadWrapper(() => new AvatarSprite(user), 0)
            {
                RelativeSizeAxes = Axes.Both,
            };
        }

        [LongRunningLoad]
        private partial class AvatarSprite : Sprite
        {
            private readonly IUser? user;

            public AvatarSprite(IUser? user)
            {
                this.user = user;

                RelativeSizeAxes = Axes.Both;
                FillMode = FillMode.Fit;
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
            }

            [BackgroundDependencyLoader]
            private void load(LargeTextureStore textures, OnlineAssetCache onlineAssets)
            {
                // Same conditions as `DrawableAvatar`: a user without a profile has no avatar to fetch, and the
                // stored ID of a user which was never successfully looked up is not worth a request either.
                Texture? fetched = user != null && user.OnlineID > 1
                    ? usable(onlineAssets.Get((user as APIUser)?.AvatarUrl ?? $@"https://a.ppy.sh/{user.OnlineID}"))
                    : null;

                Texture = fetched ?? usable(textures.Get(@"Online/avatar-guest"));
            }

            /// <summary>
            /// Returns the given texture, or <c>null</c> when it has already been released.
            /// </summary>
            /// <remarks>
            /// Assigning a released texture to a <see cref="Sprite"/> throws immediately, because the assignment
            /// reads the texture's size. Coming up without an avatar is a much better outcome during a broadcast
            /// than an exception on the load thread.
            /// </remarks>
            private static Texture? usable(Texture? texture) => texture?.Available == true ? texture : null;

            protected override void LoadComplete()
            {
                base.LoadComplete();
                this.FadeInFromZero(300, Easing.OutQuint);
            }
        }
    }
}

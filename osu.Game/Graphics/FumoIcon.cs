// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Extensions;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Text;

namespace osu.Game.Graphics
{
    public static class FumoIcon
    {
        public const string FONT_NAME = @"FumoIcons";

        public static IconUsage NoMod => get(FumoIconMapping.NoMod);
        public static IconUsage HardRock => get(FumoIconMapping.HardRock);
        public static IconUsage FreeMod => get(FumoIconMapping.FreeMod);

        private static IconUsage get(FumoIconMapping glyph) => new IconUsage((char)glyph, FONT_NAME);

        private enum FumoIconMapping
        {
            [Description(@"NM")]
            NoMod,

            [Description(@"HR")]
            HardRock,

            [Description(@"FM")]
            FreeMod,
        }

        public class FumoIconStore : ITextureStore, ITexturedGlyphLookupStore
        {
            private readonly TextureStore textures;

            public FumoIconStore(TextureStore textures)
            {
                this.textures = textures;
            }

            public ITexturedCharacterGlyph? Get(string? fontName, char character)
            {
                if (fontName == FONT_NAME)
                    return new OsuIcon.OsuIconStore.Glyph(textures.Get($@"{fontName}/{((FumoIconMapping)character).GetDescription()}"));

                return null;
            }

            public Task<ITexturedCharacterGlyph?> GetAsync(string fontName, char character) => Task.Run(() => Get(fontName, character));

            public Texture? Get(string name, WrapMode wrapModeS, WrapMode wrapModeT) => null;

            public Texture Get(string name) => throw new NotImplementedException();

            public Task<Texture> GetAsync(string name, CancellationToken cancellationToken = default) => throw new NotImplementedException();

            public Stream GetStream(string name) => throw new NotImplementedException();

            public IEnumerable<string> GetAvailableResources() => throw new NotImplementedException();

            public Task<Texture?> GetAsync(string name, WrapMode wrapModeS, WrapMode wrapModeT, CancellationToken cancellationToken = default) => throw new NotImplementedException();

            public void Dispose()
            {
                textures.Dispose();
            }
        }
    }
}

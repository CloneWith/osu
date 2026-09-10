// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;
using osu.Framework.Graphics;

namespace osu.Game.Tournament
{
    /// <summary>
    /// Serialize <see cref="Colour4"/> instances as HEX strings to make them more human-readable.
    /// It's also because the default deserializer doesn't support it.
    /// </summary>
    internal class JsonColour4Converter : JsonConverter<Colour4>
    {
        public override void WriteJson(JsonWriter writer, Colour4 value, JsonSerializer serializer)
        {
            // Use HEX format to make the serialized output human-friendly.
            serializer.Serialize(writer, value.ToHex());
        }

        public override Colour4 ReadJson(JsonReader reader, Type objectType, Colour4 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            // The default JSON serialization method is not supported.
            if (reader.TokenType != JsonToken.String)
                return reader.TokenType != JsonToken.StartObject ? existingValue : new Colour4();

            string? str = (string?)reader.Value;

            if (str != null && Colour4.TryParseHex(str, out var colour))
                return colour;

            return new Colour4();
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.IO;

namespace osu.Game.Screens.TournamentShowcase
{
    public class ShowcaseStorage : WrappedStorage
    {
        private readonly Storage storage;

        public ShowcaseStorage(Storage storage)
            : base(storage.GetStorageForDirectory("showcase"), string.Empty)
        {
            this.storage = storage.GetStorageForDirectory("showcase");
        }

        public ShowcaseConfig GetConfig(string name)
        {
            if (!storage.Exists(name))
            {
                Logger.Log(@$"Cannot found showcase profile {name}, reverting to default.");
                return new ShowcaseConfig();
            }

            using (Stream stream = storage.GetStream(name, FileAccess.Read, FileMode.Open))
            using (var sr = new StreamReader(stream))
            {
                // TODO: Need an async Task?
                return JsonConvert.DeserializeObject<ShowcaseConfig>(sr.ReadToEnd()) ?? new ShowcaseConfig();
            }
        }

        public IEnumerable<string> ListTournaments() => GetFiles(string.Empty, "*.json");

        public void SaveChanges(ShowcaseConfig config)
        {
            // Serialise before opening stream for writing, so if there's a failure it will leave the file in the previous state.
            string serialisedLadder = GetSerialisedConfig(config);

            using (var stream = storage.CreateFileSafely(@$"{config.TournamentName.Value}-{config.RoundName.Value}.json"))
            using (var sw = new StreamWriter(stream))
                sw.Write(serialisedLadder);
        }

        public string GetSerialisedConfig(ShowcaseConfig config)
        {
            return JsonConvert.SerializeObject(config,
                new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore,
                });
        }
    }
}

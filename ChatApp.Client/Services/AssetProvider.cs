using System.Collections.Generic;

namespace ChatApp.Client.Services
{
    public class AssetProvider : IAssetProvider
    {
        private readonly Dictionary<string, string> _map = new()
        {
            ["appIcon"] = "appIcon.png",
            ["return"] = "return.png",
            ["addPerson"] = "person-add-outline.png",
            ["link"] = "link-outline.png",
            ["sunny"] = "sunny-outline.png",
            ["login"] = "log-in-outline.png",
            ["voiceCall"] = "call-outline.png",
            ["profile"] = "person-circle-outline.png",
        };

        public string GetIcon(string key)
        {
            if (_map.TryGetValue(key, out var file))
            {
                return $"avares://ChatApp.Client/Assets/{file}";
            }
            return $"avares://ChatApp.Client/Assets/{key}.png";
        }
    }
}


using System.Text.Json.Serialization;

namespace Heresy;

public class Config {
    [JsonInclude] public bool WaterToWine = false;
}

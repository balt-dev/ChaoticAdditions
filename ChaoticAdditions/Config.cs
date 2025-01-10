using System.Text.Json.Serialization;

namespace ChaoticAdditions;

public class Config {
    [JsonInclude] public bool WaterToWine = false;
}

using Newtonsoft.Json;

namespace Miningcore.Blockchain.Beam.DaemonResponses;

public class GetVersionResponse
{
    [JsonProperty("beam_network_name")]
    public string Network { get; set; }

    [JsonProperty("api_version")]
    public decimal ApiVersion { get; set; }
}

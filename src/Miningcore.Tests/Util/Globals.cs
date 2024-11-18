using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Miningcore.Tests.Util;

public class Globals
{
    public static readonly JsonSerializerSettings JsonSerializerSettings = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    };
}

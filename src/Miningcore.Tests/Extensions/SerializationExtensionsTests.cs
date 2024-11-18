using System.Collections.Generic;
using Miningcore.Extensions;
using Newtonsoft.Json;
using Xunit;

namespace Miningcore.Tests.Extensions;

public class SerializationExtensionsTests : TestBase
{

    [Fact]
    public void SafeExtensionData_Empty()
    {
        var result = JsonConvert.DeserializeObject<Foo>("{ \"bar\": 1, \"foo\": {} }");

        var extra = result!.Extra.SafeExtensionDataAs<Empty>();
        Assert.NotNull(extra);
    }

    [Fact]
    public void SafeExtensionData_Embedded()
    {
        var result = JsonConvert.DeserializeObject<Foo>("{ \"bar\": 1, \"baz\": 42 }");

        var extra = result!.Extra.SafeExtensionDataAs<Simple>();
        Assert.NotNull(extra);
        Assert.Equal(42, extra.Baz);
    }

    [Fact]
    public void SafeExtensionData_Wrapped()
    {
        var result = JsonConvert.DeserializeObject<Foo>("{ \"bar\": 1, \"foo\": { \"baz\": 42 } }");

        var extra = result!.Extra.SafeExtensionDataAs<Wrapped>("foo");
        Assert.NotNull(extra);
        Assert.Equal(42, extra.Baz);
    }

    [Fact]
    public void SafeExtensionData_Double_Wrapped()
    {
        var result = JsonConvert.DeserializeObject<Foo>("{ \"bar\": 1, \"foo\": { \"qux\": { \"baz\": 42 } } }");

        var extra = result!.Extra.SafeExtensionDataAs<Wrapped>("foo", "qux");
        Assert.NotNull(extra);
        Assert.Equal(42, extra.Baz);
    }

    [Fact]
    public void SafeExtensionData_Triple_Wrapped()
    {
        var result = JsonConvert.DeserializeObject<Foo>("{ \"bar\": 1, \"foo\": { \"qux\": { \"thud\": { \"baz\": 42 } } } }");

        var extra = result!.Extra.SafeExtensionDataAs<Wrapped>("foo", "qux", "thud");
        Assert.NotNull(extra);
        Assert.Equal(42, extra.Baz);
    }

    private class Foo
    {
        public int Bar { get; set; }

        [JsonExtensionData]
        public IDictionary<string, object> Extra { get; }
    }

    private class Empty
    {
    }

    private class Simple
    {
        public int Baz { get; }
    }

    private class Wrapped
    {
        public int Baz { get; }
    }
}

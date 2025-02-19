using Autofac;
using Newtonsoft.Json;

namespace Miningcore.Tests;

public abstract class TestBase
{

    protected readonly IContainer container;
    protected readonly JsonSerializerSettings jsonSerializerSettings;

    protected TestBase()
    {
        ModuleInitializer.Initialize();

        container = ModuleInitializer.Container;
        jsonSerializerSettings = container.Resolve<JsonSerializerSettings>();
    }
}

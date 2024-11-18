using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Autofac;
using AutoMapper;
using Miningcore.Configuration;
using Miningcore.Native;
using Miningcore.Tests.Util;

namespace Miningcore.Tests;

public static class ModuleInitializer
{
    private static readonly object initLock = new();

    private static bool isInitialized;

    public static IContainer Container { get; private set; }
    public static Dictionary<string, CoinTemplate> CoinTemplates { get; private set; }

    /// <summary>
    ///     Initializes the module.
    /// </summary>
    public static void Initialize()
    {
        lock(initLock)
        {
            if(isInitialized)
                return;

            var builder = new ContainerBuilder();

            builder.RegisterAssemblyModules(typeof(AutofacModule).GetTypeInfo().Assembly);

            // AutoMapper
            var amConf = new MapperConfiguration(cfg => { cfg.AddProfile(new AutoMapperProfile()); });

            builder.Register((ctx, parms) => amConf.CreateMapper());

            builder.RegisterType<MockMasterClock>().AsImplementedInterfaces();

            // Autofac Container
            Container = builder.Build();

            isInitialized = true;

            // Load coin templates
            var basePath = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            var defaultDefinitions = Path.Combine(basePath, "coins.json");

            var coinDefs = new[]
            {
                defaultDefinitions
            };

            CoinTemplates = CoinTemplateLoader.Load(Container, coinDefs);

            Cryptonight.InitContexts(1);
        }
    }
}

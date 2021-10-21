using FastUntility.Core;
using System;
using FastMongoDb.Core.Repository;
using FastMongoDb.Core;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MongoDbExtension
    {
        public static IServiceCollection AddFastMongoDb(this IServiceCollection serviceCollection, Action<ConfigModel> optionsAction = null)
        {
            if (optionsAction != null)
                serviceCollection.Configure(optionsAction);
            serviceCollection.AddSingleton<IMongoDbRepository, MongoDbRepository>();
            ServiceContext.Init(new ServiceEngine(serviceCollection.BuildServiceProvider()));
            return serviceCollection;
        }
    }
}

# FastMongoDb.Core

nuget url: https://www.nuget.org/packages/Fast.MongoDb.Core/

in Startup.cs Startup mothod

    //by Repository
    services.AddFastMongoDb(a => { a.ConnStr = "mongodb://127.0.0.1:27017";
                a.DbName = "test"; a.Max = 100; a.Min = 10; });

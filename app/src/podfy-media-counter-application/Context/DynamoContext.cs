using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using System.Diagnostics;

namespace podfy_media_counter_application.Context;

public class DynamoContext: IDynamoContext
{
    public DynamoDBContext Context { get; private set; }

    public DynamoContext()
    {
        if (Debugger.IsAttached)
            Context = new DynamoDBContext(new AmazonDynamoDBClient(Amazon.RegionEndpoint.USEast1));
        else
            Context = new DynamoDBContext(new AmazonDynamoDBClient(Environment.GetEnvironmentVariable("ACCESS_KEY"), Environment.GetEnvironmentVariable("SECRET_KEY"), Amazon.RegionEndpoint.USEast1));

    }
}

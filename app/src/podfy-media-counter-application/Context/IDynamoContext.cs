using Amazon.DynamoDBv2.DataModel;

namespace podfy_media_counter_application.Context;

public interface IDynamoContext
{
    DynamoDBContext Context { get; }
}


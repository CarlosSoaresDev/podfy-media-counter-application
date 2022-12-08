using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Microsoft.Extensions.DependencyInjection;
using podfy_media_counter_application.Context;
using podfy_media_counter_application.IoC;
using System.Net;
using System.Text.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace podfy_media_counter_application
{
    public class Function
    {
        public IDynamoContext dynamoContext { get; set; }

        public Function()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddServices();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            dynamoContext = serviceProvider.GetService<IDynamoContext>();
        }

        public async Task DeleteMediaCounterFunctionHandlerAsync(S3Event evnt, ILambdaContext context)
        {
            context.Logger.LogInformation($"Received message with Records {evnt.Records.Count}");
            foreach (var record in evnt.Records)
            {
                var key = record.S3.Object.Key;
                context.Logger.LogInformation($"Processed key: {key}");

                await DeleteMediaCounterByKey(key, context);
            }

            await Task.CompletedTask;
        }

        public async Task<APIGatewayProxyResponse> AddMediaCounterFunctionHandlerAsync(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var mediaCounter = JsonSerializer.Deserialize<MediaCounter>(request.Body);

            await AddMediaCounterByKey(mediaCounter, context);
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }

        public async Task<APIGatewayProxyResponse> MediaCounterFunctionHandlerAsync(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogInformation($"Received message with Records {request.QueryStringParameters}");


            var result = await GetAllMediaCounterByKey(request.QueryStringParameters.Where(w => w.Key == "Key").FirstOrDefault().Value, context);
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonSerializer.Serialize(result),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }

        private async Task DeleteMediaCounterByKey(string key, ILambdaContext context)
        {
            try
            {
                List<MediaCounter> mediaCounters;
                var conditions = new List<ScanCondition>()
                {
                    new ScanCondition("Key", ScanOperator.Equal, key)
                };

                mediaCounters = await dynamoContext.Context.ScanAsync<MediaCounter>(conditions).GetRemainingAsync();

                var batchWork = dynamoContext.Context.CreateBatchWrite<MediaCounter>();
                batchWork.AddDeleteItems(mediaCounters);
                await batchWork.ExecuteAsync();

            }
            catch (Exception ex)
            {
                context.Logger.LogError(ex.Message);
                throw;
            }
        }

        private async Task AddMediaCounterByKey(MediaCounter mediaCounter, ILambdaContext context)
        {
            try
            {
                await dynamoContext.Context.SaveAsync(mediaCounter);
            }
            catch (Exception ex)
            {
                context.Logger.LogError(ex.Message);
                throw;
            }
        }

        private async Task<long> GetAllMediaCounterByKey(string key, ILambdaContext context)
        {
            try
            {
                var conditions = new List<ScanCondition>()
                {
                    new ScanCondition("Key", ScanOperator.Equal, key)
                };

                return (await dynamoContext.Context.ScanAsync<MediaCounter>(conditions, null).GetRemainingAsync()).Count;
            }
            catch (Exception ex)
            {
                context.Logger.LogError(ex.Message);
                throw;
            }
        }
    }

    [DynamoDBTable("media-counter-application")]
    public class MediaCounter
    {
        [DynamoDBHashKey]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Key { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
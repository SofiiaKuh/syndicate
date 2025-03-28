using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SimpleLambdaFunction;

public class Function
{

	public APIGatewayProxyResponse FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
	{
		foreach (var record in sqsEvent.Records)
		{
			try
			{
				context.Logger.LogLine($"Received SQS Message: {record.Body}");

				

			}
			catch (Exception ex)
			{
				context.Logger.LogLine($"Error processing message: {ex.Message}");
			}
		}

		return new APIGatewayProxyResponse
		{
			StatusCode = 200,
			Body = "Hello world!",
			Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
		};
	}
}

using System.Collections.Generic;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SimpleLambdaFunction;

public class Function
{
    public void FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
		foreach (var record in sqsEvent.Records)
		{
			// Log the content of each SQS message
			Console.WriteLine($"Message received: {record.Body}");
			context.Logger.LogLine($"Message received: {record.Body}");

			// You can use the following code to log additional metadata if needed
			_logger.Info($"Message ID: {record.MessageId}");
			_logger.Info($"Receipt Handle: {record.ReceiptHandle}");

			// If there’s any custom logic you want to apply to the message content:
			try
			{
				// Example: Parsing JSON if the message is in JSON format
				var messageContent = record.Body; // Process the message here
				_logger.Info($"Message content: {messageContent}");

			}
			catch (Exception ex)
			{
				// Log any exceptions to CloudWatch
				context.Logger.LogLine($"Error processing message: {ex.Message}");
			}
		}
}

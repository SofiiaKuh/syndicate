using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using System;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SimpleLambdaFunction
{
	public class Function
	{
		public void SQSHandler(SQSEvent sqsEvent, ILambdaContext context)
		{
			foreach (var record in sqsEvent.Records)
			{
				try
				{
					context.Logger.LogLine($"Received SQS Message: {record.Body}");
				}
				catch (Exception ex)
				{
					context.Logger.LogLine($"Error processing SQS message: {ex.Message}");
				}
			}
		}
	}
}

using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using System;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SimpleLambdaFunction
{
	public class Function
	{
		public void SNSHandler(SNSEvent snsEvent, ILambdaContext context)
		{
			foreach (var record in snsEvent.Records)
			{
				try
				{
					context.Logger.LogLine($"Received SNS Message: {record.Sns.Message}");
				}
				catch (Exception ex)
				{
					context.Logger.LogLine($"Error processing SNS message: {ex.Message}");
				}
			}
		}
	}
}

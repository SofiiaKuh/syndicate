
using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;

using System;
using System.Collections.Generic;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SimpleLambdaFunction
{
	public class Function
	{
		public void FunctionHandler(SNSEvent snsEvent, ILambdaContext context)
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

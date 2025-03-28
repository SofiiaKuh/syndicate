using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using System;
using System.Threading.Tasks;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

public class Function
{
	public async Task Handler(SNSEvent snsEvent, ILambdaContext context)
	{
		foreach (var record in snsEvent.Records)
		{
			context.Logger.LogInformation($"Received SNS message: {record.Sns.Message}");
		}

		await Task.CompletedTask; // Placeholder for async operations if needed
	}
}

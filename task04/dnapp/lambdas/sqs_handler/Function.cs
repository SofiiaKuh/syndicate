using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using System;
using System.Threading.Tasks;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

public class Function
{
	public async Task Handler(SQSEvent sqsEvent, ILambdaContext context)
	{
		foreach (var record in sqsEvent.Records)
		{
			context.Logger.LogInformation(record.Body);
		}

		await Task.CompletedTask; // Placeholder for async operations if needed
	}
}

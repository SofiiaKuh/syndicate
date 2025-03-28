using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using System;
using System.Text.Json;
using System.Threading.Tasks;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

public class Function
{
	public async Task Handler(SQSEvent sqsEvent, ILambdaContext context)
	{
		foreach (var record in sqsEvent.Records)
		{
			Console.WriteLine(JsonSerializer.Serialize(record));
		}

		await Task.CompletedTask; // Placeholder for async operations if needed
	}
}

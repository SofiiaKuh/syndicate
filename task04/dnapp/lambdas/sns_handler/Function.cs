using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using System;
using System.Text.Json;
using System.Threading.Tasks;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

public class Function
{
	public async Task Handler(SNSEvent snsEvent, ILambdaContext context)
	{
		foreach (var record in snsEvent.Records)
		{
			Console.WriteLine(JsonSerializer.Serialize(record));
		}

		await Task.CompletedTask; // Placeholder for async operations if needed
	}
}

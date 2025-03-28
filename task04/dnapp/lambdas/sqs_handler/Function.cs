using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using System;
using System.Text.Json;
using System.Threading.Tasks;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SimpleLambdaFunction 
{
	public class Function
	{
		public async Task Handler(SQSEvent sqsEvent, ILambdaContext context)
		{
			foreach (var record in sqsEvent.Records)
			{
				context.Logger.LogLine(JsonSerializer.Serialize(record));
			}

			await Task.CompletedTask; 
		}
	}
}

using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using System;
using System.Text.Json;
using System.Threading.Tasks;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace SimpleLambdaFunction 
{
	public class Function
	{
		public async Task Handler(SNSEvent snsEvent, ILambdaContext context)
		{
			foreach (var record in snsEvent.Records)
			{
				context.Logger.LogLine(JsonSerializer.Serialize(record));
			}

			await Task.CompletedTask; 
		}
	}
}
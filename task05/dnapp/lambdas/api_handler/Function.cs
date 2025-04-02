using System;
using System.Collections.Generic;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SimpleLambdaFunction;

public class Function
{
	private static readonly AmazonDynamoDBClient _dynamoDbClient = new AmazonDynamoDBClient();

	public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
	{
		try
		{
			var tableName = Environment.GetEnvironmentVariable("table_name");
			context.Logger.LogLine($"Found table: {tableName}");

			var requestBody = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(request.Body);
			var eventId = Guid.NewGuid().ToString();
			var principalId = requestBody.ContainsKey("principalId") ? requestBody["principalId"].GetInt32() : 10;
			var createdAt = DateTime.UtcNow.ToString("o");

			var content = requestBody.ContainsKey("content") ? requestBody["content"].GetRawText() : "{}";

			var eventData = new EventResponse
			{
				id = eventId,
				principalId = principalId,
				createdAt = createdAt,
				body = JsonSerializer.Deserialize<Dictionary<string, string>>(content)  // Deserialize body as a map
			};

			var item = new Dbdata
			{
				id = eventId,
				Event = eventData
			};

			var serializedItem = JsonSerializer.Serialize(item);
			var doc = Document.FromJson(serializedItem);

			context.Logger.LogLine($"Item is going to put: {JsonSerializer.Serialize(doc)}");

			var table = Table.LoadTable(_dynamoDbClient, tableName);
			await table.PutItemAsync(doc);

			var savedItem = await table.GetItemAsync(eventId);
			if (savedItem == null)
			{
				context.Logger.LogError("Failed to write item to DynamoDB.");
				return new APIGatewayProxyResponse
				{
					StatusCode = 500,
					Body = JsonSerializer.Serialize(new { statusCode = 201, message = "Error writing to DynamoDB" }),
					Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
				};
			}
			context.Logger.LogLine($"Item put: {JsonSerializer.Serialize(savedItem)}");

			var response = new
			{
				id = eventId,
				principalId,
				createdAt,
				body = content
			};
			context.Logger.LogLine($"Create response {response}");


			return new APIGatewayProxyResponse
			{
				StatusCode = 201,
				Body = JsonSerializer.Serialize(new { statusCode = 201, Event = response }),
				Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
			};
		}
		catch (Exception ex)
		{
			context.Logger.LogError($"Error: {ex.Message}");
			return new APIGatewayProxyResponse
			{
				StatusCode = 500,
				Body = JsonSerializer.Serialize(new { statusCode = 201, message = "Internal Server Error" })
			};
		}
	}

	class Response
	{
		public int StatusCode { get; set; }
		public object? Event { get; set; }
	}

	public class EventResponse
	{
		public string id { get; set; }
		public int principalId { get; set; }
		public string createdAt { get; set; }
		public Dictionary<string, string> body { get; set; }
	}

	public class Dbdata
	{

		public string id { get; set; }
		[JsonPropertyName("event")]
		public EventResponse Event { get; set; }

	}
}

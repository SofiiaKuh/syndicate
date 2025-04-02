using System;
using System.Collections.Generic;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using System.Linq;
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
				Id = eventId,
				PrincipalId = principalId,
				CreatedAt = createdAt,
				Body = JsonSerializer.Deserialize<Dictionary<string, string>>(content)  // Deserialize body as a map
			};

			var item = new Dbdata
			{
				Id = eventId,
				Event = eventData
			};

			var doc = Document.FromJson(JsonSerializer.Serialize(item));

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
		public string Id { get; set; }
		public int PrincipalId { get; set; }
		public string CreatedAt { get; set; }
		public Dictionary<string, string> Body { get; set; }
	}

	public class Dbdata
	{

		public string Id { get; set; }
		public EventResponse Event { get; set; }

	}
}

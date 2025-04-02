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
			string tableName = await GetTableNameAsync();

			var requestBody = JsonSerializer.Deserialize<Dictionary<string, object>>(request.Body);
			var eventId = Guid.NewGuid().ToString();
			var principalId = requestBody["principalId"];
			var createdAt = DateTime.UtcNow.ToString("o");

			var body = requestBody.ContainsKey("body") ? requestBody["body"] : new Dictionary<string, object> { { "key", "value" } };

			var item = new Document
			{
				["event"] = Document.FromJson(JsonSerializer.Serialize(new
				{
					id = eventId,
					principalId,
					createdAt,
					body
				}))
			};

			var table = Table.LoadTable(_dynamoDbClient, tableName);
			await table.PutItemAsync(item);

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

			var response = new
			{
				id = eventId,
				principalId,
				createdAt,
				body
			};


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
		public object Event { get; set; }
	}
	private static async Task<string> GetTableNameAsync()
	{
		using var dynamoDbClient = new AmazonDynamoDBClient();

		var response = await dynamoDbClient.ListTablesAsync();

		// Find the table containing "Events"
		var tableName = response.TableNames.FirstOrDefault(t => t.Contains("Events"));

		if (tableName == null)
		{
			throw new Exception("No table containing 'Events' found in DynamoDB.");
		}

		return tableName;
	}
}

using System;
using System.Collections.Generic;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using System.Threading.Tasks;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;

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
				["id"] = eventId,
				["principalId"] = principalId.ToString(),
				["createdAt"] = createdAt,
				["body"] = Document.FromJson(JsonSerializer.Serialize(body))
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
		using var ssmClient = new AmazonSimpleSystemsManagementClient();
		var request = new GetParameterRequest { Name = "/learn/target_table" };

		var response = await ssmClient.GetParameterAsync(request);
		return response.Parameter.Value;
	}
}

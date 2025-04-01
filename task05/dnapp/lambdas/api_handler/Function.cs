using System;
using System.Collections.Generic;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using System.Threading.Tasks;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SimpleLambdaFunction;

public class Function
{
	private static readonly AmazonDynamoDBClient _dynamoDbClient = new AmazonDynamoDBClient();
	private static readonly string TableName = "Events";

	public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
	{
		try
		{
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

			var table = Table.LoadTable(_dynamoDbClient, TableName);
			await table.PutItemAsync(item);

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
				Body = JsonSerializer.Serialize(new { statusCode = 201, @event = response }),
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
}

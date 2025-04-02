using System;
using System.Collections.Generic;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using System.Linq;
using Amazon.DynamoDBv2.Model;
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
			var client = new AmazonDynamoDBClient();

			var tableName = Environment.GetEnvironmentVariable("table_name");
			var table = Table.LoadTable(client, tableName);
			context.Logger.LogLine($"Found table: {tableName}");

			var requestBody = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(request.Body);
			var eventId = Guid.NewGuid().ToString();
			var principalId = requestBody.ContainsKey("principalId") ? requestBody["principalId"].GetInt32() : 10;
			var createdAt = DateTime.UtcNow.ToString("o");

			var content = requestBody.ContainsKey("content") ? requestBody["content"].GetRawText() : "{}";

			//var eventData = new EventResponse
			//{
			//	id = eventId,
			//	principalId = principalId,
			//	createdAt = createdAt,
			//	body = JsonSerializer.Deserialize<Dictionary<string, string>>(content)  // Deserialize body as a map
			//};

			//var item = new Dbdata
			//{
			//	id = eventId,
			//	Event = eventData
			//};

			//var doc = Document.FromJson(JsonSerializer.Serialize(item));
			//context.Logger.LogLine($"Serialized item: {JsonSerializer.Serialize(item)}");

			//var table = Table.LoadTable(_dynamoDbClient, tableName);
			//await table.PutItemAsync(doc);
			var bodyAttributes = new Dictionary<string, AttributeValue>();
			var contentdic = JsonSerializer.Deserialize<Dictionary<string, string>>(content);
			// Loop through the content and dynamically add it to the bodyAttributes
			//foreach (var property in contentdic)
			//{
			//	bodyAttributes.Add(property.Key, new AttributeValue { S = property.Value });
			//}

			// Create the item to be added
			var item = new Dictionary<string, AttributeValue>
			{
				{ "id", new AttributeValue { S = eventId } },  
                { "event", new AttributeValue
					{
						M = new Dictionary<string, AttributeValue>
						{
							{ "id", new AttributeValue { S = eventId } },
							{ "principalId", new AttributeValue { N = "10" } },
							{ "createdAt", new AttributeValue { S = "2024-01-01T00:00:00.000000" } },
							{ "body", new AttributeValue
								{
									M = new Dictionary<string, AttributeValue>
									{
									{ "key", new AttributeValue { S = "value" } }
									}
								}
							}
						}
					}
				}
			};

			context.Logger.LogLine($"Item to put: {JsonSerializer.Serialize(item)}");


			var vrequest = new PutItemRequest
			{
				TableName = tableName,
				Item = item
			};

			await  client.PutItemAsync(vrequest);

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

	public class RequestBody
	{
		public int PrincipalId { get; set; }
		public Dictionary<string, string> Content { get; set; } // Content as a dictionary of key-value pairs
	}
}
	
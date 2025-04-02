using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.DynamoDBEvents;
using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace SimpleLambdaFunction;


public class Function
{
	private static readonly string AuditTableName = Environment.GetEnvironmentVariable("table_name") ?? "Audit";
	private readonly AmazonDynamoDBClient _dynamoDbClient = new AmazonDynamoDBClient();

	public async Task FunctionHandler(DynamoDBEvent dynamoEvent, ILambdaContext context)
	{
		foreach (var record in dynamoEvent.Records)
		{
			if (record.EventName == "INSERT")
			{
				await HandleInsert(ConvertAttributeValues(record.Dynamodb.NewImage));
			}
			else if (record.EventName == "MODIFY")
			{
				await HandleModify(ConvertAttributeValues(record.Dynamodb.OldImage), ConvertAttributeValues(record.Dynamodb.NewImage));
			}
		}
	}

	private async Task HandleInsert(Dictionary<string, AttributeValue> newImage)
	{
		var auditItem = new Dictionary<string, AttributeValue>
		{
			["id"] = new AttributeValue { S = Guid.NewGuid().ToString() },
			["itemKey"] = new AttributeValue { S = newImage["key"].S },
			["modificationTime"] = new AttributeValue { S = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.ffffffZ") },
			["newValue"] = new AttributeValue
			{
				M = new Dictionary<string, AttributeValue>
				{
					{ "key", new AttributeValue { S = newImage["key"].S } },
					{ "value", new AttributeValue { N = int.Parse(newImage["value"].N).ToString() } }
				}
			}
		};

		await _dynamoDbClient.PutItemAsync(AuditTableName, auditItem);
	}

	private async Task HandleModify(Dictionary<string, AttributeValue> oldImage, Dictionary<string, AttributeValue> newImage)
	{
		string updatedAttribute = "value";
		int oldValue = int.Parse(oldImage["value"].N);
		int newValue = int.Parse(newImage["value"].N);

		var auditItem = new Dictionary<string, AttributeValue>
		{
			["id"] = new AttributeValue { S = Guid.NewGuid().ToString() },
			["itemKey"] = new AttributeValue { S = newImage["key"].S },
			["modificationTime"] = new AttributeValue { S = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.ffffffZ") },
			["updatedAttribute"] = new AttributeValue { S = updatedAttribute },
			["oldValue"] = new AttributeValue { N = oldValue.ToString() },
			["newValue"] = new AttributeValue { N = newValue.ToString() }
		};

		await _dynamoDbClient.PutItemAsync(AuditTableName, auditItem);
	}

	private Dictionary<string, Amazon.DynamoDBv2.Model.AttributeValue> ConvertAttributeValues(Dictionary<string, Amazon.Lambda.DynamoDBEvents.DynamoDBEvent.AttributeValue> originalImage)
	{
		var convertedImage = new Dictionary<string, Amazon.DynamoDBv2.Model.AttributeValue>();

		foreach (var item in originalImage)
		{
			var attributeValue = item.Value;
			var newAttributeValue = new Amazon.DynamoDBv2.Model.AttributeValue();

			// Depending on the attribute type, map it accordingly
			if (attributeValue.S != null)
			{
				newAttributeValue.S = attributeValue.S;
			}
			else if (attributeValue.N != null)
			{
				newAttributeValue.N = attributeValue.N;
			}
			else if (attributeValue.B != null)
			{
				newAttributeValue.B = attributeValue.B;
			}
			// Add more conditions for other data types if needed

			convertedImage[item.Key] = newAttributeValue;
		}

		return convertedImage;
	}
}

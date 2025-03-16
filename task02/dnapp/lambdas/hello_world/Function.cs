using System.Collections.Generic;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SimpleLambdaFunction;

public class Function
{
	public CustomLambdaResponse FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
	{
		if (request.Resource == "/hello" && request.HttpMethod == "GET")
		{
			return new CustomLambdaResponse
			{
				statusCode = 200,
				message = "Hello from Lambda!",
			};
		}
		else
		{
			return new CustomLambdaResponse
			{
				statusCode = 400,
				message = $"Bad request syntax or unsupported method. Request path: {request.Resource}. HTTP method: {request.HttpMethod}",
			};
		}
	}
	public class CustomLambdaResponse
	{
		public int statusCode { get; set; }
		public string message { get; set; }
	}
}

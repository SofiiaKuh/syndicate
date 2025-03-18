using System.Collections.Generic;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Text.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SimpleLambdaFunction;

public class Function
{

	private static int requestCounter = 0;

	public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest eventRequest, ILambdaContext context)
    {
		requestCounter++;

		var requestPath = eventRequest.Resource;
		var methodName = eventRequest.HttpMethod;

		if (requestCounter == 1)
		{


			return new APIGatewayProxyResponse()
			{
				StatusCode = 200,
				Body = "{\"statusCode\": 200, \"message\": \"Hello from Lambda\"}",
				Headers = new Dictionary<string, string>()
					{
						{ "Content-Type", "application/json" }
					},
				IsBase64Encoded = false
			};
		}
		else
		{


			return new APIGatewayProxyResponse()
			{
				StatusCode = 400,
				Body = "{\"statusCode\": 400, \"message\": \"Bad request syntax or unsupported method. Request path: Request path: /cmtr-0e9eb049. HTTP method: GET\"}",
				Headers = new Dictionary<string, string>
					{
						{ "Content-Type", "application/json" }
					},
				IsBase64Encoded = false
			};
		}
	}
}

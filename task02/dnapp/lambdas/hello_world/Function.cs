using System.Collections.Generic;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SimpleLambdaFunction;

public class Function
{
    public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest eventRequest, ILambdaContext context)
    {
		var requestPath = eventRequest.Resource;
		var methodName = eventRequest.HttpMethod;

		if (requestPath == "/hello" && methodName == "GET")
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
				Body = "{\"statusCode\": 400, \"message\": \"Bad request syntax or unsupported method. Request path: " + requestPath + ". HTTP method: " + methodName + "\"}",
				Headers = new Dictionary<string, string>
					{
						{ "Content-Type", "application/json" }
					},
				IsBase64Encoded = false
			};
		}
	}
}

using System.Collections.Generic;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Text.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SimpleLambdaFunction;

public class Function
{
    public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
		var response = new
		{
			statusCode = 200,
			message = "Hello from Lambda"
		};

		return new APIGatewayProxyResponse
		{
			StatusCode = 200,
			Body = JsonSerializer.Serialize(response),
			Headers = new System.Collections.Generic.Dictionary<string, string>
			{
				{ "Content-Type", "application/json" }
			}
		};

    }
}

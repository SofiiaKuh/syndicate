using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SimpleLambdaFunction;

public class Function
{
	public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
	{
		if (request.HttpMethod != "GET" || request.Path != "/weather")
		{
			return new APIGatewayProxyResponse
			{
				StatusCode = (int)HttpStatusCode.BadRequest,
				Body = $"{{\"statusCode\": 400, \"message\": \"Bad request syntax or unsupported method. Request path: {request.Path}. HTTP method: {request.HttpMethod}\"}}"
			};
		}


		return new APIGatewayProxyResponse
		{
			StatusCode = (int)HttpStatusCode.OK,
			Body = "forecast",
			Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
		};
	}
}

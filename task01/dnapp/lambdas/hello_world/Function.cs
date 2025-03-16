using System.Collections.Generic;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SimpleLambdaFunction;

public class Function
{
    public CustomLambdaResponse FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
		return new CustomLambdaResponse
		{
			statusCode = 200,
			message = "Hello from Lambda",
		};
    }

	public class CustomLambdaResponse
	{
		public int statusCode { get; set; }
		public string message { get; set; }
	}
}

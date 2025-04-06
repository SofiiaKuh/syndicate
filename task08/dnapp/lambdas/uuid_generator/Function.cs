using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using Newtonsoft.Json;
using Amazon.Lambda.APIGatewayEvents;  // Required for API Gateway events

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SimpleLambdaFunction
{
	public class Function
	{
		private readonly AmazonS3Client _s3Client;
		private readonly string _bucketName;

		public Function()
		{
			_s3Client = new AmazonS3Client();
			// Retrieve the S3 bucket name from environment variables
			_bucketName = Environment.GetEnvironmentVariable("target_bucket") ?? throw new InvalidOperationException("Target bucket environment variable is not set.");
		}

		// Replace LambdaEvent with the correct event type, in this case, APIGatewayProxyRequest
		public async Task FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
		{
			// Generate 10 random UUIDs
			var uuidList = new List<string>();
			for (int i = 0; i < 10; i++)
			{
				uuidList.Add(Guid.NewGuid().ToString());
			}

			// Get current timestamp for file naming
			var fileName = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

			// Create the content for the S3 file
			var content = new
			{
				ids = uuidList
			};

			// Serialize the content to JSON
			var jsonContent = JsonConvert.SerializeObject(content);

			// Create the PutObject request
			var putObjectRequest = new Amazon.S3.Model.PutObjectRequest
			{
				BucketName = _bucketName,  // Use the bucket name from environment variable
				Key = fileName,  // File name is the current time
				ContentBody = jsonContent  // Content to store
			};

			// Upload the file to S3
			await _s3Client.PutObjectAsync(putObjectRequest);
		}
	}
}

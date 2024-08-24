using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using ThirdParty.Json.LitJson;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace S3toDynamodb_BulkUpload;

public class Function
{
    private readonly IAmazonS3 _s3Client;
    private readonly IAmazonDynamoDB _dynamoDbClient;

    public Function()
    {
        _s3Client = new AmazonS3Client();
        _dynamoDbClient = new AmazonDynamoDBClient();
    }

    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public async Task FunctionHandler(S3Event s3Event, ILambdaContext context)
    {
        Console.WriteLine($"S3 Event: {JsonConvert.SerializeObject(s3Event)}");

        foreach (var record in s3Event.Records)
        {
            var bucketName = record.S3.Bucket.Name;
            var objectKey = record.S3.Object.Key;
            var selectRequest = new SelectObjectContentRequest
            {
                BucketName = bucketName,
                Key = objectKey,
                ExpressionType = ExpressionType.SQL,
                Expression = "SELECT * FROM S3Object s",
                InputSerialization = new InputSerialization
                {
                    CSV = new CSVInput { FileHeaderInfo = FileHeaderInfo.Use, }
                },
                OutputSerialization = new OutputSerialization { JSON = new JSONOutput() }
            };

            var selectResponse = await _s3Client.SelectObjectContentAsync(selectRequest);
            List<Customer> list = new();
            using (var stream = selectResponse.Payload)
                foreach (var ev in stream)
                    if (ev is RecordsEvent records)
                        using (var reader = new StreamReader(records.Payload, Encoding.UTF8))
                            while (!reader.EndOfStream)
                            {
                                var line = await reader.ReadLineAsync();
                                Console.WriteLine($"Input data: {line}");
                                var data = JsonConvert.DeserializeObject<Customer>(line);
                                list.Add(data);
                            }
            if (list.Any())
                WriteBatchToDynamoDB(list);

        }
    }

    private void WriteBatchToDynamoDB(List<Customer> customers)
    {
        var context = new DynamoDBContext(_dynamoDbClient);
        var bookBatch = context.CreateBatchWrite<Customer>();
        bookBatch.AddPutItems(customers);
        bookBatch.ExecuteAsync();
    }
}

[DynamoDBTable("CustomerData")]
public class Customer
{
    [DynamoDBHashKey]
    public string CustomerId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
}
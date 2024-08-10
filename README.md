# Bulk Upload from S3 to Lambda to DynamoDB with DLQ Handling

This guide provides a detailed, step-by-step walkthrough for configuring bulk uploads from S3 to Lambda to DynamoDB with error handling via a Dead Letter Queue (DLQ) using Amazon SQS. The process is implemented using the AWS Console and C#, targeting .NET Core 8. The Lambda function will parse a CSV file containing customer data, convert the data into JSON, and perform bulk uploads to DynamoDB. All resources will be created in the Mumbai region.

## Prerequisites

Before getting started, ensure you have the following:

- **AWS Account**: Ensure you have an AWS account with the necessary permissions to create and manage S3, Lambda, DynamoDB, and SQS resources.
- **AWS Console Access**: Familiarity with the AWS Console for navigating and configuring services.
- **C# Development Environment**: Visual Studio or another C# IDE installed.
- **.NET SDK**: Ensure that the .NET SDK (version 8) is installed.
- **Basic Knowledge**: Familiarity with AWS services, C#, and basic concepts like Lambda, S3, DynamoDB, and SQS.

## Step 1: Create an S3 Bucket

1. **Login to AWS Console**.
2. Navigate to the **S3** service.
3. Click on **Create bucket**.
4. Provide a **Unique Bucket Name** (e.g., `customer-data-bucket-unique-id`) and choose the **Asia Pacific (Mumbai) ap-south-1** region.
5. Adjust any settings (like versioning, encryption) as per your needs, and click **Create bucket**.

## Step 2: Create a DynamoDB Table

1. Navigate to **DynamoDB** in the AWS Console.
2. Ensure the **Region** is set to **Asia Pacific (Mumbai) ap-south-1**.
3. Click on **Create table**.
4. Enter a **Table name** and a **Primary key** (e.g., `customerId`).
5. Configure additional settings like read/write capacity and click **Create**.

## Step 3: Create an SQS Queue for DLQ

1. Navigate to **SQS** in the AWS Console.
2. Ensure the **Region** is set to **Asia Pacific (Mumbai) ap-south-1**.
3. Click on **Create queue**.
4. Choose **Standard** as the type.
5. Enter a **Name** for your queue (e.g., `LambdaDLQ`).
6. Configure additional settings and click **Create queue**.

## Step 4: Create a Lambda Function

### Permissions Setup

1. **IAM Role**: Create or use an existing IAM role with the following permissions:
   - **S3 Full Access**: To read from the S3 bucket.
   - **DynamoDB Full Access**: To perform batch writes to DynamoDB.
   - **SQS SendMessage**: To send messages to the DLQ in case of errors.
   - **CloudWatch Logs**: For logging Lambda function execution details.

   Attach the following policies to your IAM role:
   - `AmazonS3FullAccess`
   - `AmazonDynamoDBFullAccess`
   - `AmazonSQSFullAccess`
   - `CloudWatchLogsFullAccess`

2. Navigate to **Lambda** in the AWS Console.
3. Ensure the **Region** is set to **Asia Pacific (Mumbai) ap-south-1**.
4. Click on **Create function**.
5. Choose **Author from scratch**.
6. Enter a **Function name** (e.g., `S3CsvToDynamoDBLambda`).
7. Select **.NET Core 8** as the runtime.
8. Under **Permissions**, choose the IAM role you configured earlier.
9. Click **Create function**.

## Step 5: Create and Upload the Lambda Function Code

### 5.1 Write the Lambda Function Code

Create your Lambda function using the .NET Core 8 runtime. Ensure the code is designed to handle S3 events, parse CSV files, and interact with DynamoDB. For more details on writing the Lambda function code, consult the [AWS Lambda documentation](https://docs.aws.amazon.com/lambda/latest/dg/welcome.html).

### 5.2 Create a ZIP File for the Lambda Deployment

1. Build the project in **Release** mode using your C# IDE.
2. Package the build output into a ZIP file containing all necessary binaries and dependencies.

### 5.3 Upload the ZIP File to Lambda

1. Go back to the **Lambda Console**.
2. In the **Code** tab, click on **Upload from** and select **.zip file**.
3. Choose the ZIP file you created.
4. Click **Save** to deploy the function.

## Step 6: Set Up S3 Event Trigger for Lambda

To create the Lambda function for processing CSV files from S3 and inserting records into DynamoDB, follow these steps:

#### **Configuring the S3 Trigger**

1. **Navigate to AWS Lambda Console**:
   - Open the [AWS Management Console](https://aws.amazon.com/console/).
   - Go to the **Lambda** service.

2. **Select Your Lambda Function**:
   - Find and click on the Lambda function you created (e.g., `S3CsvToDynamoDBLambda`).

3. **Add an S3 Trigger**:
   - In the Lambda function configuration page, scroll down to the **Designer** section.
   - Click on **Add trigger**.
   - From the list of trigger sources, select **S3**.

4. **Configure S3 Trigger Settings**:
   - **Bucket**: Choose the S3 bucket where you will upload CSV files (e.g., `customer-data-bucket-unique-id`).
   - **Event type**: Select `PUT` to trigger the Lambda function whenever a new file is uploaded to the bucket.
   - **Prefix and Suffix (optional)**: If you want to trigger the Lambda function only for files with specific names or paths, you can specify a prefix or suffix. For example, if you want to process only CSV files, you might use `.csv` as a suffix.
   - **Enabled**: Ensure this checkbox is selected to activate the trigger.

5. **Add the Trigger**:
   - Click on **Add** to complete the trigger configuration.

6. **Save Changes**:
   - Click **Save** to apply the new trigger settings to your Lambda function.

## Step 7: Configure Lambda DLQ

1. **Navigate to AWS SQS Console**:
   - Open the [AWS Management Console](https://aws.amazon.com/console/).
   - Go to the **SQS** service.

2. **Create an SQS Queue** (if not already created):
   - Click on **Create queue**.
   - Choose **Standard** as the type.
   - Enter a **Name** for your queue (e.g., `LambdaDLQ`).
   - Configure other settings as required.
   - Click **Create Queue**.

3. **Configure the Lambda Function for DLQ**:
   - Go back to the **Lambda Console** and select your Lambda function.
   - In the function configuration page, scroll down to the **Asynchronous invocation** section.
   - Click on **Edit** next to the Dead-letter queue section.
   - Select **SQS** from the options.
   - Choose the SQS queue you created earlier (`LambdaDLQ`).

4. **Save DLQ Configuration**:
   - Click **Save** to apply the DLQ settings.

## Step 8: Test the Setup

1. **Upload a CSV file** with customer data to your S3 bucket.
2. Monitor the Lambda function execution and DynamoDB table to ensure records are processed and inserted correctly.
3. Check the DLQ for any failed messages.

# Bulk Upload from S3 to Lambda to DynamoDB with DLQ Handling

This guide provides a detailed, step-by-step walkthrough for configuring bulk uploads from S3 to Lambda to DynamoDB with error handling via a Dead Letter Queue (DLQ) using Amazon SQS. The process is implemented using the AWS Console and C#, targeting .NET Core 8. The Lambda function will parse a CSV file containing customer data, convert the data into JSON, and perform bulk uploads to DynamoDB. All resources will be created in the Mumbai region.

![AWS Beginner](AWS%20Beginners.png "AWS Beginner")

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
4. Provide a **Unique Bucket Name** (e.g., `customer-data-bucket-<unique-id>`) and choose the **Asia Pacific (Mumbai) ap-south-1** region.
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

### 1. Create IAM Role for Lambda

Before setting up your Lambda function, you need to create an IAM role with the necessary permissions.

#### Step-by-Step Guide to Create IAM Role:

1. **Navigate to IAM**:
   - Open the [AWS Management Console](https://aws.amazon.com/console/).
   - Go to the **IAM** service.

2. **Create a New Role**:
   - In the IAM dashboard, click on **Roles** in the left-hand menu.
   - Click on **Create role**.

3. **Select Trusted Entity**:
   - Choose **AWS Service** as the trusted entity.
   - Under **Use case**, select **Lambda**.
   - Click **Next** to proceed.

4. **Attach Permissions**:
   - Search for and select the following policies:
     - `AmazonS3ReadOnlyAccess` - To read from the S3 bucket.
     - `AmazonDynamoDBFullAccess` - To perform batch writes to DynamoDB.
     - `AmazonSQSFullAccess` - To send messages to the DLQ in case of errors.
     - `AWSLambdaBasicExecutionRole` - For logging Lambda function execution details.
   - Click **Next**.

5. **Name the Role**:
   - Enter a name for your role, e.g., `LambdaExecutionRoleForS3ToDynamoDB`.
   - Review the role settings and click **Create role**.

6. **Attach Role to Lambda**:
   - This role will be selected during the Lambda function creation process.

### 2. Create the Lambda Function

Now that you have the IAM role set up, you can create the Lambda function.

#### Step-by-Step Guide to Create Lambda Function:

1. **Navigate to Lambda**:
   - Open the [AWS Management Console](https://aws.amazon.com/console/).
   - Go to the **Lambda** service.

2. **Set the Region**:
   - Ensure the **Region** is set to **Asia Pacific (Mumbai) ap-south-1**.

3. **Create a New Lambda Function**:
   - Click on **Create function**.

4. **Choose How to Create the Function**:
   - Select **Author from scratch**.

5. **Configure Function Settings**:
   - **Function name**: Enter a name for your function, e.g., `S3CsvToDynamoDBLambda`.
   - **Runtime**: Choose **.NET Core 8** as the runtime.

6. **Set Permissions**:
   - Under **Permissions**, choose the IAM role you created earlier (`LambdaExecutionRoleForS3ToDynamoDB`).

7. **Create the Function**:
   - Click **Create function** to finalize the setup.

## Step 5: Create and Upload the Lambda Function Code

### 5.1 Write the Lambda Function Code

Create your Lambda function using the .NET Core 8 runtime. Ensure the code is designed to handle S3 events, parse CSV files, and interact with DynamoDB. For more details on writing the Lambda function code, consult the [AWS Lambda documentation](https://docs.aws.amazon.com/lambda/latest/dg/welcome.html).

### 5.2 Create a ZIP File for the Lambda Deployment

1. Run following command where your csproj file is stored in command prompt
`dotnet lambda package *.csproj -o bin/package.zip`

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
   - **Bucket**: Choose the S3 bucket where you will upload CSV files (e.g., `customer-data-bucket-<unique-id>`).
   - **Event type**: Select `PUT` to trigger the Lambda function whenever a new file is uploaded to the bucket.
   - **Prefix and Suffix (optional)**: If you want to trigger the Lambda function only for files with specific names or paths, you can specify a prefix or suffix. For example, if you want to process only CSV files, you might use `.csv` as a suffix.
   - **Enabled**: Ensure this checkbox is selected to activate the trigger.

5. **Add the Trigger**:
   - Click on **Add** to complete the trigger configuration.

6. **Save Changes**:
   - Click **Save** to apply the new trigger settings to your Lambda function.

## Step 7: Configure Lambda DLQ

1. **Configure the Lambda Function for DLQ**:
   - Go back to the **Lambda Console** and select your Lambda function.
   - In the function configuration page, scroll down to the **Asynchronous invocation** section.
   - Click on **Edit** next to the Dead-letter queue section.
   - Select **SQS** from the options.
   - Choose the SQS queue you created earlier (`LambdaDLQ`).

2. **Save DLQ Configuration**:
   - Click **Save** to apply the DLQ settings.

## Step 8: Test the Setup

1. **[Upload a CSV file](customer_data.csv "Upload a CSV file")** with customer data to your S3 bucket.
2. Monitor the Lambda function execution and DynamoDB table to ensure records are processed and inserted correctly.
3. Check the DLQ for any failed messages.

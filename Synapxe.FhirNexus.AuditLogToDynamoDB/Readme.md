# How to use FhirNexus audit logging feature?
This project is created to demostrate differet ways to log audit logs to different destinations, such as AWS CloudWatch, AWS SQS, and File.
Application logs are classified into three categories:
- `Inbound`: Http requests coming into the application.
- `Outbound`: Http requests going out of the application using HttpClient.
- `Application`: Any other logs other than Inbound and Outbound.

## Prerequisites
Create a test project from FhirNexus template and add the following changes to generate test logs.
1. Modify the `AppointmentDataFhirHandler.cs` file to generate application logs.
2. Modify the `capability-statement.json`, `fhirengine.json` and `appsettings.Development.json` to add remote Firely server as Patient datastore so as to generate outgoing http requests.
3. Modify the `appsettings.json` to change default log level to `Warning` and set the log level to `Information` for following three SourceContexts:
	- `Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware`, it is for Inbound logs.
	- `Ihis.FhirEngine.WebApi.OutboundHttpClientAuditLogger`, it is for Outbound logs.
	- `Synapxe.FhirNexus.AuditLogToDynamoDB.Handlers.AppointmentDataFhirHandler`, only allowed SourceContext for Application logs.

Check the commit history of this project to see the changes details.

Execute `FhirNexusAuditLogging.yaml` cloudformation template to create the required resources.

## Log to AWS CloudWatch
1. Add nuget packages `Ihis.FhirEngine.WebApi.Extensions.AuditLogging.Serilog.Aws` and `Serilog.Sinks.AwsCloudWatch` to the project. 
2. Setup AWS credentials in `launchSettings.json` file.
```json
{
  "profiles": {
    "Synapxe.FhirNexus.AuditLogToDynamoDB": {
      "environmentVariables": {
        "AWS_ACCESS_KEY_ID": "<your-aws-access-key-id>",
        "AWS_SECRET_ACCESS_KEY": "<your-aws-secret-access-key>"
      }
    }
  }
}
```
3. Add file `fhirengine-serilog.cloudwatch.json`
4. Update `appsettings.json` to use `fhirengine-serilog.cloudwatch.json` file.
```json
{
  "FhirEngineSerilog": {
    "AdditionalConfigFile": "fhirengine-serilog.cloudwatch.json",
  }
}
```
5. Launch the application and test the logging funtionality using the `appointment.http` and `patient.http` files. 

## Log to AWS SQS
1. Add nuget packages `Ihis.FhirEngine.WebApi.Extensions.AuditLogging.Serilog.Aws` to the project.
2. Setup AWS credentials in `launchSettings.json` file.
```json
{
  "profiles": {
    "Synapxe.FhirNexus.AuditLogToDynamoDB": {
      "environmentVariables": {
        "AWS_ACCESS_KEY_ID": "<your-aws-access-key-id>",
        "AWS_SECRET_ACCESS_KEY": "<your-aws-secret-access-key>"
      }
    }
  }
}
```
3. Update `appsettings.Development.json` to add SQS connection strings.
4. Add file `fhirengine-serilog.sqs.json`
5. Update `appsettings.json` to use `fhirengine-serilog.sqs.json` file.
```json
{
  "FhirEngineSerilog": {
    "AdditionalConfigFile": "fhirengine-serilog.sqs.json",
  }
}
```
6. Launch the application and test the logging funtionality using the `appointment.http` and `patient.http` files. 

## Log to AWS DynamoDB
In addition to logging to AWS SQS, you can create an AWS Lambda function to log the audit logs to AWS DynamoDB. For implementation details, refer to the `lambda_function.py` file in the `Ihis.FhirEngine.WebApi.Extensions.AuditLogging.Serilog.Aws` nuget package.

## Log to File
1. Add file `fhirengine-serilog.file.json`
2. Update `appsettings.json` to use `fhirengine-serilog.file.json` file.
```json
{
  "FhirEngineSerilog": {
    "AdditionalConfigFile": "fhirengine-serilog.file.json",
  }
}
```
3. Launch the application and test the logging funtionality using the `appointment.http` and `patient.http` files.

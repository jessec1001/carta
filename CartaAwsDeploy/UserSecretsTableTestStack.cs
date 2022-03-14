using Amazon.CDK;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Cognito;

using System.Collections.Generic;
using System;

namespace CartaAwsDeploy
{
    //TODO: This entire class is just for internal testing; this code should be moved to the Resource Stack
    public class UserSecretsTableTestStack : Stack
    {
        internal UserSecretsTableTestStack(Construct scope, string id, IStackProps props = null) :
            base(scope, id, props)
        {
            // Setup the environment
            Environment environment = new Environment();

            // Create the DynamoDB table for user secrets
            TableProps secretsTableProps = new TableProps
            {
                PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
                {
                    Name = "UserId",
                    Type = AttributeType.STRING
                },
                SortKey = new Amazon.CDK.AWS.DynamoDB.Attribute
                {
                    Name = "Key",
                    Type = AttributeType.STRING
                },
                TimeToLiveAttribute = "TTL",
                // Allows for consistent reads of up to 4KB - secrets are unlikely to be larger than this
                ReadCapacity = 1,
                WriteCapacity = 1,
                RemovalPolicy = RemovalPolicy.DESTROY
            };
            Table secretsDynamoDbTable = new Table(this, "CartaSecretsTable", secretsTableProps);

            User user = new User(this, "CartaSecretsTestUser"); //TODO: This should be the CartaUser, and is just for initial testing
            secretsDynamoDbTable.Grant( //TODO: this is for initial testing, need to protect this by user key
                user,
                new string[] { "dynamodb:PutItem", "dynamodb:GetItem", "dynamodb:DescribeTable" });
            
            // Output the results of the deployment
            new CfnOutput(this, "SecretsDynamoDBTable:", new CfnOutputProps() { Value = secretsDynamoDbTable.TableName });


            // TODO: This is just temporary for testing
            CfnAccessKey accessKey =
                new CfnAccessKey(this, "accessKey", new CfnAccessKeyProps { UserName = user.UserName });
            new CfnOutput(this, "TestAccessKey:", new CfnOutputProps() { Value = accessKey.Ref });
            new CfnOutput(this, "TestSecretKey:", new CfnOutputProps() { Value = accessKey.AttrSecretAccessKey });
        }
    }
}

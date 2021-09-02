using Amazon.CDK;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Cognito;

namespace CartaAwsDeploy
{
    public class CartaAwsDeployStack : Stack
    {
        internal CartaAwsDeployStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            // Create the Carta programmatic user
            User user = new User(this, "DevUser");
            CfnAccessKey accessKey =
                new CfnAccessKey(this, "accessKey", new CfnAccessKeyProps { UserName = user.UserName });


            // Create the DynamoDB table
            Table dynamoDbTable = new Table(this, "CartaTable", new TableProps
            {
                PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute
                {
                    Name = "PK",
                    Type = AttributeType.STRING
                },
                SortKey = new Amazon.CDK.AWS.DynamoDB.Attribute
                {
                    Name = "SK",
                    Type = AttributeType.STRING
                },
                ReadCapacity = 5,
                WriteCapacity = 5,
                RemovalPolicy = RemovalPolicy.DESTROY
            });

            // Add permissions to the Carta programmatic user to access the DynamoDB table
            dynamoDbTable.GrantFullAccess(user);

            // Create the Cognito user pool and client and identity provider
            UserPool userPool = new UserPool(this, "Development User Pool", new UserPoolProps
            {
                SelfSignUpEnabled = true,
                StandardAttributes = new StandardAttributes
                {
                    Email = new StandardAttribute { Required = true, Mutable = true},
                    GivenName = new StandardAttribute { Required = true, Mutable = true },
                    FamilyName = new StandardAttribute { Required = true, Mutable = true }
                },
                AccountRecovery = AccountRecovery.EMAIL_AND_PHONE_WITHOUT_MFA,
                AutoVerify = new AutoVerifiedAttrs { Email = true, Phone = false},
                RemovalPolicy = RemovalPolicy.DESTROY
            });
            UserPoolClient userPoolClient =
                new UserPoolClient(this, "Development User Pool Client", new UserPoolClientProps
                {
                    UserPool = userPool,
                    GenerateSecret = false,
                    AuthFlows = new AuthFlow
                    {
                        UserPassword = true,
                        Custom = false,
                        UserSrp = false,
                        AdminUserPassword = false
                    },
                    OAuth = new OAuthSettings
                    {
                        CallbackUrls = new[] { "https://localhost:5001/signin-oidc" },
                        Flows = new OAuthFlows { AuthorizationCodeGrant = true },
                        Scopes = new[] { OAuthScope.EMAIL, OAuthScope.OPENID, OAuthScope.OPENID, OAuthScope.PROFILE}
                    },
                    SupportedIdentityProviders = new[] { UserPoolClientIdentityProvider.COGNITO }
                });
            string username = id.Split('-')[1];
            userPool.AddDomain("Development User Pool Domain", new UserPoolDomainOptions
            {
                CognitoDomain = new CognitoDomainOptions { DomainPrefix = $"cartadevcdk-{username}"}
            });
            CfnIdentityPool identityPool = new CfnIdentityPool(this, "Development Identity Pool", new CfnIdentityPoolProps
            {
                AllowUnauthenticatedIdentities = false,
                CognitoIdentityProviders = new CfnIdentityPool.CognitoIdentityProviderProperty[]
                {
                    new CfnIdentityPool.CognitoIdentityProviderProperty
                    {
                        ClientId = userPoolClient.UserPoolClientId,
                        ProviderName = userPool.UserPoolProviderName
                }}
            });

            // Attach permissions the Carta programmatic user to acess the user pool
            PolicyStatement policyStatement = new PolicyStatement(new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                Actions = new[]
                {
                    "cognito-idp:AdminInitiateAuth",
                    "cognito-idp:ListUsersInGroup",
                    "cognito-idp:ListGroups",
                    "cognito-idp:ListUsers",
                    "cognito-idp:AdminCreateUser",
                    "cognito-idp:AdminSetUserPassword",
                    "cognito-idp:AdminGetUser"
                },
                Resources = new[] {$"arn:aws:cognito-idp:{Region}:{Account}:userpool/{userPool.UserPoolId}" }
            }); ;
            user.AddToPolicy(policyStatement);

            // Output results of the CDK Deployment for use by CartaWeb
            new CfnOutput(this, "AccessKey:", new CfnOutputProps() { Value = accessKey.Ref });
            new CfnOutput(this, "SecretKey:", new CfnOutputProps() { Value = accessKey.AttrSecretAccessKey });
            new CfnOutput(this, "RegionEndpoint:", new CfnOutputProps() { Value = Region });
            new CfnOutput(this, "DynamoDBTable:", new CfnOutputProps() { Value = dynamoDbTable.TableName });
            new CfnOutput(this, "UserPoolId:", new CfnOutputProps() { Value = userPool.UserPoolId });
            new CfnOutput(this, "UserPoolClientId:", new CfnOutputProps() { Value = userPoolClient.UserPoolClientId });

        }
    }
}

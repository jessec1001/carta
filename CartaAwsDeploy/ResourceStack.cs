using Amazon.CDK;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Cognito;

using System.Collections.Generic;
using System;

namespace CartaAwsDeploy
{
    public class ResourceStack : Stack
    {
        internal ResourceStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            // Setup the environment
            Environment environment = new Environment();
        
            // Create the DynamoDB table
            TableProps tableProps = new TableProps
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
            };
            if (environment.AccountType == AccountType.PRODUCTION)
                tableProps.PointInTimeRecovery = true;
            Table dynamoDbTable = new Table(this, "CartaTable", tableProps);
            
            // Create the Cognito user pool and client and identity provider
            UserPool userPool = new UserPool(this, "CartaUserPool", new UserPoolProps
            {
                SelfSignUpEnabled = false,
                StandardAttributes = new StandardAttributes
                {
                    Email = new StandardAttribute { Required = true, Mutable = true},
                    GivenName = new StandardAttribute { Required = true, Mutable = true },
                    FamilyName = new StandardAttribute { Required = true, Mutable = true }
                },
                CustomAttributes = new Dictionary<string, ICustomAttribute>
                {
                    {
                        "tenantId",
                        new StringAttribute(new StringAttributeProps { MinLen = 2, MaxLen = 50, Mutable = true })
                    }
                },
                PasswordPolicy = new PasswordPolicy
                {
                    MinLength = 16,
                    RequireLowercase = true,
                    RequireDigits = true,
                    RequireUppercase = true,
                    RequireSymbols = true
                },
                UserVerification = new UserVerificationConfig
                {
                    EmailSubject = "Your Carta Verification Code",
                    EmailBody =
                    "<div style=\"text-align: center; font-family: sans-serif\"> " +
                        "<img src=\"https://carta.contextualize.us.com/carta.png\" style=\"height: 4rem; margin-bottom: 2rem\" />" +
                        "<h1>Carta Verification Code</h1>" +
                        "<p style=\"font-size: 1.1rem\">" +
                            "Your verification code is below. <br />" +
                            "<span style=\"font-size: 2rem\">{####}</span>" +
                        "</p>" +
                        "<p style=\"padding: 1rem 0rem; font-size: 1.5rem\">" +
                            "We hope you enjoy your stay." +
                        "</p>" +
                    "</div>",
                    EmailStyle = VerificationEmailStyle.CODE
                },
                UserInvitation = new UserInvitationConfig
                {
                    EmailSubject = "Welcome to Carta!",
                    EmailBody =
                    "<div style=\"text-align: center; font-family: sans-serif\"> " +
                        "<img src=\"https://carta.contextualize.us.com/carta.png\" style=\"height: 4rem; margin-bottom: 2rem\" />" +
                        "<h1>Welcome to Carta</h1>" +
                        "<p style=\"font-size: 1.1rem\">" +
                            "Your username is: <br />" +
                            "<span style=\"font-size: 2rem\">{username}</span>" +
                        "</p>" +
                        "<p style=\"font-size: 1.1rem\">" +
                            "Your temporary password is: <br />" +
                            "<span style=\"font-size: 2rem\">{####}</span>" +
                        "</p>" +
                        "<p style=\"padding: 1rem 0rem; font-size: 1.5rem\">" +
                            "We hope you enjoy your stay." +
                        "</p>" +
                    "</div>"
                },
                AccountRecovery = AccountRecovery.EMAIL_AND_PHONE_WITHOUT_MFA,
                AutoVerify = new AutoVerifiedAttrs { Email = true, Phone = false},
                RemovalPolicy = RemovalPolicy.DESTROY
            });
            string callbackUrl = this.Node.TryGetContext($"{environment.ContextPrefix}:cognitoCallbackUrl").ToString();
            string[] callbackUrls;
            if (environment.AccountType == AccountType.DEVELOPMENT)
                callbackUrls = new[] { "https://localhost:5001/signin-oidc", callbackUrl };
            else
                callbackUrls = new[] { callbackUrl };
            UserPoolClient userPoolClient =
                new UserPoolClient(this, "CartaUserPoolClient", new UserPoolClientProps
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
                        CallbackUrls = callbackUrls,
                        Flows = new OAuthFlows { AuthorizationCodeGrant = true },
                        Scopes = new[] { OAuthScope.EMAIL, OAuthScope.OPENID, OAuthScope.OPENID, OAuthScope.PROFILE }
                    },
                    SupportedIdentityProviders = new[] { UserPoolClientIdentityProvider.COGNITO },
                    WriteAttributes = new ClientAttributes().
                        WithStandardAttributes(new StandardAttributesMask
                        {
                            Email = true,
                            FamilyName = true,
                            GivenName = true
                        }).
                        WithCustomAttributes( new string[] { "custom:tenantId" })
                }) ;
            string domainPrefix =
                this.Node.TryGetContext($"{environment.ContextPrefix}:cognitoUserPoolDomainPrefix").ToString();
            UserPoolDomain userPoolDomain = new UserPoolDomain(this, "CartaUserPoolDomain", new UserPoolDomainProps
            {
                UserPool = userPool,
                CognitoDomain = new CognitoDomainOptions { DomainPrefix = domainPrefix }
            });
            CfnIdentityPool identityPool =
                new CfnIdentityPool(this, "CartaIdentityPool", new CfnIdentityPoolProps
            {
                AllowUnauthenticatedIdentities = false,
                CognitoIdentityProviders = new CfnIdentityPool.CognitoIdentityProviderProperty[]
                {
                    new CfnIdentityPool.CognitoIdentityProviderProperty
                    {
                        ClientId = userPoolClient.UserPoolClientId,
                        ProviderName = userPool.UserPoolProviderName
                    }
                }
            });

            // Create policy with permissions to acess the user pool
            PolicyStatement cognitoPolicyStatement = new PolicyStatement(new PolicyStatementProps
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
            });

            // Create policy with permissions to add a Cloudwatch logging group
            PolicyStatement createLogCloudwatchPolicyStatement = new PolicyStatement(new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                Actions = new[]
                {
                    "logs:CreateLogStream",
                    "logs:DescribeLogGroups",
                    "logs:CreateLogGroup"
                },
                Resources = new[] { $"arn:aws:logs:*:{Account}:log-group:*" }
            });

            // Create policy with permissions to log Cloudwatch events
            PolicyStatement putEventsCloudwatchPolicyStatement = new PolicyStatement(new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                Actions = new[]
                {
                    "logs:PutLogEvents"
                },
                Resources = new[] { $"arn:aws:logs:*:{Account}:log-group:*:log-stream:*" }
            });

            // Add DynamoDB, Cognito and Cloudwatch permissions to user/role
            // Also output keys for the development environment
            if (environment.AccountType == AccountType.DEVELOPMENT)
            {
                User user = new User(this, "CartaUser");
                dynamoDbTable.GrantFullAccess(user);
                user.AddToPolicy(cognitoPolicyStatement);

                CfnAccessKey accessKey =
                    new CfnAccessKey(this, "accessKey", new CfnAccessKeyProps { UserName = user.UserName });
                new CfnOutput(this, "AccessKey:", new CfnOutputProps() { Value = accessKey.Ref });
                new CfnOutput(this, "SecretKey:", new CfnOutputProps() { Value = accessKey.AttrSecretAccessKey });

            }
            IRole role =
                Role.FromRoleArn(this, "CartaEBRole", $"arn:aws:iam::{Account}:role/aws-elasticbeanstalk-ec2-role");
            dynamoDbTable.GrantFullAccess(role);
            role.AddToPrincipalPolicy(cognitoPolicyStatement);
            role.AddToPrincipalPolicy(createLogCloudwatchPolicyStatement);
            role.AddToPrincipalPolicy(putEventsCloudwatchPolicyStatement);

            // Output the results of the deployment
            new CfnOutput(this, "RegionEndpoint:", new CfnOutputProps() { Value = Region });
            new CfnOutput(this, "DynamoDBTable:", new CfnOutputProps() { Value = dynamoDbTable.TableName });
            new CfnOutput(this, "UserPoolId:", new CfnOutputProps() { Value = userPool.UserPoolId });
            new CfnOutput(this, "UserPoolClientId:", new CfnOutputProps() { Value = userPoolClient.UserPoolClientId });
        }
    }
}
    
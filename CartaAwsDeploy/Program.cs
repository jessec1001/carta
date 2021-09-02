using Amazon.CDK;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using System.Threading.Tasks;
using System;
using System.Text.RegularExpressions;

namespace CartaAwsDeploy
{
    sealed class Program
    {
        public static async Task Main(string[] args)
        {
            // Get username of user deploying the stack
            AmazonSecurityTokenServiceClient client = new AmazonSecurityTokenServiceClient();
            GetCallerIdentityResponse response = await client.GetCallerIdentityAsync(new GetCallerIdentityRequest{});
            string arn = response.Arn;
            string[] arnWords = arn.Split('/');
            string userName = arnWords[arnWords.Length-1];

            // Strip out non-alpha numeric characters to comply with Cloudformation stack name requirements
            userName = Regex.Replace(userName, "[^a-zA-Z0-9]", String.Empty);

            // Synthesize the stack
            App app = new App();
            new CartaAwsDeployStack(app, $"CartaAwsDeployStack-{userName}", new StackProps{});
            app.Synth();
        }
    }
}

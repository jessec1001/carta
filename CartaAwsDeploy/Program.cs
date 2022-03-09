using Amazon.CDK;

namespace CartaAwsDeploy
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            // Create the app
            App app = new App();

            // Get a stack suffix (defaults to blank) - this can be used in the dev environment to create unique
            // stacks for e.g. each user. 
            string stackSuffix = "";
            object stackSuffixProperty = app.Node.TryGetContext($"stackSuffix");
            if (stackSuffixProperty != null)
                stackSuffix = "-" + stackSuffix + stackSuffixProperty.ToString();

            // Create the stacks
            new ElasticBeanstalkStack( app, $"ElasticBeanstalkStack{stackSuffix}", new StackProps { });
            new ResourceStack( app, $"ResourceStack{stackSuffix}", new StackProps { });
            new UserSecretsTableTestStack(app, $"TestStack{stackSuffix}", new StackProps { }); //TODO: remove this line after internal testing

            // Synthesize the stacks
            app.Synth();
        }

    }
}

using Amazon.CDK;
using Amazon.CDK.AWS.S3.Assets;
using Amazon.CDK.AWS.ElasticBeanstalk;

using System.IO;
using System;

namespace CartaAwsDeploy
{
    public class ElasticBeanstalkStack : Stack
    {
        internal ElasticBeanstalkStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            // Setup
            Environment environment = new Environment();
            string[] files = Directory.GetFiles(".", "package*.zip");
            if (files.Length != 1)
            {
                Console.WriteLine("Exactly one file matching package*.zip must exist. The Elastic Beanstalk " +
                    "stack will not be executed.");
                return;
            }

            string file = files[0];
            Asset releaseZip = new Asset(this, Path.GetFileNameWithoutExtension(file), new AssetProps { Path = file });
            string appName = "CartaWeb";

            // Create the application
            CfnApplication application = new CfnApplication(this, "Application") { ApplicationName = appName };

            // Create the environment option settings
            string certificateId =
                this.Node.TryGetContext($"{environment.ContextPrefix}:elasticBeanstalkCertificate").ToString();
            string instanceType =
                this.Node.TryGetContext($"{environment.ContextPrefix}:elasticBeanstalkInstanceType").ToString();
            CfnEnvironment.OptionSettingProperty[] optionSettings = new[]
            {
                new CfnEnvironment.OptionSettingProperty
                {
                   Namespace = "aws:autoscaling:launchconfiguration",
                   OptionName = "IamInstanceProfile",
                   Value = "aws-elasticbeanstalk-ec2-role"
                },
                new CfnEnvironment.OptionSettingProperty
                {
                   Namespace = "aws:elasticbeanstalk:environment",
                   OptionName = "ServiceRole",
                   Value = "aws-elasticbeanstalk-service-role",
                },
                new CfnEnvironment.OptionSettingProperty
                {
                   Namespace = "aws:elasticbeanstalk:environment",
                   OptionName = "EnvironmentType",
                   Value = "LoadBalanced"
                },
                new CfnEnvironment.OptionSettingProperty
                {
                   Namespace = "aws:elasticbeanstalk:environment",
                   OptionName = "LoadBalancerType",
                   Value = "application"
                },
                new CfnEnvironment.OptionSettingProperty
                {
                   Namespace = "aws:elbv2:listener:443",
                   OptionName = "ListenerEnabled",
                   Value = "true"
                },
                new CfnEnvironment.OptionSettingProperty
                {
                   Namespace = "aws:elbv2:listener:443",
                   OptionName = "Protocol",
                   Value = "HTTPS"
                },
                new CfnEnvironment.OptionSettingProperty
                {
                   Namespace = "aws:elbv2:listener:443",
                   OptionName = "SSLCertificateArns",
                   Value = $"arn:aws:acm:{Region}:{Account}:certificate/{certificateId}"
                },
                new CfnEnvironment.OptionSettingProperty
                {
                   Namespace = "aws:elbv2:listener:443",
                   OptionName = "DefaultProcess",
                   Value = "default"
                },
                new CfnEnvironment.OptionSettingProperty
                {
                   Namespace = "aws:elasticbeanstalk:managedactions",
                   OptionName = "ManagedActionsEnabled",
                   Value = "true"
                },
                new CfnEnvironment.OptionSettingProperty
                {
                   Namespace = "aws:elasticbeanstalk:managedactions",
                   OptionName = "PreferredStartTime",
                   Value = "Tue:09:00"
                },
                new CfnEnvironment.OptionSettingProperty
                {
                   Namespace = "aws:elasticbeanstalk:managedactions:platformupdate",
                   OptionName = "UpdateLevel",
                   Value = "minor"
                },
                new CfnEnvironment.OptionSettingProperty
                {
                   Namespace = "aws:elasticbeanstalk:managedactions:platformupdate",
                   OptionName = "InstanceRefreshEnabled",
                   Value = "true"
                },
                new CfnEnvironment.OptionSettingProperty
                {
                   Namespace = "aws:ec2:instances",
                   OptionName = "InstanceTypes",
                   Value = instanceType
                },
                new CfnEnvironment.OptionSettingProperty
                {
                   Namespace = "aws:elasticbeanstalk:sns:topics",
                   OptionName = "Notification Endpoint",
                   Value = "aws.production@contextualize.us.com"    
                }
            };

            // Create the application version
            // Note: CDK will strip all dots and dashes from the version when it creates the Version label,
            // which means a version of v0.2.2 and v0.22 both become v022.
            // For this reason, rather use the commit prefix to denote version - this also
            // aligns with continous deployment.
            // The version must conform to the reg expression /^[A-Za-z][A-Za-z0-9]{1,254}$/,
            // and cannot be null or empty - else the CDK deploy and destroy runtime cmd fails.
            string version = "commit00000000"; 
            object commitShaProperty = this.Node.TryGetContext("commitSHA");
            if (commitShaProperty != null)
                version = $"commit{commitShaProperty.ToString()}";
            CfnApplicationVersion applicationVersion =
                new CfnApplicationVersion(this, version, new CfnApplicationVersionProps
                {
                    ApplicationName = appName,
                    // Set description to ease tracing release back to a release commit tag 
                    Description = Path.GetFileNameWithoutExtension(file),
                    SourceBundle = new CfnApplicationVersion.SourceBundleProperty
                    {
                        S3Bucket = releaseZip.S3BucketName,
                        S3Key = releaseZip.S3ObjectKey
                    }
                });

            // Create the environment with application
            CfnEnvironment cfnEnvironment = new CfnEnvironment(this, "Environment", new CfnEnvironmentProps
            {
                EnvironmentName = "Carta",
                ApplicationName = appName,
                SolutionStackName = "64bit Amazon Linux 2 v2.2.8 running .NET Core",
                OptionSettings = optionSettings,
                VersionLabel = applicationVersion.Ref
            });
            applicationVersion.AddDependsOn(application);

        }
    }
}

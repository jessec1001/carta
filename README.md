# Carta
[![Commitizen friendly](https://img.shields.io/badge/commitizen-friendly-brightgreen.svg)](http://commitizen.github.io/cz-cli/)
[![Code coverage](https://gitlab.com/contextualize/carta/-/jobs/artifacts/master/raw/coverage/badge_shieldsio_linecoverage_blue.svg?job=docs-coverage)](https://contextualize.gitlab.io/carta/coverage/)

Carta is a web-based API and application that provides graph-based tools for accessing, exploring, and transforming existing datasets and models. 

In this repository, there are three projects that implement the Carta infrastructure. All of the projects utilize the .NET Core framework and are written in [C# 9.0](https://docs.microsoft.com/en-us/dotnet/csharp/).
- `CartaCore` contains the primary data structures and algorithms that allow Carta to run.
- `CartaTest` contains unit tests written using the [NUnit3](https://nunit.org/) framework to test the functionality of `CartaCore`.
- `CartaWeb` contains Model-View-Controller (MVC) logic that runs a web API and a client-facing interface. `CartaWeb` is written using [ASP.NET](https://dotnet.microsoft.com/apps/aspnet) on the backend and [ReactJS](https://reactjs.org/) on the frontend.

## Requisites
In order to run the Carta projects locally, you will need to have [.NET 5.0 SDK](https://dotnet.microsoft.com/download/dotnet/5.0) installed. It is installable on Windows, Linux, and Mac.

To check if .NET Core was successfully installed, you can run:
```bash
dotnet --list-sdks
```
This should produce a version matching `5.0.x`.

## Usage
The `CartaCore` project is only built and referenced by `CartaTest` and `CartaWeb`. It is not executable by itself. You can run tests and the web application by following the instructions in the proceding sections.

### Test
The unit tests require a local DynamoDB docker instance to be running:
```bash
docker pull amazon/dynamodb-local
docker run -p 8000:8000 -d amazon/dynamodb-local
```
To run the unit tests from the root directory or the `CartaTest` directory, you can run:
```bash
dotnet test
```
To stop the docker instance after the tests have run, first identify the container ID:
```bash
docker ps
```
Stop the docker instance:
```bash
docker stop containerID
```

### Web
To run the web API and application from the root directory, you can run:
```bash
dotnet run --project CartaWeb
```

Alternatively, you can work from within the `CartaWeb` directory:
```bash
cd CartaWeb
dotnet run
```

Note: This application runs in the foreground of a terminal. If the terminal is closed, the application will stop serving requests.

#### Options
By default, the HTTP server is hosted on http://localhost:5000 and the HTTPS server is hosted on http://localhost:5001. To specify different IP addresses, you can use the `--urls` option. For instance,
```bash
dotnet run --project CartaWeb --urls "http://127.0.0.1:8081;https://127.0.0.1:8082"
```
runs the HTTP server on http://localhost:8081 and the HTTPS server on https://localhost:8082.

Also,
```bash
dotnet run --project CartaWeb --urls "http://*:8081;https://*:8082"
```
exposes the application to the local network. From there, you could use [Port Forwarding](https://www.howtogeek.com/66214/how-to-forward-ports-on-your-router/) to allow remote connections from external networks from ports 8081 (HTTP) and 8082 (HTTPS).

Through the AWS CDK, Carta can also be run locally with a sandboxed AWS environment. See [Running Carta with CDK](https://gitlab.com/contextualize/carta/-/wikis/Running-Carta-with-CDK) for more information.

## Standards
We use standards of coding in order to stay consistent and allow our code to be easily understood.

- We try to use [Commitizen](https://github.com/commitizen/cz-cli)-friendly commit messages.

## Attributions
We use the following [NuGet](https://www.nuget.org/) packages:
- [`QuikGraph`](https://www.nuget.org/packages/QuikGraph/)
- [`QuikGraph.Serialization`](https://www.nuget.org/packages/QuikGraph.Serialization/)
- [`NUnit`](https://www.nuget.org/packages/NUnit/)
- [`NUnit3TestAdapter`](https://www.nuget.org/packages/NUnit3TestAdapter/4.0.0-beta.1)
- [`coverlet.msbuild`](https://www.nuget.org/packages/coverlet.msbuild/)

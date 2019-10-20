#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0
#addin "Cake.Docker"
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var buildDir = Directory("./src/Web/bin") + Directory(configuration);

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore("./src/CQRS_Proj.sln");
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    if(IsRunningOnWindows())
    {
      // Use MSBuild
      MSBuild("./src/CQRS_Proj.sln", settings =>
        settings.SetConfiguration(configuration));
    }
    else
    {
      // Use XBuild
      XBuild("./src/CQRS_Proj.sln", settings =>
        settings.SetConfiguration(configuration));
    }
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    NUnit3("./src/**/bin/" + configuration + "/*.UnitTests.dll", new NUnit3Settings {
        NoResults = true
        });
});

Task("Run-Docker-Integration-Tests")
    .Does(() => {
        DockerComposeUp("");
    });

Task("Run-test-env1")
    .Does(() => {
	var dockerComposeFile = "./src/docker-compose.yml";
        DockerComposeUp(new DockerComposeUpSettings(){Files = new string[]{dockerComposeFile}, DetachedMode = true}, "mongodb rabbitmq eventst");

        var retry = 3;
        while(retry > 0){
            try
            {
                DockerComposeExec(new DockerComposeExecSettings(){DisablePseudoTTYAllocation = true, Files = new string[]{dockerComposeFile}}, 
                "mongodb", "mongoimport --db test --collection AuctionsReadModel --drop --type json --file docker-entrypoint-initdb.d/test-env1/test_auctions.json --jsonArray");
                DockerComposeExec(new DockerComposeExecSettings(){DisablePseudoTTYAllocation = true, Files = new string[]{dockerComposeFile}}, 
                "mongodb", "mongoimport --db test --collection UsersReadModel --drop --type json --file docker-entrypoint-initdb.d/test-env1/test_users.json --jsonArray");
                Information("MongoDb data was successuflly imported");
                return;
            }
            catch (System.Exception)
            {
                Information("Cannot run mongoimport. Retrying in 5 seconds...");
                System.Threading.Thread.Sleep(5000);
                retry--;
            }
            if(retry == 0){
                Error("Could not import test-env1 data");
                DockerComposeDown(new DockerComposeDownSettings(){Files = new string[]{dockerComposeFile}});
                throw new Exception();
            }
        }
    });


Task("Run-infrastructure-test-env")
    .Does(() => {
	    var dockerComposeFile = "./src/docker-compose.yml";
        DockerComposeUp(new DockerComposeUpSettings(){Files = new string[]{dockerComposeFile}, DetachedMode = true}, "mongodb rabbitmq eventst");
    });

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Run-Unit-Tests");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);

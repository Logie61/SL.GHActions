using System.IO.Compression;

using static Bullseye.Targets;
using static SimpleExec.Command;

var solutionName = "SL.GHActions";

Project[] toPublish =
    [
        $"{solutionName}.Web/{solutionName}.Web.csproj",
    ];

Target("clean", async () =>
{
    if (Directory.Exists(Path.GetFullPath("artifacts")))
    {
        await TryDeleteDirectory(Path.GetFullPath($"artifacts"));
    }

    Run("dotnet", "clean -c Release");
});

Target("build", dependsOn: ["clean"], () => Run("dotnet", $"build ./SL.GHActions.Web/SL.GHActions.Web.csproj -c Release"));

Target("publish", dependsOn: ["build", "test"], forEach: toPublish, action: project =>
{
    Console.WriteLine($"Publishing {project}");
    Run("dotnet", $"publish {project.Path} --configuration Release --no-build --nologo --output \"{Path.GetFullPath($"artifacts/{project.OutputDirectoryName()}")}\"");
});

Target("test", () => Run("dotnet", "test"));

Target("pr", dependsOn: ["test"]);

Target("publish-and-zip",
    dependsOn: ["publish"],
    forEach: toPublish, action: async project =>
    {
        Console.WriteLine($"Zipping {project}");
        ZipFile.CreateFromDirectory(Path.GetFullPath($"artifacts/{project.OutputDirectoryName()}"), Path.GetFullPath($"artifacts/{project.OutputDirectoryName()}.zip"));

        Console.WriteLine($"Deleting {project} publish directory");
        await TryDeleteDirectory(Path.GetFullPath($"artifacts/{project.OutputDirectoryName()}"));
    });
args = args.Where(arg => arg != null).ToArray();

await RunTargetsAndExitAsync(args, ex => ex is SimpleExec.ExitCodeException);
return;

// Local Function
static async Task<bool> TryDeleteDirectory(
   string directoryPath,
   int maxRetries = 10,
   int millisecondsDelay = 30)
{
    ArgumentNullException.ThrowIfNull(directoryPath);
    ArgumentOutOfRangeException.ThrowIfLessThan(maxRetries, 1);
    ArgumentOutOfRangeException.ThrowIfLessThan(millisecondsDelay, 1);

    for (int i = 0; i < maxRetries; ++i)
    {
        try
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }

            return true;
        }
        catch (IOException)
        {
            await Task.Delay(millisecondsDelay);
        }
        catch (UnauthorizedAccessException)
        {
            await Task.Delay(millisecondsDelay);
        }
    }

    return false;
}

file sealed record Project(string Path)
{
    public static implicit operator Project(string path) => new(path);

    public string OutputDirectoryName() => ToString().Replace(".csproj", string.Empty);

    public override string ToString()
    {
        return this.Path.Split('/').Last();
    }
}
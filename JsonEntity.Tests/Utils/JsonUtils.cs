namespace JsonEntity.Tests.Utils;

internal class JsonUtils
{
    public static async Task CreateJson(string entityName = "Foo")
    {
        var directory = Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Data"));
        using var stream = File.Create(Path.Combine(directory.FullName, $"{entityName}.json"));
        await stream.DisposeAsync();
    }

    public static void DeleteJson(string entityName = "Foo")
    {
        var directory = Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Data"));
        File.Delete(Path.Combine(directory.FullName, $"{entityName}.json"));
        Directory.Delete(directory.FullName);
    }
}

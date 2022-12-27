namespace JsonEntity.Configurations;

public sealed record JsonEntityConfiguration<T>
{
    public string JsonPath { get; }      
    public T Entity { get; init; }

    private JsonEntityConfiguration(string jsonPath)
    {
        JsonPath = jsonPath;
    }

    public static JsonEntityConfiguration<T> Create(string jsonPath)
    {
        return new JsonEntityConfiguration<T>(jsonPath);
    }
}

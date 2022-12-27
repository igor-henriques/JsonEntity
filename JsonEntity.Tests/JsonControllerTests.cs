[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]

namespace JsonEntity.Tests;

public sealed class JsonControllerTests
{
    internal sealed record Foo : IBaseJsonEntity
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long RandomNumber { get; set; }
    }
    
    private readonly Faker<Foo> FooFaker;
    private readonly IJsonController<Foo> _jsonController;
    private readonly string _fileDirectory;

    public JsonControllerTests()
    {
        this.FooFaker = new Faker<Foo>()
            .RuleFor(f => f.Id, g => g.IndexFaker)
            .RuleFor(f => f.Name, g => g.Person.FirstName)
            .RuleFor(f => f.RandomNumber, g => g.Random.Long());

        var directory = Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Data"));
        this._fileDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Data");

        _jsonController = new JsonController<Foo>(
            JsonEntityConfiguration<Foo>.Create(Path.Combine(Directory.GetCurrentDirectory(), "Data", $"{nameof(Foo)}.json")),
            GetLogger());      
    }

    private static ILogger<JsonController<Foo>> GetLogger() => Mock.Of<ILogger<JsonController<Foo>>>();

    [Fact]
    public void Implement_Interface_Without_File_Created_Should_Throw_Exception()
    {
        var logMock = Mock.Of<ILogger<JsonController<Foo>>>();        

        var controllerImplementation = () => new JsonController<Foo>(
            JsonEntityConfiguration<Foo>.Create(Path.Combine(Directory.GetCurrentDirectory(), "Data", $"{nameof(Foo)}.json")),
        GetLogger());

        Directory.Delete(_fileDirectory);
        controllerImplementation.Should().Throw<Exception>();
    }

    [Fact]
    public async Task Implement_Interface_With_File_Created_Should_Succeed()
    {
        await JsonUtils.CreateJson(nameof(Foo));

        var controllerImplementation = () => new JsonController<Foo>(
            JsonEntityConfiguration<Foo>.Create(Path.Combine(Directory.GetCurrentDirectory(), "Data", $"{nameof(Foo)}.json")),
            GetLogger());
        
        controllerImplementation.Should().NotThrow<Exception>();

        JsonUtils.DeleteJson(nameof(Foo));
    }

    [Fact]
    public async Task Add_Record_With_AutoGenerateId_Should_Succeed()
    {
        await JsonUtils.CreateJson(nameof(Foo));        

        var record = FooFaker.Generate(1)[0] with { Id = 5 };
        record = await _jsonController.InsertAsync(record, true, true);
        var recordFromJson = await _jsonController.FirstOrDefaultAsync(x => x.Id.Equals(record.Id));
        recordFromJson.Should().BeEquivalentTo(record);

        JsonUtils.DeleteJson(nameof(Foo));
    }    
}
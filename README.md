# JsonEntity
Implementation of LINQ-based entity to json file

# How to use

* Preparing for Dependency Injection

1. You need to provide a full path json file to the JsonController constructor and inject it as a singleton. For example:
``` C#
await Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
    
        services.AddSingleton(_ => JsonEntityConfiguration<Estoque>.Create(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Estoque.json")));
        services.AddScoped<IJsonController<Estoque>, JsonController<Estoque>>();
        
        services.AddSingleton(_ => JsonEntityConfiguration<Produto>.Create(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Produto.json")));
        services.AddScoped<IJsonController<Produto>, JsonController<Produto>>();
        
        services.AddSingleton(_ => JsonEntityConfiguration<Venda>.Create(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Venda.json")));
        services.AddScoped<IJsonController<Venda>, JsonController<Venda>>();
        
    }).Build().RunAsync();
```
PS.: the library do NOT create the json file, either the necessary directories. Make sure to create yourself.

2. Create an inheritance from IBaseJsonEntity into all your entities, which makes you implement the `long Id` property:
``` C#
internal record Produto : IBaseJsonEntity
{
    public long Id { get; init; }
    public string Nome { get; init; }
    public decimal ValorUnitario { get; init; }
}
```

3. Get the IJsonController<T> from the dependency injection, like this:
``` C#
private readonly IJsonController<Produto> produtoContext;

public MainWorker(IJsonController<Produto> produtoContext)
{
    this.produtoContext = produtoContext;
}
```
  
Methods available: 
  * FirstOrDefaultAsync
  * ToListAsync
  * Except
  * RemoveAsync
  * InsertAsync
  * UpdateAsync
  * AnyAsync
  * LastOrDefaultAsync
  * WhereAsync
  
  This library works by using Stream, instead loading the entire json file, saving memory and better performance therefore

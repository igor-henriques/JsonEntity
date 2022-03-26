# JsonEntity
Implementation of LINQ-based entity to json file

# How to use

* Preparing for Dependency Injection

1. You need to provide a full path json file to the JsonController constructor and inject it as a singleton. For example:
``` C#
await Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddSingleton<IJsonController<Estoque>, JsonController<Estoque>>(x => new JsonController<Estoque>(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Estoque.json")));
        services.AddSingleton<IJsonController<Produto>, JsonController<Produto>>(x => new JsonController<Produto>(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Produto.json")));
        services.AddSingleton<IJsonController<Venda>, JsonController<Venda>>(x => new JsonController<Venda>(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Venda.json")));        
    }).Build().RunAsync();
```
PS.: the library do NOT create the json file, either the necessary directories. Make sure to create yourself.

2. Get the IJsonController<T> from the dependency injection, like this:
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
  
  This library works by using Stream, instead loading the entire json file, saving memory and better performance therefore

namespace JsonEntity.Library;

public sealed class JsonController<T> : IJsonController<T> where T : IBaseJsonEntity, new()
{
    private readonly string _jsonPath;
    private readonly ILogger<JsonController<T>> _logger;

    public JsonController(
        JsonEntityConfiguration<T> configuration,
        ILogger<JsonController<T>> logger)
    {
        if (!Directory.Exists(Directory.GetParent(configuration.JsonPath).FullName))
            throw new ArgumentException($"Directory {Directory.GetParent(configuration.JsonPath).FullName} don't exist");

        this._jsonPath = configuration.JsonPath;
        this._logger = logger;
    }

    /// <summary>
    /// Insert an entity into the provided json file, with an overload to generate sequential identity. Doesn't check for unique.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="verbose"></param>
    /// <param name="generateSequentialId"></param>
    /// <returns></returns>
    public async Task<T> InsertAsync(
        T entity,
        bool generateSequentialId = false,
        bool verbose = false,
        CancellationToken cancellationToken = default)
    {
        if (generateSequentialId)
        {
            using var fileStream = File.OpenRead(_jsonPath);
            entity.Id = CountLines(fileStream);
        }
        else
        {
            if (await this.AnyAsync(x => x.Id.Equals(entity.Id)))
                throw new Exception($"Key violation: Id {entity.Id} already exists");
        }

        await File.AppendAllTextAsync(_jsonPath, JsonConvert.SerializeObject(entity).Trim() + "\n", cancellationToken);

        if (verbose)
            _logger.LogInformation("Record {entity} inserted", entity);

        return entity;
    }

    /// <summary>
    /// References the entity ID field to update it into the json file
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="verbose"></param>
    /// <returns></returns>
    public async Task UpdateAsync(
        T entity,
        bool verbose = false,
        CancellationToken cancellationToken = default)
    {
        using (var tempFile = File.Create(string.Concat(_jsonPath, ".temp")))
        {
            using var fileStream = File.OpenRead(_jsonPath);
            using var streamReader = new StreamReader(fileStream);
            using var streamWriter = new StreamWriter(tempFile);

            T currentObject = default;

            while ((currentObject = JsonConvert.DeserializeObject<T>(await streamReader.ReadLineAsync() ?? "")) != null)
            {
                if (verbose)
                    _logger.LogInformation("Updating record from {currentObject} to {entity}", currentObject, entity);
                
                ReadOnlyMemory<char> objSerialized = MemoryExtensions.AsMemory(JsonConvert.SerializeObject(currentObject.Id.Equals(entity.Id) ? entity : currentObject));

                await streamWriter.WriteLineAsync(
                    objSerialized,
                    cancellationToken);
            }
        }

        File.Delete(_jsonPath);
        File.Move(string.Concat(_jsonPath, ".temp"), _jsonPath);

        if (verbose)
            _logger.LogInformation("Record {entity} updated", entity);
    }

    /// <summary>
    /// Removes an entity from the provided json file
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="verbose"></param>
    /// <returns></returns>
    public async Task RemoveAsync(
        Func<T, bool> condition,
        bool verbose = false,
        CancellationToken cancellationToken = default)
    {
        using (var tempFile = File.Create(string.Concat(_jsonPath, ".temp")))
        {
            using var fileStream = File.OpenRead(_jsonPath);
            using var streamReader = new StreamReader(fileStream);
            using var tempWriter = new StreamWriter(tempFile);

            T currentObject = default;

            while ((currentObject = JsonConvert.DeserializeObject<T>(await streamReader.ReadLineAsync() ?? "")) != null)
            {
                if (!condition.Invoke(currentObject))
                {
                    ReadOnlyMemory<char> objSerialized = MemoryExtensions.AsMemory(JsonConvert.SerializeObject(currentObject));
                    await tempWriter.WriteLineAsync(objSerialized, cancellationToken);
                }
                else if (verbose)
                {
                    _logger.LogInformation("Record {currentObject} removed", currentObject);
                }
            }
        }

        File.Delete(_jsonPath);
        File.Move(string.Concat(_jsonPath, ".temp"), _jsonPath);

        if (verbose)
            _logger.LogInformation("Record removed");
    }

    /// <summary>
    /// Seek for the first entity in the provided json file, or default if there's no entity or if the condition is not satisfied
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    public async Task<T> FirstOrDefaultAsync(Func<T, bool> condition = null)
    {
        using var fileStream = File.OpenRead(_jsonPath);
        using var streamReader = new StreamReader(fileStream);

        T currentObject = default;

        while ((currentObject = JsonConvert.DeserializeObject<T>(await streamReader.ReadLineAsync() ?? "")) != null)
        {
            if (condition is null | (condition?.Invoke(currentObject)).GetValueOrDefault())
                return currentObject;
        }

        return default;
    }

    /// <summary>
    /// Convert all entities in the provided json into a List<T>
    /// </summary>
    /// <returns></returns>
    public async Task<List<T>> ToListAsync()
    {
        List<T> list = new();

        using var fileStream = File.OpenRead(_jsonPath);
        using var streamReader = new StreamReader(fileStream);

        T currentObject = default;

        while ((currentObject = JsonConvert.DeserializeObject<T>(await streamReader.ReadLineAsync() ?? "")) != null)
        {
            list.Add(currentObject);
        }

        return list;
    }

    /// <summary>
    /// Produces the difference between the provided collection and the entities into the json file
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="verbose"></param>
    /// <returns></returns>
    public async Task<IEnumerable<T>> Except(IEnumerable<T> entities, bool verbose = false)
    {
        List<T> list = new();

        using var fileStream = File.OpenRead(_jsonPath);
        using var streamReader = new StreamReader(fileStream);

        T currentObject = default;

        while ((currentObject = JsonConvert.DeserializeObject<T>(await streamReader.ReadLineAsync() ?? "")) != null)
        {
            if (!entities.Any(x => x.Id == currentObject.Id))
            {
                list.Add(currentObject);
            }
            else if (verbose)
            {
                _logger.LogInformation("Record {currentObject} ignored", currentObject);
            }
        }

        return list;
    }

    /// <summary>
    /// Verifies if there's any entity in the json file with the provided match
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    public async Task<bool> AnyAsync(Func<T, bool> condition = null)
    {
        using var fileStream = File.OpenRead(_jsonPath);
        using var streamReader = new StreamReader(fileStream);

        T currentObject = default;

        while ((currentObject = JsonConvert.DeserializeObject<T>(await streamReader.ReadLineAsync() ?? "")) != null)
        {
            if (condition is null | (condition?.Invoke(currentObject)).GetValueOrDefault())
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Seek for the last entity in the provided json file, or default if there's no entity or if the condition is not satisfied
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    public async Task<T> LastOrDefaultAsync(Func<T, bool> condition = null)
    {
        using var fileStream = File.OpenRead(_jsonPath);
        using var streamReader = new StreamReader(fileStream);

        T response = default;

        T currentObject = default;

        while ((currentObject = JsonConvert.DeserializeObject<T>(await streamReader.ReadLineAsync() ?? "")) != null)
        {
            if (condition is null | (condition?.Invoke(currentObject)).GetValueOrDefault())
                response = currentObject;
        }

        return response;
    }

    /// <summary>
    /// Seek for all entities which satisfies the specified condition
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    public async Task<IEnumerable<T>> WhereAsync(Func<T, bool> condition)
    {
        List<T> list = new();

        using var fileStream = File.OpenRead(_jsonPath);
        using var streamReader = new StreamReader(fileStream);

        T currentObject = default;

        while ((currentObject = JsonConvert.DeserializeObject<T>(await streamReader.ReadLineAsync() ?? "")) != null)
        {
            if (condition.Invoke(currentObject))
                list.Add(currentObject);
        }

        return list;
    }

    /// <summary>
    /// Thanks to: https://stackoverflow.com/users/8000382/walter-verhoeven
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    private static long CountLines(Stream stream)
    {
        const char CR = '\r';
        const char LF = '\n';
        const char NULL = (char)0;

        var lineCount = 0L;

        var byteBuffer = new byte[1024 * 1024];
        const int BytesAtTheTime = 4;
        var detectedEOL = NULL;
        var currentChar = NULL;

        int bytesRead;

        while ((bytesRead = stream.Read(byteBuffer, 0, byteBuffer.Length)) > 0)
        {
            var i = 0;
            for (; i <= bytesRead - BytesAtTheTime; i += BytesAtTheTime)
            {
                currentChar = (char)byteBuffer[i];

                if (detectedEOL != NULL)
                {
                    if (currentChar == detectedEOL) { lineCount++; }

                    currentChar = (char)byteBuffer[i + 1];
                    if (currentChar == detectedEOL) { lineCount++; }

                    currentChar = (char)byteBuffer[i + 2];
                    if (currentChar == detectedEOL) { lineCount++; }

                    currentChar = (char)byteBuffer[i + 3];
                    if (currentChar == detectedEOL) { lineCount++; }
                }
                else
                {
                    if (currentChar == LF || currentChar == CR)
                    {
                        detectedEOL = currentChar;
                        lineCount++;
                    }

                    i -= BytesAtTheTime - 1;
                }
            }

            for (; i < bytesRead; i++)
            {
                currentChar = (char)byteBuffer[i];

                if (detectedEOL != NULL)
                {
                    if (currentChar == detectedEOL) { lineCount++; }
                }
                else
                {
                    if (currentChar == LF || currentChar == CR)
                    {
                        detectedEOL = currentChar;
                        lineCount++;
                    }
                }
            }
        }

        if (currentChar != LF && currentChar != CR && currentChar != NULL)
        {
            lineCount++;
        }

        return lineCount;
    }
}
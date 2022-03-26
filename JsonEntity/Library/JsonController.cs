namespace JsonEntity.Library;

public sealed class JsonController<T> : IJsonController<T> where T : IBaseJsonEntity, new()
{
    private readonly string jsonPath;

    public JsonController(string jsonPath)
    {
        Console.WriteLine(Directory.GetParent(jsonPath).FullName);

        if (!Directory.Exists(Directory.GetParent(jsonPath).FullName))
            throw new ArgumentException($"Directory {Directory.GetParent(jsonPath).FullName} don't exist");

        this.jsonPath = jsonPath;
    }

    public async Task InsertAsync(T entity, bool generateSequentialId = false, bool verbose = false)
    {
        if (generateSequentialId)
        {
            using (var fileStream = File.OpenRead(jsonPath))
            {
                entity.Id = CountLines(fileStream);
            }
        }

        await File.AppendAllTextAsync(jsonPath, JsonConvert.SerializeObject(entity) + "\n");

        if (verbose)
            Console.WriteLine($"Record {entity} inserted");
    }

    public async Task UpdateAsync(T entity, bool verbose = false)
    {
        using (var tempFile = File.Create(jsonPath + ".temp"))
        {
            using var fileStream = File.OpenRead(jsonPath);

            using var streamReader = new StreamReader(fileStream);

            using var streamWriter = new StreamWriter(tempFile);

            T currentObject = default(T);

            while ((currentObject = JsonConvert.DeserializeObject<T>(await streamReader.ReadLineAsync() ?? "")) != null)
            {
                if (verbose)
                    Console.WriteLine($"Updating record from {currentObject} to {entity}");

                await streamWriter.WriteLineAsync(JsonConvert.SerializeObject(currentObject.Id.Equals(entity.Id) ? entity : currentObject));
            }
        }

        File.Delete(jsonPath);

        File.Move(jsonPath + ".temp", jsonPath);

        if (verbose)
            Console.WriteLine($"Record {entity} updated");
    }

    public async Task RemoveAsync(Func<T, bool> condition, bool verbose = false)
    {
        using (var tempFile = File.Create(jsonPath + ".temp"))
        {
            using var fileStream = File.OpenRead(jsonPath);

            using var streamReader = new StreamReader(fileStream);

            using var tempWriter = new StreamWriter(tempFile);

            T currentObject = default(T);

            while ((currentObject = JsonConvert.DeserializeObject<T>(await streamReader.ReadLineAsync() ?? "")) != null)
            {
                if (!condition.Invoke(currentObject))
                {
                    await tempWriter.WriteLineAsync(JsonConvert.SerializeObject(currentObject));
                }
                else
                {
                    if (verbose)
                        Console.WriteLine($"Removing record {currentObject}");
                }
            }
        }

        File.Delete(jsonPath);

        File.Move(jsonPath + ".temp", jsonPath);

        if (verbose)
            Console.WriteLine($"Records removed");
    }

    public async Task<T> FirstOrDefaultAsync(Func<T, bool> condition = null)
    {
        using var fileStream = File.OpenRead(jsonPath);

        using var streamReader = new StreamReader(fileStream);

        T currentObject = default(T);

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
    /// <param name="condition"></param>
    /// <returns></returns>
    public async Task<List<T>> ToListAsync(Func<T, bool> condition = null)
    {
        List<T> list = new List<T>();

        using var fileStream = File.OpenRead(jsonPath);

        using var streamReader = new StreamReader(fileStream);

        T currentObject = default(T);

        while ((currentObject = JsonConvert.DeserializeObject<T>(await streamReader.ReadLineAsync() ?? "")) != null)
        {
            if (condition is null | (condition?.Invoke(currentObject)).GetValueOrDefault())
                list.Add(currentObject);
        }

        return list;
    }

    public async Task<IEnumerable<T>> Except(IEnumerable<T> entities, bool verbose = false)
    {
        List<T> list = new List<T>();

        using var fileStream = File.OpenRead(jsonPath);

        using var streamReader = new StreamReader(fileStream);

        T currentObject = default(T);

        while ((currentObject = JsonConvert.DeserializeObject<T>(await streamReader.ReadLineAsync() ?? "")) != null)
        {
            if (!entities.Any(x => x.Id == currentObject.Id))
            {
                list.Add(currentObject);
            }
            else
            {
                if (verbose)
                    Console.WriteLine($"Record {currentObject} ignored");
            }
        }

        return list;
    }

    public async Task<bool> AnyAsync(Func<T, bool> condition = null)
    {
        using var fileStream = File.OpenRead(jsonPath);

        using var streamReader = new StreamReader(fileStream);

        T currentObject = default(T);

        while ((currentObject = JsonConvert.DeserializeObject<T>(await streamReader.ReadLineAsync() ?? "")) != null)
        {
            if (condition is null | (condition?.Invoke(currentObject)).GetValueOrDefault())
            {
                return true;
            }
        }

        return false;
    }

    public async Task<T> LastOrDefaultAsync(Func<T, bool> condition = null)
    {
        using var fileStream = File.OpenRead(jsonPath);

        using var streamReader = new StreamReader(fileStream);

        T response = default(T);

        T currentObject = default(T);

        while ((currentObject = JsonConvert.DeserializeObject<T>(await streamReader.ReadLineAsync() ?? "")) != null)
        {
            if (condition is null | (condition?.Invoke(currentObject)).GetValueOrDefault())
                response = currentObject;
        }

        return response; 
    }

    /// <summary>
    /// Credits: https://stackoverflow.com/users/8000382/walter-verhoeven
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
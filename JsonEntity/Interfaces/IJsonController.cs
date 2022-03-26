namespace JsonEntity.Interfaces;

public interface IJsonController<T> where T : IBaseJsonEntity, new()
{
    /// <summary>
    /// Seek for the first entity in the provided json file, or default(T) if there's no entity or if the condition is not satisfied
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    Task<T> FirstOrDefaultAsync(Func<T, bool> condition = null);

    /// <summary>
    /// Convert all entities in the provided json into a List<T>
    /// </summary>
    /// <returns></returns>
    Task<List<T>> ToListAsync();

    /// <summary>
    /// Produces the difference between the provided collection and the entities into the json file
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="verbose"></param>
    /// <returns></returns>
    Task<IEnumerable<T>> Except(IEnumerable<T> entities, bool verbose = false);

    /// <summary>
    /// Removes an entity from the provided json file
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="verbose"></param>
    /// <returns></returns>
    Task RemoveAsync(Func<T, bool> condition, bool verbose = false);

    /// <summary>
    /// Insert an entity into the provided json file, with an overload to generate sequential identity. Doesn't check for unique.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="verbose"></param>
    /// <param name="generateSequentialId"></param>
    /// <returns></returns>
    Task InsertAsync(T entity, bool verbose = false, bool generateSequentialId = false);

    /// <summary>
    /// References the entity ID field to update it into the json file
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="verbose"></param>
    /// <returns></returns>
    Task UpdateAsync(T entity, bool verbose = false);

    /// <summary>
    /// Verifies if there's any entity in the json file with the provided match
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    Task<bool> AnyAsync(Func<T, bool> condition = null);

    /// <summary>
    /// Seek for the last entity in the provided json file, or default(T) if there's no entity or if the condition is not satisfied
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    Task<T> LastOrDefaultAsync(Func<T, bool> condition = null);

    /// <summary>
    /// Seek for all entities which satisfies the specified condition
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    Task<IEnumerable<T>> WhereAsync(Func<T, bool> condition);
}
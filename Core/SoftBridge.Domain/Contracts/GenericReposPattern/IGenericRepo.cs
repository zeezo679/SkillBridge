using SoftBridge.Domain.Contracts;
using SoftBridge.Domain.Contracts.SpecificationPattern;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SoftBridge.Domain.Contracts.GenericReposPattern
{
    // The IGenericRepo interface defines a contract for a generic repository pattern,
    // it used to be an abstracted common repository that can be used for any entity type,
    // it provides a set of common methods for performing CRUD (Create, Read, Update, Delete) operations on entities.
    public interface IGenericRepo<TEntity,TKey> where TEntity : IEntity<TKey>
    {
        // put async because it may involve I/O operations, such as database access
        // not put async for update and delete because they may not involve I/O operations
        // update and delete work with RAM not with database directly, so they can be synchronous
        Task<IReadOnlyList<TEntity>> GetAllAsync();
        Task<TEntity?> GetByIdAsync(TKey id); // ? because it may return null if the entity with the specified id does not exist

        Task<IReadOnlyList<TEntity>> GetAllWithSpecAsync(ISpecifications<TEntity, TKey> specifications);
        Task<TEntity?> GetByIdWithSpecAsync(ISpecifications<TEntity, TKey> specifications); // ? because it may return null if the entities with the specified ids do not exist

        Task<int> GetCountAsync(ISpecifications<TEntity, TKey> specifications);

        Task AddAsync(TEntity entity); 
        void Update(TEntity entity);
        void Delete(TEntity entity);
    }
}

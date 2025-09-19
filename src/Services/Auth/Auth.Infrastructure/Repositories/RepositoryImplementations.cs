using Microsoft.EntityFrameworkCore;using Microsoft.EntityFrameworkCore;using Microsoft.EntityFrameworkCore;using Microsoft.EntityFrameworkCore;using Microsoft.EntityFrameworkCore;using Microsoft.EntityFrameworkCore;using Microsoft.EntityFrameworkCore;using Microsoft.EntityFrameworkCore;using Microsoft.EntityFrameworkCore;using Microsoft.EntityFrameworkCore;using Microsoft.EntityFrameworkCore;using Microsoft.EntityFrameworkCore;

using Auth.Domain.Entities;

using Auth.Domain.Interfaces;using Auth.Domain.Entities;

using Auth.Infrastructure.Data;

using Auth.Domain.Interfaces;using Auth.Domain.Entities;

namespace Auth.Infrastructure.Repositories;

using Auth.Infrastructure.Data;

/// <summary>

/// Implémentation de base pour tous les repositoriesusing Auth.Domain.Interfaces;using Auth.Domain.Entities;

/// </summary>

public abstract class BaseRepository<T> : IBaseRepository<T> where T : classnamespace Auth.Infrastructure.Repositories;

{

    protected readonly AuthDbContext _context;using Auth.Infrastructure.Data;

    protected readonly DbSet<T> _dbSet;

/// <summary>

    /// <summary>

    /// Constructeur de base pour tous les repositories/// Implémentation de base pour tous les repositoriesusing Auth.Domain.Interfaces;using Auth.Domain.Entities;

    /// </summary>

    /// <param name="context">Contexte de base de données</param>/// </summary>

    protected BaseRepository(AuthDbContext context)

    {public abstract class BaseRepository<T> : IBaseRepository<T> where T : classnamespace Auth.Infrastructure.Repositories;

        _context = context ?? throw new ArgumentNullException(nameof(context));

        _dbSet = context.Set<T>();{

    }

    protected readonly AuthDbContext _context;using Auth.Infrastructure.Data;

    /// <summary>

    /// Récupère une entité par son ID    protected readonly DbSet<T> _dbSet;

    /// </summary>

    /// <param name="id">ID de l'entité</param>/// <summary>

    /// <param name="cancellationToken">Token d'annulation</param>

    /// <returns>L'entité trouvée ou null</returns>    /// <summary>

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)

    {    /// Constructeur de base pour tous les repositories/// Implémentation de base pour tous les repositoriesusing Auth.Domain.Interfaces;using Auth.Domain.Entities;

        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);

    }    /// </summary>



    /// <summary>    /// <param name="context">Contexte de base de données</param>/// </summary>

    /// Récupère toutes les entités

    /// </summary>    protected BaseRepository(AuthDbContext context)

    /// <param name="cancellationToken">Token d'annulation</param>

    /// <returns>Liste de toutes les entités</returns>    {public abstract class BaseRepository<T> : IBaseRepository<T> where T : classnamespace Auth.Infrastructure.Repositories;

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)

    {        _context = context ?? throw new ArgumentNullException(nameof(context));

        return await _dbSet.ToListAsync(cancellationToken);

    }        _dbSet = context.Set<T>();{



    /// <summary>    }

    /// Ajoute une nouvelle entité

    /// </summary>    protected readonly AuthDbContext _context;using Auth.Infrastructure.Data;

    /// <param name="entity">Entité à ajouter</param>

    /// <param name="cancellationToken">Token d'annulation</param>    /// <summary>

    /// <returns>L'entité ajoutée</returns>

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)    /// Récupère une entité par son ID    protected readonly DbSet<T> _dbSet;

    {

        if (entity == null)    /// </summary>

            throw new ArgumentNullException(nameof(entity));

    /// <param name="id">ID de l'entité</param>/// <summary>

        var entry = await _dbSet.AddAsync(entity, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);    /// <param name="cancellationToken">Token d'annulation</param>

        return entry.Entity;

    }    /// <returns>L'entité trouvée ou null</returns>    /// <summary>



    /// <summary>    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)

    /// Met à jour une entité existante

    /// </summary>    {    /// Constructeur de base/// Implémentation de base pour tous les repositoriesusing Auth.Domain.Interfaces;using Auth.Domain.Entities;

    /// <param name="entity">Entité à mettre à jour</param>

    /// <param name="cancellationToken">Token d'annulation</param>        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);

    /// <returns>Task</returns>

    public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)    }    /// </summary>

    {

        if (entity == null)

            throw new ArgumentNullException(nameof(entity));

    /// <summary>    protected BaseRepository(AuthDbContext context)/// </summary>

        _dbSet.Update(entity);

        await _context.SaveChangesAsync(cancellationToken);    /// Récupère toutes les entités

    }

    /// </summary>    {

    /// <summary>

    /// Supprime une entité    /// <param name="cancellationToken">Token d'annulation</param>

    /// </summary>

    /// <param name="entity">Entité à supprimer</param>    /// <returns>Liste de toutes les entités</returns>        _context = context ?? throw new ArgumentNullException(nameof(context));public abstract class BaseRepository<T> : IBaseRepository<T> where T : classnamespace Auth.Infrastructure.Repositories;

    /// <param name="cancellationToken">Token d'annulation</param>

    /// <returns>Task</returns>    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)

    public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)

    {    {        _dbSet = context.Set<T>();

        if (entity == null)

            throw new ArgumentNullException(nameof(entity));        return await _dbSet.ToListAsync(cancellationToken);



        _dbSet.Remove(entity);    }    }{

        await _context.SaveChangesAsync(cancellationToken);

    }



    /// <summary>    /// <summary>

    /// Supprime une entité par son ID

    /// </summary>    /// Ajoute une nouvelle entité

    /// <param name="id">ID de l'entité à supprimer</param>

    /// <param name="cancellationToken">Token d'annulation</param>    /// </summary>    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)    protected readonly AuthDbContext _context;using Auth.Infrastructure.Data;

    /// <returns>Task</returns>

    public virtual async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)    /// <param name="entity">Entité à ajouter</param>

    {

        var entity = await GetByIdAsync(id, cancellationToken);    /// <param name="cancellationToken">Token d'annulation</param>    {

        if (entity != null)

        {    /// <returns>L'entité ajoutée</returns>

            await DeleteAsync(entity, cancellationToken);

        }    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);    protected readonly DbSet<T> _dbSet;

    }

    {

    /// <summary>

    /// Vérifie si une entité existe        if (entity == null)    }

    /// </summary>

    /// <param name="id">ID de l'entité</param>            throw new ArgumentNullException(nameof(entity));

    /// <param name="cancellationToken">Token d'annulation</param>

    /// <returns>True si l'entité existe, false sinon</returns>/// <summary>

    public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)

    {        var entry = await _dbSet.AddAsync(entity, cancellationToken);

        return await _dbSet.FindAsync(new object[] { id }, cancellationToken) != null;

    }        await _context.SaveChangesAsync(cancellationToken);    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)

}
        return entry.Entity;

    }    {    /// <summary>



    /// <summary>        return await _dbSet.ToListAsync(cancellationToken);

    /// Met à jour une entité existante

    /// </summary>    }    /// Constructeur de base/// Implémentation de base pour tous les repositoriesusing Auth.Domain.Interfaces;using Auth.Domain.Entities;

    /// <param name="entity">Entité à mettre à jour</param>

    /// <param name="cancellationToken">Token d'annulation</param>

    /// <returns>Task</returns>

    public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)    /// </summary>

    {

        if (entity == null)    {

            throw new ArgumentNullException(nameof(entity));

        if (entity == null)    protected BaseRepository(AuthDbContext context)/// </summary>

        _dbSet.Update(entity);

        await _context.SaveChangesAsync(cancellationToken);            throw new ArgumentNullException(nameof(entity));

    }

    {

    /// <summary>

    /// Supprime une entité        await _dbSet.AddAsync(entity, cancellationToken);

    /// </summary>

    /// <param name="entity">Entité à supprimer</param>        await _context.SaveChangesAsync(cancellationToken);        _context = context ?? throw new ArgumentNullException(nameof(context));public abstract class BaseRepository<T> : IBaseRepository<T> where T : classnamespace Auth.Infrastructure.Repositories

    /// <param name="cancellationToken">Token d'annulation</param>

    /// <returns>Task</returns>        return entity;

    public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)

    {    }        _dbSet = context.Set<T>();

        if (entity == null)

            throw new ArgumentNullException(nameof(entity));



        _dbSet.Remove(entity);    public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)    }{

        await _context.SaveChangesAsync(cancellationToken);

    }    {



    /// <summary>        if (entity == null)

    /// Supprime une entité par son ID

    /// </summary>            throw new ArgumentNullException(nameof(entity));

    /// <param name="id">ID de l'entité à supprimer</param>

    /// <param name="cancellationToken">Token d'annulation</param>    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)    protected readonly AuthDbContext _context;{using Auth.Infrastructure.Data;

    /// <returns>Task</returns>

    public virtual async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)        _dbSet.Update(entity);

    {

        var entity = await GetByIdAsync(id, cancellationToken);        await _context.SaveChangesAsync(cancellationToken);    {

        if (entity != null)

        {    }

            await DeleteAsync(entity, cancellationToken);

        }        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);    protected readonly DbSet<T> _dbSet;

    }

    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)

    /// <summary>

    /// Vérifie si une entité existe    {    }

    /// </summary>

    /// <param name="id">ID de l'entité</param>        var entity = await GetByIdAsync(id, cancellationToken);

    /// <param name="cancellationToken">Token d'annulation</param>

    /// <returns>True si l'entité existe, false sinon</returns>        if (entity != null)    /// <summary>

    public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)

    {        {

        return await _dbSet.FindAsync(new object[] { id }, cancellationToken) != null;

    }            _dbSet.Remove(entity);    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)

}
            await _context.SaveChangesAsync(cancellationToken);

        }    {    protected BaseRepository(AuthDbContext context)

    }

}        return await _dbSet.ToListAsync(cancellationToken);

    }    {    /// Implémentation de base pour tous les repositoriesusing Auth.Domain.Interfaces;using Auth.Domain.Entities;



    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)        _context = context ?? throw new ArgumentNullException(nameof(context));

    {

        if (entity == null)        _dbSet = context.Set<T>();    /// </summary>

            throw new ArgumentNullException(nameof(entity));

    }

        await _dbSet.AddAsync(entity, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);    public abstract class BaseRepository<T> : IBaseRepository<T> where T : classnamespace Auth.Infrastructure.Repositories

        return entity;

    }    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)



    public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)    {    {

    {

        if (entity == null)        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);

            throw new ArgumentNullException(nameof(entity));

    }        protected readonly AuthDbContext _context;{using Auth.Infrastructure.Data;

        _dbSet.Update(entity);

        await _context.SaveChangesAsync(cancellationToken);

    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)        protected readonly DbSet<T> _dbSet;

    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)

    {    {

        var entity = await GetByIdAsync(id, cancellationToken);

        if (entity != null)        return await _dbSet.ToListAsync(cancellationToken);    /// <summary>

        {

            _dbSet.Remove(entity);    }

            await _context.SaveChangesAsync(cancellationToken);

        }        protected BaseRepository(AuthDbContext context)

    }

}    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)

    {        {    /// Base repository implementationusing Auth.Domain.Interfaces;using Auth.Domain.Entities;

        if (entity == null)

            throw new ArgumentNullException(nameof(entity));            _context = context ?? throw new ArgumentNullException(nameof(context));



        await _dbSet.AddAsync(entity, cancellationToken);            _dbSet = context.Set<T>();    /// </summary>

        return entity;

    }        }



    public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)    public abstract class BaseRepository<T> : IBaseRepository<T> where T : classnamespace Auth.Infrastructure.Repositories

    {

        if (entity == null)        public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)

            throw new ArgumentNullException(nameof(entity));

        {    {

        _dbSet.Update(entity);

        await Task.CompletedTask;            return await _dbSet.FindAsync(new object[] { id }, cancellationToken);

    }

        }        protected readonly AuthDbContext _context;{using Auth.Infrastructure.Data;

    public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)

    {

        if (entity == null)

            throw new ArgumentNullException(nameof(entity));        public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)        protected readonly DbSet<T> _dbSet;



        _dbSet.Remove(entity);        {

        await Task.CompletedTask;

    }            return await _dbSet.ToListAsync(cancellationToken);    /// <summary>



    public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)        }

    {

        return await _dbSet.FindAsync(new object[] { id }, cancellationToken) != null;        protected BaseRepository(AuthDbContext context)

    }

        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)

    public virtual async Task SaveChangesAsync(CancellationToken cancellationToken = default)

    {        {        {    /// Base repository implementationusing Auth.Domain.Interfaces;using Auth.Domain.Entities;using Auth.Domain.Entities;

        await _context.SaveChangesAsync(cancellationToken);

    }            if (entity == null)

}

                throw new ArgumentNullException(nameof(entity));            _context = context;

/// <summary>

/// Implémentation du repository des utilisateurs

/// </summary>

public class UserRepositoryImplementation : BaseRepository<User>, IUserRepository            var entry = await _dbSet.AddAsync(entity, cancellationToken);            _dbSet = context.Set<T>();    /// </summary>

{

    public UserRepositoryImplementation(AuthDbContext context) : base(context)            return entry.Entity;

    {

    }        }        }



    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)

    {

        if (string.IsNullOrWhiteSpace(email))        public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)    public abstract class BaseRepository<T> : IBaseRepository<T> where T : classnamespace Auth.Infrastructure.Repositories

            return null;

        {

        return await _dbSet

            .Include(u => u.UserRoles)            if (entity == null)        public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)

                .ThenInclude(ur => ur.Role)

                    .ThenInclude(r => r.RolePermissions)                throw new ArgumentNullException(nameof(entity));

                        .ThenInclude(rp => rp.Permission)

            .Include(u => u.UserSessions)        {    {

            .Include(u => u.Devices)

            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), cancellationToken);            _dbSet.Update(entity);

    }

            await Task.CompletedTask;            return await _dbSet.FindAsync(new object[] { id }, cancellationToken);

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)

    {        }

        if (string.IsNullOrWhiteSpace(username))

            return null;        }        protected readonly AuthDbContext _context;{using Auth.Infrastructure.Data;



        return await _dbSet        public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)

            .Include(u => u.UserRoles)

                .ThenInclude(ur => ur.Role)        {

                    .ThenInclude(r => r.RolePermissions)

                        .ThenInclude(rp => rp.Permission)            if (entity == null)

            .Include(u => u.UserSessions)

            .Include(u => u.Devices)                throw new ArgumentNullException(nameof(entity));        public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)        protected readonly DbSet<T> _dbSet;

            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower(), cancellationToken);

    }



    public async Task<User?> GetByIdWithRolesAsync(Guid userId, CancellationToken cancellationToken = default)            _dbSet.Remove(entity);        {

    {

        return await _dbSet            await Task.CompletedTask;

            .Include(u => u.UserRoles)

                .ThenInclude(ur => ur.Role)        }            return await _dbSet.ToListAsync(cancellationToken);    /// <summary>

                    .ThenInclude(r => r.RolePermissions)

                        .ThenInclude(rp => rp.Permission)

            .Include(u => u.UserSessions)

            .Include(u => u.Devices)        public virtual async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)        }

            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

    }        {



    public async Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)            var entity = await GetByIdAsync(id, cancellationToken);        protected BaseRepository(AuthDbContext context)

    {

        return await _dbSet            if (entity != null)

            .Where(u => u.IsActive)

            .Include(u => u.UserRoles)            {        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)

                .ThenInclude(ur => ur.Role)

            .ToListAsync(cancellationToken);                await DeleteAsync(entity, cancellationToken);

    }

            }        {        {    /// Base repository implementationusing Auth.Domain.Interfaces;using Auth.Domain.Interfaces;

    public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default)

    {        }

        if (string.IsNullOrWhiteSpace(email))

            return false;            var entry = await _dbSet.AddAsync(entity, cancellationToken);



        var query = _dbSet.Where(u => u.Email.ToLower() == email.ToLower());        public virtual async Task SaveChangesAsync(CancellationToken cancellationToken = default)

        

        if (excludeUserId.HasValue)        {            return entry.Entity;            _context = context;

            query = query.Where(u => u.Id != excludeUserId.Value);

            await _context.SaveChangesAsync(cancellationToken);

        return !await query.AnyAsync(cancellationToken);

    }        }        }



    public async Task<bool> IsUsernameUniqueAsync(string username, Guid? excludeUserId = null, CancellationToken cancellationToken = default)    }

    {

        if (string.IsNullOrWhiteSpace(username))}            _dbSet = context.Set<T>();    /// </summary>

            return false;

        public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)

        var query = _dbSet.Where(u => u.Username.ToLower() == username.ToLower());

                {        }

        if (excludeUserId.HasValue)

            query = query.Where(u => u.Id != excludeUserId.Value);            _dbSet.Update(entity);



        return !await query.AnyAsync(cancellationToken);            await Task.CompletedTask;    public abstract class BaseRepository<T> : IBaseRepository<T> where T : classnamespace Auth.Infrastructure.Repositories

    }

}        }



/// <summary>        public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)

/// Implémentation du repository des rôles

/// </summary>        public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)

public class RoleRepositoryImplementation : BaseRepository<Role>, IRoleRepository

{        {        {    {

    public RoleRepositoryImplementation(AuthDbContext context) : base(context)

    {            _dbSet.Remove(entity);

    }

            await Task.CompletedTask;            return await _dbSet.FindAsync(new object[] { id }, cancellationToken);

    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)

    {        }

        if (string.IsNullOrWhiteSpace(name))

            return null;        }        protected readonly AuthDbContext _context;{using Auth.Infrastructure.Data;using Auth.Infrastructure.Data;



        return await _dbSet        public virtual async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)

            .Include(r => r.RolePermissions)

                .ThenInclude(rp => rp.Permission)        {

            .Include(r => r.UserRoles)

                .ThenInclude(ur => ur.User)            var entity = await GetByIdAsync(id, cancellationToken);

            .FirstOrDefaultAsync(r => r.Name.ToLower() == name.ToLower(), cancellationToken);

    }            if (entity != null)        public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)        protected readonly DbSet<T> _dbSet;



    public async Task<IEnumerable<Role>> GetRolesWithPermissionsAsync(CancellationToken cancellationToken = default)            {

    {

        return await _dbSet                await DeleteAsync(entity, cancellationToken);        {

            .Include(r => r.RolePermissions)

                .ThenInclude(rp => rp.Permission)            }

            .ToListAsync(cancellationToken);

    }        }            return await _dbSet.ToListAsync(cancellationToken);    /// <summary>



    public async Task<bool> IsNameUniqueAsync(string name, Guid? excludeRoleId = null, CancellationToken cancellationToken = default)

    {

        if (string.IsNullOrWhiteSpace(name))        public virtual async Task SaveChangesAsync(CancellationToken cancellationToken = default)        }

            return false;

        {

        var query = _dbSet.Where(r => r.Name.ToLower() == name.ToLower());

                    await _context.SaveChangesAsync(cancellationToken);        protected BaseRepository(AuthDbContext context)

        if (excludeRoleId.HasValue)

            query = query.Where(r => r.Id != excludeRoleId.Value);        }



        return !await query.AnyAsync(cancellationToken);    }        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)

    }

}}

        {        {    /// Base repository implementation

/// <summary>

/// Implémentation du repository des permissions            var entry = await _dbSet.AddAsync(entity, cancellationToken);

/// </summary>

public class PermissionRepositoryImplementation : BaseRepository<Permission>, IPermissionRepository            return entry.Entity;            _context = context;

{

    public PermissionRepositoryImplementation(AuthDbContext context) : base(context)        }

    {

    }            _dbSet = context.Set<T>();    /// </summary>



    public async Task<Permission?> GetByNameAsync(string name, CancellationToken cancellationToken = default)        public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)

    {

        if (string.IsNullOrWhiteSpace(name))        {        }

            return null;

            _dbSet.Update(entity);

        return await _dbSet

            .Include(p => p.RolePermissions)            await Task.CompletedTask;    public abstract class BaseRepository<T> : IBaseRepository<T> where T : classnamespace Auth.Infrastructure.Repositoriesnamespace Auth.Infrastructure.Repositories

                .ThenInclude(rp => rp.Role)

            .FirstOrDefaultAsync(p => p.Name.ToLower() == name.ToLower(), cancellationToken);        }

    }

        public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)

    public async Task<IEnumerable<Permission>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)

    {        public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)

        return await _context.Permissions

            .Where(p => p.RolePermissions        {        {    {

                .Any(rp => rp.Role.UserRoles

                    .Any(ur => ur.UserId == userId)))            _dbSet.Remove(entity);

            .ToListAsync(cancellationToken);

    }            await Task.CompletedTask;            return await _dbSet.FindAsync(new object[] { id }, cancellationToken);



    public async Task<bool> IsNameUniqueAsync(string name, Guid? excludePermissionId = null, CancellationToken cancellationToken = default)        }

    {

        if (string.IsNullOrWhiteSpace(name))        }        protected readonly AuthDbContext _context;{{

            return false;

        public virtual async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)

        var query = _dbSet.Where(p => p.Name.ToLower() == name.ToLower());

                {

        if (excludePermissionId.HasValue)

            query = query.Where(p => p.Id != excludePermissionId.Value);            var entity = await GetByIdAsync(id, cancellationToken);



        return !await query.AnyAsync(cancellationToken);            if (entity != null)        public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)        protected readonly DbSet<T> _dbSet;

    }

}            {



/// <summary>                await DeleteAsync(entity, cancellationToken);        {

/// Implémentation du repository des appareils

/// </summary>            }

public class DeviceRepositoryImplementation : BaseRepository<Device>, IDeviceRepository

{        }            return await _dbSet.ToListAsync(cancellationToken);    /// <summary>    /// <summary>

    public DeviceRepositoryImplementation(AuthDbContext context) : base(context)

    {

    }

        public virtual async Task SaveChangesAsync(CancellationToken cancellationToken = default)        }

    public async Task<Device?> GetByDeviceKeyAsync(string deviceKey, CancellationToken cancellationToken = default)

    {        {

        if (string.IsNullOrWhiteSpace(deviceKey))

            return null;            await _context.SaveChangesAsync(cancellationToken);        protected BaseRepository(AuthDbContext context)



        return await _dbSet        }

            .Include(d => d.User)

            .FirstOrDefaultAsync(d => d.DeviceKey == deviceKey, cancellationToken);    }        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)

    }



    public async Task<IEnumerable<Device>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)

    {    /// <summary>        {        {    /// Base repository implementation    /// Base repository implementation

        return await _dbSet

            .Where(d => d.UserId == userId)    /// User repository implementation

            .Include(d => d.User)

            .OrderByDescending(d => d.LastUsedAt)    /// </summary>            var entry = await _dbSet.AddAsync(entity, cancellationToken);

            .ToListAsync(cancellationToken);

    }    public class UserRepository : BaseRepository<User>, IUserRepository



    public async Task<IEnumerable<Device>> GetActiveDevicesAsync(CancellationToken cancellationToken = default)    {            return entry.Entity;            _context = context;

    {

        return await _dbSet        public UserRepository(AuthDbContext context) : base(context)

            .Where(d => d.IsActive)

            .Include(d => d.User)        {        }

            .OrderByDescending(d => d.LastUsedAt)

            .ToListAsync(cancellationToken);        }

    }

            _dbSet = context.Set<T>();    /// </summary>    /// </summary>

    public async Task<bool> IsDeviceKeyUniqueAsync(string deviceKey, Guid? excludeDeviceId = null, CancellationToken cancellationToken = default)

    {        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)

        if (string.IsNullOrWhiteSpace(deviceKey))

            return false;        {        public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)



        var query = _dbSet.Where(d => d.DeviceKey == deviceKey);            return await _dbSet

        

        if (excludeDeviceId.HasValue)                .Include(u => u.UserRoles)        {        }

            query = query.Where(d => d.Id != excludeDeviceId.Value);

                    .ThenInclude(ur => ur.Role)

        return !await query.AnyAsync(cancellationToken);

    }                        .ThenInclude(r => r.RolePermissions)            _dbSet.Update(entity);

}

                            .ThenInclude(rp => rp.Permission)

/// <summary>

/// Implémentation du repository des sessions utilisateur                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);            await Task.CompletedTask;    public abstract class BaseRepository<T> : IBaseRepository<T> where T : class    public abstract class BaseRepository<T> : IBaseRepository<T> where T : class

/// </summary>

public class UserSessionRepositoryImplementation : BaseRepository<UserSession>, IUserSessionRepository        }

{

    public UserSessionRepositoryImplementation(AuthDbContext context) : base(context)        }

    {

    }        public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)



    public async Task<UserSession?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)        {        public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)

    {

        if (string.IsNullOrWhiteSpace(refreshToken))            return await _dbSet

            return null;

                .Include(u => u.UserRoles)        public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)

        return await _dbSet

            .Include(s => s.User)                    .ThenInclude(ur => ur.Role)

            .Include(s => s.Device)

            .FirstOrDefaultAsync(s => s.RefreshToken == refreshToken, cancellationToken);                        .ThenInclude(r => r.RolePermissions)        {        {    {    {

    }

                            .ThenInclude(rp => rp.Permission)

    public async Task<IEnumerable<UserSession>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)

    {                .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);            _dbSet.Remove(entity);

        return await _dbSet

            .Where(s => s.UserId == userId)        }

            .Include(s => s.User)

            .Include(s => s.Device)            await Task.CompletedTask;            return await _dbSet.FindAsync(new object[] { id }, cancellationToken);

            .OrderByDescending(s => s.CreatedAt)

            .ToListAsync(cancellationToken);        public async Task<bool> ExistsAsync(string email, string username, CancellationToken cancellationToken = default)

    }

        {        }

    public async Task<IEnumerable<UserSession>> GetActiveSessionsAsync(CancellationToken cancellationToken = default)

    {            return await _dbSet.AnyAsync(u => u.Email == email || u.Username == username, cancellationToken);

        var now = DateTime.UtcNow;

        return await _dbSet        }        }        protected readonly AuthDbContext _context;        protected readonly AuthDbContext _context;

            .Where(s => s.IsActive && s.ExpiresAt > now)

            .Include(s => s.User)

            .Include(s => s.Device)

            .OrderByDescending(s => s.LastAccessedAt)        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName, CancellationToken cancellationToken = default)        public virtual async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)

            .ToListAsync(cancellationToken);

    }        {



    public async Task<IEnumerable<UserSession>> GetExpiredSessionsAsync(CancellationToken cancellationToken = default)            return await _dbSet        {

    {

        var now = DateTime.UtcNow;                .Include(u => u.UserRoles)

        return await _dbSet

            .Where(s => s.ExpiresAt <= now || !s.IsActive)                    .ThenInclude(ur => ur.Role)            var entity = await GetByIdAsync(id, cancellationToken);

            .Include(s => s.User)

            .Include(s => s.Device)                .Where(u => u.UserRoles.Any(ur => ur.Role.Name == roleName))

            .ToListAsync(cancellationToken);

    }                .ToListAsync(cancellationToken);            if (entity != null)        public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)        protected readonly DbSet<T> _dbSet;        protected readonly DbSet<T> _dbSet;



    public async Task InvalidateUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default)        }

    {

        var sessions = await _dbSet            {

            .Where(s => s.UserId == userId && s.IsActive)

            .ToListAsync(cancellationToken);        public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm, CancellationToken cancellationToken = default)



        foreach (var session in sessions)        {                await DeleteAsync(entity, cancellationToken);        {

        {

            session.Invalidate();            return await _dbSet

        }

                .Where(u => u.Email.Contains(searchTerm) ||             }

        await Task.CompletedTask;

    }                           u.Username.Contains(searchTerm) || 



    public async Task CleanupExpiredSessionsAsync(CancellationToken cancellationToken = default)                           u.FirstName.Contains(searchTerm) ||         }            return await _dbSet.ToListAsync(cancellationToken);

    {

        var expiredSessions = await GetExpiredSessionsAsync(cancellationToken);                           u.LastName.Contains(searchTerm))

        

        foreach (var session in expiredSessions)                .ToListAsync(cancellationToken);

        {

            _dbSet.Remove(session);        }

        }

        public virtual async Task SaveChangesAsync(CancellationToken cancellationToken = default)        }

        await Task.CompletedTask;

    }        public async Task<(IEnumerable<User> Users, int TotalCount)> GetPagedUsersAsync(

}

            int pageNumber,         {

/// <summary>

/// Implémentation du repository des logs d'audit            int pageSize, 

/// </summary>

public class AuditLogRepositoryImplementation : BaseRepository<AuditLog>, IAuditLogRepository            string? searchTerm = null,            await _context.SaveChangesAsync(cancellationToken);        protected BaseRepository(AuthDbContext context)        protected BaseRepository(AuthDbContext context)

{

    public AuditLogRepositoryImplementation(AuthDbContext context) : base(context)            CancellationToken cancellationToken = default)

    {

    }        {        }



    public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)            var query = _dbSet.AsQueryable();

    {

        return await _dbSet    }        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)

            .Where(a => a.UserId == userId)

            .OrderByDescending(a => a.Timestamp)            if (!string.IsNullOrWhiteSpace(searchTerm))

            .ToListAsync(cancellationToken);

    }            {



    public async Task<IEnumerable<AuditLog>> GetByActionAsync(string action, CancellationToken cancellationToken = default)                query = query.Where(u => u.Email.Contains(searchTerm) || 

    {

        if (string.IsNullOrWhiteSpace(action))                                       u.Username.Contains(searchTerm) ||     /// <summary>        {        {        {

            return new List<AuditLog>();

                                       u.FirstName.Contains(searchTerm) || 

        return await _dbSet

            .Where(a => a.Action.ToLower() == action.ToLower())                                       u.LastName.Contains(searchTerm));    /// User repository implementation

            .OrderByDescending(a => a.Timestamp)

            .ToListAsync(cancellationToken);            }

    }

    /// </summary>            var result = await _dbSet.AddAsync(entity, cancellationToken);

    public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)

    {            var totalCount = await query.CountAsync(cancellationToken);

        return await _dbSet

            .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)                public class UserRepository : BaseRepository<User>, IUserRepository

            .OrderByDescending(a => a.Timestamp)

            .ToListAsync(cancellationToken);            var users = await query

    }

                .Skip((pageNumber - 1) * pageSize)    {            return result.Entity;            _context = context;            _context = context;

    public async Task<IEnumerable<AuditLog>> GetByIpAddressAsync(string ipAddress, CancellationToken cancellationToken = default)

    {                .Take(pageSize)

        if (string.IsNullOrWhiteSpace(ipAddress))

            return new List<AuditLog>();                .Include(u => u.UserRoles)        public UserRepository(AuthDbContext context) : base(context)



        return await _dbSet                    .ThenInclude(ur => ur.Role)

            .Where(a => a.IpAddress == ipAddress)

            .OrderByDescending(a => a.Timestamp)                .ToListAsync(cancellationToken);        {        }

            .ToListAsync(cancellationToken);

    }



    public async Task<IEnumerable<AuditLog>> GetRecentLogsAsync(int count = 100, CancellationToken cancellationToken = default)            return (users, totalCount);        }

    {

        return await _dbSet        }

            .OrderByDescending(a => a.Timestamp)

            .Take(count)    }            _dbSet = context.Set<T>();            _dbSet = context.Set<T>();

            .ToListAsync(cancellationToken);

    }



    public async Task CleanupOldLogsAsync(int retentionDays = 90, CancellationToken cancellationToken = default)    /// <summary>        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)

    {

        var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);    /// Role repository implementation

        

        var oldLogs = await _dbSet    /// </summary>        {        public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)

            .Where(a => a.Timestamp < cutoffDate)

            .ToListAsync(cancellationToken);    public class RoleRepository : BaseRepository<Role>, IRoleRepository



        foreach (var log in oldLogs)    {            return await _dbSet

        {

            _dbSet.Remove(log);        public RoleRepository(AuthDbContext context) : base(context)

        }

        {                .Include(u => u.UserRoles)        {        }        }

        await Task.CompletedTask;

    }        }

}
                    .ThenInclude(ur => ur.Role)

        public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)

        {                        .ThenInclude(r => r.RolePermissions)            _dbSet.Update(entity);

            return await _dbSet

                .Include(r => r.RolePermissions)                            .ThenInclude(rp => rp.Permission)

                    .ThenInclude(rp => rp.Permission)

                .FirstOrDefaultAsync(r => r.Name == name, cancellationToken);                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);        }

        }

        }

        public async Task<IEnumerable<Role>> GetRolesWithPermissionsAsync(CancellationToken cancellationToken = default)

        {

            return await _dbSet

                .Include(r => r.RolePermissions)        public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)

                    .ThenInclude(rp => rp.Permission)

                .ToListAsync(cancellationToken);        {        public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)        public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)        public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)

        }

            return await _dbSet

        public async Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default)

        {                .Include(u => u.UserRoles)        {

            return await _dbSet.AnyAsync(r => r.Name == name, cancellationToken);

        }                    .ThenInclude(ur => ur.Role)



        public async Task AddPermissionToRoleAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)                        .ThenInclude(r => r.RolePermissions)            _dbSet.Remove(entity);        {        {

        {

            var rolePermission = new RolePermission                             .ThenInclude(rp => rp.Permission)

            { 

                RoleId = roleId,                 .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);        }

                PermissionId = permissionId 

            };        }

            

            _context.Set<RolePermission>().Add(rolePermission);            return await _dbSet.FindAsync(new object[] { id }, cancellationToken);            return await _dbSet.FindAsync(new object[] { id }, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

        }        public async Task<bool> ExistsAsync(string email, string username, CancellationToken cancellationToken = default)



        public async Task RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)        {        public virtual async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)

        {

            var rolePermission = await _context.Set<RolePermission>()            return await _dbSet.AnyAsync(u => u.Email == email || u.Username == username, cancellationToken);

                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, cancellationToken);

                    }        {        }        }

            if (rolePermission != null)

            {

                _context.Set<RolePermission>().Remove(rolePermission);

                await _context.SaveChangesAsync(cancellationToken);        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName, CancellationToken cancellationToken = default)            var entity = await GetByIdAsync(id, cancellationToken);

            }

        }        {

    }

            return await _dbSet            if (entity != null)

    /// <summary>

    /// Permission repository implementation                .Include(u => u.UserRoles)

    /// </summary>

    public class PermissionRepository : BaseRepository<Permission>, IPermissionRepository                    .ThenInclude(ur => ur.Role)            {

    {

        public PermissionRepository(AuthDbContext context) : base(context)                .Where(u => u.UserRoles.Any(ur => ur.Role.Name == roleName))

        {

        }                .ToListAsync(cancellationToken);                await DeleteAsync(entity, cancellationToken);        public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)        public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)



        public async Task<Permission?> GetByNameAsync(string name, CancellationToken cancellationToken = default)        }

        {

            return await _dbSet.FirstOrDefaultAsync(p => p.Name == name, cancellationToken);            }

        }

        public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm, CancellationToken cancellationToken = default)

        public async Task<IEnumerable<Permission>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)

        {        {        }        {        {

            return await _context.Set<RolePermission>()

                .Where(rp => rp.RoleId == roleId)            return await _dbSet

                .Select(rp => rp.Permission)

                .ToListAsync(cancellationToken);                .Where(u => u.Email.Contains(searchTerm) || 

        }

                           u.Username.Contains(searchTerm) || 

        public async Task<IEnumerable<Permission>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)

        {                           u.FirstName.Contains(searchTerm) ||         public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)            return await _dbSet.ToListAsync(cancellationToken);            return await _dbSet.ToListAsync(cancellationToken);

            return await _context.Set<UserRole>()

                .Where(ur => ur.UserId == userId)                           u.LastName.Contains(searchTerm))

                .SelectMany(ur => ur.Role.RolePermissions)

                .Select(rp => rp.Permission)                .ToListAsync(cancellationToken);        {

                .Distinct()

                .ToListAsync(cancellationToken);        }

        }

            return await _dbSet.FindAsync(new object[] { id }, cancellationToken) != null;        }        }

        public async Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default)

        {        public async Task<(IEnumerable<User> Users, int TotalCount)> GetPagedUsersAsync(

            return await _dbSet.AnyAsync(p => p.Name == name, cancellationToken);

        }            int pageNumber,         }

    }

            int pageSize, 

    /// <summary>

    /// User session repository implementation            string? searchTerm = null,

    /// </summary>

    public class UserSessionRepository : BaseRepository<UserSession>, IUserSessionRepository            CancellationToken cancellationToken = default)

    {

        public UserSessionRepository(AuthDbContext context) : base(context)        {        public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)

        {

        }            var query = _dbSet.AsQueryable();



        public async Task<UserSession?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)        {        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)

        {

            return await _dbSet            if (!string.IsNullOrWhiteSpace(searchTerm))

                .Include(s => s.User)

                .FirstOrDefaultAsync(s => s.Token == token, cancellationToken);            {            return await _context.SaveChangesAsync(cancellationToken);

        }

                query = query.Where(u => u.Email.Contains(searchTerm) || 

        public async Task<IEnumerable<UserSession>> GetActiveSessionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)

        {                                       u.Username.Contains(searchTerm) ||         }        {        {

            return await _dbSet

                .Where(s => s.UserId == userId && s.IsActive && s.ExpiresAt > DateTime.UtcNow)                                       u.FirstName.Contains(searchTerm) || 

                .ToListAsync(cancellationToken);

        }                                       u.LastName.Contains(searchTerm));    }



        public async Task<bool> IsTokenValidAsync(string token, CancellationToken cancellationToken = default)            }

        {

            return await _dbSet.AnyAsync(s => s.Token == token &&             var result = await _dbSet.AddAsync(entity, cancellationToken);            var result = await _dbSet.AddAsync(entity, cancellationToken);

                                             s.IsActive && 

                                             s.ExpiresAt > DateTime.UtcNow,             var totalCount = await query.CountAsync(cancellationToken);

                                             cancellationToken);

        }                /// <summary>



        public async Task InvalidateSessionAsync(string token, CancellationToken cancellationToken = default)            var users = await query

        {

            var session = await _dbSet.FirstOrDefaultAsync(s => s.Token == token, cancellationToken);                .Skip((pageNumber - 1) * pageSize)    /// User repository implementation            return result.Entity;            return result.Entity;

            if (session != null)

            {                .Take(pageSize)

                session.IsActive = false;

                await _context.SaveChangesAsync(cancellationToken);                .Include(u => u.UserRoles)    /// </summary>

            }

        }                    .ThenInclude(ur => ur.Role)



        public async Task InvalidateAllUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default)                .ToListAsync(cancellationToken);    public class UserRepository : BaseRepository<User>, IUserRepository        }        }

        {

            var sessions = await _dbSet

                .Where(s => s.UserId == userId && s.IsActive)

                .ToListAsync(cancellationToken);            return (users, totalCount);    {



            foreach (var session in sessions)        }

            {

                session.IsActive = false;    }        public UserRepository(AuthDbContext context) : base(context)

            }



            await _context.SaveChangesAsync(cancellationToken);

        }    /// <summary>        {



        public async Task CleanupExpiredSessionsAsync(CancellationToken cancellationToken = default)    /// Role repository implementation

        {

            var expiredSessions = await _dbSet    /// </summary>        }        public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)        public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)

                .Where(s => s.ExpiresAt <= DateTime.UtcNow)

                .ToListAsync(cancellationToken);    public class RoleRepository : BaseRepository<Role>, IRoleRepository



            _dbSet.RemoveRange(expiredSessions);    {

            await _context.SaveChangesAsync(cancellationToken);

        }        public RoleRepository(AuthDbContext context) : base(context)

    }

        {        public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)        {        {

    /// <summary>

    /// Device repository implementation        }

    /// </summary>

    public class DeviceRepository : BaseRepository<Device>, IDeviceRepository        {

    {

        public DeviceRepository(AuthDbContext context) : base(context)        public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)

        {

        }        {            return await _dbSet.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);            _dbSet.Update(entity);            _dbSet.Update(entity);



        public async Task<Device?> GetByDeviceKeyAsync(string deviceKey, CancellationToken cancellationToken = default)            return await _dbSet

        {

            return await _dbSet.FirstOrDefaultAsync(d => d.DeviceKey == deviceKey, cancellationToken);                .Include(r => r.RolePermissions)        }

        }

                    .ThenInclude(rp => rp.Permission)

        public async Task<IEnumerable<Device>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)

        {                .FirstOrDefaultAsync(r => r.Name == name, cancellationToken);        }        }

            return await _dbSet

                .Where(d => d.UserId == userId)        }

                .OrderByDescending(d => d.LastUsedAt)

                .ToListAsync(cancellationToken);        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)

        }

        public async Task<IEnumerable<Role>> GetRolesWithPermissionsAsync(CancellationToken cancellationToken = default)

        public async Task<bool> IsDeviceValidAsync(string deviceKey, CancellationToken cancellationToken = default)

        {        {        {

            return await _dbSet.AnyAsync(d => d.DeviceKey == deviceKey && d.IsActive, cancellationToken);

        }            return await _dbSet



        public async Task UpdateLastUsedAsync(string deviceKey, CancellationToken cancellationToken = default)                .Include(r => r.RolePermissions)            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken);

        {

            var device = await _dbSet.FirstOrDefaultAsync(d => d.DeviceKey == deviceKey, cancellationToken);                    .ThenInclude(rp => rp.Permission)

            if (device != null)

            {                .ToListAsync(cancellationToken);        }        public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)        public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)

                device.LastUsedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync(cancellationToken);        }

            }

        }



        public async Task DeactivateDeviceAsync(string deviceKey, CancellationToken cancellationToken = default)        public async Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default)

        {

            var device = await _dbSet.FirstOrDefaultAsync(d => d.DeviceKey == deviceKey, cancellationToken);        {        public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken cancellationToken = default)        {        {

            if (device != null)

            {            return await _dbSet.AnyAsync(r => r.Name == name, cancellationToken);

                device.IsActive = false;

                await _context.SaveChangesAsync(cancellationToken);        }        {

            }

        }

    }

        public async Task AddPermissionToRoleAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)            return await _dbSet.FirstOrDefaultAsync(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail.ToLowerInvariant(), cancellationToken);            _dbSet.Remove(entity);            _dbSet.Remove(entity);

    /// <summary>

    /// Audit log repository implementation        {

    /// </summary>

    public class AuditLogRepository : BaseRepository<AuditLog>, IAuditLogRepository            var rolePermission = new RolePermission         }

    {

        public AuditLogRepository(AuthDbContext context) : base(context)            { 

        {

        }                RoleId = roleId,         }        }



        public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)                PermissionId = permissionId 

        {

            return await _dbSet            };        public async Task<User?> GetWithRolesAsync(Guid userId, CancellationToken cancellationToken = default)

                .Where(log => log.UserId == userId)

                .OrderByDescending(log => log.Timestamp)            

                .ToListAsync(cancellationToken);

        }            _context.Set<RolePermission>().Add(rolePermission);        {



        public async Task<IEnumerable<AuditLog>> GetByActionAsync(string action, CancellationToken cancellationToken = default)            await _context.SaveChangesAsync(cancellationToken);

        {

            return await _dbSet        }            return await _dbSet

                .Where(log => log.Action == action)

                .OrderByDescending(log => log.Timestamp)

                .ToListAsync(cancellationToken);

        }        public async Task RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)                .Include(u => u.UserRoles)        public virtual async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)        public virtual async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)



        public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(        {

            DateTime startDate, 

            DateTime endDate,             var rolePermission = await _context.Set<RolePermission>()                .ThenInclude(ur => ur.Role)

            CancellationToken cancellationToken = default)

        {                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, cancellationToken);

            return await _dbSet

                .Where(log => log.Timestamp >= startDate && log.Timestamp <= endDate)                            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);        {        {

                .OrderByDescending(log => log.Timestamp)

                .ToListAsync(cancellationToken);            if (rolePermission != null)

        }

            {        }

        public async Task<(IEnumerable<AuditLog> Logs, int TotalCount)> GetPagedLogsAsync(

            int pageNumber,                 _context.Set<RolePermission>().Remove(rolePermission);

            int pageSize, 

            Guid? userId = null,                await _context.SaveChangesAsync(cancellationToken);            var entity = await GetByIdAsync(id, cancellationToken);            var entity = await GetByIdAsync(id, cancellationToken);

            string? action = null,

            DateTime? startDate = null,            }

            DateTime? endDate = null,

            CancellationToken cancellationToken = default)        }        public async Task<User?> GetWithPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)

        {

            var query = _dbSet.AsQueryable();    }



            if (userId.HasValue)        {            if (entity != null)            if (entity != null)

                query = query.Where(log => log.UserId == userId.Value);

    /// <summary>

            if (!string.IsNullOrWhiteSpace(action))

                query = query.Where(log => log.Action == action);    /// Permission repository implementation            return await _dbSet



            if (startDate.HasValue)    /// </summary>

                query = query.Where(log => log.Timestamp >= startDate.Value);

    public class PermissionRepository : BaseRepository<Permission>, IPermissionRepository                .Include(u => u.UserRoles)            {            {

            if (endDate.HasValue)

                query = query.Where(log => log.Timestamp <= endDate.Value);    {



            var totalCount = await query.CountAsync(cancellationToken);        public PermissionRepository(AuthDbContext context) : base(context)                .ThenInclude(ur => ur.Role)

            

            var logs = await query        {

                .OrderByDescending(log => log.Timestamp)

                .Skip((pageNumber - 1) * pageSize)        }                .ThenInclude(r => r.RolePermissions)                await DeleteAsync(entity, cancellationToken);                await DeleteAsync(entity, cancellationToken);

                .Take(pageSize)

                .ToListAsync(cancellationToken);



            return (logs, totalCount);        public async Task<Permission?> GetByNameAsync(string name, CancellationToken cancellationToken = default)                .ThenInclude(rp => rp.Permission)

        }

        {

        public async Task CleanupOldLogsAsync(DateTime cutoffDate, CancellationToken cancellationToken = default)

        {            return await _dbSet.FirstOrDefaultAsync(p => p.Name == name, cancellationToken);                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);            }            }

            var oldLogs = await _dbSet

                .Where(log => log.Timestamp < cutoffDate)        }

                .ToListAsync(cancellationToken);

        }

            _dbSet.RemoveRange(oldLogs);

            await _context.SaveChangesAsync(cancellationToken);        public async Task<IEnumerable<Permission>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)

        }

    }        {        }        }

}
            return await _context.Set<RolePermission>()

                .Where(rp => rp.RoleId == roleId)        public async Task<User?> GetWithRolesAndPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)

                .Select(rp => rp.Permission)

                .ToListAsync(cancellationToken);        {

        }

            return await GetWithPermissionsAsync(userId, cancellationToken);

        public async Task<IEnumerable<Permission>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)

        {        }        public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)        public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)

            return await _context.Set<UserRole>()

                .Where(ur => ur.UserId == userId)

                .SelectMany(ur => ur.Role.RolePermissions)

                .Select(rp => rp.Permission)        public async Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)        {        {

                .Distinct()

                .ToListAsync(cancellationToken);        {

        }

            return await _dbSet.Where(u => u.IsActive && !u.IsDeleted).ToListAsync(cancellationToken);            return await _dbSet.FindAsync(new object[] { id }, cancellationToken) != null;            return await _dbSet.FindAsync(new object[] { id }, cancellationToken) != null;

        public async Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default)

        {        }

            return await _dbSet.AnyAsync(p => p.Name == name, cancellationToken);

        }        }        }

    }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName, CancellationToken cancellationToken = default)

    /// <summary>

    /// User session repository implementation        {

    /// </summary>

    public class UserSessionRepository : BaseRepository<UserSession>, IUserSessionRepository            return await _dbSet

    {

        public UserSessionRepository(AuthDbContext context) : base(context)                .Include(u => u.UserRoles)        public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)        public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)

        {

        }                .ThenInclude(ur => ur.Role)



        public async Task<UserSession?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)                .Where(u => u.UserRoles.Any(ur => ur.Role.Name == roleName))        {        {

        {

            return await _dbSet                .ToListAsync(cancellationToken);

                .Include(s => s.User)

                .FirstOrDefaultAsync(s => s.Token == token, cancellationToken);        }            return await _context.SaveChangesAsync(cancellationToken);            return await _context.SaveChangesAsync(cancellationToken);

        }



        public async Task<IEnumerable<UserSession>> GetActiveSessionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)

        {        public async Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)        }        }

            return await _dbSet

                .Where(s => s.UserId == userId && s.IsActive && s.ExpiresAt > DateTime.UtcNow)        {

                .ToListAsync(cancellationToken);

        }            return await _dbSet.AnyAsync(u => u.Username == username, cancellationToken);    }    }



        public async Task<bool> IsTokenValidAsync(string token, CancellationToken cancellationToken = default)        }

        {

            return await _dbSet.AnyAsync(s => s.Token == token && 

                                             s.IsActive && 

                                             s.ExpiresAt > DateTime.UtcNow,         public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)

                                             cancellationToken);

        }        {    /// <summary>    /// <summary>



        public async Task InvalidateSessionAsync(string token, CancellationToken cancellationToken = default)            return await _dbSet.AnyAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken);

        {

            var session = await _dbSet.FirstOrDefaultAsync(s => s.Token == token, cancellationToken);        }    /// User repository implementation    /// User repository implementation

            if (session != null)

            {

                session.IsActive = false;

                await _context.SaveChangesAsync(cancellationToken);        public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default)    /// </summary>    /// </summary>

            }

        }        {



        public async Task InvalidateAllUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default)            return await UsernameExistsAsync(username, cancellationToken);    public class UserRepository : BaseRepository<User>, IUserRepository    public class UserRepository : BaseRepository<User>, IUserRepository

        {

            var sessions = await _dbSet        }

                .Where(s => s.UserId == userId && s.IsActive)

                .ToListAsync(cancellationToken);    {    {



            foreach (var session in sessions)        public async Task<(IEnumerable<User> users, int totalCount)> GetUsersAsync(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)

            {

                session.IsActive = false;        {        public UserRepository(AuthDbContext context) : base(context)        public UserRepository(AuthDbContext context) : base(context)

            }

            var query = _dbSet.Where(u => !u.IsDeleted);

            await _context.SaveChangesAsync(cancellationToken);

        }            var totalCount = await query.CountAsync(cancellationToken);        {        {



        public async Task CleanupExpiredSessionsAsync(CancellationToken cancellationToken = default)            var users = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        {

            var expiredSessions = await _dbSet            return (users, totalCount);        }        }

                .Where(s => s.ExpiresAt <= DateTime.UtcNow)

                .ToListAsync(cancellationToken);        }



            _dbSet.RemoveRange(expiredSessions);    }

            await _context.SaveChangesAsync(cancellationToken);

        }}

    }        public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)        public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)



    /// <summary>        {        {

    /// Device repository implementation

    /// </summary>            return await _dbSet            return await _dbSet

    public class DeviceRepository : BaseRepository<Device>, IDeviceRepository

    {                .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);                .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

        public DeviceRepository(AuthDbContext context) : base(context)

        {        }        }

        }



        public async Task<Device?> GetByDeviceKeyAsync(string deviceKey, CancellationToken cancellationToken = default)

        {        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)

            return await _dbSet.FirstOrDefaultAsync(d => d.DeviceKey == deviceKey, cancellationToken);

        }        {        {



        public async Task<IEnumerable<Device>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)            return await _dbSet            return await _dbSet

        {

            return await _dbSet                .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken);                .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken);

                .Where(d => d.UserId == userId)

                .OrderByDescending(d => d.LastUsedAt)        }        }

                .ToListAsync(cancellationToken);

        }



        public async Task<bool> IsDeviceValidAsync(string deviceKey, CancellationToken cancellationToken = default)        public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken cancellationToken = default)        public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken cancellationToken = default)

        {

            return await _dbSet.AnyAsync(d => d.DeviceKey == deviceKey && d.IsActive, cancellationToken);        {        {

        }

            return await _dbSet            return await _dbSet

        public async Task UpdateLastUsedAsync(string deviceKey, CancellationToken cancellationToken = default)

        {                .FirstOrDefaultAsync(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail.ToLowerInvariant(), cancellationToken);                .FirstOrDefaultAsync(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail.ToLowerInvariant(), cancellationToken);

            var device = await _dbSet.FirstOrDefaultAsync(d => d.DeviceKey == deviceKey, cancellationToken);

            if (device != null)        }        }

            {

                device.LastUsedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync(cancellationToken);

            }        public async Task<User?> GetWithRolesAsync(Guid userId, CancellationToken cancellationToken = default)        public async Task<User?> GetWithRolesAsync(Guid userId, CancellationToken cancellationToken = default)

        }

        {        {

        public async Task DeactivateDeviceAsync(string deviceKey, CancellationToken cancellationToken = default)

        {            return await _dbSet            return await _dbSet

            var device = await _dbSet.FirstOrDefaultAsync(d => d.DeviceKey == deviceKey, cancellationToken);

            if (device != null)                .Include(u => u.UserRoles)                .Include(u => u.UserRoles)

            {

                device.IsActive = false;                .ThenInclude(ur => ur.Role)                .ThenInclude(ur => ur.Role)

                await _context.SaveChangesAsync(cancellationToken);

            }                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        }

    }        }        }



    /// <summary>

    /// Audit log repository implementation

    /// </summary>        public async Task<User?> GetWithPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)        public async Task<User?> GetWithPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)

    public class AuditLogRepository : BaseRepository<AuditLog>, IAuditLogRepository

    {        {        {

        public AuditLogRepository(AuthDbContext context) : base(context)

        {            return await _dbSet            return await _dbSet

        }

                .Include(u => u.UserRoles)                .Include(u => u.UserRoles)

        public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)

        {                .ThenInclude(ur => ur.Role)                .ThenInclude(ur => ur.Role)

            return await _dbSet

                .Where(log => log.UserId == userId)                .ThenInclude(r => r.RolePermissions)                .ThenInclude(r => r.RolePermissions)

                .OrderByDescending(log => log.Timestamp)

                .ToListAsync(cancellationToken);                .ThenInclude(rp => rp.Permission)                .ThenInclude(rp => rp.Permission)

        }

                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        public async Task<IEnumerable<AuditLog>> GetByActionAsync(string action, CancellationToken cancellationToken = default)

        {        }        }

            return await _dbSet

                .Where(log => log.Action == action)

                .OrderByDescending(log => log.Timestamp)

                .ToListAsync(cancellationToken);        public async Task<User?> GetWithRolesAndPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)        public async Task<User?> GetWithRolesAndPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)

        }

        {        {

        public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(

            DateTime startDate,             return await GetWithPermissionsAsync(userId, cancellationToken);            return await GetWithPermissionsAsync(userId, cancellationToken);

            DateTime endDate, 

            CancellationToken cancellationToken = default)        }        }

        {

            return await _dbSet

                .Where(log => log.Timestamp >= startDate && log.Timestamp <= endDate)

                .OrderByDescending(log => log.Timestamp)        public async Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)        public async Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)

                .ToListAsync(cancellationToken);

        }        {        {



        public async Task<(IEnumerable<AuditLog> Logs, int TotalCount)> GetPagedLogsAsync(            return await _dbSet            return await _dbSet

            int pageNumber, 

            int pageSize,                 .Where(u => u.IsActive && !u.IsDeleted)                .Where(u => u.IsActive && !u.IsDeleted)

            Guid? userId = null,

            string? action = null,                .ToListAsync(cancellationToken);                .ToListAsync(cancellationToken);

            DateTime? startDate = null,

            DateTime? endDate = null,        }        }

            CancellationToken cancellationToken = default)

        {

            var query = _dbSet.AsQueryable();

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName, CancellationToken cancellationToken = default)        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName, CancellationToken cancellationToken = default)

            if (userId.HasValue)

                query = query.Where(log => log.UserId == userId.Value);        {        {



            if (!string.IsNullOrWhiteSpace(action))            return await _dbSet            return await _dbSet

                query = query.Where(log => log.Action == action);

                .Include(u => u.UserRoles)                .Include(u => u.UserRoles)

            if (startDate.HasValue)

                query = query.Where(log => log.Timestamp >= startDate.Value);                .ThenInclude(ur => ur.Role)                .ThenInclude(ur => ur.Role)



            if (endDate.HasValue)                .Where(u => u.UserRoles.Any(ur => ur.Role.Name == roleName))                .Where(u => u.UserRoles.Any(ur => ur.Role.Name == roleName))

                query = query.Where(log => log.Timestamp <= endDate.Value);

                .ToListAsync(cancellationToken);                .ToListAsync(cancellationToken);

            var totalCount = await query.CountAsync(cancellationToken);

                    }        }

            var logs = await query

                .OrderByDescending(log => log.Timestamp)

                .Skip((pageNumber - 1) * pageSize)

                .Take(pageSize)        public async Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)        public async Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)

                .ToListAsync(cancellationToken);

        {        {

            return (logs, totalCount);

        }            return await _dbSet            return await _dbSet



        public async Task CleanupOldLogsAsync(DateTime cutoffDate, CancellationToken cancellationToken = default)                .AnyAsync(u => u.Username == username, cancellationToken);                .AnyAsync(u => u.Username == username, cancellationToken);

        {

            var oldLogs = await _dbSet        }        }

                .Where(log => log.Timestamp < cutoffDate)

                .ToListAsync(cancellationToken);



            _dbSet.RemoveRange(oldLogs);        public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)        public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)

            await _context.SaveChangesAsync(cancellationToken);

        }        {        {

    }

}            return await _dbSet            return await _dbSet

                .AnyAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken);                .AnyAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken);

        }        }



        public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default)        public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default)

        {        {

            return await UsernameExistsAsync(username, cancellationToken);            return await UsernameExistsAsync(username, cancellationToken);

        }        }



        public async Task<(IEnumerable<User> users, int totalCount)> GetUsersAsync(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)        public async Task<(IEnumerable<User> users, int totalCount)> GetUsersAsync(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)

        {        {

            var query = _dbSet.Where(u => !u.IsDeleted);            var query = _dbSet.Where(u => !u.IsDeleted);

                        

            var totalCount = await query.CountAsync(cancellationToken);            var totalCount = await query.CountAsync(cancellationToken);

                        

            var users = await query            var users = await query

                .Skip((pageNumber - 1) * pageSize)                .Skip((pageNumber - 1) * pageSize)

                .Take(pageSize)                .Take(pageSize)

                .ToListAsync(cancellationToken);                .ToListAsync(cancellationToken);



            return (users, totalCount);            return (users, totalCount);

        }        }

    }    }



    /// <summary>    /// <summary>

    /// Role repository implementation    /// Role repository implementation

    /// </summary>    /// </summary>

    public class RoleRepository : BaseRepository<Role>, IRoleRepository    public class RoleRepository : BaseRepository<Role>, IRoleRepository

    {    {

        public RoleRepository(AuthDbContext context) : base(context)        public RoleRepository(AuthDbContext context) : base(context)

        {        {

        }        }



        public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)        public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)

        {        {

            return await _dbSet            return await _dbSet

                .FirstOrDefaultAsync(r => r.Name == name, cancellationToken);                .FirstOrDefaultAsync(r => r.Name == name, cancellationToken);

        }        }



        public async Task<IEnumerable<Role>> GetActiveRolesAsync(CancellationToken cancellationToken = default)        public async Task<IEnumerable<Role>> GetActiveRolesAsync(CancellationToken cancellationToken = default)

        {        {

            return await _dbSet            return await _dbSet

                .Where(r => r.IsActive && !r.IsDeleted)                .Where(r => r.IsActive && !r.IsDeleted)

                .ToListAsync(cancellationToken);                .ToListAsync(cancellationToken);

        }        }



        public async Task<IEnumerable<Role>> GetRolesByUserAsync(Guid userId, CancellationToken cancellationToken = default)        public async Task<IEnumerable<Role>> GetRolesByUserAsync(Guid userId, CancellationToken cancellationToken = default)

        {        {

            return await _context.UserRoles            return await _context.UserRoles

                .Where(ur => ur.UserId == userId)                .Where(ur => ur.UserId == userId)

                .Select(ur => ur.Role)                .Select(ur => ur.Role)

                .ToListAsync(cancellationToken);                .ToListAsync(cancellationToken);

        }        }



        public async Task<IEnumerable<Role>> GetRolesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)        public async Task<IEnumerable<Role>> GetRolesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)

        {        {

            return await GetRolesByUserAsync(userId, cancellationToken);            return await GetRolesByUserAsync(userId, cancellationToken);

        }        }



        public async Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)        public async Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)

        {        {

            return await _context.RolePermissions            return await _context.RolePermissions

                .Where(rp => rp.RoleId == roleId)                .Where(rp => rp.RoleId == roleId)

                .Select(rp => rp.Permission)                .Select(rp => rp.Permission)

                .ToListAsync(cancellationToken);                .ToListAsync(cancellationToken);

        }        }



        public async Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default)        public async Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default)

        {        {

            return await _dbSet            return await _dbSet

                .AnyAsync(r => r.Name == name, cancellationToken);                .AnyAsync(r => r.Name == name, cancellationToken);

        }        }

    }    }



    /// <summary>    /// <summary>

    /// Permission repository implementation    /// Permission repository implementation

    /// </summary>    /// </summary>

    public class PermissionRepository : BaseRepository<Permission>, IPermissionRepository    public class PermissionRepository : BaseRepository<Permission>, IPermissionRepository

    {    {

        public PermissionRepository(AuthDbContext context) : base(context)        public PermissionRepository(AuthDbContext context) : base(context)

        {        {

        }        }



        public async Task<Permission?> GetByNameAsync(string name, CancellationToken cancellationToken = default)        public async Task<Permission?> GetByNameAsync(string name, CancellationToken cancellationToken = default)

        {        {

            return await _dbSet            return await _dbSet

                .FirstOrDefaultAsync(p => p.Name == name, cancellationToken);                .FirstOrDefaultAsync(p => p.Name == name, cancellationToken);

        }        }



        public async Task<IEnumerable<Permission>> GetActivePermissionsAsync(CancellationToken cancellationToken = default)        public async Task<IEnumerable<Permission>> GetActivePermissionsAsync(CancellationToken cancellationToken = default)

        {        {

            return await _dbSet            return await _dbSet

                .Where(p => p.IsActive && !p.IsDeleted)                .Where(p => p.IsActive && !p.IsDeleted)

                .ToListAsync(cancellationToken);                .ToListAsync(cancellationToken);

        }        }



        public async Task<IEnumerable<Permission>> GetPermissionsByRoleAsync(Guid roleId, CancellationToken cancellationToken = default)        public async Task<IEnumerable<Permission>> GetPermissionsByRoleAsync(Guid roleId, CancellationToken cancellationToken = default)

        {        {

            return await _context.RolePermissions            return await _context.RolePermissions

                .Where(rp => rp.RoleId == roleId)                .Where(rp => rp.RoleId == roleId)

                .Select(rp => rp.Permission)                .Select(rp => rp.Permission)

                .ToListAsync(cancellationToken);                .ToListAsync(cancellationToken);

        }        }



        public async Task<IEnumerable<Permission>> GetPermissionsByUserAsync(Guid userId, CancellationToken cancellationToken = default)        public async Task<IEnumerable<Permission>> GetPermissionsByUserAsync(Guid userId, CancellationToken cancellationToken = default)

        {        {

            return await _context.UserRoles            return await _context.UserRoles

                .Where(ur => ur.UserId == userId)                .Where(ur => ur.UserId == userId)

                .SelectMany(ur => ur.Role.RolePermissions)                .SelectMany(ur => ur.Role.RolePermissions)

                .Select(rp => rp.Permission)                .Select(rp => rp.Permission)

                .Distinct()                .Distinct()

                .ToListAsync(cancellationToken);                .ToListAsync(cancellationToken);

        }        }



        public async Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default)        public async Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default)

        {        {

            return await _dbSet            return await _dbSet

                .AnyAsync(p => p.Name == name, cancellationToken);                .AnyAsync(p => p.Name == name, cancellationToken);

        }        }

    }    }



    /// <summary>    /// <summary>

    /// Device repository implementation    /// Device repository implementation

    /// </summary>    /// </summary>

    public class DeviceRepository : BaseRepository<Device>, IDeviceRepository    public class DeviceRepository : BaseRepository<Device>, IDeviceRepository

    {    {

        public DeviceRepository(AuthDbContext context) : base(context)        public DeviceRepository(AuthDbContext context) : base(context)

        {        {

        }        }



        public async Task<Device?> GetByDeviceKeyAsync(string deviceKey, CancellationToken cancellationToken = default)        public async Task<Device?> GetByDeviceKeyAsync(string deviceKey, CancellationToken cancellationToken = default)

        {        {

            return await _dbSet            return await _dbSet

                .FirstOrDefaultAsync(d => d.DeviceKey == deviceKey, cancellationToken);                .FirstOrDefaultAsync(d => d.DeviceKey == deviceKey, cancellationToken);

        }        }



        public async Task<IEnumerable<Device>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)        public async Task<IEnumerable<Device>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)

        {        {

            return await _dbSet            return await _dbSet

                .Where(d => d.UserId == userId && !d.IsDeleted)                .Where(d => d.UserId == userId && !d.IsDeleted)

                .ToListAsync(cancellationToken);                .ToListAsync(cancellationToken);

        }        }



        public async Task<IEnumerable<Device>> GetActiveDevicesAsync(CancellationToken cancellationToken = default)        public async Task<IEnumerable<Device>> GetActiveDevicesAsync(CancellationToken cancellationToken = default)

        {        {

            return await _dbSet            return await _dbSet

                .Where(d => d.IsActive && !d.IsDeleted)                .Where(d => d.IsActive && !d.IsDeleted)

                .ToListAsync(cancellationToken);                .ToListAsync(cancellationToken);

        }        }



        public async Task<IEnumerable<Device>> GetTrustedDevicesAsync(Guid userId, CancellationToken cancellationToken = default)        public async Task<IEnumerable<Device>> GetTrustedDevicesAsync(Guid userId, CancellationToken cancellationToken = default)

        {        {

            return await _dbSet            return await _dbSet

                .Where(d => d.UserId == userId && d.IsTrusted && !d.IsDeleted)                .Where(d => d.UserId == userId && d.IsTrusted && !d.IsDeleted)

                .ToListAsync(cancellationToken);                .ToListAsync(cancellationToken);

        }        }



        public async Task<IEnumerable<Device>> GetDevicesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)        public async Task<IEnumerable<Device>> GetDevicesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)

        {        {

            return await GetByUserIdAsync(userId, cancellationToken);            return await GetByUserIdAsync(userId, cancellationToken);

        }        }



        public async Task<int> GetActiveDeviceCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)        public async Task<int> GetActiveDeviceCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)

        {        {

            return await _dbSet            return await _dbSet

                .CountAsync(d => d.UserId == userId && d.IsActive && !d.IsDeleted, cancellationToken);                .CountAsync(d => d.UserId == userId && d.IsActive && !d.IsDeleted, cancellationToken);

        }        }



        public async Task<bool> DeviceKeyExistsAsync(string deviceKey, CancellationToken cancellationToken = default)        public async Task<bool> DeviceKeyExistsAsync(string deviceKey, CancellationToken cancellationToken = default)

        {        {

            return await _dbSet            return await _dbSet

                .AnyAsync(d => d.DeviceKey == deviceKey, cancellationToken);                .AnyAsync(d => d.DeviceKey == deviceKey, cancellationToken);

        }        }



        public async Task<bool> IsDeviceTrustedAsync(string deviceKey, Guid userId, CancellationToken cancellationToken = default)        public async Task<bool> IsDeviceTrustedAsync(string deviceKey, Guid userId, CancellationToken cancellationToken = default)

        {        {

            return await _dbSet            return await _dbSet

                .AnyAsync(d => d.DeviceKey == deviceKey && d.UserId == userId && d.IsTrusted, cancellationToken);                .AnyAsync(d => d.DeviceKey == deviceKey && d.UserId == userId && d.IsTrusted, cancellationToken);

        }        }

    }    }



    /// <summary>    /// <summary>

    /// UserSession repository implementation    /// UserSession repository implementation

    /// </summary>    /// </summary>

    public class UserSessionRepository : BaseRepository<UserSession>, IUserSessionRepository    public class UserSessionRepository : BaseRepository<UserSession>, IUserSessionRepository

    {    {

        public UserSessionRepository(AuthDbContext context) : base(context)        public UserSessionRepository(AuthDbContext context) : base(context)

        {        {

        }        }



        public async Task<UserSession?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)        public async Task<UserSession?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)

        {        {

            return await _dbSet            return await _dbSet

                .FirstOrDefaultAsync(s => s.RefreshToken == refreshToken, cancellationToken);                .FirstOrDefaultAsync(s => s.RefreshToken == refreshToken, cancellationToken);

        }        }



        public async Task<IEnumerable<UserSession>> GetActiveSessionsByUserAsync(Guid userId, CancellationToken cancellationToken = default)        public async Task<IEnumerable<UserSession>> GetActiveSessionsByUserAsync(Guid userId, CancellationToken cancellationToken = default)

        {        {

            return await _dbSet            return await _dbSet

                .Where(s => s.UserId == userId && s.IsActive && s.ExpiresAt > DateTime.UtcNow)                .Where(s => s.UserId == userId && s.IsActive && s.ExpiresAt > DateTime.UtcNow)

                .ToListAsync(cancellationToken);                .ToListAsync(cancellationToken);

        }        }



        public async Task<UserSession?> GetActiveSessionByDeviceAsync(Guid userId, string deviceKey, CancellationToken cancellationToken = default)        public async Task<UserSession?> GetActiveSessionByDeviceAsync(Guid userId, string deviceKey, CancellationToken cancellationToken = default)

        {        {

            return await _dbSet            return await _dbSet

                .FirstOrDefaultAsync(s => s.UserId == userId && s.DeviceKey == deviceKey && s.IsActive && s.ExpiresAt > DateTime.UtcNow, cancellationToken);                .FirstOrDefaultAsync(s => s.UserId == userId && s.DeviceKey == deviceKey && s.IsActive && s.ExpiresAt > DateTime.UtcNow, cancellationToken);

        }        }



        public async Task<UserSession> CreateAsync(UserSession userSession, CancellationToken cancellationToken = default)        public async Task<UserSession> CreateAsync(UserSession userSession, CancellationToken cancellationToken = default)

        {        {

            return await AddAsync(userSession, cancellationToken);            return await AddAsync(userSession, cancellationToken);

        }        }



        public async Task ExpireSessionAsync(string refreshToken, CancellationToken cancellationToken = default)        public async Task ExpireSessionAsync(string refreshToken, CancellationToken cancellationToken = default)

        {        {

            var session = await GetByRefreshTokenAsync(refreshToken, cancellationToken);            var session = await GetByRefreshTokenAsync(refreshToken, cancellationToken);

            if (session != null)            if (session != null)

            {            {

                session.IsActive = false;                session.IsActive = false;

                session.UpdatedAt = DateTime.UtcNow;                session.UpdatedAt = DateTime.UtcNow;

                await UpdateAsync(session, cancellationToken);                await UpdateAsync(session, cancellationToken);

            }            }

        }        }



        public async Task ExpireAllUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default)        public async Task ExpireAllUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default)

        {        {

            var sessions = await _dbSet            var sessions = await _dbSet

                .Where(s => s.UserId == userId && s.IsActive)                .Where(s => s.UserId == userId && s.IsActive)

                .ToListAsync(cancellationToken);                .ToListAsync(cancellationToken);



            foreach (var session in sessions)            foreach (var session in sessions)

            {            {

                session.IsActive = false;                session.IsActive = false;

                session.UpdatedAt = DateTime.UtcNow;                session.UpdatedAt = DateTime.UtcNow;

            }            }



            if (sessions.Any())            if (sessions.Any())

            {            {

                _dbSet.UpdateRange(sessions);                _dbSet.UpdateRange(sessions);

            }            }

        }        }



        public async Task CleanupExpiredSessionsAsync(CancellationToken cancellationToken = default)        public async Task CleanupExpiredSessionsAsync(CancellationToken cancellationToken = default)

        {        {

            var expiredSessions = await _dbSet            var expiredSessions = await _dbSet

                .Where(s => s.ExpiresAt <= DateTime.UtcNow || !s.IsActive)                .Where(s => s.ExpiresAt <= DateTime.UtcNow || !s.IsActive)

                .ToListAsync(cancellationToken);                .ToListAsync(cancellationToken);



            if (expiredSessions.Any())            if (expiredSessions.Any())

            {            {

                _dbSet.RemoveRange(expiredSessions);                _dbSet.RemoveRange(expiredSessions);

            }            }

        }        }

    }    }



    /// <summary>    /// <summary>

    /// AuditLog repository implementation    /// AuditLog repository implementation

    /// </summary>    /// </summary>

    public class AuditLogRepository : BaseRepository<AuditLog>, IAuditLogRepository    public class AuditLogRepository : BaseRepository<AuditLog>, IAuditLogRepository

    {    {

        public AuditLogRepository(AuthDbContext context) : base(context)        public AuditLogRepository(AuthDbContext context) : base(context)

        {        {

        }        }



        public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)        public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)

        {        {

            return await _dbSet            return await _dbSet

                .Where(a => a.UserId == userId)                .Where(a => a.UserId == userId)

                .OrderByDescending(a => a.CreatedAt)                .OrderByDescending(a => a.CreatedAt)

                .ToListAsync(cancellationToken);                .ToListAsync(cancellationToken);

        }        }



        public async Task<IEnumerable<AuditLog>> GetByActionAsync(string action, CancellationToken cancellationToken = default)        public async Task<IEnumerable<AuditLog>> GetByActionAsync(string action, CancellationToken cancellationToken = default)

        {        {

            return await _dbSet            return await _dbSet

                .Where(a => a.Action == action)                .Where(a => a.Action == action)

                .OrderByDescending(a => a.CreatedAt)                .OrderByDescending(a => a.CreatedAt)

                .ToListAsync(cancellationToken);                .ToListAsync(cancellationToken);

        }        }



        public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)        public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)

        {        {

            return await _dbSet            return await _dbSet

                .Where(a => a.CreatedAt >= startDate && a.CreatedAt <= endDate)                .Where(a => a.CreatedAt >= startDate && a.CreatedAt <= endDate)

                .OrderByDescending(a => a.CreatedAt)                .OrderByDescending(a => a.CreatedAt)

                .ToListAsync(cancellationToken);                .ToListAsync(cancellationToken);

        }        }



        public async Task<IEnumerable<AuditLog>> GetByIpAddressAsync(string ipAddress, CancellationToken cancellationToken = default)        public async Task<IEnumerable<AuditLog>> GetByIpAddressAsync(string ipAddress, CancellationToken cancellationToken = default)

        {        {

            return await _dbSet            return await _dbSet

                .Where(a => a.IpAddress == ipAddress)                .Where(a => a.IpAddress == ipAddress)

                .OrderByDescending(a => a.CreatedAt)                .OrderByDescending(a => a.CreatedAt)

                .ToListAsync(cancellationToken);                .ToListAsync(cancellationToken);

        }        }



        public async Task CleanupOldLogsAsync(DateTime beforeDate, CancellationToken cancellationToken = default)        public async Task CleanupOldLogsAsync(DateTime beforeDate, CancellationToken cancellationToken = default)

        {        {

            var oldLogs = await _dbSet            var oldLogs = await _dbSet

                .Where(a => a.CreatedAt < beforeDate)                .Where(a => a.CreatedAt < beforeDate)

                .ToListAsync(cancellationToken);                .ToListAsync(cancellationToken);



            if (oldLogs.Any())            if (oldLogs.Any())

            {            {

                _dbSet.RemoveRange(oldLogs);                _dbSet.RemoveRange(oldLogs);

            }            }

        }        }

    }    }

}}
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public virtual async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FindAsync(new object[] { id }, cancellationToken) != null;
        }
    }

    /// <summary>
    /// User repository implementation
    /// </summary>
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(AuthDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }

        public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail, cancellationToken);
        }

        public async Task<User?> GetWithRolesAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        }

        public async Task<User?> GetWithPermissionsAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ThenInclude(r => r!.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        }

        public async Task<bool> IsUsernameExistsAsync(string username, int? excludeUserId = null, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Where(u => u.Username == username);
            if (excludeUserId.HasValue)
                query = query.Where(u => u.Id != excludeUserId.Value);

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<bool> IsEmailExistsAsync(string email, int? excludeUserId = null, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Where(u => u.Email == email);
            if (excludeUserId.HasValue)
                query = query.Where(u => u.Id != excludeUserId.Value);

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(u => u.IsActive)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Where(u => u.UserRoles.Any(ur => ur.Role!.Name == roleName))
                .ToListAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Device repository implementation
    /// </summary>
    public class DeviceRepository : BaseRepository<Device>, IDeviceRepository
    {
        public DeviceRepository(AuthDbContext context) : base(context)
        {
        }

        public async Task<Device?> GetByDeviceKeyAsync(string deviceKey, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.DeviceKey == deviceKey, cancellationToken);
        }

        public async Task<IEnumerable<Device>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.LastUsedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Device>> GetActiveDevicesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(d => d.IsActive)
                .Include(d => d.User)
                .OrderByDescending(d => d.LastUsedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> IsDeviceKeyExistsAsync(string deviceKey, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(d => d.DeviceKey == deviceKey, cancellationToken);
        }

        public async Task<bool> ValidateDeviceAsync(string deviceKey, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(d => d.DeviceKey == deviceKey && d.IsActive, cancellationToken);
        }

        public async Task UpdateLastUsedAsync(int deviceId, string? ipAddress = null, CancellationToken cancellationToken = default)
        {
            var device = await GetByIdAsync(deviceId, cancellationToken);
            if (device != null)
            {
                device.LastUsedAt = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(ipAddress))
                    device.LastIpAddress = ipAddress;

                await UpdateAsync(device, cancellationToken);
            }
        }
    }

    /// <summary>
    /// User session repository implementation
    /// </summary>
    public class UserSessionRepository : BaseRepository<UserSession>, IUserSessionRepository
    {
        public UserSessionRepository(AuthDbContext context) : base(context)
        {
        }

        public async Task<UserSession?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(s => s.User)
                .Include(s => s.Device)
                .FirstOrDefaultAsync(s => s.RefreshToken == refreshToken && s.IsActive, cancellationToken);
        }

        public async Task<IEnumerable<UserSession>> GetActiveSessionsByUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(s => s.Device)
                .Where(s => s.UserId == userId && s.IsActive && s.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(s => s.LastActivityAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserSession>> GetActiveSessionsByDeviceAsync(int deviceId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(s => s.User)
                .Where(s => s.DeviceId == deviceId && s.IsActive && s.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(s => s.LastActivityAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<UserSession?> GetActiveSessionAsync(int userId, int? deviceId = null, CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .Where(s => s.UserId == userId && s.IsActive && s.ExpiresAt > DateTime.UtcNow);

            if (deviceId.HasValue)
                query = query.Where(s => s.DeviceId == deviceId.Value);

            return await query
                .OrderByDescending(s => s.LastActivityAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task InvalidateSessionAsync(int sessionId, CancellationToken cancellationToken = default)
        {
            var session = await GetByIdAsync(sessionId, cancellationToken);
            if (session != null)
            {
                session.IsActive = false;
                await UpdateAsync(session, cancellationToken);
            }
        }

        public async Task InvalidateAllUserSessionsAsync(int userId, CancellationToken cancellationToken = default)
        {
            var sessions = await _dbSet
                .Where(s => s.UserId == userId && s.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var session in sessions)
            {
                session.IsActive = false;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task InvalidateExpiredSessionsAsync(CancellationToken cancellationToken = default)
        {
            var expiredSessions = await _dbSet
                .Where(s => s.IsActive && s.ExpiresAt <= DateTime.UtcNow)
                .ToListAsync(cancellationToken);

            foreach (var session in expiredSessions)
            {
                session.IsActive = false;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateLastActivityAsync(int sessionId, CancellationToken cancellationToken = default)
        {
            var session = await GetByIdAsync(sessionId, cancellationToken);
            if (session != null)
            {
                session.LastActivityAt = DateTime.UtcNow;
                await UpdateAsync(session, cancellationToken);
            }
        }
    }
}

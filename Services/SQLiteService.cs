using ApiDevBP.Entities;
using Microsoft.Extensions.Options;
using SQLite;

namespace ApiDevBP.Services
{
    public class SQLiteService
    {
        private readonly SQLiteConnection _db;
        private readonly ILogger<SQLiteService> _logger;

        public SQLiteService(ILogger<SQLiteService> logger, IOptions<SQLiteOption> options)
        {
            _logger = logger;

            var databasePath = options.Value.DatabasePath;
            _db = new SQLiteConnection($"Data Source={databasePath};Version=3;");
            _db.CreateTable<UserEntity>();
        }

        public bool SaveUser(UserEntity user)
        {
            try
            {
                var existingUser = _db.Table<UserEntity>().FirstOrDefault(u => u.Name == user.Name && u.Lastname == user.Lastname);
                if (existingUser != null)
                {
                    _logger.LogWarning("No se pudo insertar el usuario. Ya existe un usuario con el mismo Name y Lastname.");
                    return false;
                }

                var result = _db.Insert(user);
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar usuario en la base de datos");
                return false;
            }
        }

        public IEnumerable<UserEntity> GetUsers()
        {
            try
            {
                return [.. _db.Table<UserEntity>()];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios de la base de datos");
                return Enumerable.Empty<UserEntity>();
            }
        }

        public UserEntity? GetUserById(int id)
        {
            try
            {
                return _db.Find<UserEntity>(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario por ID de la base de datos");
                return null;
            }
        }

        public bool UpdateUser(UserEntity user)
        {
            try
            {
                // Verificar si ya existe un usuario con el mismo Name y Lastname, excluyendo el usuario actual
                var existingUser = _db.Table<UserEntity>().FirstOrDefault(u => u.Name == user.Name && u.Lastname == user.Lastname && u.Id != user.Id);
                if (existingUser != null)
                {
                    _logger.LogWarning("No se pudo actualizar el usuario. Ya existe otro usuario con el mismo Name y Lastname.");
                    return false;
                }

                _db.Update(user);
                _logger.LogInformation("Usuario actualizado correctamente.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario en la base de datos.");
                return false;
            }
        }

        public bool DeleteUser(UserEntity user)
        {
            try
            {
                var result = _db.Delete(user);
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario de la base de datos");
                return false;
            }
        }
    }
}
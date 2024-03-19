using ApiDevBP.Entities;
using ApiDevBP.Models;
using ApiDevBP.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace ApiDevBP.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly SQLiteService _db;
        private readonly IMapper _mapper;
        private readonly ILogger<UsersController> _logger;

        public UsersController(ILogger<UsersController> logger, SQLiteService sqliteService, IMapper mapper)
        {
            _logger = logger;
            _db = sqliteService;
            _mapper = mapper;
        }

        /// <summary>
        /// Obtiene todos los usuarios en la base.
        /// </summary>
        [HttpGet]
        public IActionResult GetUsers()
        {
            try
            {
                var users = _db.GetUsers();
                var userModels = _mapper.Map<IEnumerable<UserModel>>(users);

                _logger.LogInformation("Usuarios obtenidos exitosamente");

                return Ok(userModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener usuarios");
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Obtiene un usuario por su ID.
        /// </summary>
        /// <param name="id">ID del usuario a buscar.</param>
        /// <returns> Usuario correspondiente al ID.</returns>
        [HttpGet("{id}")]
        public IActionResult GetUserById(int id)
        {
            try
            {
                var user = _db.GetUserById(id);
                if (user == null)
                {
                    _logger.LogWarning("Usuario con ID {Id} no encontrado", id);
                    return NotFound();
                }

                var userModel = _mapper.Map<UserModel>(user);

                _logger.LogInformation("Usuario con ID {Id} obtenido exitosamente", id);

                return Ok(userModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener usuario por ID");
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Guarda un nuevo usuario en la base de datos.
        /// </summary>
        /// <param name="userModel">Datos ingresados por el usuario.</param>
        /// <returns> True si se guardó correctamente sino False.</returns>
        [HttpPost]
        public IActionResult SaveUser(UserModel userModel)
        {
            try
            {
                var userEntity = _mapper.Map<UserEntity>(userModel);

                // Validar si se está intentando crear un usuario con el mismo Name y Lastname
                if (!_db.SaveUser(userEntity))
                {
                    _logger.LogWarning("No se pudo crear el usuario. Ya existe otro usuario con el mismo Name y Lastname.");
                    return Conflict();
                }

                _logger.LogInformation("Usuario guardado exitosamente");
                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al guardar usuario");
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Actualiza un usuario existente por su ID.
        /// </summary>
        /// <param name="id">ID ingresado del usuario a actualizar.</param>
        /// <param name="userModel"> Datos ingresados por el usuario.</param>
        /// <returns> True si se actualizó correctamente sino False.</returns>
        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, UserModel userModel)
        {
            try
            {
                var existingUser = _db.GetUserById(id);
                if (existingUser == null)
                {
                    _logger.LogWarning("Usuario con ID {Id} no encontrado", id);
                    return NotFound();
                }

                // Mapear los campos actualizados desde el UserModel al UserEntity existente
                var updatedUser = _mapper.Map(userModel, existingUser);

                // Validar si se está intentando actualizar un usuario con el mismo Name y Lastname
                if (!_db.UpdateUser(updatedUser))
                {
                    _logger.LogWarning("No se pudo actualizar el usuario. Ya existe otro usuario con el mismo Name y Lastname.");
                    return Conflict();
                }

                _logger.LogInformation("Usuario con ID {Id} actualizado exitosamente", id);
                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al actualizar usuario");
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Elimina un usuario por su ID.
        /// </summary>
        /// <param name="id">ID ingresado del usuario a eliminar.</param>
        /// <returns> True si se eliminó correctamente sino False.</returns>
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            try
            {
                var user = _db.GetUserById(id);
                if (user == null)
                {
                    _logger.LogWarning("Usuario con ID {Id} no encontrado", id);
                    return NotFound();
                }

                var result = _db.DeleteUser(user);

                if (result)
                {
                    _logger.LogInformation("Usuario con ID {Id} eliminado exitosamente", id);
                    return Ok(true);
                }

                _logger.LogError("Error al eliminar usuario con ID {Id}", id);
                return StatusCode(500);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al eliminar usuario");
                return StatusCode(500);
            }
        }
    }
}
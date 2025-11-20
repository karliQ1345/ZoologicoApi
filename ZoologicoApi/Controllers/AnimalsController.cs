using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZoologicoApi.Data;
using ZoologicoApi.Models;

namespace ZoologicoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnimalsController : ControllerBase
    {
        private readonly ZoologicoContext _context;

        public AnimalsController(ZoologicoContext context)
        {
            _context = context;
        }

        // GET: api/Animals
        [HttpGet]
        public async Task<ActionResult<ApiResult<IEnumerable<Animal>>>> GetAnimales()
        {
            try
            {
                // Incluye las relaciones para devolver la Raza y la Especie completas
                var data = await _context.Animales
                                         .Include(a => a.Raza)
                                         .Include(a => a.Especie)
                                         .ToListAsync();

                // 200 OK
                return Ok(ApiResult<IEnumerable<Animal>>.SuccessResult(data));
            }
            catch (Exception ex)
            {
                // 500 Internal Server Error
                return StatusCode(500, ApiResult<IEnumerable<Animal>>.ErrorResult($"Error al obtener la lista de animales: {ex.Message}", 500));
            }
        }

        // GET: api/Animals/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResult<Animal>>> GetAnimal(int id)
        {
            try
            {
                var animal = await _context.Animales
                                           .Include(a => a.Raza)
                                           .Include(a => a.Especie)
                                           .FirstOrDefaultAsync(a => a.Id == id);

                if (animal == null)
                {
                    // 404 Not Found
                    return NotFound(ApiResult<Animal>.ErrorResult($"Animal con ID {id} no encontrado.", 404));
                }

                // 200 OK
                return Ok(ApiResult<Animal>.SuccessResult(animal));
            }
            catch (Exception ex)
            {
                // 500 Internal Server Error
                return StatusCode(500, ApiResult<Animal>.ErrorResult($"Error al obtener el animal: {ex.Message}", 500));
            }
        }

        // PUT: api/Animals/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResult<object>>> PutAnimal(int id, Animal animal)
        {
            if (id != animal.Id)
            {
                // 400 Bad Request
                return BadRequest(ApiResult<object>.ErrorResult("El ID de la ruta no coincide con el ID del animal.", 400));
            }

            _context.Entry(animal).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AnimalExists(id))
                {
                    // 404 Not Found
                    return NotFound(ApiResult<object>.ErrorResult("Animal no encontrado para actualizar.", 404));
                }
                else
                {
                    // 500 Internal Server Error (Error de concurrencia)
                    return StatusCode(500, ApiResult<object>.ErrorResult("Error de concurrencia al actualizar el animal.", 500));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<object>.ErrorResult($"Error al actualizar el animal: {ex.Message}", 500));
            }

            // 204 No Content (Éxito sin devolver datos)
            return NoContent();
        }

        // POST: api/Animals
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ApiResult<Animal>>> PostAnimal(Animal animal)
        {
            try
            {
                // Opcional: Validar que RazaId y EspecieId existan antes de guardar
                if (!_context.Razas.Any(r => r.Id == animal.RazaId) || !_context.Especies.Any(e => e.Id == animal.EspecieId))
                {
                    return BadRequest(ApiResult<Animal>.ErrorResult("RazaId o EspecieId son inválidos.", 400));
                }

                _context.Animales.Add(animal);
                await _context.SaveChangesAsync();

                // 201 Created
                // Devolvemos el objeto creado en el formato ApiResult
                return CreatedAtAction(nameof(GetAnimal), new { id = animal.Id }, ApiResult<Animal>.SuccessResult(animal, "Animal creado.", 201));
            }
            catch (Exception ex)
            {
                // 500 Internal Server Error
                return StatusCode(500, ApiResult<Animal>.ErrorResult($"Error al crear el animal: {ex.Message}", 500));
            }
        }

        // DELETE: api/Animals/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResult<object>>> DeleteAnimal(int id)
        {
            try
            {
                var animal = await _context.Animales.FindAsync(id);
                if (animal == null)
                {
                    // 404 Not Found
                    return NotFound(ApiResult<object>.ErrorResult("Animal no encontrado para eliminar.", 404));
                }

                _context.Animales.Remove(animal);
                await _context.SaveChangesAsync();

                // 200 OK con mensaje de éxito (alternativa a 204 No Content)
                return Ok(ApiResult<object>.SuccessResult(null, "Animal eliminado con éxito.", 200));
            }
            catch (Exception ex)
            {
                // 500 Internal Server Error
                return StatusCode(500, ApiResult<object>.ErrorResult($"Error al eliminar el animal: {ex.Message}", 500));
            }
        }

        private bool AnimalExists(int id)
        {
            return _context.Animales.Any(e => e.Id == id);
        }
    }
}

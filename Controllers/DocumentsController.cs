using Microsoft.AspNetCore.Mvc;
using apiDocument.Models;
using Newtonsoft.Json;

namespace apiDocument.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly EventsContext _context;

        public DocumentsController(EventsContext context)
        {
            _context = context;
        }

        // GET: api/Documents
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Evento>>> GetEvento()
        {
            var documents = await _context.GetDocumentsList();
            return Ok(documents);
        }
        
        // POST: api/Documents/Insertar
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Evento>> PostDocument(Evento evento)
        {
            try
            {
                // Obtiene el archivo adjunto del formData
                byte[] imagen = evento.ImagenBase64 != null ? Convert.FromBase64String(evento.ImagenBase64) : new byte[0];

                // Guarda la imagen en una subcarpeta y obtén la ruta
                evento.RutaImagen = GuardarImagenEnSubcarpeta(imagen);

                // Hacemos las asignaciones correspondientes
                evento!.RegistroFotografico = imagen;

                // Guarda el evento en la base de datos
                var resultado = await _context.GuardarEvento(evento);

                // Devuelve el resultado
                return File(resultado, "application/pdf", "Evento.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private static string GuardarImagenEnSubcarpeta(byte[] archivo)
        {
            try
            {
                // Ruta de la carpeta donde se guardarán las imágenes (relativa al directorio del proyecto)
                var carpetaDestino = Path.Combine("Imagenes", "ImagenesEvento");

                // Ruta física completa del directorio del proyecto
                var directorioProyecto = Directory.GetCurrentDirectory();

                // Ruta completa de la carpeta de destino
                var rutaCompletaCarpetaDestino = Path.Combine(directorioProyecto, carpetaDestino);

                // Verifica si la carpeta destino no existe, y si no, créala
                if (!Directory.Exists(rutaCompletaCarpetaDestino))
                {
                    Directory.CreateDirectory(rutaCompletaCarpetaDestino);
                }

                // Genera un nombre de archivo único para evitar colisiones
                var nombreArchivo = Guid.NewGuid().ToString() + ".jpg"; // Cambia la extensión según el tipo de archivo que recibas
                var rutaImagen = Path.Combine(rutaCompletaCarpetaDestino, nombreArchivo);

                // Guarda el archivo en la carpeta destino
                using (var fileStream = new FileStream(rutaImagen, FileMode.Create))
                {
                    fileStream.WriteAsync(archivo, 0, archivo.Length);
                }

                // Retorna la ruta relativa de la imagen guardada
                return Path.Combine(carpetaDestino, nombreArchivo);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }


    }
}

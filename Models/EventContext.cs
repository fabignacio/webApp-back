using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Buffers.Text;
using System.Data;



namespace apiDocument.Models
{
    public class EventsContext : DbContext
    {
        public EventsContext(DbContextOptions<EventsContext> options) : base(options) { }
        public async Task<List<Evento>> GetDocumentsList()
        {
            return await Events.FromSqlRaw("EXECUTE sp_ObtenerEventos").ToListAsync();
        }

        public async Task<byte[]> GuardarEvento(Evento evento)
        {

            //Insertamos el evento y obtener el resultado
            var newEvento = await InsertarEvento(evento);

            //Generamos el PDF
            byte[] archivoPDF = await GenerarPDF(newEvento);

            return archivoPDF;
        }

        public async Task<List<Evento>> InsertarEvento(Evento evento)
        {
            DateTime horaActual = DateTime.Now.ToLocalTime();

            var result = await Events
                .FromSqlRaw("EXECUTE sp_InsertarEvento @Fecha, @Descripcion, @Impacto, @RegistroFotografico, @AccionesInmediatas, @AtencionEvento, @ImagenBase64, @RutaImagen, @Operacion, @FechaAntecedente, @Ubicacion, @HoraInformada, @Nombre, @Cargo, @DanioGenerado, @Equipos",

                //Datos Evento
                new SqlParameter("@Fecha", SqlDbType.NVarChar, 10) { Value = evento.Fecha.ToString("yyyy-MM-dd") },
                new SqlParameter("@Descripcion", SqlDbType.NVarChar, -1) { Value = evento.Descripcion },
                new SqlParameter("@Impacto", SqlDbType.NVarChar, 5) { Value = evento.Impacto },
                new SqlParameter("@RegistroFotografico", SqlDbType.VarBinary, -1) { Value = evento.RegistroFotografico },
                new SqlParameter("@AccionesInmediatas", SqlDbType.NVarChar, -1) { Value = evento.AccionesInmediatas },
                new SqlParameter("@AtencionEvento", SqlDbType.NVarChar, -1) { Value = evento.AtencionEvento },
                new SqlParameter("@ImagenBase64", SqlDbType.NVarChar, -1) { Value = evento.ImagenBase64 },
                new SqlParameter("@RutaImagen", SqlDbType.NVarChar, -1) { Value = evento.RutaImagen },

                //Datos Antecedentes
                new SqlParameter("@Operacion", SqlDbType.NVarChar, -1) { Value = evento.Operacion },
                new SqlParameter("@FechaAntecedente", SqlDbType.NVarChar, 10) { Value = evento.FechaAntecedente.ToString("yyyy-MM-dd") },
                new SqlParameter("@Ubicacion", SqlDbType.NVarChar, -1) { Value = evento.Ubicacion },
                new SqlParameter("@HoraInformada", SqlDbType.Time) { Value = horaActual.TimeOfDay },

                //Datos Personal Involucrado
                new SqlParameter("@Nombre", SqlDbType.NVarChar, -1) { Value = evento.Nombre },
                new SqlParameter("@Cargo", SqlDbType.NVarChar, -1) { Value = evento.Cargo },
                new SqlParameter("@DanioGenerado", SqlDbType.NVarChar, -1) { Value = evento.DanioGenerado },
                new SqlParameter("@Equipos", SqlDbType.NVarChar, -1) { Value = evento.Equipos })

                .ToListAsync();
            return result;
        }

        public async Task<byte[]> GenerarPDF(List<Evento> evento)
        {

            //Definimos el tamaño del documento
            PageSize pageSize = new PageSize(PageSize.A4);

            float alturaPagina = pageSize.GetHeight();
            float anchoPagina = pageSize.GetWidth();

            // Definir los colores en variables
            Color colorFondo = new DeviceRgb(43, 15, 189); 
            Color colorNumero = new DeviceRgb(255, 207, 0);
            Color bajoImpacto = new DeviceRgb(25, 197, 114); 
            Color medioImpacto = new DeviceRgb(223, 142, 54);
            Color altoImpacto = new DeviceRgb(255, 107, 107);
            Color colorTexto = ColorConstants.WHITE; 

            //Imagen Camión
            string camionPath = "./imagenes/TransportesNazar.jpeg";
            ImageData camionData = ImageDataFactory.Create(camionPath);
            Image cam = new Image(camionData);

            //Configuramos y ancho altura del camion
            cam.SetWidth(anchoPagina);
            cam.SetHeight(150);

            await using (MemoryStream memoryStream = new MemoryStream())
            {
                try
                {
                    PdfWriter pdfWriter = new PdfWriter(memoryStream);
                    PdfDocument pdfDocument = new PdfDocument(pdfWriter);
                    Document pdf = new Document(pdfDocument);

                    // Configurar el documento para que no tenga margen
                    pdf.SetMargins(0, 0, 0, 0);

                    //Agregamos el contenido al documento
                    foreach (var document in evento)
                    {
                        // Agregar los datos del documento al PDF
                        Paragraph numeroFecha = new Paragraph()
                            .SetBackgroundColor(colorFondo)
                            .SetPadding(5)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .Add(new Text(document.Fecha.ToString("dd-MM-yyyy")).SetFontColor(colorTexto))
                            .Add(new Text(Environment.NewLine)) // Salto de línea
                            .Add(new Text($"N°: {document.ID}").SetFontColor(colorNumero));
                        // Agregar el contenedor al PDF
                        pdf.Add(numeroFecha);

                        //Camión
                        Paragraph Camion = new Paragraph()
                            .SetTextAlignment(TextAlignment.LEFT)
                            .Add(cam);
                        pdf.Add(Camion);

                        //Descripción
                        Paragraph Descripcion = new Paragraph()
                           .SetBackgroundColor(colorFondo)
                           .SetPadding(5)
                           .Add(new Text("DESCRIPCIÓN"))
                            .SetFontColor(colorTexto)
                            .SetFontSize(20)
                            .SetFont(PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN));
                        pdf.Add(Descripcion);
                        pdf.Add(new Paragraph(document.Descripcion));

                        //Impacto
                        Paragraph Impacto = new Paragraph()
                           .SetBackgroundColor(colorFondo)
                           .SetPadding(5)
                           .Add(new Text("TIPO DE INCIDENTE"))
                            .SetFontColor(colorTexto)
                            .SetFontSize(20).
                            SetFont(PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN));
                        pdf.Add(Impacto);

                        // Crear un Paragraph para las opciones
                        Paragraph tipoImpacto = new Paragraph();

                        // Agregar el texto de las opciones con estilos y la marca (X) según corresponda
                        tipoImpacto
                            .Add(new Text("Bajo Impacto: ").SetFontColor(bajoImpacto)).SetFontSize(16).SetFont(PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN))
                            .Add(new Text(document.Impacto == "BAJO" ? "X" : "").SetFontColor(bajoImpacto)).SetFontSize(16).SetFont(PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN))
                            .Add(new Text(" Medio Impacto: ").SetFontColor(medioImpacto)).SetFontSize(16).SetFont(PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN))
                            .Add(new Text(document.Impacto == "MEDIO" ? "X" : "").SetFontColor(medioImpacto)).SetFontSize(16).SetFont(PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN))
                            .Add(new Text(" Alto Impacto: ").SetFontColor(altoImpacto)).SetFontSize(16).SetFont(PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN))
                            .Add(new Text(document.Impacto == "ALTO" ? "X" : "").SetFontColor(altoImpacto).SetFontSize(16).SetFont(PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN)));
                        pdf.Add(tipoImpacto);

                        //Lista Personal Involucrado
                        List listPersonal = new List()
                               .SetSymbolIndent(12)
                               .SetListSymbol("\u2022")
                               .SetFontSize(16);
                        listPersonal.Add(new ListItem($"Nombre: {document.Nombre}"))
                            .SetBold()
                            .SetFont(PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN));
                        listPersonal.Add(new ListItem($"Cargo: {document.Cargo} "))
                            .SetBold()
                            .SetFont(PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN));
                        listPersonal.Add(new ListItem($"Daño generado: {document.DanioGenerado} "))
                            .SetBold()
                            .SetFont(PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN));
                        listPersonal.Add(new ListItem($"Equipos: {document.Equipos} "))
                            .SetBold()
                            .SetFont(PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN));

                        //Lista Antecedentes
                        List listAntecedentes = new List()
                               .SetSymbolIndent(16)
                               .SetListSymbol("\u2022")
                               .SetFontSize(12);
                        listAntecedentes.Add(new ListItem($"Operación: {document.Operacion} "))
                            .SetBold()
                            .SetFont(PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN));
                        listAntecedentes.Add(new ListItem($"Fecha: {document.FechaAntecedente.ToString("dddd-MM-yyyy")} "))
                            .SetBold()
                            .SetFont(PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN));
                        listAntecedentes.Add(new ListItem($"Ubicación: {document.Ubicacion}"))
                            .SetBold()
                            .SetFont(PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN));
                        listAntecedentes.Add(new ListItem($"Hora Informada: {document.HoraInformada} "))
                            .SetBold()
                            .SetFont(PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN));

                        //Personal Involucrado y Antecedentes
                        Table tablePersonalAntecedentes = new Table(2)
                                .SetWidth(UnitValue.CreatePercentValue(100))
                                .SetBackgroundColor(colorFondo);

                        Cell cellPersonal = new Cell()
                            .Add(new Paragraph("PERSONAL INVOLUCRADO"))
                            .SetFontColor(colorTexto)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetPadding(5)
                            .SetFontColor(colorTexto)
                            .SetFontSize(20)
                            .SetFont(PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN));

                        Cell cellAntecedentes = new Cell()
                            .Add(new Paragraph("ANTECEDENTES"))
                            .SetFontColor(colorTexto)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetPaddingTop(5)
                            .SetPaddingBottom(5)
                            .SetPaddingLeft(5)
                            .SetFontColor(colorTexto)
                            .SetFontSize(20)
                            .SetFont(PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN));

                        Cell cellDataPersonal = new Cell().Add(listPersonal).SetBackgroundColor(colorTexto);
                        Cell cellDataAntecedentes = new Cell().Add(listAntecedentes).SetBackgroundColor(colorTexto);

                        tablePersonalAntecedentes
                            .AddCell(cellPersonal)
                            .AddCell(cellAntecedentes)
                            .AddCell(cellDataPersonal)
                            .AddCell(cellDataAntecedentes);

                        pdf.Add(tablePersonalAntecedentes);

                        //Registro Fotografico y Acciones Inmediatas
                        Table tableRegistroAcciones = new Table(2)
                            .SetWidth(UnitValue.CreatePercentValue(100))
                            .SetBackgroundColor(colorFondo);

                        Cell cellRegistroFotografico = new Cell()
                            .Add(new Paragraph("REGISTRO FOTOGRÁFICO"))
                            .SetFontColor(colorTexto)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetPadding(5)
                            .SetFontColor(colorTexto)
                            .SetFontSize(20)
                            .SetFont(PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN));

                        Cell cellAcciones = new Cell()
                            .Add(new Paragraph("ACCIONES INMEDIATAS"))
                            .SetFontColor(colorTexto)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetPaddingTop(5)
                            .SetPaddingBottom(5)
                            .SetPaddingLeft(5)
                            .SetFontColor(colorTexto)
                            .SetFontSize(20)
                            .SetFont(PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN));

                        //Convertimos la foto guardada en base de datos a imagen
                        ImageData RegistroFotografico = ImageDataFactory.Create(document.RegistroFotografico);
                        Image registro = new Image(RegistroFotografico);

                        //Modificamos ancho y alto de la imagen
                        registro.SetWidth(200).SetHeight(250);

                        Cell cellDataRegistro = new Cell()
                            .Add(registro)
                            .SetBackgroundColor(colorTexto);
                        Cell cellDataAcciones = new Cell().Add(new Paragraph(document.AccionesInmediatas)).SetBackgroundColor(colorTexto);

                        tableRegistroAcciones
                            .AddCell(cellRegistroFotografico)
                            .AddCell(cellAcciones)
                            .AddCell(cellDataRegistro)
                            .AddCell(cellDataAcciones);

                        pdf.Add(tableRegistroAcciones);

                        //Atencion al Evento
                        Paragraph AtencionEvento = new Paragraph()
                           .SetBackgroundColor(colorFondo)
                           .SetPadding(5)
                           .Add(new Text("ATENCIÓN AL EVENTO"))
                            .SetFontColor(colorTexto)
                            .SetFontSize(20)
                            .SetFont(PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN));
                        pdf.Add(AtencionEvento);
                        pdf.Add(new Paragraph(document.AtencionEvento));

                        //Píe de página
                        Paragraph piePagina = new Paragraph()
                            .SetBackgroundColor(colorFondo)
                            .SetFixedPosition(0, 0, alturaPagina) // Fijar posición en la parte inferior de la página
                            .SetHeight(20); // Altura del párrafo
                        pdf.Add(piePagina);

                    }

                    pdf.Close();
                    return memoryStream.ToArray();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return memoryStream.ToArray();
                }
            }
        }

        public DbSet<Evento> Events { get; set; } = null!;
    }
}

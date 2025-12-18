using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using UPIICafeWeb.Models; 
using Microsoft.Extensions.Configuration; 

namespace UPIICafeWeb.Controllers
{
    public class MenuController : Controller
    {
        private readonly string cadenaSQL;

        public MenuController(IConfiguration config)
        {
            cadenaSQL = config.GetConnectionString("CadenaSQL");
        }

        // ==========================================
        // 1. VISTA MENÚ GENERAL (Index)
        // ==========================================
        public IActionResult Index()
        {
            // 1. SEGURIDAD: Verificar si el usuario inició sesión
            int? idRol = HttpContext.Session.GetInt32("RolUsuario");
            if (idRol == null) return RedirectToAction("Index", "Acceso");

            List<ProductoModel> lista = new List<ProductoModel>();

            using (SqlConnection cn = new SqlConnection(cadenaSQL))
            {
                // 2. CONSULTA SQL
                // Nota: Agregamos 'p.descrip' para mostrar la descripción en la tarjeta
                string query = @"
                    SELECT 
                        p.id_prod, 
                        p.nom_prod, 
                        p.descrip, 
                        p.precio, 
                        p.img_url,
                        p.id_categoria,  
                        ISNULL(d.porc_desc, 0) AS descuento_aplicable
                    FROM Productos p
                    LEFT JOIN Descuento d ON p.id_prod = d.id_prod AND d.id_rol = @idRol";

                SqlCommand cmd = new SqlCommand(query, cn);
                cmd.Parameters.AddWithValue("@idRol", idRol);

                cn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new ProductoModel
                    {
                        Id = Convert.ToInt32(reader["id_prod"]),
                        Nombre = reader["nom_prod"].ToString(),
                        
                        // AQUÍ CAPTURAMOS LA DESCRIPCIÓN
                        Descripcion = reader["descrip"].ToString(),

                        PrecioOriginal = Convert.ToDecimal(reader["precio"]),
                        ImagenUrl = reader["img_url"].ToString(),
                        IdCategoria = Convert.ToInt32(reader["id_categoria"]),
                        Descuento = Convert.ToDecimal(reader["descuento_aplicable"])
                    });
                }
            }

            return View(lista);
        }

        // ==========================================
        // 2. VISTA DE COCINA (SOLO TRABAJADORES - ROL 5)
        // ==========================================
        public IActionResult Cocina()
        {
            // 1. Verificamos si hay sesión
            int? idRol = HttpContext.Session.GetInt32("RolUsuario");

            // 2. SEGURIDAD ESTRICTA:
            // Si no hay sesión O el rol no es 5 (Trabajador), lo sacamos
            if (idRol == null || idRol != 5) 
            {
                return RedirectToAction("Index", "Acceso");
            }

            return View();
        }

    } // Fin de la clase
}
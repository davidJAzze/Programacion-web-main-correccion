using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration; 

namespace UPIICafeWeb.Controllers
{
    public class AccesoController : Controller
    {
        private readonly string cadenaSQL;

        public AccesoController(IConfiguration config)
        {
            cadenaSQL = config.GetConnectionString("CadenaSQL");
        }

        // ==========================================
        // 1. VISTA LOGIN GENERAL (GET)
        // ==========================================
        [HttpGet]
        public IActionResult Index()
        {
            return View(); 
        }

        // ==========================================
        // 2. VALIDAR LOGIN GENERAL (POST)
        // Permite entrar a Alumnos (Rol 2) y Profesores (Rol 3)
        // NOTA: Actualmente solo valida Boleta/RFC. 
        // ==========================================
        [HttpPost]
        public IActionResult Validar(string boleta_rfc)
        {
            using (SqlConnection cn = new SqlConnection(cadenaSQL))
            {
                // Buscamos si existe el usuario por Boleta o RFC
                // (Para mayor seguridad, en el futuro deberías validar también la contraseña aquí)
                string query = "SELECT id_rol FROM Usuarios WHERE boleta_rfc = @dato";
                SqlCommand cmd = new SqlCommand(query, cn);
                cmd.Parameters.AddWithValue("@dato", boleta_rfc);

                cn.Open();
                object resultado = cmd.ExecuteScalar(); 

                if (resultado != null)
                {
                    int idRol = Convert.ToInt32(resultado);
                    HttpContext.Session.SetInt32("RolUsuario", idRol);
                    
                    // Todos van al menú de comida
                    return RedirectToAction("Index", "Menu"); 
                }
                else
                {
                    // Si no existe, recargamos el login
                    return RedirectToAction("Index");
                }
            }
        }

        // ==========================================
        // 3. ENTRAR COMO VISITANTE (Rol 4)
        // ==========================================
        [HttpGet]
        public IActionResult EntrarComoVisitante()
        {
            HttpContext.Session.SetInt32("RolUsuario", 4); 
            return RedirectToAction("Index", "Menu");
        }

        // ==========================================
        // 4. VISTA REGISTRO (GET)
        // ==========================================
        [HttpGet]
        public IActionResult Registro()
        {
            return View();
        }

        // ==========================================
        // 5. PROCESAR REGISTRO (POST) - LÓGICA DE 3 ROLES
        // ==========================================
        [HttpPost]
        public IActionResult RegistrarUsuario(string rol, string nombre, string ape_pat, string correo, string boleta_rfc, string password, string adminKey)
        {
            int idRolAsignar = 0;

            // --- VALIDACIÓN GLOBAL: TODOS DEBEN TENER CONTRASEÑA ---
            if (string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Debes definir una contraseña.";
                return View("Registro");
            }

            // --- CASO 1: ALUMNO ---
            if (rol == "alumno")
            {
                // Validación: Exactamente 10 dígitos
                if (boleta_rfc.Length != 10)
                {
                    ViewBag.Error = "La Boleta de alumno debe tener exactamente 10 dígitos.";
                    return View("Registro");
                }
                
                idRolAsignar = 2; // ID Alumno en SQL
            }
            
            // --- CASO 2: PROFESOR ---
            else if (rol == "profesor")
            {
                // Validación: 12 o 13 caracteres (RFC)
                if (boleta_rfc.Length < 12 || boleta_rfc.Length > 13)
                {
                    ViewBag.Error = "El RFC de profesor debe tener 12 o 13 caracteres.";
                    return View("Registro");
                }

                idRolAsignar = 3; // ID Profesor en SQL
            }

            // --- CASO 3: TRABAJADOR ---
            else if (rol == "trabajador")
            {
                // Validación: 12 o 13 caracteres (RFC)
                if (boleta_rfc.Length < 12 || boleta_rfc.Length > 13)
                {
                    ViewBag.Error = "El RFC de trabajador debe tener 12 o 13 caracteres.";
                    return View("Registro");
                }
                // Validación: CLAVE ADMIN OBLIGATORIA
                if (adminKey != "Admin123") 
                {
                    ViewBag.Error = "La Clave de Admin es incorrecta.";
                    return View("Registro");
                }

                idRolAsignar = 5; // ID Trabajador en SQL
            }

            // --- GUARDADO EN BASE DE DATOS ---
            try
            {
                using (SqlConnection cn = new SqlConnection(cadenaSQL))
                {
                    string query = "INSERT INTO Usuarios (nombre, ape_pat, boleta_rfc, correo, password, id_rol) VALUES (@nom, @ape, @bol, @corr, @pass, @rol)";
                    
                    SqlCommand cmd = new SqlCommand(query, cn);
                    cmd.Parameters.AddWithValue("@nom", nombre);
                    cmd.Parameters.AddWithValue("@ape", ape_pat);
                    cmd.Parameters.AddWithValue("@bol", boleta_rfc);
                    cmd.Parameters.AddWithValue("@corr", correo);
                    cmd.Parameters.AddWithValue("@pass", password); // Guardamos la contraseña manual
                    cmd.Parameters.AddWithValue("@rol", idRolAsignar);

                    cn.Open();
                    cmd.ExecuteNonQuery();
                }
                // Si todo sale bien, ir al Login
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al registrar: Es posible que la Boleta/RFC o el Correo ya existan.";
                return View("Registro");
            }
        }

        // ==========================================
        // 6. VISTA LOGIN TRABAJADOR (GET)
        // ==========================================
        [HttpGet]
        public IActionResult LoginTrabajador()
        {
            return View();
        }

        // ==========================================
        // 7. VALIDAR TRABAJADOR (POST)
        // ==========================================
        [HttpPost]
        public IActionResult ValidarTrabajador(string rfc, string password)
        {
            using (SqlConnection cn = new SqlConnection(cadenaSQL))
            {
                // SEGURIDAD: Solo permite pasar si id_rol es 5 (Trabajador)
                string query = "SELECT id_rol FROM Usuarios WHERE boleta_rfc = @rfc AND password = @pass AND id_rol = 5";
                
                SqlCommand cmd = new SqlCommand(query, cn);
                cmd.Parameters.AddWithValue("@rfc", rfc);
                cmd.Parameters.AddWithValue("@pass", password);

                cn.Open();
                object resultado = cmd.ExecuteScalar();

                if (resultado != null)
                {
                    int idRol = Convert.ToInt32(resultado);
                    HttpContext.Session.SetInt32("RolUsuario", idRol);

                    // Redirige a la PANTALLA DE COCINA
                    return RedirectToAction("Cocina", "Menu");
                }
                else
                {
                    ViewBag.Error = "Acceso denegado. Verifica tus datos o permisos.";
                    return View("LoginTrabajador");
                }
            }
        }

        // ==========================================
        // 8. CERRAR SESIÓN (Salir)
        // ==========================================
        public IActionResult Salir()
        {
            // Borra la sesión actual
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

    } // Fin de la clase
}
using Newtonsoft.Json;

namespace Opentk_2222.Clases
{
    public class Serializar
    {
        static string path = "../../../EscenasGuardadas/";

        public static void GuardarComoJson<T>(T objeto, string nombreArchivo)
        {
            try
            {
                Directory.CreateDirectory(path);
                string json = JsonConvert.SerializeObject(objeto, Newtonsoft.Json.Formatting.Indented);
                string ruta = Path.Combine(path, nombreArchivo);
                File.WriteAllText(ruta, json);
                Console.WriteLine($"Escena guardada: {ruta}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error guardando: {ex.Message}");
            }
        }

        public static T CargarDesdeJson<T>(string nombreArchivo)
        {
            try
            {
                string ruta = Path.Combine(path, nombreArchivo);
                if (!File.Exists(ruta))
                {
                    Console.WriteLine($"Archivo no existe: {ruta}");
                    return default;
                }

                string json = File.ReadAllText(ruta);
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando: {ex.Message}");
                return default;
            }
        }

        public static List<string> ListarEscenasGuardadas()
        {
            try
            {
                if (!Directory.Exists(path))
                    return new List<string>();

                var archivos = Directory.GetFiles(path, "*.json");
                return archivos.Select(Path.GetFileName).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error listando archivos: {ex.Message}");
                return new List<string>();
            }
        }
    }
}
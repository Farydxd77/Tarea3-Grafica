using OpenTK.Mathematics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Opentk_2222.Configuration
{
    // Clase principal de configuración
    public class GameConfig
    {
        public RenderingConfig Rendering { get; set; } = new();
        public CameraConfig Camera { get; set; } = new();
        public LightingConfig Lighting { get; set; } = new();
        public InputConfig Input { get; set; } = new();
        public ObjectsConfig Objects { get; set; } = new();

        // Método para cargar configuración desde archivo
        public static GameConfig LoadFromFile(string filePath = "config.json")
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    var options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = true,
                        Converters = { new Vector3Converter() }
                    };

                    return JsonSerializer.Deserialize<GameConfig>(json, options) ?? new GameConfig();
                }
                else
                {
                    // Crear archivo de configuración por defecto si no existe
                    var defaultConfig = new GameConfig();
                    defaultConfig.SaveToFile(filePath);
                    return defaultConfig;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando configuración: {ex.Message}");
                Console.WriteLine("Usando configuración por defecto...");
                return new GameConfig();
            }
        }

        // Método para guardar configuración
        public void SaveToFile(string filePath = "config.json")
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true,
                    Converters = { new Vector3Converter() }
                };

                string json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(filePath, json);
                Console.WriteLine($"Configuración guardada en: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error guardando configuración: {ex.Message}");
            }
        }
    }

    // Configuraciones específicas por categoría
    public class RenderingConfig
    {
        public int WindowWidth { get; set; } = 1024;
        public int WindowHeight { get; set; } = 768;
        public string WindowTitle { get; set; } = "Sistema Jerárquico 3D - Monitor, CPU y Teclado";
        public Vector3 BackgroundColor { get; set; } = new(0.95f, 0.95f, 0.95f);
        public float RotationSpeed { get; set; } = 0.5f;
        public bool EnableVSync { get; set; } = true;
        public bool EnableDepthTest { get; set; } = true;
        public bool EnableCullFace { get; set; } = true;
    }

    public class CameraConfig
    {
        public Vector3 InitialPosition { get; set; } = new(3f, 2f, 4f);
        public Vector3 InitialTarget { get; set; } = new(0f, -0.5f, 0f);
        public Vector3 UpVector { get; set; } = new(0f, 1f, 0f);
        public float FieldOfView { get; set; } = 45f; // En grados
        public float NearPlane { get; set; } = 0.1f;
        public float FarPlane { get; set; } = 100f;
        public float MovementSpeed { get; set; } = 3.0f;
        public float RotationSpeed { get; set; } = 1.5f;
    }

    public class LightingConfig
    {
        public Vector3 Position { get; set; } = new(2f, 4f, 2f);
        public Vector3 Color { get; set; } = new(1f, 1f, 1f);
        public float Intensity { get; set; } = 1.0f;
        public float AmbientStrength { get; set; } = 0.4f;
        public float SpecularStrength { get; set; } = 0.6f;
        public float Shininess { get; set; } = 64f;
    }

    public class InputConfig
    {
        public string MoveForward { get; set; } = "W";
        public string MoveBackward { get; set; } = "S";
        public string MoveLeft { get; set; } = "A";
        public string MoveRight { get; set; } = "D";
        public string MoveUp { get; set; } = "Q";
        public string MoveDown { get; set; } = "E";
        public string RotateLeft { get; set; } = "Left";
        public string RotateRight { get; set; } = "Right";
        public string Exit { get; set; } = "Escape";
        public bool InvertMouseY { get; set; } = false;
        public float MouseSensitivity { get; set; } = 1.0f;
    }

    public class ObjectsConfig
    {
        public ObjectDefinition Monitor { get; set; } = new()
        {
            Position = new(0f, -0.3f, -0.3f),
            BaseColor = new(0.15f, 0.15f, 0.15f)
        };

        public ObjectDefinition CPU { get; set; } = new()
        {
            Position = new(1.8f, -0.8f, -0.2f),
            BaseColor = new(0.25f, 0.25f, 0.3f)
        };

        public ObjectDefinition Teclado { get; set; } = new()
        {
            Position = new(0f, -1.4f, 0.8f),
            BaseColor = new(0.05f, 0.05f, 0.05f)
        };

        public ObjectDefinition Escritorio { get; set; } = new()
        {
            Position = new(0f, -1.5f, 0f),
            BaseColor = new(0.4f, 0.3f, 0.2f)
        };
    }

    public class ObjectDefinition
    {
        public Vector3 Position { get; set; }
        public Vector3 BaseColor { get; set; }
        public Vector3 Rotation { get; set; } = Vector3.Zero;
        public Vector3 Scale { get; set; } = Vector3.One;
        public bool Visible { get; set; } = true;
    }

    // Converter personalizado para Vector3 en JSON
    public class Vector3Converter : JsonConverter<Vector3>
    {
        public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                float x = 0, y = 0, z = 0;

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                        break;

                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        string propertyName = reader.GetString()!.ToLower();
                        reader.Read();

                        switch (propertyName)
                        {
                            case "x":
                                x = reader.GetSingle();
                                break;
                            case "y":
                                y = reader.GetSingle();
                                break;
                            case "z":
                                z = reader.GetSingle();
                                break;
                        }
                    }
                }

                return new Vector3(x, y, z);
            }

            throw new JsonException("Expected StartObject token");
        }

        public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("x", value.X);
            writer.WriteNumber("y", value.Y);
            writer.WriteNumber("z", value.Z);
            writer.WriteEndObject();
        }
    }

    // Manager para acceso global a la configuración
    public static class ConfigManager
    {
        private static GameConfig? _instance;

        public static GameConfig Instance
        {
            get
            {
                _instance ??= GameConfig.LoadFromFile();
                return _instance;
            }
        }

        public static void Reload()
        {
            _instance = GameConfig.LoadFromFile();
        }

        public static void Save()
        {
            Instance.SaveToFile();
        }
    }
}
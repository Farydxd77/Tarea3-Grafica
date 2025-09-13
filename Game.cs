using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Opentk_2222.Clases;
using Opentk_2222.Configuration;
using Opentk_2222.Builders;

namespace Opentk_2222
{
    internal class Game : GameWindow
    {
        // Configuración cargada desde archivo
        private readonly GameConfig config;

        // Componentes principales
        private Escenario escenario;
        private Dictionary<string, ObjectRenderData> renderObjects;

        // Rendering
        private int shaderProgram;
        private CameraSettings camera;
        private LightingSettings lighting;
        private float rotationTime;

        public Game() : base(
            GameWindowSettings.Default,
            CreateWindowSettings())
        {
            // Cargar configuración al inicializar
            config = ConfigManager.Instance;

            Console.WriteLine("Configuración cargada:");
            Console.WriteLine($"- Resolución: {config.Rendering.WindowWidth}x{config.Rendering.WindowHeight}");
            Console.WriteLine($"- Velocidad de rotación: {config.Rendering.RotationSpeed}");
            Console.WriteLine($"- Velocidad de cámara: {config.Camera.MovementSpeed}");

            CenterWindow();
        }

        private static NativeWindowSettings CreateWindowSettings()
        {
            var config = ConfigManager.Instance;
            return new NativeWindowSettings
            {
                Size = new Vector2i(config.Rendering.WindowWidth, config.Rendering.WindowHeight),
                Title = config.Rendering.WindowTitle,
                WindowState = WindowState.Normal
            };
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            ConfigurarOpenGL();
            InicializarShaders();
            ConfigurarCamara();
            ConfigurarIluminacion();

            // Crear el escenario usando el builder pattern
            CrearEscenarioConBuilder();

            // Preparar para renderizado
            PrepararBuffersRenderizado();

            Console.WriteLine("Escenario creado:");
            Console.WriteLine(escenario.ObtenerEstadisticas());
        }

        private void ConfigurarOpenGL()
        {
            if (config.Rendering.EnableDepthTest)
                GL.Enable(EnableCap.DepthTest);

            if (config.Rendering.EnableCullFace)
            {
                GL.Enable(EnableCap.CullFace);
                GL.CullFace(CullFaceMode.Back);
            }

            var bg = config.Rendering.BackgroundColor;
            GL.ClearColor(bg.X, bg.Y, bg.Z, 1.0f);
        }

        private void ConfigurarCamara()
        {
            var camConfig = config.Camera;
            camera = new CameraSettings
            {
                Position = camConfig.InitialPosition,
                Target = camConfig.InitialTarget,
                Up = camConfig.UpVector,
                FOV = MathHelper.DegreesToRadians(camConfig.FieldOfView),
                AspectRatio = (float)Size.X / Size.Y,
                NearPlane = camConfig.NearPlane,
                FarPlane = camConfig.FarPlane
            };
        }

        private void ConfigurarIluminacion()
        {
            var lightConfig = config.Lighting;
            lighting = new LightingSettings
            {
                Position = lightConfig.Position,
                Color = lightConfig.Color,
                Intensity = lightConfig.Intensity
            };
        }

        private void CrearEscenarioConBuilder()
        {
            escenario = new Escenario("Setup de Computadora");
            renderObjects = new Dictionary<string, ObjectRenderData>();

            // Usar el ObjectBuilder con configuraciones del archivo
            var objectsConfig = config.Objects;

            // Crear objetos usando el builder pattern
            var escritorio = ObjectBuilder.Presets
                .Escritorio(objectsConfig.Escritorio.Position)
                .Construir();

            var monitor = ObjectBuilder.Presets
                .Monitor(objectsConfig.Monitor.Position)
                .Construir();

            var cpu = ObjectBuilder.Presets
                .CPU(objectsConfig.CPU.Position)
                .Construir();

            var teclado = ObjectBuilder.Presets
                .Teclado(objectsConfig.Teclado.Position)
                .Construir();

            // Ejemplo de cómo crear objetos personalizados con el builder
            var objetoPersonalizado = ObjectBuilder
                .Crear("ObjetoCustom", new Vector3(2f, 0f, 2f), new Vector3(0.5f, 0.2f, 0.8f))
                .AgregarParte("Cuerpo", Vector3.Zero, new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.8f, 0.2f, 0.2f))
                .AgregarParte("Antena", new Vector3(0f, 0.3f, 0f), new Vector3(0.05f, 0.3f, 0.05f), new Vector3(0.9f, 0.9f, 0.1f))
                .Construir();

            // Agregar al escenario
            escenario.AgregarObjetos(escritorio, monitor, cpu, teclado);

            // Solo agregar el objeto personalizado si está habilitado en configuración
            if (config.Objects.Monitor.Visible) // Usando como ejemplo, podrías agregar más flags
            {
                escenario.AgregarObjetos(objetoPersonalizado);
            }
        }

        // Método alternativo para crear objetos usando datos del config
        private Objeto CrearObjetoDesdeConfig(string tipo)
        {
            return tipo.ToLower() switch
            {
                "monitor" => ObjectBuilder.Presets.Monitor(config.Objects.Monitor.Position).Construir(),
                "cpu" => ObjectBuilder.Presets.CPU(config.Objects.CPU.Position).Construir(),
                "teclado" => ObjectBuilder.Presets.Teclado(config.Objects.Teclado.Position).Construir(),
                "escritorio" => ObjectBuilder.Presets.Escritorio(config.Objects.Escritorio.Position).Construir(),
                _ => throw new ArgumentException($"Tipo de objeto desconocido: {tipo}")
            };
        }

        private void InicializarShaders()
        {
            // Los shaders ahora usan valores de configuración para iluminación
            string vertexShader = @"
                #version 330 core
                layout (location = 0) in vec3 aPosition;
                layout (location = 1) in vec3 aNormal;
                
                uniform mat4 model;
                uniform mat4 view;
                uniform mat4 projection;
                uniform mat3 normalMatrix;
                
                out vec3 FragPos;
                out vec3 Normal;
                
                void main()
                {
                    FragPos = vec3(model * vec4(aPosition, 1.0));
                    Normal = normalMatrix * aNormal;
                    gl_Position = projection * view * vec4(FragPos, 1.0);
                }";

            string fragmentShader = @"
                #version 330 core
                out vec4 FragColor;
                
                in vec3 FragPos;
                in vec3 Normal;
                
                uniform vec3 objectColor;
                uniform vec3 lightPos;
                uniform vec3 lightColor;
                uniform vec3 viewPos;
                uniform float ambientStrength;
                uniform float specularStrength;
                uniform float shininess;
                
                void main()
                {
                    vec3 ambient = ambientStrength * lightColor;
                    
                    vec3 norm = normalize(Normal);
                    vec3 lightDir = normalize(lightPos - FragPos);
                    float diff = max(dot(norm, lightDir), 0.0);
                    vec3 diffuse = diff * lightColor;
                    
                    vec3 viewDir = normalize(viewPos - FragPos);
                    vec3 reflectDir = reflect(-lightDir, norm);
                    float spec = pow(max(dot(viewDir, reflectDir), 0.0), shininess);
                    vec3 specular = specularStrength * spec * lightColor;
                    
                    vec3 result = (ambient + diffuse + specular) * objectColor;
                    FragColor = vec4(result, 1.0);
                }";

            shaderProgram = CrearProgramaShader(vertexShader, fragmentShader);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            rotationTime += (float)args.Time * config.Rendering.RotationSpeed;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.UseProgram(shaderProgram);

            // Configurar matrices de cámara
            Matrix4 view = Matrix4.LookAt(camera.Position, camera.Target, camera.Up);
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(
                camera.FOV, camera.AspectRatio, camera.NearPlane, camera.FarPlane);

            SetUniform("view", view);
            SetUniform("projection", projection);
            SetUniform("lightPos", lighting.Position);
            SetUniform("lightColor", lighting.Color);
            SetUniform("viewPos", camera.Position);

            // Usar valores de configuración para iluminación
            SetUniform("ambientStrength", config.Lighting.AmbientStrength);
            SetUniform("specularStrength", config.Lighting.SpecularStrength);
            SetUniform("shininess", config.Lighting.Shininess);

            // Renderizar cada objeto
            foreach (var kvp in renderObjects)
            {
                var objData = kvp.Value;
                var objeto = escenario.BuscarObjeto(kvp.Key);

                // Matriz de transformación con rotación suave del escenario
                Matrix4 model = Matrix4.CreateRotationY(rotationTime * 0.2f) *
                               Matrix4.CreateTranslation(objData.Position);

                Matrix3 normalMatrix = Matrix3.Transpose(Matrix3.Invert(new Matrix3(model)));

                SetUniform("model", model);
                SetUniform("normalMatrix", normalMatrix);
                SetUniform("objectColor", objData.BaseColor);

                GL.BindVertexArray(objData.VAO);
                GL.DrawElements(PrimitiveType.Triangles, objData.IndexCount, DrawElementsType.UnsignedInt, 0);
            }

            Context.SwapBuffers();
            base.OnRenderFrame(args);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            var input = KeyboardState;
            float deltaTime = (float)args.Time;

            // Controles de cámara usando configuración
            ProcesarInputCamara(input, deltaTime);

            // Salir con la tecla configurada
            if (input.IsKeyPressed(ParseKey(config.Input.Exit)))
                Close();

            // Tecla F1 para recargar configuración
            if (input.IsKeyPressed(Keys.F1))
            {
                ConfigManager.Reload();
                Console.WriteLine("Configuración recargada!");
            }

            // Tecla F2 para guardar configuración actual
            if (input.IsKeyPressed(Keys.F2))
            {
                ConfigManager.Save();
                Console.WriteLine("Configuración guardada!");
            }

            base.OnUpdateFrame(args);
        }

        private void ProcesarInputCamara(KeyboardState input, float deltaTime)
        {
            float speed = config.Camera.MovementSpeed * deltaTime;
            Vector3 forward = Vector3.Normalize(camera.Target - camera.Position);
            Vector3 right = Vector3.Normalize(Vector3.Cross(forward, camera.Up));

            var inputConfig = config.Input;

            if (input.IsKeyDown(ParseKey(inputConfig.MoveForward)))
                camera.Position += forward * speed;
            if (input.IsKeyDown(ParseKey(inputConfig.MoveBackward)))
                camera.Position -= forward * speed;
            if (input.IsKeyDown(ParseKey(inputConfig.MoveLeft)))
                camera.Position -= right * speed;
            if (input.IsKeyDown(ParseKey(inputConfig.MoveRight)))
                camera.Position += right * speed;
            if (input.IsKeyDown(ParseKey(inputConfig.MoveUp)))
                camera.Position += camera.Up * speed;
            if (input.IsKeyDown(ParseKey(inputConfig.MoveDown)))
                camera.Position -= camera.Up * speed;

            // Rotación de cámara con las teclas configuradas
            if (input.IsKeyDown(ParseKey(inputConfig.RotateLeft)))
            {
                float rotSpeed = config.Camera.RotationSpeed * deltaTime;
                Matrix4 rotation = Matrix4.CreateRotationY(rotSpeed);
                camera.Position = Vector3.TransformPosition(camera.Position - camera.Target, rotation) + camera.Target;
            }
            if (input.IsKeyDown(ParseKey(inputConfig.RotateRight)))
            {
                float rotSpeed = config.Camera.RotationSpeed * deltaTime;
                Matrix4 rotation = Matrix4.CreateRotationY(-rotSpeed);
                camera.Position = Vector3.TransformPosition(camera.Position - camera.Target, rotation) + camera.Target;
            }
        }

        // Método helper para parsear teclas desde string
        private Keys ParseKey(string keyString)
        {
            return Enum.TryParse<Keys>(keyString, out Keys key) ? key : Keys.Escape;
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, e.Width, e.Height);
            camera.AspectRatio = (float)e.Width / e.Height;
        }

        protected override void OnUnload()
        {
            // Limpiar recursos OpenGL
            foreach (var objData in renderObjects.Values)
            {
                GL.DeleteVertexArray(objData.VAO);
                GL.DeleteBuffer(objData.VBO);
                GL.DeleteBuffer(objData.EBO);
            }

            GL.DeleteProgram(shaderProgram);
            base.OnUnload();
        }

        // Método para preparar buffers de renderizado
        private void PrepararBuffersRenderizado()
        {
            foreach (var objeto in escenario.Objetos)
            {
                var vertices = objeto.ObtenerVerticesParaRender();
                var indices = objeto.ObtenerIndicesParaRender();

                // Crear buffers OpenGL
                int vao = GL.GenVertexArray();
                int vbo = GL.GenBuffer();
                int ebo = GL.GenBuffer();

                GL.BindVertexArray(vao);

                // Buffer de vértices
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count * sizeof(float),
                             vertices.ToArray(), BufferUsageHint.StaticDraw);

                // Buffer de índices
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
                GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Count * sizeof(uint),
                             indices.ToArray(), BufferUsageHint.StaticDraw);

                // Configurar atributos de vértices
                // Posición (location = 0)
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
                GL.EnableVertexArrayAttrib(vao, 0);

                // Normal (location = 1)
                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
                GL.EnableVertexArrayAttrib(vao, 1);

                GL.BindVertexArray(0);

                // Guardar datos de renderizado
                renderObjects[objeto.Nombre] = new ObjectRenderData
                {
                    VAO = vao,
                    VBO = vbo,
                    EBO = ebo,
                    IndexCount = indices.Count,
                    BaseColor = objeto.ColorBase,
                    Position = objeto.Posicion,
                    Rotation = objeto.Rotacion,
                    Scale = objeto.Escala
                };
            }
        }

        // Métodos auxiliares para shaders
        private int CrearProgramaShader(string vertexSource, string fragmentSource)
        {
            int vertexShader = CompileShader(vertexSource, ShaderType.VertexShader);
            int fragmentShader = CompileShader(fragmentSource, ShaderType.FragmentShader);

            int program = GL.CreateProgram();
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);
            GL.LinkProgram(program);

            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(program);
                throw new Exception($"Error linking shader program: {infoLog}");
            }

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            return program;
        }

        private int CompileShader(string source, ShaderType type)
        {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error compiling {type} shader: {infoLog}");
            }

            return shader;
        }

        private void SetUniform(string name, Matrix4 matrix)
        {
            int location = GL.GetUniformLocation(shaderProgram, name);
            GL.UniformMatrix4(location, false, ref matrix);
        }

        private void SetUniform(string name, Matrix3 matrix)
        {
            int location = GL.GetUniformLocation(shaderProgram, name);
            GL.UniformMatrix3(location, false, ref matrix);
        }

        private void SetUniform(string name, Vector3 vector)
        {
            int location = GL.GetUniformLocation(shaderProgram, name);
            GL.Uniform3(location, vector);
        }

        private void SetUniform(string name, float value)
        {
            int location = GL.GetUniformLocation(shaderProgram, name);
            GL.Uniform1(location, value);
        }

        // Estructuras auxiliares para organización
        public struct ObjectRenderData
        {
            public int VAO;
            public int VBO;
            public int EBO;
            public int IndexCount;
            public Vector3 BaseColor;
            public Vector3 Position;
            public Vector3 Rotation;
            public Vector3 Scale;
        }

        public struct CameraSettings
        {
            public Vector3 Position;
            public Vector3 Target;
            public Vector3 Up;
            public float FOV;
            public float AspectRatio;
            public float NearPlane;
            public float FarPlane;
        }

        public struct LightingSettings
        {
            public Vector3 Position;
            public Vector3 Color;
            public float Intensity;
        }
    }
}
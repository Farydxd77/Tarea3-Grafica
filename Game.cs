// ===================== Game.cs CON MÉTODO UNIVERSAL =====================
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Opentk_2222.Clases;

namespace Opentk_2222
{
    internal class Game : GameWindow
    {
        // Variables simples - sin config externa
        private Escenario escenario;
        private Dictionary<string, ObjectRenderData> renderObjects;
        private int shaderProgram;
        private Vector3 cameraPos = new(3f, 2f, 4f);
        private Vector3 cameraTarget = new(0f, -0.5f, 0f);
        private Vector3 lightPos = new(2f, 4f, 2f);
        private float rotationTime;

        // Struct simple para definir partes
        public struct ParteInfo
        {
            public string Nombre;
            public Vector3 Posicion;
            public Vector3 Tamaño;
            public Vector3 Color;

            public ParteInfo(string nombre, Vector3 posicion, Vector3 tamaño, Vector3 color)
            {
                Nombre = nombre;
                Posicion = posicion;
                Tamaño = tamaño;
                Color = color;
            }
        }

        // Configuración directa en el constructor
        public Game() : base(
            GameWindowSettings.Default,
            new NativeWindowSettings
            {
                ClientSize = new Vector2i(1024, 768),  // ← CAMBIAR Size por ClientSize
                Title = "Setup de Computadora 3D",
                WindowState = WindowState.Normal
            })
        {
            //CenterWindow();
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            // Setup OpenGL
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.ClearColor(0.95f, 0.95f, 0.95f, 1.0f);

            // Crear shaders
            InicializarShaders();

            // Crear escena directamente
            CrearEscenaSimple();

            // Preparar rendering
            PrepararBuffersRenderizado();
        }

        private void CrearEscenaSimple()
        {
            escenario = new Escenario("Setup de Computadora");
            renderObjects = new Dictionary<string, ObjectRenderData>();

            // Crear objetos con UN SOLO método - más flexible
            var escritorio = CrearObjeto("Escritorio",
                new Vector3(0f, -1.5f, 0f),
                new Vector3(0.4f, 0.3f, 0.2f),
                new ParteInfo("Superficie", Vector3.Zero, new Vector3(5.0f, 0.1f, 3.0f), new Vector3(0.4f, 0.3f, 0.2f))
            );

            var monitor = CrearObjeto("Monitor",
                new Vector3(0f, -0.3f, -0.3f),
                new Vector3(0.15f, 0.15f, 0.15f),
                new ParteInfo("Pantalla", Vector3.Zero, new Vector3(2.4f, 1.5f, 0.06f), new Vector3(0.02f, 0.02f, 0.05f)),
                new ParteInfo("Base", new Vector3(0f, -0.8f, 0f), new Vector3(0.6f, 0.2f, 0.4f), new Vector3(0.2f, 0.2f, 0.2f))
            );

            var cpu = CrearObjeto("CPU",
                new Vector3(1.8f, -0.8f, -0.2f),
                new Vector3(0.25f, 0.25f, 0.3f),
                new ParteInfo("Carcasa", Vector3.Zero, new Vector3(0.5f, 1.6f, 0.8f), new Vector3(0.2f, 0.2f, 0.25f)),
                new ParteInfo("BotonEncendido", new Vector3(-0.15f, -0.5f, 0.43f), new Vector3(0.05f, 0.05f, 0.01f), new Vector3(0.8f, 0.2f, 0.2f))
            );

            var teclado = CrearObjeto("Teclado",
                new Vector3(0f, -1.4f, 0.8f),
                new Vector3(0.05f, 0.05f, 0.05f),
                new ParteInfo("BaseTeclado", Vector3.Zero, new Vector3(2.8f, 0.08f, 0.8f), new Vector3(0.05f, 0.05f, 0.05f)),
                new ParteInfo("BarraEspaciadora", new Vector3(0f, 0.06f, 0.2f), new Vector3(1.2f, 0.025f, 0.1f), new Vector3(0.15f, 0.15f, 0.15f))
            );

           

            escenario.AgregarObjetos(escritorio, monitor, cpu, teclado);
        }

        // UN SOLO método que crea cualquier objeto
        private Objeto CrearObjeto(string nombre, Vector3 posicion, Vector3 colorBase, params ParteInfo[] partes)
        {
            var objeto = new Objeto(nombre);
            objeto.Posicion = posicion;
            objeto.ColorBase = colorBase;

            foreach (var info in partes)
            {
                var parte = Parte.CrearCubo(info.Nombre, info.Posicion, info.Tamaño, info.Color);
                objeto.AgregarParte(parte);
            }

            return objeto;
        }

        private void InicializarShaders()
        {
            string vertexShader = @"#version 330 core
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

            string fragmentShader = @"#version 330 core
out vec4 FragColor;

in vec3 FragPos;
in vec3 Normal;

uniform vec3 objectColor;
uniform vec3 lightPos;
uniform vec3 lightColor;
uniform vec3 viewPos;

void main()
{
    vec3 ambient = 0.4 * vec3(1.0, 1.0, 1.0);
    
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(lightPos - FragPos);
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = diff * vec3(1.0, 1.0, 1.0);
    
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 64.0);
    vec3 specular = 0.6 * spec * vec3(1.0, 1.0, 1.0);
    
    vec3 result = (ambient + diffuse + specular) * objectColor;
    FragColor = vec4(result, 1.0);
}";

            shaderProgram = CrearProgramaShader(vertexShader, fragmentShader);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            rotationTime += (float)e.Time * 0.5f;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.UseProgram(shaderProgram);

            Matrix4 view = Matrix4.LookAt(cameraPos, cameraTarget, Vector3.UnitY);
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(45f), (float)ClientSize.X / ClientSize.Y, 0.1f, 100f);

            SetUniform("view", view);
            SetUniform("projection", projection);
            SetUniform("lightPos", lightPos);
            SetUniform("lightColor", Vector3.One);
            SetUniform("viewPos", cameraPos);

            foreach (var kvp in renderObjects)
            {
                var objData = kvp.Value;

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
            base.OnRenderFrame(e);
        }

       
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, e.Width, e.Height);
        }

        protected override void OnUnload()
        {
            foreach (var objData in renderObjects.Values)
            {
                GL.DeleteVertexArray(objData.VAO);
                GL.DeleteBuffer(objData.VBO);
                GL.DeleteBuffer(objData.EBO);
            }

            GL.DeleteProgram(shaderProgram);
            base.OnUnload();
        }

        private void PrepararBuffersRenderizado()
        {
            foreach (var objeto in escenario.Objetos)
            {
                var vertices = objeto.ObtenerVerticesParaRender();
                var indices = objeto.ObtenerIndicesParaRender();

                int vao = GL.GenVertexArray();
                int vbo = GL.GenBuffer();
                int ebo = GL.GenBuffer();

                GL.BindVertexArray(vao);

                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count * sizeof(float),
                             vertices.ToArray(), BufferUsageHint.StaticDraw);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
                GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Count * sizeof(uint),
                             indices.ToArray(), BufferUsageHint.StaticDraw);

                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
                GL.EnableVertexArrayAttrib(vao, 0);

                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
                GL.EnableVertexArrayAttrib(vao, 1);

                GL.BindVertexArray(0);

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
        private void GuardarEscena(string nombreArchivo = "mi_escena.json")
        {
            var escenaData = new EscenaData(escenario, cameraPos, cameraTarget, lightPos);
            Serializar.GuardarComoJson(escenaData, nombreArchivo);
        }

        private void CargarEscena(string nombreArchivo = "mi_escena.json")
        {
            var escenaData = Serializar.CargarDesdeJson<EscenaData>(nombreArchivo);

            if (escenaData != null)
            {
                // Limpiar escena actual
                escenario.LimpiarEscenario();
                renderObjects.Clear();

                // Restaurar cámara y luz
                cameraPos = escenaData.PosicionCamara;
                cameraTarget = escenaData.ObjetivoCamara;
                lightPos = escenaData.PosicionLuz;

                // Recrear objetos
                foreach (var objetoData in escenaData.Objetos)
                {
                    var partes = objetoData.Partes.Select(p =>
                        new ParteInfo(p.Nombre, p.Posicion, p.Tamaño, p.Color)
                    ).ToArray();

                    var objeto = CrearObjeto(objetoData.Nombre, objetoData.Posicion, objetoData.ColorBase, partes);
                    escenario.AgregarObjeto(objeto);
                }

                // Preparar buffers para los nuevos objetos
                PrepararBuffersRenderizado();

                Console.WriteLine($"✅ Escena '{escenaData.Nombre}' cargada correctamente!");
            }
        }

        private void ListarEscenas()
        {
            var escenas = Serializar.ListarEscenasGuardadas();
            Console.WriteLine("📁 Escenas guardadas:");
            foreach (var escena in escenas)
            {
                Console.WriteLine($"  • {escena}");
            }
        }

        // ==================== CONTROLES ACTUALIZADOS ====================

        // Actualizar el método OnUpdateFrame para incluir nuevos controles:
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            var input = KeyboardState;
            float deltaTime = (float)e.Time;
            float speed = 3.0f * deltaTime;

            // Controles de cámara existentes...
            Vector3 forward = Vector3.Normalize(cameraTarget - cameraPos);
            Vector3 right = Vector3.Normalize(Vector3.Cross(forward, Vector3.UnitY));

            if (input.IsKeyDown(Keys.W)) cameraPos += forward * speed;
            if (input.IsKeyDown(Keys.S)) cameraPos -= forward * speed;
            if (input.IsKeyDown(Keys.A)) cameraPos -= right * speed;
            if (input.IsKeyDown(Keys.D)) cameraPos += right * speed;
            if (input.IsKeyDown(Keys.Q)) cameraPos += Vector3.UnitY * speed;
            if (input.IsKeyDown(Keys.E)) cameraPos -= Vector3.UnitY * speed;

            if (input.IsKeyDown(Keys.Left))
            {
                Matrix4 rotation = Matrix4.CreateRotationY(1.5f * deltaTime);
                cameraPos = Vector3.TransformPosition(cameraPos - cameraTarget, rotation) + cameraTarget;
            }
            if (input.IsKeyDown(Keys.Right))
            {
                Matrix4 rotation = Matrix4.CreateRotationY(-1.5f * deltaTime);
                cameraPos = Vector3.TransformPosition(cameraPos - cameraTarget, rotation) + cameraTarget;
            }

            // NUEVOS CONTROLES PARA SERIALIZACIÓN
            if (input.IsKeyPressed(Keys.F5)) // Guardar escena
            {
                GuardarEscena("setup_computadora.json");
            }

            if (input.IsKeyPressed(Keys.F6)) // Cargar escena
            {
                CargarEscena("setup_computadora.json");
            }

            if (input.IsKeyPressed(Keys.F7)) // Listar escenas
            {
                ListarEscenas();
            }

            if (input.IsKeyPressed(Keys.Escape)) Close();

            base.OnUpdateFrame(e);
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
    }
}
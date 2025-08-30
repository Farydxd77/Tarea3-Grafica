using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Opentk_2222.Clases;
using System;
using System.Collections.Generic;
using System.IO;

namespace Opentk_2222
{
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

    public class ShaderManager : IDisposable
    {
        public int ProgramId { get; private set; }

        public ShaderManager()
        {
            CreateShaderProgram();
        }

        private void CreateShaderProgram()
        {
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
                
                void main()
                {
                    // Ambient
                    float ambientStrength = 0.3;
                    vec3 ambient = ambientStrength * lightColor;
                    
                    // Diffuse
                    vec3 norm = normalize(Normal);
                    vec3 lightDir = normalize(lightPos - FragPos);
                    float diff = max(dot(norm, lightDir), 0.0);
                    vec3 diffuse = diff * lightColor;
                    
                    // Specular
                    float specularStrength = 0.5;
                    vec3 viewDir = normalize(viewPos - FragPos);
                    vec3 reflectDir = reflect(-lightDir, norm);
                    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
                    vec3 specular = specularStrength * spec * lightColor;
                    
                    vec3 result = (ambient + diffuse + specular) * objectColor;
                    FragColor = vec4(result, 1.0);
                }";

            int vertexShaderId = CompileShader(vertexShader, ShaderType.VertexShader);
            int fragmentShaderId = CompileShader(fragmentShader, ShaderType.FragmentShader);

            ProgramId = GL.CreateProgram();
            GL.AttachShader(ProgramId, vertexShaderId);
            GL.AttachShader(ProgramId, fragmentShaderId);
            GL.LinkProgram(ProgramId);

            ValidateProgram(ProgramId);

            GL.DeleteShader(vertexShaderId);
            GL.DeleteShader(fragmentShaderId);
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

        private void ValidateProgram(int program)
        {
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(program);
                throw new Exception($"Error linking shader program: {infoLog}");
            }
        }

        public void Use() => GL.UseProgram(ProgramId);

        public void SetMatrix4(string name, Matrix4 matrix)
        {
            int location = GL.GetUniformLocation(ProgramId, name);
            GL.UniformMatrix4(location, false, ref matrix);
        }

        public void SetVector3(string name, Vector3 vector)
        {
            int location = GL.GetUniformLocation(ProgramId, name);
            GL.Uniform3(location, vector);
        }

        public void SetMatrix3(string name, Matrix3 matrix)
        {
            int location = GL.GetUniformLocation(ProgramId, name);
            GL.UniformMatrix3(location, false, ref matrix);
        }

        public void Dispose()
        {
            GL.DeleteProgram(ProgramId);
        }
    }

    public static class MeshGenerator
    {
        public static (float[] vertices, uint[] indices) CreateCube(Vector3 size)
        {
            float w = size.X / 2f;
            float h = size.Y / 2f;
            float d = size.Z / 2f;

            float[] vertices = {
                // Positions          // Normals
                // Front face
                -w, -h,  d,   0, 0, 1,
                 w, -h,  d,   0, 0, 1,
                 w,  h,  d,   0, 0, 1,
                -w,  h,  d,   0, 0, 1,
                // Back face
                -w, -h, -d,   0, 0, -1,
                 w, -h, -d,   0, 0, -1,
                 w,  h, -d,   0, 0, -1,
                -w,  h, -d,   0, 0, -1,
                // Left face
                -w, -h, -d,  -1, 0, 0,
                -w, -h,  d,  -1, 0, 0,
                -w,  h,  d,  -1, 0, 0,
                -w,  h, -d,  -1, 0, 0,
                // Right face
                 w, -h, -d,   1, 0, 0,
                 w, -h,  d,   1, 0, 0,
                 w,  h,  d,   1, 0, 0,
                 w,  h, -d,   1, 0, 0,
                // Bottom face
                -w, -h, -d,   0, -1, 0,
                 w, -h, -d,   0, -1, 0,
                 w, -h,  d,   0, -1, 0,
                -w, -h,  d,   0, -1, 0,
                // Top face
                -w,  h, -d,   0, 1, 0,
                 w,  h, -d,   0, 1, 0,
                 w,  h,  d,   0, 1, 0,
                -w,  h,  d,   0, 1, 0
            };

            uint[] indices = {
                0,  1,  2,   2,  3,  0,   // Front
                4,  5,  6,   6,  7,  4,   // Back
                8,  9,  10,  10, 11, 8,   // Left
                12, 13, 14,  14, 15, 12,  // Right
                16, 17, 18,  18, 19, 16,  // Bottom
                20, 21, 22,  22, 23, 20   // Top
            };

            return (vertices, indices);
        }

        public static (float[] vertices, uint[] indices) CreateRoundedCube(Vector3 size, int segments = 8)
        {
            // Simplified rounded cube - just a regular cube for now
            return CreateCube(size);
        }
    }

    internal class Game : GameWindow
    {
        // Configuración
        private const float ROTATION_SPEED = 0.3f;
        private const float CAMERA_DISTANCE = 6f;
        private const float LIGHT_HEIGHT = 5f;

        // Render vars
        private Escenario escenario;
        private Dictionary<string, ObjectRenderData> renderObjects;
        private ShaderManager shaderManager;

        // Camera y lighting
        private CameraSettings camera;
        private Vector3 lightPos;
        private Vector3 lightColor;
        private float rotationTime;

        // Input
        private bool[] keys = new bool[512];

        public Game(int width, int height) : base(GameWindowSettings.Default, new NativeWindowSettings
        {
            Size = new Vector2i(width, height),
            Title = "Computadora 3D - OpenTK",
            WindowState = WindowState.Normal
        })
        {
            CenterWindow();
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            escenario = new Escenario();
            renderObjects = new Dictionary<string, ObjectRenderData>();

            // Camera setup
            camera = new CameraSettings
            {
                Position = new Vector3(4f, 2f, CAMERA_DISTANCE),
                Target = new Vector3(0f, 0f, 0f),
                Up = Vector3.UnitY,
                FOV = MathHelper.DegreesToRadians(45f),
                AspectRatio = (float)Size.X / Size.Y,
                NearPlane = 0.1f,
                FarPlane = 100f
            };

            // Lighting
            lightPos = new Vector3(2f, LIGHT_HEIGHT, 3f);
            lightColor = new Vector3(1f, 1f, 1f);
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            // OpenGL setup
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.ClearColor(0.05f, 0.05f, 0.1f, 1.0f);

            // Initialize systems
            shaderManager = new ShaderManager();

            // Create 3D objects
            CreateComputerSetup();
            SetupBuffers();
        }

        private void CreateComputerSetup()
        {
            CreateMonitor();
            CreateCPU();
            CreateKeyboard();
            CreateMouse();
            CreateDesk();
        }

        private void CreateMonitor()
        {
            var monitor = new Objeto("Monitor");

            // Pantalla principal (más realista)
            var pantalla = new Parte("Pantalla");
            var poligonoPantalla = CrearCuboConNormales(0f, 0.3f, 0f, 1.8f, 1.1f, 0.05f);
            pantalla.AgregarPoligono(poligonoPantalla);

            // Marco de la pantalla
            var marco = new Parte("Marco");
            var poligonoMarco = CrearCuboConNormales(0f, 0.3f, 0.03f, 1.9f, 1.2f, 0.02f);
            marco.AgregarPoligono(poligonoMarco);

            // Base del monitor más estable
            var baseMonitor = new Parte("Base");
            var poligonoBase = CrearCuboConNormales(0f, -0.4f, 0.1f, 0.6f, 0.15f, 0.6f);
            baseMonitor.AgregarPoligono(poligonoBase);

            // Soporte del monitor
            var soporte = new Parte("Soporte");
            var poligonoSoporte = CrearCuboConNormales(0f, -0.1f, 0.05f, 0.05f, 0.4f, 0.05f);
            soporte.AgregarPoligono(poligonoSoporte);

            monitor.AgregarParte(pantalla);
            monitor.AgregarParte(marco);
            monitor.AgregarParte(baseMonitor);
            monitor.AgregarParte(soporte);
            monitor.Posicion = new Vector3(-1.2f, 0.5f, -0.5f);

            escenario.AgregarObjeto(monitor);
        }

        private void CreateCPU()
        {
            var cpu = new Objeto("CPU");

            // Carcasa principal (torre)
            var carcasa = new Parte("Carcasa");
            var poligonoCarcasa = CrearCuboConNormales(0f, 0f, 0f, 0.4f, 1.6f, 0.8f);
            carcasa.AgregarPoligono(poligonoCarcasa);

            // Panel frontal con detalles
            var panel = new Parte("Panel");
            var poligonoPanel = CrearCuboConNormales(0f, 0.2f, 0.41f, 0.35f, 0.3f, 0.02f);
            panel.AgregarPoligono(poligonoPanel);

            // Botón de encendido
            var boton = new Parte("Boton");
            var poligonoBoton = CrearCuboConNormales(-0.1f, -0.3f, 0.42f, 0.04f, 0.04f, 0.01f);
            boton.AgregarPoligono(poligonoBoton);

            // Ventilador frontal
            var ventilador = new Parte("Ventilador");
            var poligonoVentilador = CrearCuboConNormales(0.1f, -0.1f, 0.41f, 0.15f, 0.15f, 0.02f);
            ventilador.AgregarPoligono(poligonoVentilador);

            cpu.AgregarParte(carcasa);
            cpu.AgregarParte(panel);
            cpu.AgregarParte(boton);
            cpu.AgregarParte(ventilador);
            cpu.Posicion = new Vector3(1.0f, -0.3f, -0.2f);

            escenario.AgregarObjeto(cpu);
        }

        private void CreateKeyboard()
        {
            var teclado = new Objeto("Teclado");

            // Base del teclado
            var baseTeclado = new Parte("BaseTeclado");
            var poligonoBase = CrearCuboConNormales(0f, 0f, 0f, 1.8f, 0.08f, 0.6f);
            baseTeclado.AgregarPoligono(poligonoBase);

            // Teclas principales
            var teclas = new Parte("Teclas");
            var poligonoTeclas = CrearCuboConNormales(0f, 0.05f, 0f, 1.7f, 0.03f, 0.5f);
            teclas.AgregarPoligono(poligonoTeclas);

            // Barra espaciadora
            var barra = new Parte("BarraEspaciadora");
            var poligonoBarra = CrearCuboConNormales(0f, 0.08f, 0.15f, 0.8f, 0.02f, 0.08f);
            barra.AgregarPoligono(poligonoBarra);

            teclado.AgregarParte(baseTeclado);
            teclado.AgregarParte(teclas);
            teclado.AgregarParte(barra);
            teclado.Posicion = new Vector3(0.2f, -0.75f, 1.2f);

            escenario.AgregarObjeto(teclado);
        }

        private void CreateMouse()
        {
            var mouse = new Objeto("Mouse");

            // Cuerpo del mouse
            var cuerpo = new Parte("CuerpoMouse");
            var poligonoCuerpo = CrearCuboConNormales(0f, 0f, 0f, 0.25f, 0.08f, 0.4f);
            cuerpo.AgregarPoligono(poligonoCuerpo);

            // Botones
            var botones = new Parte("BotonesMouse");
            var poligonoBotones = CrearCuboConNormales(0f, 0.045f, -0.1f, 0.2f, 0.01f, 0.15f);
            botones.AgregarPoligono(poligonoBotones);

            mouse.AgregarParte(cuerpo);
            mouse.AgregarParte(botones);
            mouse.Posicion = new Vector3(1.5f, -0.7f, 0.8f);

            escenario.AgregarObjeto(mouse);
        }

        private void CreateDesk()
        {
            var escritorio = new Objeto("Escritorio");

            // Superficie del escritorio
            var superficie = new Parte("SuperficieEscritorio");
            var poligonoSuperficie = CrearCuboConNormales(0f, 0f, 0f, 4f, 0.1f, 2.5f);
            superficie.AgregarPoligono(poligonoSuperficie);

            // Pata frontal izquierda
            var pata1 = new Parte("Pata1");
            var poligonoPata1 = CrearCuboConNormales(-1.8f, -0.5f, -1f, 0.1f, 0.8f, 0.1f);
            pata1.AgregarPoligono(poligonoPata1);

            // Pata frontal derecha
            var pata2 = new Parte("Pata2");
            var poligonoPata2 = CrearCuboConNormales(1.8f, -0.5f, -1f, 0.1f, 0.8f, 0.1f);
            pata2.AgregarPoligono(poligonoPata2);

            // Pata trasera izquierda
            var pata3 = new Parte("Pata3");
            var poligonoPata3 = CrearCuboConNormales(-1.8f, -0.5f, 1f, 0.1f, 0.8f, 0.1f);
            pata3.AgregarPoligono(poligonoPata3);

            // Pata trasera derecha
            var pata4 = new Parte("Pata4");
            var poligonoPata4 = CrearCuboConNormales(1.8f, -0.5f, 1f, 0.1f, 0.8f, 0.1f);
            pata4.AgregarPoligono(poligonoPata4);

            escritorio.AgregarParte(superficie);
            escritorio.AgregarParte(pata1);
            escritorio.AgregarParte(pata2);
            escritorio.AgregarParte(pata3);
            escritorio.AgregarParte(pata4);
            escritorio.Posicion = new Vector3(0f, -0.85f, 0f);

            escenario.AgregarObjeto(escritorio);
        }

        private Poligono CrearCuboConNormales(float x, float y, float z, float width, float height, float depth)
        {
            var (vertices, indices) = MeshGenerator.CreateCube(new Vector3(width, height, depth));

            var poligono = new Poligono();

            // Convertir vertices planos a Punto con offset
            for (int i = 0; i < vertices.Length; i += 6) // 6 porque tenemos pos + normal
            {
                poligono.AgregarVertice(new Punto(
                    vertices[i] + x,     // X + offset
                    vertices[i + 1] + y, // Y + offset  
                    vertices[i + 2] + z  // Z + offset
                ));
            }

            // Agregar indices
            foreach (var indice in indices)
            {
                poligono.AgregarIndice(indice);
            }

            return poligono;
        }

        private void SetupBuffers()
        {
            foreach (var objeto in escenario.Objetos)
            {
                var todosVertices = new List<float>();
                var todosIndices = new List<uint>();
                uint baseIndex = 0;

                foreach (var parte in objeto.Partes)
                {
                    foreach (var poligono in parte.Poligonos)
                    {
                        var (cubeVertices, _) = MeshGenerator.CreateCube(new Vector3(1f, 1f, 1f));

                        for (int i = 0; i < poligono.Vertices.Count; i++)
                        {
                            var vertex = poligono.Vertices[i];
                            todosVertices.Add(vertex.X);
                            todosVertices.Add(vertex.Y);
                            todosVertices.Add(vertex.Z);

                            // Agregar normales (simplificado)
                            int normalIndex = (i % 24) * 6 + 3; // Cada 4 vertices, offset +3 para normal
                            if (normalIndex + 2 < cubeVertices.Length)
                            {
                                todosVertices.Add(cubeVertices[normalIndex]);
                                todosVertices.Add(cubeVertices[normalIndex + 1]);
                                todosVertices.Add(cubeVertices[normalIndex + 2]);
                            }
                            else
                            {
                                todosVertices.Add(0f);
                                todosVertices.Add(1f);
                                todosVertices.Add(0f);
                            }
                        }

                        foreach (var indice in poligono.Indices)
                        {
                            todosIndices.Add(baseIndex + indice);
                        }

                        baseIndex += (uint)poligono.Vertices.Count;
                    }
                }

                // Crear buffers
                int vao = GL.GenVertexArray();
                int vbo = GL.GenBuffer();
                int ebo = GL.GenBuffer();

                GL.BindVertexArray(vao);

                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                GL.BufferData(BufferTarget.ArrayBuffer, todosVertices.Count * sizeof(float), todosVertices.ToArray(), BufferUsageHint.StaticDraw);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
                GL.BufferData(BufferTarget.ElementArrayBuffer, todosIndices.Count * sizeof(uint), todosIndices.ToArray(), BufferUsageHint.StaticDraw);

                // Position attribute
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
                GL.EnableVertexArrayAttrib(vao, 0);

                // Normal attribute
                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
                GL.EnableVertexArrayAttrib(vao, 1);

                GL.BindVertexArray(0);

                renderObjects[objeto.Nombre] = new ObjectRenderData
                {
                    VAO = vao,
                    VBO = vbo,
                    EBO = ebo,
                    IndexCount = todosIndices.Count,
                    BaseColor = GetObjectColor(objeto.Nombre),
                    Position = objeto.Posicion,
                    Rotation = Vector3.Zero,
                    Scale = Vector3.One
                };
            }
        }

        private Vector3 GetObjectColor(string objectName)
        {
            return objectName switch
            {
                "Monitor" => new Vector3(0.15f, 0.15f, 0.15f),      // Gris muy oscuro
                "CPU" => new Vector3(0.3f, 0.3f, 0.35f),           // Gris metálico
                "Teclado" => new Vector3(0.05f, 0.05f, 0.05f),     // Negro profundo
                "Mouse" => new Vector3(0.2f, 0.2f, 0.25f),         // Gris ratón
                "Escritorio" => new Vector3(0.4f, 0.25f, 0.15f),   // Madera
                _ => new Vector3(0.6f, 0.6f, 0.6f)                 // Gris por defecto
            };
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            rotationTime += (float)args.Time * ROTATION_SPEED;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            shaderManager.Use();

            // Camera matrices
            Matrix4 view = Matrix4.LookAt(camera.Position, camera.Target, camera.Up);
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(
                camera.FOV, camera.AspectRatio, camera.NearPlane, camera.FarPlane);

            shaderManager.SetMatrix4("view", view);
            shaderManager.SetMatrix4("projection", projection);
            shaderManager.SetVector3("lightPos", lightPos);
            shaderManager.SetVector3("lightColor", lightColor);
            shaderManager.SetVector3("viewPos", camera.Position);

            // Render objects
            foreach (var kvp in renderObjects)
            {
                var objData = kvp.Value;

                // Model matrix with gentle rotation for the whole scene
                Matrix4 model = Matrix4.CreateRotationY(rotationTime) *
                               Matrix4.CreateTranslation(objData.Position);

                Matrix3 normalMatrix = Matrix3.Transpose(Matrix3.Invert(new Matrix3(model)));

                shaderManager.SetMatrix4("model", model);
                shaderManager.SetMatrix3("normalMatrix", normalMatrix);
                shaderManager.SetVector3("objectColor", objData.BaseColor);

                GL.BindVertexArray(objData.VAO);
                GL.DrawElements(PrimitiveType.Triangles, objData.IndexCount, DrawElementsType.UnsignedInt, 0);
            }

            Context.SwapBuffers();
            base.OnRenderFrame(args);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            var input = KeyboardState;

            // Camera controls
            float speed = 2.0f * (float)args.Time;

            if (input.IsKeyDown(Keys.W))
                camera.Position += Vector3.Normalize(camera.Target - camera.Position) * speed;
            if (input.IsKeyDown(Keys.S))
                camera.Position -= Vector3.Normalize(camera.Target - camera.Position) * speed;
            if (input.IsKeyDown(Keys.A))
            {
                Vector3 right = Vector3.Normalize(Vector3.Cross(camera.Target - camera.Position, camera.Up));
                camera.Position -= right * speed;
            }
            if (input.IsKeyDown(Keys.D))
            {
                Vector3 right = Vector3.Normalize(Vector3.Cross(camera.Target - camera.Position, camera.Up));
                camera.Position += right * speed;
            }

            if (input.IsKeyDown(Keys.Escape))
                Close();

            base.OnUpdateFrame(args);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, e.Width, e.Height);
            camera.AspectRatio = (float)e.Width / e.Height;
        }

        protected override void OnUnload()
        {
            foreach (var objData in renderObjects.Values)
            {
                GL.DeleteVertexArray(objData.VAO);
                GL.DeleteBuffer(objData.VBO);
                GL.DeleteBuffer(objData.EBO);
            }

            shaderManager?.Dispose();
            base.OnUnload();
        }
    }
}
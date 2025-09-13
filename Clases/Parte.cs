using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace Opentk_2222.Clases
{
    public class Parte
    {
        public string Nombre { get; set; }
        public List<Poligono> Caras { get; set; } // Renombrado de Poligonos a Caras para mayor claridad
        public Punto CentroMasa { get; private set; }
        public Vector3 Posicion { get; set; }
        public Vector3 Rotacion { get; set; }
        public Vector3 Escala { get; set; }
        public Vector3 Color { get; set; } // Color específico de la parte

        public Parte(string nombre)
        {
            Nombre = nombre;
            Caras = new List<Poligono>();
            CentroMasa = new Punto(0, 0, 0);
            Posicion = Vector3.Zero;
            Rotacion = Vector3.Zero;
            Escala = Vector3.One;
            Color = Vector3.One; // Blanco por defecto
        }

        public void AgregarCara(Poligono cara)
        {
            Caras.Add(cara);
            CalcularCentroMasa();
        }

        public void AgregarCaras(params Poligono[] caras)
        {
            foreach (var cara in caras)
            {
                Caras.Add(cara);
            }
            CalcularCentroMasa();
        }

        private void CalcularCentroMasa()
        {
            if (Caras.Count == 0)
            {
                CentroMasa = new Punto(0, 0, 0);
                return;
            }

            var todosPuntos = new List<Punto>();
            foreach (var cara in Caras)
            {
                todosPuntos.AddRange(cara.Vertices);
            }

            CentroMasa = Punto.CalcularCentroMasa(todosPuntos);
        }

        public Matrix4 GetMatrizTransformacion()
        {
            Matrix4 translation = Matrix4.CreateTranslation(Posicion);
            Matrix4 rotationX = Matrix4.CreateRotationX(Rotacion.X);
            Matrix4 rotationY = Matrix4.CreateRotationY(Rotacion.Y);
            Matrix4 rotationZ = Matrix4.CreateRotationZ(Rotacion.Z);
            Matrix4 scale = Matrix4.CreateScale(Escala);

            return translation * rotationZ * rotationY * rotationX * scale;
        }

        // Método estático para crear un cubo como parte
        public static Parte CrearCubo(string nombre, Vector3 posicion, Vector3 tamaño, Vector3 color)
        {
            var parte = new Parte(nombre);
            parte.Posicion = posicion;
            parte.Color = color;

            float x = posicion.X, y = posicion.Y, z = posicion.Z;
            float w = tamaño.X / 2f, h = tamaño.Y / 2f, d = tamaño.Z / 2f;

            // Definir los 8 vértices del cubo
            var vertices = new Punto[]
            {
                // Cara frontal
                new Punto(x - w, y - h, z + d), // 0
                new Punto(x + w, y - h, z + d), // 1
                new Punto(x + w, y + h, z + d), // 2
                new Punto(x - w, y + h, z + d), // 3
                
                // Cara trasera
                new Punto(x - w, y - h, z - d), // 4
                new Punto(x + w, y - h, z - d), // 5
                new Punto(x + w, y + h, z - d), // 6
                new Punto(x - w, y + h, z - d), // 7
            };
            // Algunas caras tienen vértices en orden incorrecto (normales invertidas)
            var caraTrasera = Poligono.CrearCaraCuadrada(vertices[4], vertices[5], vertices[6], vertices[7]);
            var caraIzquierda = Poligono.CrearCaraCuadrada(vertices[0], vertices[4], vertices[7], vertices[3]);
            var caraDerecha = Poligono.CrearCaraCuadrada(vertices[5], vertices[1], vertices[2], vertices[6]);
            var caraInferior = Poligono.CrearCaraCuadrada(vertices[0], vertices[1], vertices[5], vertices[4]);

            // Crear las 6 caras del cubo
            var caraFrontal = Poligono.CrearCaraCuadrada(vertices[0], vertices[1], vertices[2], vertices[3]);
            //var caraTrasera = Poligono.CrearCaraCuadrada(vertices[5], vertices[4], vertices[7], vertices[6]);
            //var caraIzquierda = Poligono.CrearCaraCuadrada(vertices[4], vertices[0], vertices[3], vertices[7]);
            //var caraDerecha = Poligono.CrearCaraCuadrada(vertices[1], vertices[5], vertices[6], vertices[2]);
            //var caraInferior = Poligono.CrearCaraCuadrada(vertices[4], vertices[5], vertices[1], vertices[0]);
            var caraSuperior = Poligono.CrearCaraCuadrada(vertices[3], vertices[2], vertices[6], vertices[7]);

            parte.AgregarCaras(caraFrontal, caraTrasera, caraIzquierda, caraDerecha, caraInferior, caraSuperior);

            return parte;
        }

        public override string ToString()
        {
            return $"{Nombre} - {Caras.Count} caras - Centro: {CentroMasa}";
        }
    }
}
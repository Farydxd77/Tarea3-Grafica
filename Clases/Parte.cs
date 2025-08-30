using System;
using System.Collections.Generic;
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
        public List<Poligono> Poligonos { get; set; }
        public Vector3 CentroMasa { get; private set; }
        public Vector3 Posicion { get; set; }
        public Vector3 Rotacion { get; set; }
        public Vector3 Escala { get; set; }

        public Parte(string nombre)
        {
            Nombre = nombre;
            Poligonos = new List<Poligono>();
            Posicion = Vector3.Zero;
            Rotacion = Vector3.Zero;
            Escala = Vector3.One;
        }

        public void AgregarPoligono(Poligono poligono)
        {
            Poligonos.Add(poligono);
            CalcularCentroMasa();
        }

        private void CalcularCentroMasa()
        {
            if (Poligonos.Count == 0) return;

            float totalX = 0, totalY = 0, totalZ = 0;
            int totalVertices = 0;

            foreach (var poligono in Poligonos)
            {
                foreach (var vertice in poligono.Vertices)
                {
                    totalX += vertice.X;
                    totalY += vertice.Y;
                    totalZ += vertice.Z;
                    totalVertices++;
                }
            }

            if (totalVertices > 0)
            {
                CentroMasa = new Vector3(
                    totalX / totalVertices,
                    totalY / totalVertices,
                    totalZ / totalVertices
                );
            }
        }

        public Matrix4 GetMatrixTransformacion()
        {
            Matrix4 translation = Matrix4.CreateTranslation(Posicion);
            Matrix4 rotationX = Matrix4.CreateRotationX(Rotacion.X);
            Matrix4 rotationY = Matrix4.CreateRotationY(Rotacion.Y);
            Matrix4 rotationZ = Matrix4.CreateRotationZ(Rotacion.Z);
            Matrix4 scale = Matrix4.CreateScale(Escala);

            return scale * rotationX * rotationY * rotationZ * translation;
        }
    }
}

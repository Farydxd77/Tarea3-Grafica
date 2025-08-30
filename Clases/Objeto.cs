using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// ============= CLASE OBJETO =============
using System.Collections.Generic;
using OpenTK.Mathematics;


namespace Opentk_2222.Clases
{
    public class Objeto
    {
        public string Nombre { get; set; }
        public List<Parte> Partes { get; set; }
        public Vector3 CentroMasa { get; private set; }
        public Vector3 Posicion { get; set; }
        public Vector3 Rotacion { get; set; }
        public Vector3 Escala { get; set; }

        public Objeto(string nombre)
        {
            Nombre = nombre;
            Partes = new List<Parte>();
            Posicion = Vector3.Zero;
            Rotacion = Vector3.Zero;
            Escala = Vector3.One;
        }

        public void AgregarParte(Parte parte)
        {
            Partes.Add(parte);
            CalcularCentroMasa();
        }

        private void CalcularCentroMasa()
        {
            if (Partes.Count == 0) return;

            float totalX = 0, totalY = 0, totalZ = 0;

            foreach (var parte in Partes)
            {
                totalX += parte.CentroMasa.X;
                totalY += parte.CentroMasa.Y;
                totalZ += parte.CentroMasa.Z;
            }

            CentroMasa = new Vector3(
                totalX / Partes.Count,
                totalY / Partes.Count,
                totalZ / Partes.Count
            );
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
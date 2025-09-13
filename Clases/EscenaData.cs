using OpenTK.Mathematics;

namespace Opentk_2222.Clases
{
    public class EscenaData
    {
        public string Nombre { get; set; }
        public List<ObjetoData> Objetos { get; set; } = new();
        public Vector3 PosicionCamara { get; set; }
        public Vector3 ObjetivoCamara { get; set; }
        public Vector3 PosicionLuz { get; set; }

        public EscenaData() { }

        public EscenaData(Escenario escenario, Vector3 camPos, Vector3 camTarget, Vector3 lightPos)
        {
            Nombre = escenario.Nombre;
            PosicionCamara = camPos;
            ObjetivoCamara = camTarget;
            PosicionLuz = lightPos;

            foreach (var objeto in escenario.Objetos)
            {
                Objetos.Add(new ObjetoData(objeto));
            }
        }
    }

    public class ObjetoData
    {
        public string Nombre { get; set; }
        public Vector3 Posicion { get; set; }
        public Vector3 ColorBase { get; set; }
        public List<ParteData> Partes { get; set; } = new();

        public ObjetoData() { }

        public ObjetoData(Objeto objeto)
        {
            Nombre = objeto.Nombre;
            Posicion = objeto.Posicion;
            ColorBase = objeto.ColorBase;

            foreach (var parte in objeto.Partes)
            {
                Partes.Add(new ParteData(parte));
            }
        }
    }

    public class ParteData
    {
        public string Nombre { get; set; }
        public Vector3 Posicion { get; set; }
        public Vector3 Tamaño { get; set; }
        public Vector3 Color { get; set; }

        public ParteData() { }

        public ParteData(Parte parte)
        {
            Nombre = parte.Nombre;
            Posicion = parte.Posicion;
            Color = parte.Color;
            Tamaño = new Vector3(1, 1, 1); // Placeholder
        }
    }
}
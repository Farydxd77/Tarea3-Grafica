using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace Opentk_2222.Clases
{
    public class Poligono
    {
        public List<Punto> Vertices { get; set; }
        public List<uint> Indices { get; set; }

        public Poligono()
        {
            Vertices = new List<Punto>();
            Indices = new List<uint>();
        }

        public void AgregarVertice(Punto punto)
        {
            Vertices.Add(punto);
        }

        public void AgregarIndice(uint indice)
        {
            Indices.Add(indice);
        }
    }
}
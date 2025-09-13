using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;


using OpenTK.Mathematics;

namespace Opentk_2222.Clases
{
    public class Escenario
    {
        public List<Objeto> Objetos { get; set; }
        public Punto CentroMasa { get; private set; }
        public string Nombre { get; set; }

        public Escenario(string nombre = "Escenario Principal")
        {
            Nombre = nombre;
            Objetos = new List<Objeto>();
            CentroMasa = new Punto(0, 0, 0);
        }

        public void AgregarObjeto(Objeto objeto)
        {
            Objetos.Add(objeto);
            CalcularCentroMasa();
        }

        public void AgregarObjetos(params Objeto[] objetos)
        {
            foreach (var objeto in objetos)
            {
                Objetos.Add(objeto);
            }
            CalcularCentroMasa();
        }

        public void EliminarObjeto(string nombre)
        {
            Objetos.RemoveAll(o => o.Nombre == nombre);
            CalcularCentroMasa();
        }

        public Objeto BuscarObjeto(string nombre)
        {
            return Objetos.Find(o => o.Nombre == nombre);
        }

        public List<Objeto> BuscarObjetosPorPatron(string patron)
        {
            return Objetos.Where(o => o.Nombre.Contains(patron)).ToList();
        }

        private void CalcularCentroMasa()
        {
            if (Objetos.Count == 0)
            {
                CentroMasa = new Punto(0, 0, 0);
                return;
            }

            var centrosMasa = Objetos.Select(o => o.CentroMasa).ToList();
            CentroMasa = Punto.CalcularCentroMasa(centrosMasa);
        }

        public void LimpiarEscenario()
        {
            Objetos.Clear();
            CentroMasa = new Punto(0, 0, 0);
        }

        public int ContarTotalCaras()
        {
            return Objetos.Sum(o => o.Partes.Sum(p => p.Caras.Count));
        }

        public int ContarTotalVertices()
        {
            return Objetos.Sum(o => o.Partes.Sum(p => p.Caras.Sum(c => c.Vertices.Count)));
        }

        // Método para obtener estadísticas del escenario
        public string ObtenerEstadisticas()
        {
            return $"""
                Escenario: {Nombre}
                Objetos: {Objetos.Count}
                Total de Partes: {Objetos.Sum(o => o.Partes.Count)}
                Total de Caras: {ContarTotalCaras()}
                Total de Vértices: {ContarTotalVertices()}
                Centro de Masa: {CentroMasa}
                """;
        }

        public override string ToString()
        {
            return $"{Nombre} - {Objetos.Count} objetos - Centro: {CentroMasa}";
        }
    }
}
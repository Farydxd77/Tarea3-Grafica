using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace Opentk_2222.Clases
{
    public class Escenario
    {
        public List<Objeto> Objetos { get; set; }

        public Escenario()
        {
            Objetos = new List<Objeto>();
        }

        public void AgregarObjeto(Objeto objeto)
        {
            Objetos.Add(objeto);
        }

        public void EliminarObjeto(string nombre)
        {
            Objetos.RemoveAll(o => o.Nombre == nombre);
        }

        public Objeto BuscarObjeto(string nombre)
        {
            return Objetos.Find(o => o.Nombre == nombre);
        }
    }
}
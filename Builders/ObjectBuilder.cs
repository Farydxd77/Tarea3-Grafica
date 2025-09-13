using OpenTK.Mathematics;
using Opentk_2222.Clases;

namespace Opentk_2222.Builders
{
    public class ObjectBuilder
    {
        private Objeto objeto;
        private List<ParteDefinition> partes;

        public struct ParteDefinition
        {
            public string Nombre { get; set; }
            public Vector3 Posicion { get; set; }
            public Vector3 Escala { get; set; }
            public Vector3 Color { get; set; }
        }

        private ObjectBuilder(string nombre, Vector3 posicion, Vector3 colorBase)
        {
            objeto = new Objeto(nombre)
            {
                Posicion = posicion,
                ColorBase = colorBase
            };
            partes = new List<ParteDefinition>();
        }

        // Factory method para iniciar la construcción
        public static ObjectBuilder Crear(string nombre, Vector3 posicion, Vector3 colorBase)
        {
            return new ObjectBuilder(nombre, posicion, colorBase);
        }

        // Método fluido para agregar partes
        public ObjectBuilder AgregarParte(string nombre, Vector3 posicion, Vector3 escala, Vector3 color)
        {
            partes.Add(new ParteDefinition
            {
                Nombre = nombre,
                Posicion = posicion,
                Escala = escala,
                Color = color
            });
            return this;
        }

        // Método para agregar múltiples partes de una vez
        public ObjectBuilder AgregarPartes(params ParteDefinition[] partesArray)
        {
            partes.AddRange(partesArray);
            return this;
        }

        // Método para construir el objeto final
        public Objeto Construir()
        {
            var partesObjeto = new List<Parte>();

            foreach (var def in partes)
            {
                var parte = Parte.CrearCubo(def.Nombre, def.Posicion, def.Escala, def.Color);
                partesObjeto.Add(parte);
            }

            objeto.AgregarPartes(partesObjeto.ToArray());
            return objeto;
        }

        // Métodos de conveniencia para objetos comunes
        public static class Presets
        {
            public static ObjectBuilder Monitor(Vector3 posicion)
            {
                return ObjectBuilder.Crear("Monitor", posicion, new Vector3(0.15f, 0.15f, 0.15f))
                    .AgregarParte("Pantalla", new Vector3(0f, 0f, 0f), new Vector3(2.4f, 1.5f, 0.06f), new Vector3(0.02f, 0.02f, 0.05f))
                    .AgregarParte("Marco", new Vector3(0f, 0f, -0.04f), new Vector3(2.5f, 1.6f, 0.03f), new Vector3(0.1f, 0.1f, 0.1f))
                    .AgregarParte("BaseMonitor", new Vector3(0f, -0.85f, 0.1f), new Vector3(0.6f, 0.08f, 0.4f), new Vector3(0.2f, 0.2f, 0.2f))
                    .AgregarParte("Soporte", new Vector3(0f, -0.42f, 0.05f), new Vector3(0.06f, 0.5f, 0.06f), new Vector3(0.15f, 0.15f, 0.15f));
            }

            public static ObjectBuilder CPU(Vector3 posicion)
            {
                return ObjectBuilder.Crear("CPU", posicion, new Vector3(0.25f, 0.25f, 0.3f))
                    .AgregarParte("Carcasa", new Vector3(0f, 0f, 0f), new Vector3(0.5f, 1.6f, 0.8f), new Vector3(0.2f, 0.2f, 0.25f))
                    .AgregarParte("PanelFrontal", new Vector3(0f, 0.1f, 0.42f), new Vector3(0.45f, 0.3f, 0.02f), new Vector3(0.1f, 0.1f, 0.15f))
                    .AgregarParte("BotonEncendido", new Vector3(-0.15f, -0.5f, 0.43f), new Vector3(0.05f, 0.05f, 0.01f), new Vector3(0.8f, 0.2f, 0.2f))
                    .AgregarParte("Ventilacion", new Vector3(0.1f, -0.2f, 0.42f), new Vector3(0.15f, 0.15f, 0.02f), new Vector3(0.05f, 0.05f, 0.05f))
                    .AgregarParte("LED", new Vector3(-0.15f, -0.6f, 0.43f), new Vector3(0.02f, 0.02f, 0.005f), new Vector3(0.2f, 0.8f, 0.2f));
            }

            public static ObjectBuilder Teclado(Vector3 posicion)
            {
                return ObjectBuilder.Crear("Teclado", posicion, new Vector3(0.05f, 0.05f, 0.05f))
                    .AgregarParte("BaseTeclado", new Vector3(0f, 0f, 0f), new Vector3(2.8f, 0.08f, 0.8f), new Vector3(0.05f, 0.05f, 0.05f))
                    .AgregarParte("AreaTeclas", new Vector3(0f, 0.045f, -0.03f), new Vector3(2.6f, 0.025f, 0.55f), new Vector3(0.1f, 0.1f, 0.1f))
                    .AgregarParte("BarraEspaciadora", new Vector3(0f, 0.06f, 0.2f), new Vector3(1.2f, 0.025f, 0.1f), new Vector3(0.15f, 0.15f, 0.15f))
                    .AgregarParte("TeclasFuncion", new Vector3(0f, 0.06f, -0.32f), new Vector3(2.0f, 0.02f, 0.06f), new Vector3(0.12f, 0.12f, 0.12f))
                    .AgregarParte("IndicadoresLED", new Vector3(1.0f, 0.065f, -0.28f), new Vector3(0.12f, 0.01f, 0.03f), new Vector3(0.8f, 0.8f, 0.2f));
            }

            public static ObjectBuilder Escritorio(Vector3 posicion)
            {
                return ObjectBuilder.Crear("Escritorio", posicion, new Vector3(0.4f, 0.3f, 0.2f))
                    .AgregarParte("Superficie", new Vector3(0f, 0f, 0f), new Vector3(5.0f, 0.1f, 3.0f), new Vector3(0.4f, 0.3f, 0.2f));
            }
        }
    }
}
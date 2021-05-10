namespace PlantUml.Reflector
{
    public class Layer
    {
        public Layer(Layers layers)
        {
            Layers = layers;
        }

        public Layers Layers { get; init;  }

        public bool Shows(Layers target)
        {
            return Layers switch
            {
                Layers.All => true,
                _ => (Layers & target) == target 
            };
        }
    }
}
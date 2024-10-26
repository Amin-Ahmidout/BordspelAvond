using GraphQL.Types;

namespace BordspelAvond.Presentation.GraphQL
{
    public class GameNightSchema : Schema
    {
        public GameNightSchema(IServiceProvider provider) : base(provider)
        {
            Query = provider.GetRequiredService<GameNightQuery>();
        }
    }
}

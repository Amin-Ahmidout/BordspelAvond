using GraphQL.Types;
using BordspelAvond.Application.Interfaces;
using GraphQL;

namespace BordspelAvond.Presentation.GraphQL
{
    public class GameNightQuery : ObjectGraphType
    {
        public GameNightQuery(IGameNightService gameNightService)
        {
            Field<ListGraphType<GameNightType>>(
                "gameNights",
                resolve: context => gameNightService.GetAllGameNightsAsync()
            );

            Field<GameNightType>(
                "gameNight",
                arguments: new QueryArguments(new QueryArgument<IdGraphType> { Name = "id" }),
                resolve: context =>
                {
                    var id = context.GetArgument<int>("id");
                    return gameNightService.GetGameNightByIdAsync(id);
                }
            );
        }
    }
}

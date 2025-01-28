using GraphQL.Types;
using BordspelAvond.Application.Interfaces;
using GraphQL;

namespace BordspelAvond.Presentation.GraphQL
{
    public class GameNightQuery : ObjectGraphType
    {
        public GameNightQuery(IServiceProvider serviceProvider)
        {
            Field<ListGraphType<GameNightType>>("gameNights")
                .ResolveAsync(async context =>
                {
                    // Haal een nieuwe service-instantie op binnen de scope
                    var gameNightService = serviceProvider.GetRequiredService<IGameNightService>();
                    return await gameNightService.GetAllGameNightsAsync();
                });

            Field<GameNightType>("gameNight")
                .Arguments(new QueryArguments(new QueryArgument<IdGraphType> { Name = "id" }))
                .ResolveAsync(async context =>
                {
                    var id = context.GetArgument<int>("id");
                    var gameNightService = serviceProvider.GetRequiredService<IGameNightService>();
                    return await gameNightService.GetGameNightByIdAsync(id);
                });
        }
    }
}
using GraphQL.Types;
using BordspelAvond.Core.Domain;

namespace BordspelAvond.Presentation.GraphQL
{
    public class GameNightType : ObjectGraphType<GameNight>
    {
        public GameNightType()
        {
            Field(x => x.Id).Description("The ID of the GameNight.");
            Field(x => x.Address).Description("The address of the GameNight.");
            Field(x => x.MaxPlayers).Description("The maximum number of players.");
            Field(x => x.DateAndTime).Description("The date and time of the GameNight.");
            Field(x => x.Is18Plus).Description("Indicates if the GameNight is for adults only.");
            Field(x => x.ProvidesVegetarianOptions).Description("Indicates if the GameNight provides vegetarian options.");
            Field(x => x.ProvidesLactoseFreeOptions).Description("Indicates if the GameNight provides lactose-free options.");
            Field(x => x.ProvidesNutsFreeOptions).Description("Indicates if the GameNight provides nut-free options.");
            Field(x => x.ProvidesAlcoholFreeOptions).Description("Indicates if the GameNight provides alcohol-free options.");
            Field(x => x.IsSnackRequired).Description("Indicates if a snack is required.");
        }
    }
}

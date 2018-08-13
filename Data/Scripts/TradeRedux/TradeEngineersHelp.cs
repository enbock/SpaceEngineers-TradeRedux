
namespace TradeRedux
{
    public static class TradeEngineersHelp
    {
        public static void ShowHelp(string screen, string title=null)
        {
            Sandbox.ModAPI.MyAPIGateway.Utilities.ShowMissionScreen("Trade Engineers help",
                null,
                title,
                TradeEngineersHelp.HELPGENERAL);
        }

        public static string HELPGENERAL = 
"Welcome to Trade Engineers. This mods allows trading all ressources used in the game with trading posts that could be placed all around "+
"the known universe. Just drag&drop stuff bought by a station into its buying container to get credits. "+
"Buy stuff from a stations selling container by placing credits in the container marked with the ressource you would like to buy.";
    }
}

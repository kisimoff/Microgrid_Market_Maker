/*
 * Author: Simon Powers
 * An Environment Agent that sends information to a Household Agent
 * about that household's demand, generation, and prices to buy and sell
 * from the utility company, on that day. Responds whenever pinged
 * by a Household Agent with a "start" message.
 */

using ActressMas;
class EnvironmentAgent : Agent
{
    private Random rand = new Random();

    private const int MinGeneration = 5; //min possible generation from renewable energy on a day for a household (in kWh)
    private const int MaxGeneration = 15; //max possible generation from renewable energy on a day for a household (in kWh)
    private const int MinDemand = 5; //min possible demand on a day for a household (in kWh)
    private const int MaxDemand = 15; //max possible demand on a day for a household (in kWh)
    private const int MinPriceToBuyFromUtility = 12; //min possible price to buy 1kWh from the utility company (in pence)
    private const int MaxPriceToBuyFromUtility = 22; //max possible price to buy 1kWh from the utility company (in pence)
    private const int MinPriceToSellToUtility = 2; //min possible price to sell 1kWh to the utility company (in pence)
    private const int MaxPriceToSellToUtility = 5; //max possible price to sell 1kWh to the utility company (in pence)

    public override void Act(Message message)

    {
        switch (message.Content)
        {
            case "start": //this agent only responds to "start" messages
                string senderID = message.Sender; //get the sender's name so we can reply to them
                int demand = rand.Next(MinDemand, MaxDemand); //the household's demand in kWh
                int generation = rand.Next(MinGeneration, MaxGeneration); //the household's demand in kWh
                int priceToBuyFromUtility = rand.Next(MinPriceToBuyFromUtility, MaxPriceToBuyFromUtility); //what the household's utility company
                                                                                                           //charges to buy 1kWh from it
                int priceToSellToUtility = rand.Next(MinPriceToSellToUtility, MaxPriceToSellToUtility);    //what the household's utility company
                                                                                                           //offers to buy 1kWh of renewable energy for
                string content = $"inform {demand} {generation} {priceToBuyFromUtility} {priceToSellToUtility}";
                Send(senderID, content); //send the message with this information back to the household agent that requested it
                //FileWriteMessages($"Environment: {senderID} {content}");
                FileWriteMessages("env");

                break;

            default:
                break;
        }

    }
    public static async Task FileWriteMessages(string report)
    {
        using StreamWriter file = new("C:/Users/Vincent/Desktop/AllMessages.txt", append: true);

        await file.WriteLineAsync(report);
    }
}
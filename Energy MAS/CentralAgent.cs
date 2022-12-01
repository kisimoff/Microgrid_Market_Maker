using ActressMas;

//IMPORTNAT: FORMAT TO SEND - 
//Send("Household:03", "testing");

namespace Energy_MAS
{
    public class CentralAgent : Agent
    {

        //To Do:
        //1. export each agent info to a file
        //2. evaluate if the calculations are correct

        private int myGeneration, myDemand, myPriceBuyUT, myPriceSellUT, overallEnergy, myMoney, OverallEnergy, _turnsToWait, _turnsWaited, agentsCount;
        private bool step1, step2, DutchWait = false; //flow control bools
        private string? Status; // sustainable, buying, selling. Declared after initiation phase
        private int PriceDutch = 23; //(= Max price to Buy from UT) starting price for Dutch Auction 
        private int myPendingEnergy = 0; //updates on accepted offer (pending energy to be recived), used to make descisions of future offers
        List<string> messages = new List<string>(); //gets all the messages, recives one meassage from each participant with his information
        List<string> buyers = new List<string>(); //filters the buyers from messages list
        List<string> sellers = new List<string>(); //filters the sellers from messages list
        Tuple<string, int, int>? buyOffers;
        Tuple<string, int, int>? sellOffers;


        public override void Setup() // initialisation phase
        {



        }

        public override void ActDefault() //step control, to initiate auction after everyone recived the first broadcast. 
        {

            if (!step1)
            {
                _turnsWaited = _turnsWaited + 1;
                Console.WriteLine($"CCC: {_turnsWaited}");
                if (_turnsWaited >= 10)
                {

                    //Broadcast($"calculatePrice {overallEnergy} {agentsCount}");
                    SendToMany(buyers, $"calculatePriceBuyer {overallEnergy}");
                    SendToMany(sellers, $"calculatePriceSeller {overallEnergy}");

                    step1 = true;
                }
            }

            /*   if (!step1)
               {
                   if (--_turnsToWait <= 0)
                   {
                   }
               }
               else if (!step2)
               {
                   if (--_turnsToWait <= 0)
                   {
                   }
               }*/


        }

        public override void Act(Message message)
        {
            //Thread.Sleep(100); //used for debugging
            try
            {
                Console.WriteLine($"\t{message.Format()}");
                message.Parse(out string action, out string parameters);

                switch (action)
                {
                    case "centralInform":
                        HandleCentral(parameters);
                        break;
                    case "priceBuy":
                        HandlePriceBuy(parameters);
                        break;
                    case "priceSell":
                        HandlePriceSell(parameters);
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        private void HandleCentral(string parameters) //split the parameters, calculate energy balance and broadcast its name status and energy
        {
            string[] messageSplit = parameters.Split(" ");
            string agentName = messageSplit[0].Replace("[", "").Replace("]", "");
            string agentStatus = messageSplit[1];
            string agentEnergy = messageSplit[2];

            overallEnergy = overallEnergy + (Int32.Parse(agentEnergy));
            agentsCount = agentsCount + 1;
            Console.WriteLine($" \n CCC: Recived: {agentName}; OverallEnergy:{overallEnergy}, agentsCount: {agentsCount} \n");

            if (agentStatus == "buying")
            {
                buyers.Add(agentName);
            }
            else if (agentStatus == "selling")
            {
                sellers.Add(agentName);
            }
        }

        private void HandlePriceBuy(string parameters) //split the parameters, calculate energy balance and broadcast its name status and energy
        {
            string[] messageSplit = parameters.Split(" ");
            string buyerName = messageSplit[0].Replace("[", "").Replace("]", "");
            string  = messageSplit[1];
            string agentEnergy = messageSplit[2];

            overallEnergy = overallEnergy + (Int32.Parse(agentEnergy));
            agentsCount = agentsCount + 1;
            Console.WriteLine($" \n CCC: Recived: {agentName}; OverallEnergy:{overallEnergy}, agentsCount: {agentsCount} \n");

            if (agentStatus == "buying")
            {
                buyers.Add(agentName);
            }
            else if (agentStatus == "selling")
            {
                sellers.Add(agentName);
            }
        }

        private void HandlePriceSell(string parameters) //split the parameters, calculate energy balance and broadcast its name status and energy
        {
            string[] messageSplit = parameters.Split(" ");
            string agentName = messageSplit[0].Replace("[", "").Replace("]", "");
            string agentStatus = messageSplit[1];
            string agentEnergy = messageSplit[2];

            overallEnergy = overallEnergy + (Int32.Parse(agentEnergy));
            agentsCount = agentsCount + 1;
            Console.WriteLine($" \n CCC: Recived: {agentName}; OverallEnergy:{overallEnergy}, agentsCount: {agentsCount} \n");

            if (agentStatus == "buying")
            {
                buyers.Add(agentName);
            }
            else if (agentStatus == "selling")
            {
                sellers.Add(agentName);
            }
        }










        public static async Task FileWriteReport(string report)
        {
            using StreamWriter file = new("C:/Users/Vincent/Desktop/AgentsOutput.txt", append: true);

            await file.WriteLineAsync(report);
        }

    }
}

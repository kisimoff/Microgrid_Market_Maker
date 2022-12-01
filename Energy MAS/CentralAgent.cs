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
        int buyBidsCount = 0;
        int sellerBidsCount = 0;
        /*        Tuple<string, int, int>? buyOffers;
                Tuple<string, int, int>? sellOffers;
                struct buyBids
                {
                    public string name; 
                    public int price;
                    public int amount;

                    public void getId(int name)
                    {
                       Console.WriteLine("Name: " + name);
                    }
                }
                struct sellBids
                {
                    public string name;
                    public int price;
                    public int amount;

                    public void getId(int name)
                    {
                        Console.WriteLine("Name: " + name);
                    }

                }*/
        class BuyBid
        {
            public string? agentName { get; set; }
            public int price { get; set; }
            public int amount { get; set; }
        }

        List<BuyBid> buyBidsList = new List<BuyBid>();

        class SellBid
        {
            public string? agentName { get; set; }
            public int price { get; set; }
            public int amount { get; set; }
        }

        List<SellBid> sellBidsList = new List<SellBid>();

        public override void Setup() // initialisation phase
        {



        }

        public override void ActDefault() //step control, to initiate auction after everyone recived the first broadcast. 
        {

            if (!step1)
            {
                _turnsWaited = _turnsWaited + 1;
                //Console.WriteLine($"CCC: {_turnsWaited}");
                if (_turnsWaited >= 10)
                {
                    //Broadcast($"calculatePrice {overallEnergy} {agentsCount}");
                    SendToMany(buyers, $"calculatePriceBuy {overallEnergy}");
                    SendToMany(sellers, $"calculatePriceSell {overallEnergy}");
                    step1 = true;
                    _turnsWaited = 0;
                }
            }
            if (step2)
            {
                _turnsWaited = _turnsWaited + 1;
                //Console.WriteLine($"CCC: {_turnsWaited}");
                if (_turnsWaited >= 10)
                {
                    SortLists();
                    step2 = false;
                    _turnsWaited = 0;
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
            int buyerAmountToBuy = Int32.Parse(messageSplit[1]);
            int buyerPriceToBuy = Int32.Parse(messageSplit[2]);
            buyBidsList.Add(new BuyBid { agentName = buyerName, amount = buyerAmountToBuy, price = buyerPriceToBuy });
            buyBidsCount = buyBidsCount + 1;
            Console.WriteLine($"BuyList: {buyBidsList.Count}; BuyCount: {buyBidsCount}");
            _turnsWaited = 0;
            step2 = true;
            //Console.WriteLine($"\n[CENTRAL]: Recived: from:{buyerName}; PriceBuy:{buyerPriceToBuy}; Amount:{buyerAmountToBuy}; \n");
            //BuyBid buyersName = new BuyBid { agentName = buyerName, amount = buyerAmountToBuy, price = buyerPriceToBuy };
            //buyBidsList.Add(buyersName);
            //Console.WriteLine($"Adding buy bid from:[{buyersName.agentName}]; With price:{buyersName.price};");

        }

        private void HandlePriceSell(string parameters) //split the parameters, calculate energy balance and broadcast its name status and energy
        {
            string[] messageSplit = parameters.Split(" ");
            string sellerName = messageSplit[0].Replace("[", "").Replace("]", "");
            int sellerAmountToBuy = Int32.Parse(messageSplit[1]);
            int sellerPriceToBuy = Int32.Parse(messageSplit[2]);
            sellBidsList.Add(new SellBid { agentName = sellerName, amount = sellerAmountToBuy, price = sellerPriceToBuy });
            sellerBidsCount = sellerBidsCount + 1;
            Console.WriteLine($"SellList: {sellBidsList.Count}; SellCount:{sellerBidsCount}");
            _turnsWaited = 0;
            step2 = true;




            // Console.WriteLine($"\n[CENTRAL]: Recived: from:{sellerName}; PriceSell:{sellerPriceToBuy}; Amount:{sellerAmountToBuy}; \n");

        }




        private void SortLists()
        {


            buyBidsList = buyBidsList.OrderByDescending(o => o.price).ToList();
            sellBidsList = sellBidsList.OrderBy(o => o.price).ToList();


            foreach (BuyBid buyBidsList in buyBidsList) // Loop through List with foreach
            {
                Console.WriteLine($"[BUY]: {buyBidsList.price}$; From: {buyBidsList.agentName}; ");
            }

            foreach (SellBid sellBidsList in sellBidsList) // Loop through List with foreach
            {
                Console.WriteLine($"[SELL]: {sellBidsList.price}$; From: {sellBidsList.agentName};");
            }

            if (buyBidsList[0].price >= sellBidsList[0].price)
            {
                Console.WriteLine($"[MATCH]: {buyBidsList[0].price} is >= than {sellBidsList[0].price};");
            }



        }





        public static async Task FileWriteReport(string report)
        {
            using StreamWriter file = new("C:/Users/Vincent/Desktop/AgentsOutput.txt", append: true);

            await file.WriteLineAsync(report);
        }

    }
}

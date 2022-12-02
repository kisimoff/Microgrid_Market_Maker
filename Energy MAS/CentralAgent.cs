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

        private int overallEnergy, _turnsWaited, agentsCount;
        private bool step1, step2, neverMached, check, step3, step4 = false; //flow control bools
        private int PriceDutch = 23; //(= Max price to Buy from UT) starting price for Dutch Auction 
        List<string> messages = new List<string>(); //gets all the messages, recives one meassage from each participant with his information
        List<string> buyers = new List<string>(); //filters the buyers from messages list
        List<string> sellers = new List<string>(); //filters the sellers from messages list
        int buyBidsCount = 0;
        int sellerBidsCount = 0;
        bool printOrders = false;
        class BuyBid
        {
            public string? agentName { get; set; }
            public int price { get; set; }
            public int amount { get; set; }
        }


        class SellBid
        {
            public string? agentName { get; set; }
            public int price { get; set; }
            public int amount { get; set; }
        }

        List<BuyBid> buyBidsList = new List<BuyBid>();
        List<SellBid> sellBidsList = new List<SellBid>();


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
            if (step3)
            {
                _turnsWaited = _turnsWaited + 1;
                if (_turnsWaited >= 100)
                {
                    if (sellBidsList.Count == 0)
                    {


                        NoSellersLeft();
                    }
                    if (buyBidsList.Count == 0)
                    {

                        NoBuyersLeft();
                    }

                    step3 = false;
                    step4 = true;
                    _turnsWaited = 0;
                }

            }
            if (step4)
            {
                _turnsWaited = _turnsWaited + 1;
                if (_turnsWaited >= 100)
                {
                    GenerateReport();
                    step4 = false;
                    _turnsWaited = 0;

                }

            }


        }

        public override void Act(Message message)
        {
            //Thread.Sleep(100); //used for debugging
            try
            {
                Console.WriteLine($"\t{message.Format()}");
                message.Parse(out string action, out string parameters);
                FileWriteMessages("central");
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
            // Console.WriteLine($" \n CCC: Recived: {agentName}; OverallEnergy:{overallEnergy}, agentsCount: {agentsCount} \n");

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

            if (check)
            {

                for (int i = 0; i < buyBidsList.Count; i++) // Loop through List with foreach
                {
                    if (buyBidsList[i].agentName == buyerName)
                    {
                        buyBidsList[i].price = buyerPriceToBuy;
                    }
                }
            }
            else
            {
                buyBidsList.Add(new BuyBid { agentName = buyerName, amount = buyerAmountToBuy, price = buyerPriceToBuy });
                buyBidsCount = buyBidsCount + 1;

            }


            // Console.WriteLine($"BuyList: {buyBidsList.Count}; BuyCount: {buyBidsCount}");
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


            if (check)
            {

                for (int i = 0; i < sellBidsList.Count; i++) // Loop through List with foreach
                {
                    if (sellBidsList[i].agentName == sellerName)
                    {
                        sellBidsList[i].price = sellerPriceToBuy;
                    }
                }
            }
            else
            {
                sellBidsList.Add(new SellBid { agentName = sellerName, amount = sellerAmountToBuy, price = sellerPriceToBuy });

            }


            sellerBidsCount = sellerBidsCount + 1;
            // Console.WriteLine($"SellList: {sellBidsList.Count}; SellCount:{sellerBidsCount}");
            _turnsWaited = 0;
            step2 = true;
            // Console.WriteLine($"\n[CENTRAL]: Recived: from:{sellerName}; PriceSell:{sellerPriceToBuy}; Amount:{sellerAmountToBuy}; \n");

        }


        private void SortLists()
        {


            buyBidsList = buyBidsList.OrderByDescending(o => o.price).ToList();
            sellBidsList = sellBidsList.OrderBy(o => o.price).ToList();

            if (printOrders)
            {


                foreach (BuyBid buyBidsList in buyBidsList) // Loop through List with foreach
                {
                    Console.WriteLine($"[BUY]: {buyBidsList.price}$; Amount:{buyBidsList.amount}; From:{buyBidsList.agentName}; ");
                }

                foreach (SellBid sellBidsList in sellBidsList) // Loop through List with foreach
                {
                    Console.WriteLine($"[SELL]: {sellBidsList.price}$; Amount:{sellBidsList.amount}; From:{sellBidsList.agentName};");
                }
            }
            if (buyBidsList[0].price >= sellBidsList[0].price)
            {
                neverMached = false;
                //Console.WriteLine($"[MATCH]: {buyBidsList[0].price} is >= than {sellBidsList[0].price};");
                HandleMatches();
            }
            else
            {
                neverMached = true;
                // Console.WriteLine($"[NO MATCH]: {buyBidsList[0].price} is >= than {sellBidsList[0].price};");

                noMatch();
            }



        }
        private void NoBuyersLeft()
        {
            for (int i = 0; i < sellBidsList.Count; i++) // Loop through List with foreach
            {
                Send(sellBidsList[i].agentName, "noBuyersLeft");
            }
        }
        private void NoSellersLeft()
        {
            for (int i = 0; i < buyBidsList.Count; i++) // Loop through List with foreach
            {
                Send(buyBidsList[i].agentName, "noSellersLeft");
            }
        }

        private void HandleMatches()
        {
            int sellerIndex = 0;
            int buyerIndex = 0;
            int amountToSend = 0;
            string? nameToSend;
            int priceSend = 0;
            string allBuyers = "";


            while (sellBidsList[sellerIndex].amount > 0) //this works if there is negative energy balance if positive loop forever.
                                                         //if possiive,  there are more sellers, and there will be no buyers left
            {
                if (sellBidsList[sellerIndex].amount >= Math.Abs(buyBidsList[buyerIndex].amount))
                {
                    nameToSend = buyBidsList[buyerIndex].agentName;
                    amountToSend = Math.Abs(buyBidsList[buyerIndex].amount);
                    priceSend = buyBidsList[buyerIndex].price;

                    allBuyers = allBuyers + $"{nameToSend},{priceSend},{amountToSend};";

                    sellBidsList[sellerIndex].amount = sellBidsList[sellerIndex].amount - amountToSend;

                    //Console.WriteLine($"About to remove:{buyBidsList[buyerIndex].agentName}, Energy {buyBidsList[buyerIndex].amount}, Energy to be sent: {amountToSend}, listSize: {buyBidsList.Count} ");
                    buyBidsList.RemoveAt(buyerIndex);

                    // Console.WriteLine($"{allBuyers} myEnergy:{sellBidsList[sellerIndex].amount}");

                    //buyBidsList[buyerIndex].amount = 0;


                    //Console.WriteLine($"Now next is:{buyBidsList[buyerIndex].agentName}");
                    if (buyBidsList.Count == 0)
                    {
                        break;
                    }

                }
                else //buyer demand is higher than seller amount
                {

                    nameToSend = buyBidsList[buyerIndex].agentName;
                    priceSend = buyBidsList[buyerIndex].price;
                    amountToSend = sellBidsList[sellerIndex].amount;
                    sellBidsList[sellerIndex].amount = 0;
                    allBuyers = allBuyers + $"{nameToSend},{priceSend},{amountToSend};";
                    // Console.WriteLine($"{allBuyers} myEnergy:{sellBidsList[sellerIndex].amount}");
                    buyBidsList[buyerIndex].amount = buyBidsList[buyerIndex].amount + amountToSend;

                    //Console.WriteLine($"About to remove:{buyBidsList[buyerIndex].agentName}");
                    //buyBidsList.RemoveAt(buyerIndex);
                    //Console.WriteLine($"Now next is:{buyBidsList[buyerIndex].agentName}");

                }
            };



            // Console.WriteLine($"[SELLER] My energy should be 0 - {sellBidsList[sellerIndex].amount};{sellBidsList[sellerIndex].agentName} Im about to get removed from the list.");
            Send(sellBidsList[sellerIndex].agentName, $"requestSendEnergy {allBuyers}");
            if (sellBidsList[sellerIndex].amount == 0)
            {
                sellBidsList.RemoveAt(sellerIndex);

            }
            else
            {
                Console.WriteLine("******************************GOTIT***************************");
            }

            if (sellBidsList.Count == 0 || buyBidsList.Count == 0)
            {
                //NoSellersLeft();
                step3 = true;
                return;

            }

            if (buyBidsList[0].price >= sellBidsList[0].price)
            {
                // Console.WriteLine($"[MATCH]: {buyBidsList[0].price} is >= than {sellBidsList[0].price};");
                HandleMatches();
            }
            else
            {
                noMatch();

            }

            /*
                        if (sellBidsList.Count == 0)
                        {
                            NoSellersLeft();
                            step3 = true;
                            return;

                        }
                        if (buyBidsList.Count == 0)
                        {
                            NoBuyersLeft();
                            step3 = true;
                            return;
                        }
            */




            /*  Console.WriteLine($"[NO MATCH!]");
          foreach (BuyBid buyBidsList in buyBidsList) // Loop through List with foreach
          {
              Console.WriteLine($"[BUY]: {buyBidsList.price}$; Amount:{buyBidsList.amount}; From:{buyBidsList.agentName}; ");
              buyers.Add(buyBidsList.agentName);
              //Send(buyBidsList.agentName, $"noMatch, {sellBidsList[0].price}");
          }

          foreach (SellBid sellBidsList in sellBidsList) // Loop through List with foreach
          {
              Console.WriteLine($"[SELL]: {sellBidsList.price}$; Amount:{sellBidsList.amount}; From:{sellBidsList.agentName};");
              sellers.Add(sellBidsList.agentName);
              //Send(sellBidsList.agentName, $"noMatch, {buyBidsList[0].price}");
          }
          SendToMany(buyers, $"noMatchBuyer {sellBidsList[0].price}");
          SendToMany(sellers, $"noMatchSeller {buyBidsList[0].price}");
*/

        }



        private void noMatch()
        {
            check = true;
            // Console.WriteLine($"[NO MATCH!]");


            if (neverMached)
            {

                SendToMany(buyers, $"noMatchBuyer {sellBidsList[0].price}");
                SendToMany(sellers, $"noMatchSeller {buyBidsList[0].price}");
            }
            else
            {
                buyers.Clear();
                sellers.Clear();
                foreach (BuyBid buyBidsList in buyBidsList) // Loop through List with foreach
                {
                    // Console.WriteLine($"[BUY]: {buyBidsList.price}$; Amount:{buyBidsList.amount}; From:{buyBidsList.agentName}; ");
                    buyers.Add(buyBidsList.agentName);
                    //Send(buyBidsList.agentName, $"noMatch, {sellBidsList[0].price}");
                }

                foreach (SellBid sellBidsList in sellBidsList) // Loop through List with foreach
                {
                    // Console.WriteLine($"[SELL]: {sellBidsList.price}$; Amount:{sellBidsList.amount}; From:{sellBidsList.agentName};");
                    sellers.Add(sellBidsList.agentName);
                    //Send(sellBidsList.agentName, $"noMatch, {buyBidsList[0].price}");
                }
                SendToMany(buyers, $"noMatchBuyer {sellBidsList[0].price}");
                SendToMany(sellers, $"noMatchSeller {buyBidsList[0].price}");
            }

        }

        public static async Task FileWriteReport(string report)
        {
            using StreamWriter file = new("C:/Users/Vincent/Desktop/AgentsOutput.txt", append: true);

            await file.WriteLineAsync(report);
        }

        public static async Task FileWriteMessages(string report)
        {
            using StreamWriter file = new("C:/Users/Vincent/Desktop/AllMessages.txt", append: true);

            await file.WriteLineAsync(report);
        }


        private void GenerateReport()
        {
            int messagesCountFile = File.ReadAllLines("C:/Users/Vincent/Desktop/AllMessages.txt").Length;
            Console.WriteLine($"Total Messages Exchanged: {messagesCountFile}");
            File.Delete("C:/Users/Vincent/Desktop/AllMessages.txt");

        }
    }
}

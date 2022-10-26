using ActressMas;

//IMPORTNAT: FORMAT TO SEND - 
//Send("Household:03", "testing");

namespace Energy_MAS
{
    public class HouseholdAgent : Agent
    {
        private int MyGeneraton;//generation from renewable energy on a day for a household (in kWh)
        private int MyDemand; // demand on a day for a household (in kWh)
        private int MyPriceBuyUT; // buy 1kWh from the utility company (in pence)
        private int MyPriceSellUT;
        private string? Status;
        private int Energy;
        private int OverallEnergy;
        List<string> messages = new List<string>();
        private bool step1 = false;
        private bool step2 = false;
        private int PriceDutch = 23;
        private int _turnsToWait;
        private bool DutchWait = false;
        List<string> buyers = new List<string>();

        public override void Setup()
        {
            Send("environment", "start");
            _turnsToWait = 2;
        }

        public override void ActDefault() //step control, to initiate auction after everyone recived the first broadcast. 
        {

            if (!step1)
            {
                if (--_turnsToWait <= 0)
                {
                    AllMessagesRescived();

                    //Dutch_Auction();
                }
            }
            else if (!step2)
            {
                if (--_turnsToWait <= 0)
                {
                    Auction_Decider();
                }
            }
            else if (DutchWait)
            {
                if (--_turnsToWait <= 0)
                {
                    Dutch_Auction();
                    DutchWait = false;

                }
            }

        }

        public override void Act(Message message)
        {
            try
            {
                if (Status == "sustainable")
                {
                    //  break;
                }

                Console.WriteLine($"\t{message.Format()}");
                message.Parse(out string action, out string parameters);

                switch (action)
                {
                    case "inform": //activates when Setup: send start to env
                        HandleInfromation(parameters);
                        break;


                    case "broadcast":
                        if (Status == "sustainable")
                        {
                            break;
                        }
                        HandleBoradcast(parameters);
                        break;

                    case "dutchAuction":
                        Console.WriteLine("Dutch Auction Activated");

                        if (Status == "buying")
                        {
                            string[] offerSplit = parameters.Split(" ");
                            int PriceToBuy = MyPriceBuyUT - 1;
                            int OfferPrice = Int32.Parse(offerSplit[0]);
                            int OfferEnergyAmount = Int32.Parse(offerSplit[1]);
                            /*  if (OfferEnergyAmount > Energy)
                              {
                                  OfferEnergyAmount = OfferEnergyAmount - Energy;
                              }*/
                            if (PriceToBuy <= OfferPrice)
                            {
                                Send(message.Sender, $"buyingDutch {OfferPrice} {OfferEnergyAmount}");
                            }

                            Console.WriteLine($"\n [{Name}]: im {Status} and i have {Energy} energy.\n");
                        }
                        break;

                    case "buyingDutch":
                        string[] buyingSplit = parameters.Split(" ");
                        int buyingPrice = Int32.Parse(buyingSplit[0]);
                        int buyingAmount = Int32.Parse(buyingSplit[1]);



                        break;

                    case "japanAuction":
                        Console.WriteLine("Japaneese Auction Activated");

                        if (Status == "buying")
                        {
                            Console.WriteLine($"\n [{Name}]: im {Status} and i have {Energy} energy.\n");

                        }
                        break;

                    case "offer":
                        //string[] offerSplit = parameters.Split(" ");
                        //   Console.WriteLine($"Im {Status} and i recived an offer for buying at {offerSplit[0]} from {message.Sender}");
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


        private void HandleInfromation(string parameters) //split the parameters, calculate energy balance and broadcast its name status and energy
        {
            string[] infoSplit = parameters.Split(" ");
            MyDemand = Int32.Parse(infoSplit[0]);
            MyGeneraton = Int32.Parse(infoSplit[1]);
            MyPriceBuyUT = Int32.Parse(infoSplit[2]);
            MyPriceSellUT = Int32.Parse(infoSplit[3]);
            Energy = MyGeneraton - MyDemand;
            if (Energy > 0) { Status = "selling"; } else if (Energy == 0) { Status = "sustainable"; Stop(); } else { Status = "buying"; }
            Console.WriteLine($"[{Name}] Energy Balance: {Energy}; Status: {Status}; \nInfo - Demand: {MyDemand}; Generation: {MyGeneraton}; Price to buy from UT: {MyPriceBuyUT}; Price to sell to UT: {MyPriceSellUT}; \n");
            if (Status != "sustainable")
            {
                Broadcast($"broadcast [{Name}] {Status} {Energy}");
            }

            _turnsToWait = 100;
        }


        private void HandleBoradcast(string parameters) //add each message recived in a list and calculate overall energy
        {
            string[] broadcastSplit = parameters.Split(" ");
            messages.Add(parameters);
            /*string NameSender = broadcastSplit[0];
            string StatusSender = broadcastSplit[1];
            string EnergySender = broadcastSplit[2];*/
            OverallEnergy = OverallEnergy + (Int32.Parse(broadcastSplit[2]));
            // Console.WriteLine($"\n [{Name}]: Recived message from {NameSender} Who is: {StatusSender} Balance: {EnergySender} \n Total Messages Recived:{messages.Count}  OverallEnergy: {OverallEnergy + Energy} \n");
            _turnsToWait = 2;

        }



        private void AllMessagesRescived() //when all messeges recived step1 is complete
        {
            step1 = true;
            Console.WriteLine($"1.-------------- [{Name}]: Messages recived:{messages.Count} OverallEnergy: {OverallEnergy + Energy}");
            _turnsToWait = 2;

        }



        private void Auction_Decider() //two auctions - Dutch when (overall energy) is > 0 and Japaneese when (overall energy) is < 0
        {
            step2 = true;

            Extract_Buyers(messages);

            if ((OverallEnergy + Energy) > 0) // Dutch Auction
            {
                // Console.WriteLine("SELL some to EC");
                if (Status == "selling")
                {
                    Console.WriteLine($"\n [{Name}]: im {Status} and i have {Energy} energy. Im about to start Dutch Auction.\n");
                    //Thread.Sleep(1000);
                    Dutch_Auction();

                }
            }
            else if ((OverallEnergy + Energy) < 0) // Japaneese Auction
            {
                //   Console.WriteLine("BUY some from EC");
                if (Status == "selling")
                {
                    Console.WriteLine($"\n [{Name}]: im {Status} and i have {Energy} energy. Im about to start Japaneese  Auction.\n");
                    Japaneese_Auction();

                }
            }
            else
            {
                Console.WriteLine("We are self-sutainable, the energy company would starve.");


            }

        }
        private void Dutch_Auction()  // When (overall energy) is > 0
        /*     In a Dutch auction, an initial price is set that is very high, after which the price is gradually decreased.At any
moment, any bidder can claim the item.*/
        {
            SendToMany(buyers, $"dutchAuction {PriceDutch} {Energy}");
            _turnsToWait = messages.Count;
            DutchWait = true;
            PriceDutch--;
        }

        private void Japaneese_Auction() // When(overall energy) is < 0
        /* In a Japanese auction, the initial price is zero; the price is then gradually increased.A bidder
can leave the room when the price becomes too high for her.Once there is only one bidder remaining,
that bidder wins the item, and pays the price at which the last other bidder left the room.*/
        {
            SendToMany(buyers, "japanAuction");

        }

        private void Extract_Buyers(List<String> messages) //for each message, get the buyer's name and strip "[" and  "]"
        {
            // List<string> players = new List<string>();

            foreach (string household in messages)
            {
                string[] householdSplit = household.Split(" ");
                if (householdSplit[1] == "buying")
                {
                    string housholdStripped = householdSplit[0].Replace("[", "").Replace("]", "");

                    buyers.Add(housholdStripped);

                }
            }
        }



    }


}

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


        public override void Setup()
        {
            Send("environment", "start");
            _turnsToWait = 2;
        }

        public override void ActDefault()
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
                            Console.WriteLine($"\n [{Name}]: im {Status} and i have {Energy} energy.\n");
                        }
                        break;

                    case "japanAuction":
                        Console.WriteLine("Japaneese Auction Activated");

                        if (Status == "buying")
                        {
                            Console.WriteLine($"\n [{Name}]: im {Status} and i have {Energy} energy.\n");

                        }
                        break;

                    case "offer":
                        string[] offerSplit = parameters.Split(" ");
                        Console.WriteLine($"Im {Status} and i recived an offer for buying at {offerSplit[0]} from {message.Sender}");
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


        private void HandleInfromation(string parameters)
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


        private void HandleBoradcast(string parameters)
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



        private void AllMessagesRescived()
        {
            step1 = true;
            Console.WriteLine($"1.-------------- [{Name}]: Messages recived:{messages.Count} OverallEnergy: {OverallEnergy + Energy}");
            _turnsToWait = 2;

        }



        private void Auction_Decider()
        {
            step2 = true;

            List<string> buyers = Extract_Buyers(messages);

            if ((OverallEnergy + Energy) > 0) // Dutch Auction
            {
                // Console.WriteLine("SELL some to EC");
                if (Status == "selling")
                {
                    Console.WriteLine($"\n [{Name}]: im {Status} and i have {Energy} energy. Im about to start Dutch Auction.\n");
                    //Thread.Sleep(1000);
                    Dutch_Auction(buyers);

                }
            }
            else if ((OverallEnergy + Energy) < 0) // Japaneese Auction
            {
                //   Console.WriteLine("BUY some from EC");
                if (Status == "selling")
                {
                    Console.WriteLine($"\n [{Name}]: im {Status} and i have {Energy} energy. Im about to start Japaneese  Auction.\n");
                    Japaneese_Auction(buyers);

                }
            }
            else
            {
                Console.WriteLine("We are self-sutainable, the energy company would starve.");


            }

        }
        private void Dutch_Auction(List<String> buyers)  //Too much energy
        /*     In a Dutch auction, an initial price is set that is very high, after which the price is gradually decreased.At any
moment, any bidder can claim the item.*/
        {

            SendToMany(buyers, "dutchAuction");

        }

        private void Japaneese_Auction(List<String> buyers) //Too little energy
        /* In a Japanese auction, the initial price is zero; the price is then gradually increased.Abidder
can leave the room when the price becomes too high for her.Once there is only one bidder remaining,
that bidder wins the item, and pays the price at which the last other bidder left the room.*/
        {
            SendToMany(buyers, "japanAuction");

        }

        private List<String> Extract_Buyers(List<String> messages)
        {
            List<string> players = new List<string>();

            foreach (string household in messages)
            {
                string[] householdSplit = household.Split(" ");
                if (householdSplit[1] == "buying")
                {
                    string housholdStripped = householdSplit[0].Replace("[", "").Replace("]", "");

                    players.Add(housholdStripped);
                    // Broadcast("Hi There");
                    //PriceDutch--; 
                    //  Console.WriteLine($"He is a buyer: {housholdStripped}");
                }
            }
            return players;
        }



    }


}

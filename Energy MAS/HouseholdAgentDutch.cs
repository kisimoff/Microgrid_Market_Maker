using ActressMas;

//IMPORTNAT: FORMAT TO SEND - 
//Send("Household:03", "testing");

namespace Energy_MAS
{
    public class HouseholdAgentDutch : Agent
    {

        //To Do:
        //1. export each agent info to a file
        //2. evaluate if the calculations are correct

        private int myGeneration, myDemand, myPriceBuyUT, myPriceSellUT, myEnergy, myMoney, OverallEnergy, _turnsToWait, _turnsWaited;
        private bool step1, step2, DutchWait, step4, step5 = false; //flow control bools
        private string? Status; // sustainable, buying, selling. Declared after initiation phase
        private int PriceDutch = 23; //(= Max price to Buy from UT) starting price for Dutch Auction 
        private int myPendingEnergy = 0; //updates on accepted offer (pending energy to be recived), used to make descisions of future offers
        List<string> messages = new List<string>(); //gets all the messages, recives one meassage from each participant with his information
        List<string> buyers = new List<string>(); //filters the buyers from messages list
        List<string> sellers = new List<string>(); //filters the sellers from messages list
        private int Unclean = 0;
        private string agentNumber = "20";

        public override void Setup() // initialisation phase
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
                    step1 = true;
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
                    DutchWait = false;
                    Dutch_Auction_Announce();
                }
            }

            if (step4)
            {
                _turnsWaited = _turnsWaited + 1;
                if (_turnsWaited >= 100)
                {
                    if (Status != "sustainable")
                    {
                        if (Name == "Household:01")
                        {
                            FileCountMessages();
                            step4 = false;
                            step5 = true;
                        }

                    }
                    _turnsWaited = 0;

                }

            }
            if (step5)
            {
                _turnsWaited = _turnsWaited + 1;
                if (_turnsWaited >= 300)
                {
                    if (myEnergy == 0)
                    {
                        if (Name == "Household:01")
                        {
                            int houseHoldCount = 1;
                            int sellersCount = 0;
                            int sellersProfit = 0;
                            int buyersCount = 0;
                            int buyersSpent = 0;
                            int uncleanSent = Unclean;

                            if (Status == "buying")
                            {
                                houseHoldCount = houseHoldCount + 1;
                                buyersCount = buyersCount + 1;
                                buyersSpent = buyersSpent + myMoney;
                                uncleanSent = uncleanSent + Unclean;

                                Send($"Household:{houseHoldCount:D2}", $"infoReport {houseHoldCount} {buyersCount} {buyersSpent} {sellersCount} {sellersProfit} {uncleanSent} ");
                                step5 = false;
                                _turnsWaited = 0;
                                Console.WriteLine($"\n[Report Agent]: {Name}: Role: Buyer; Money:{myMoney};\n");

                            }
                            else if (Status == "selling")
                            {
                                houseHoldCount = houseHoldCount + 1;
                                sellersCount = sellersCount + 1;
                                sellersProfit = sellersProfit + myMoney;
                                Send($"Household:{houseHoldCount:D2}", $"infoReport {houseHoldCount} {buyersCount} {buyersSpent} {sellersCount} {sellersProfit} {uncleanSent}");
                                step5 = false;
                                _turnsWaited = 0;
                                Console.WriteLine($"\n[Report Agent]: {Name}: Role: Seller; Money:{myMoney};\n");

                            }
                            else
                            {
                                houseHoldCount = houseHoldCount + 1;
                                Send($"Household:{houseHoldCount:D2}", $"infoReport {houseHoldCount} {buyersCount} {buyersSpent} {sellersCount} {sellersProfit} {uncleanSent}");
                                step5 = false;
                                _turnsWaited = 0;
                                Console.WriteLine($"\n[Report Agent]: {Name}: Role: Sustainable; Money:0;\n");

                            }

                        }

                    }
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
                FileWriteMessages("agent");

                switch (action)
                {
                    case "inform": //on recived message with "info" tag from EnvironmentAgent
                        HandleInfromation(parameters);
                        break;


                    case "broadcast":
                        if (Status != "sustainable")
                        {
                            HandleBoradcast(parameters);
                        }
                        break;

                    case "dutchAuctionOffer": //recived by Buyer
                        // sent by Seller when they have exess energy to sell
                        if (Status == "buying")
                        {
                            HandleDuchAuctionOffer(parameters, message);
                        }
                        break;


                    case "dutchAuctionOfferAccept": // recived by Seller
                        // sent by Buyer when they are happy with the price
                        HandleDutchAuctionOfferAccept(parameters, message);
                        break;

                    case "refuseOffer": //recived by Buyer
                        // sent by Seller when he can no lognger sell, as his energy would be below 0
                        //      Console.WriteLine($"[{Name}] My energy is {myEnergy} Offer refused from: {message.Sender}");
                        myPendingEnergy = 0;
                        break;

                    case "sendEnergy": // recived by Buyer
                        // sent by Seller when a buyer accepts an offer
                        if (myEnergy == 0)
                        {
                            Send(message.Sender, "buyerOut");
                            break;
                        }
                        else
                        {
                            string[] sendEnergySplit = parameters.Split(" ");
                            int sendEnergyBuyingAmount = Int32.Parse(sendEnergySplit[0]);
                            int sendEnergyTotalPrice = Int32.Parse(sendEnergySplit[1]);
                            myEnergy = (myEnergy) + sendEnergyBuyingAmount;
                            myMoney = myMoney - sendEnergyTotalPrice;
                            Console.WriteLine($"\t \t [{Name}]  bought  from {message.Sender}  amount: {sendEnergyBuyingAmount} for {sendEnergyTotalPrice}. my energy balance is: {myEnergy} my money balance {myMoney}");

                        }
                        myPendingEnergy = 0;
                        break;

                    case "buyerOut": //recived by Seller,
                        //send by Buyer when the Buyer Energy = 0;
                        buyers.Remove(message.Sender);
                        Console.WriteLine($"Buyers number: {buyers.Count}");

                        if (buyers.Count == 0) //if no buyers tell everyone
                        {
                            //   Console.WriteLine($"Overallenergy: {OverallEnergy + myEnergy} IN NO BUYERS");
                            Broadcast("noBuyersLeft");
                        }
                        break;

                    case "sellerOut":
                        sellers.Remove(message.Sender);
                        Console.WriteLine($"Sellers number: {sellers.Count}");
                        if (sellers.Count == 0)
                        {
                            //    Console.WriteLine($"Overallenergy: {OverallEnergy + myEnergy} IN NO SELLERS");

                            Broadcast("noSellersLeft");
                        }
                        break;

                    case "noBuyersLeft": //recived by anyone
                        //sell all exess energy to UC, in duch auction only sellers do it
                        //
                        if (myEnergy > 0)
                        {
                            Console.WriteLine($" \t \t {Name} Before selling to UC, Money: {myMoney}");

                            myMoney = myMoney + (myPriceSellUT * (myEnergy));
                            myEnergy = 0;
                            Console.WriteLine($" \t \t {Name} Sold to UC, Money: {myMoney}  OverallEnergy {myEnergy + OverallEnergy}");
                            string report = Name + " My Energy: " + myEnergy + "; Status: " + Status + "; Demand: " + myDemand + "; Generation: " + myGeneration + "; Price to buy from UT: " + myPriceBuyUT + "; Price to sell to UT: " + myPriceSellUT + "; Money:" + myMoney;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                            FileWriteReport(report);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        }
                        step4 = true;

                        break;

                    case "noSellersLeft": //recived by anyone
                        //sell all exess energy to UC, in duch auction only sellers do it
                        //
                        if (myEnergy < 0)
                        {
                            Unclean = myEnergy;

                            Console.WriteLine($" \t \t {Name} Before buying from UC, Money: {myMoney}");

                            myMoney = myMoney - (myPriceBuyUT * Math.Abs(myEnergy));
                            myEnergy = 0;
                            Console.WriteLine($" \t \t {Name} Bought from UC, Money: {myMoney}; OverallEnergy {myEnergy + OverallEnergy}");

                            string report = Name + " My Energy: " + myEnergy + "; Status: " + Status + "; Demand: " + myDemand + "; Generation: " + myGeneration + "; Price to buy from UT: " + myPriceBuyUT + "; Price to sell to UT: " + myPriceSellUT + "; Money:" + myMoney;


#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                            FileWriteReport(report);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        }

                        step4 = true;
                        break;

                    case "infoReport":
                        string[] infoSplit = parameters.Split(" ");
                        int houseHoldCount = Int32.Parse(infoSplit[0]);
                        int buyersCount = Int32.Parse(infoSplit[1]);
                        int buyersSpent = Int32.Parse(infoSplit[2]);
                        int sellersCount = Int32.Parse(infoSplit[3]);
                        int sellersProfit = Int32.Parse(infoSplit[4]);
                        int uncleanSent = Int32.Parse(infoSplit[5]);

                        if (Name == $"Household:{agentNumber}") //20
                        {
                            if (Status == "buying")
                            {
                                buyersCount = buyersCount + 1;
                                buyersSpent = buyersSpent + myMoney;
                                uncleanSent = uncleanSent + Unclean;
                                Console.WriteLine($"\n[Report Agent]: {Name}: Role: Buyer; Money:{myMoney};\n");


                            }
                            if (Status == "selling")
                            {
                                sellersCount = sellersCount + 1;
                                sellersProfit = sellersProfit + myMoney;
                                Console.WriteLine($"\n[Report Agent]: {Name}: Role: Seller; Money:{myMoney};\n");


                            }
                            if (Status == "sustainable")
                            {
                                Console.WriteLine($"\n[Report Agent]: {Name}: Role: Sustainable; Money:0;\n");

                            }

                            double averageBuyerSpend = buyersSpent / buyersCount;
                            double averageSellerEarned = sellersProfit / sellersCount;



                            string report = $"{{\"Session\": \"{DateTime.Now.ToString("yyyyMMddHHmmssfff")}\", \"average buyer money\": \"-{averageBuyerSpend}\", \"average seller money\": \"{averageSellerEarned}\", \"unclean energy bought\": \"{Math.Abs(uncleanSent)}\"  }},";
                            //FileWriteSingleLineReport(report);

                            Console.WriteLine($"[Report Summary]: Average Buyer Money: {averageBuyerSpend}; Average Seller Money: {averageSellerEarned}; Unclean Energy Bought: {Math.Abs(uncleanSent)};");

                            return;

                        }

                        if (Status == "buying")
                        {
                            houseHoldCount = houseHoldCount + 1;
                            buyersCount = buyersCount + 1;
                            buyersSpent = buyersSpent + myMoney;
                            uncleanSent = uncleanSent + Unclean;

                            Send($"Household:{houseHoldCount:D2}", $"infoReport {houseHoldCount} {buyersCount} {buyersSpent} {sellersCount} {sellersProfit} {uncleanSent} ");
                            Console.WriteLine($"\n[Report Agent]: {Name}: Role: Buyer; Money:{myMoney};\n");

                            break;
                        }
                        if (Status == "selling")
                        {
                            houseHoldCount = houseHoldCount + 1;
                            sellersCount = sellersCount + 1;
                            sellersProfit = sellersProfit + myMoney;


                            Send($"Household:{houseHoldCount:D2}", $"infoReport {houseHoldCount} {buyersCount} {buyersSpent} {sellersCount} {sellersProfit} {uncleanSent}");
                            Console.WriteLine($"\n[Report Agent]: {Name}: Role: Seller; Money:{myMoney};\n");

                            break;
                        }
                        else
                        {
                            houseHoldCount = houseHoldCount + 1;
                            Send($"Household:{houseHoldCount:D2}", $"infoReport {houseHoldCount} {buyersCount} {buyersSpent} {sellersCount} {sellersProfit} {uncleanSent} ");
                            Console.WriteLine($"\n[Report Agent]: {Name}: Role: Sustainable; Money:0;\n");
                            break;
                        }
                        break;
                    case "japanAuction":
                        Console.WriteLine("Japaneese Auction Activated");

                        if (Status == "buying")
                        {
                            //      Console.WriteLine($"\n [{Name}]: im {Status} and i have {myEnergy} energy.\n");
                        }
                        break;

                    case "offer":
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
            myDemand = Int32.Parse(infoSplit[0]);
            myGeneration = Int32.Parse(infoSplit[1]);
            myPriceBuyUT = Int32.Parse(infoSplit[2]);
            myPriceSellUT = Int32.Parse(infoSplit[3]);
            myEnergy = myGeneration - myDemand;
            if (myEnergy > 0) { Status = "selling"; } else if (myEnergy == 0) { Status = "sustainable"; } else { Status = "buying"; }
            //  Console.WriteLine($"[{Name}] myEnergy Balance: {myEnergy}; Status: {Status}; \nInfo - Demand: {myDemand}; Generation: {myGeneration}; Price to buy from UT: {myPriceBuyUT}; Price to sell to UT: {myPriceSellUT}; \n");
            if (Status != "sustainable")
            {
                Broadcast($"broadcast [{Name}] {Status} {myEnergy}");
            }
            _turnsToWait = 2;
        }


        private void HandleBoradcast(string parameters) //add each message recived in a list and calculate overall energy
        {
            string[] broadcastSplit = parameters.Split(" ");
            messages.Add(parameters);
            /*string NameSender = broadcastSplit[0];
            string StatusSender = broadcastSplit[1];
            string EnergySender = broadcastSplit[2];*/
            OverallEnergy = OverallEnergy + (Int32.Parse(broadcastSplit[2]));
            _turnsToWait = 2;

        }



        private void AllMessagesRescived() //when all messeges recived step1 is complete
        {
            step1 = true;
            Console.WriteLine($"1.-------------- [{Name}]: Messages recived:{messages.Count} OverallEnergy: {OverallEnergy + myEnergy}");
            _turnsToWait = 2;

        }



        private void Auction_Decider() //two auctions - Dutch when (overall energy) is > 0 and Japaneese when (overall energy) is < 0
        {
            step2 = true;

            Extract_Players(messages);


            if ((OverallEnergy + myEnergy) > 0) // Dutch Auction
            {
                if (Status == "selling")
                {

                    //         Console.WriteLine($"\n [{Name}]: im {Status} and i have {myEnergy} energy. Im about to start Dutch Auction.\n");
                    //Thread.Sleep(1000);
                    Dutch_Auction_Announce();

                }
            }
            else if ((OverallEnergy + myEnergy) < 0) // Japaneese Auction
            {
                if (Status == "selling")
                {

                    Dutch_Auction_Announce();

                    //         Console.WriteLine($"\n [{Name}]: im {Status} and i have {myEnergy} energy. Im about to start Japaneese  Auction.\n");
                    // Japaneese_Auction_Announce();

                }
            }
            else
            {
                //        Console.WriteLine("We are self-sutainable, the energy company would starve.");


            }

        }
        private void Dutch_Auction_Announce()  // When (overall energy) is > 0
        /*     In a Dutch auction, an initial price is set that is very high, after which the price is gradually decreased.At any
moment, any bidder can claim the item.*/
        {
            if (myEnergy > 0)
            {
                SendToMany(buyers, $"dutchAuctionOffer {PriceDutch} {myEnergy}");
                _turnsToWait = messages.Count;
                DutchWait = true;
                PriceDutch--;


            }
        }

        private void Japaneese_Auction_Announce() // When(overall energy) is < 0
        /* In a Japanese auction, the initial price is zero; the price is then gradually increased.A bidder
can leave the room when the price becomes too high for her.Once there is only one bidder remaining,
that bidder wins the item, and pays the price at which the last other bidder left the room.*/
        {
            SendToMany(buyers, "japanAuction");

        }

        private void Extract_Players(List<String> messages) //for each message, get the buyer's name and strip "[" and  "]"
        {

            for (var i = 0; i < messages.Count; i++)

            {
                string[] householdSplit = messages[i].Split(" ");
                string housholdStripped = householdSplit[0].Replace("[", "").Replace("]", "");

                if (householdSplit[1] == "buying")
                {

                    buyers.Add(housholdStripped);

                }
                else if (householdSplit[1] == "selling")
                {

                    sellers.Add(housholdStripped);

                }
            }
        }

        private void Broadcast_online()
        {
            Broadcast("onine");

        }

        private void HandleDuchAuctionOffer(string parameters, Message message)
        {
            if (myEnergy == 0)
            {
                Send(message.Sender, "buyerOut");
            }
            else
            {
                string[] offerSplit = parameters.Split(" ");
                int PriceToBuy = myPriceBuyUT - 1;
                int OfferPrice = Int32.Parse(offerSplit[0]);
                int OfferEnergyAmount = Int32.Parse(offerSplit[1]);
                if (PriceToBuy >= OfferPrice) // if Buy price is better than UT price
                {
                    if ((myEnergy) + myPendingEnergy + OfferEnergyAmount <= 0) // my demand is bigger or matches the offer, im buying all the energy
                    {
                        Send(message.Sender, $"dutchAuctionOfferAccept {OfferPrice} {OfferEnergyAmount}");
                        myPendingEnergy = myPendingEnergy + OfferEnergyAmount; //we are adding that we have pending energy to be recived
                    }

                    else if ((myEnergy) + myPendingEnergy + OfferEnergyAmount > 0) //my demand is less than the offer so im buing less
                    {

                        if ((myEnergy) + myPendingEnergy < 0)
                        {
                            Send(message.Sender, $"dutchAuctionOfferAccept {OfferPrice} {Math.Abs((myEnergy) + myPendingEnergy)}");
                            myPendingEnergy = myPendingEnergy + Math.Abs((myEnergy) + myPendingEnergy);
                        }

                    }

                }

            }


        }

        public static async Task FileWriteReport(string report)
        {
            using StreamWriter file = new("C:/Users/Vincent/Desktop/AgentsOutput.txt", append: true);

            await file.WriteLineAsync(report);
        }

        private void HandleDutchAuctionOfferAccept(string parameters, Message message)
        {
            if (myEnergy > 0)
            {
                string[] buyingSplit = parameters.Split(" ");
                int buyingPrice = Int32.Parse(buyingSplit[0]);
                int buyingAmount = Int32.Parse(buyingSplit[1]);

                if (myEnergy < buyingAmount)
                {
                    buyingAmount = myEnergy;
                    //  Console.WriteLine($"[{Name}] Offer Accepted by {message.Sender}; Sending less, as i dont have enought: {buyingAmount} My energy is {myEnergy}");
                    myMoney = myMoney + (buyingPrice * buyingAmount);
                    myEnergy = myEnergy - buyingAmount;
                    Send(message.Sender, $"sendEnergy {buyingAmount} {(buyingPrice * buyingAmount)}");
                    //     Console.WriteLine($"[{Name}] I've just send to {message.Sender} This amount: {buyingAmount} for {buyingPrice * buyingAmount}. Now my energy balance is: {myEnergy} and i've earned {myMoney}");
                }
                else
                {
                    //  Console.WriteLine($"[{Name}] Offer Accepted by {message.Sender}; He wants to buy {buyingAmount} for {buyingPrice}. Total = {buyingPrice * buyingAmount}");
                    myMoney = myMoney + (buyingPrice * buyingAmount);
                    myEnergy = myEnergy - buyingAmount;
                    Send(message.Sender, $"sendEnergy {buyingAmount} {(buyingPrice * buyingAmount)}");
                    // Console.WriteLine($"[{Name}] I've just send to {message.Sender} This amount: {buyingAmount} for {buyingPrice * buyingAmount}. Now my energy balance is: {myEnergy} and i've earned {myMoney}");
                }

                if (myEnergy == 0)

                {
                    SendToMany(sellers, "sellerOut");

                }

            }
            else
            {
                // Console.WriteLine($"ERRRRR:[{Name}] My energy is below zero - {myEnergy}");
                Send(message.Sender, "refuseOffer");
            }


        }
        public static async Task FileWriteMessages(string report)
        {
            using StreamWriter file = new("C:/Users/Vincent/Desktop/AllMessagesDutch.txt", append: true);

            await file.WriteLineAsync(report);
        }
        private void FileCountMessages()
        {
            int messagesCountFile = File.ReadAllLines("C:/Users/Vincent/Desktop/AllMessagesDutch.txt").Length;
            Console.WriteLine($"\n[Report Messages]: Total Messages Exchanged: {messagesCountFile}");
            File.Delete("C:/Users/Vincent/Desktop/AllMessagesDutch.txt");

        }
    }
}

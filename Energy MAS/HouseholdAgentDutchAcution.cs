using ActressMas;

//IMPORTNAT: FORMAT TO SEND - 
//Send("Household:03", "testing");

namespace Energy_MAS
{
    public class HouseholdAgentDutchAuction : Agent
    {

        private int myGeneration, myDemand, myPriceBuyUT, myPriceSellUT, myEnergy, myMoneyEarned, myMoneySpent, OverallEnergy, _turnsToWait;
        private bool step1, step2, DutchWait = false; //flow control bools
        private string? Status; // sustainable, buying, selling. Declared after initiation phase
        private int PriceDutch = 23; //(= Max price to Buy from UT) starting price for Dutch Auction 
        private int myPendingEnergy = 0; //updates on accepted offer (pending energy to be recived), used to make descisions of future offers
        List<string> messages = new List<string>(); //gets all the messages, recives one meassage from each participant with his information
        List<string> buyers = new List<string>(); //filters the buyers from messages list

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
                }
            }
            else if (!step2)
            {
                Dutch_Auction_Announce();
            }


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
                        // sent by Seller when they are happy with the price
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
                            Send(message.Sender, "imOut");
                            break;
                        }
                        else
                        {
                            string[] sendEnergySplit = parameters.Split(" ");
                            int sendEnergyBuyingAmount = Int32.Parse(sendEnergySplit[0]);
                            int sendEnergyTotalPrice = Int32.Parse(sendEnergySplit[1]);
                            myEnergy = (myEnergy) + sendEnergyBuyingAmount;
                            myMoneySpent = myMoneySpent + sendEnergyTotalPrice;
                        }
                        //  Console.WriteLine($"\t \t [{Name}] I've just bought  from {message.Sender} This amount: {sendEnergyBuyingAmount} for {sendEnergyTotalPrice}. Now my energy balance is: {myEnergy} and i've spend {myMoneySpent}");
                        myPendingEnergy = 0;
                        break;

                    case "imOut": //recived by Seller,
                        //send by Buyer when the Buyer Energy = 0;
                        buyers.Remove(message.Sender);
                        if (buyers.Count == 0) //if no buyers tell everyone
                        {
                            Broadcast("noBuyersLeft");
                        }
                        break;

                    case "noBuyersLeft": //recived by anyone
                        //sell all exess energy to UC, in duch auction only sellers do it
                        if (myEnergy > 0)
                        {
                            myMoneyEarned = myMoneyEarned + (myPriceSellUT * myEnergy);
                            myEnergy = 0;
                            Console.WriteLine($" \t \t {Name} Sold to UC, Money: {myMoneyEarned}");
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
            if (myEnergy > 0) { Status = "selling"; } else if (myEnergy == 0) { Status = "sustainable"; Stop(); } else { Status = "buying"; }
            //  Console.WriteLine($"[{Name}] myEnergy Balance: {myEnergy}; Status: {Status}; \nInfo - Demand: {myDemand}; Generation: {myGeneration}; Price to buy from UT: {myPriceBuyUT}; Price to sell to UT: {myPriceSellUT}; \n");
            if (Status != "sustainable")
            {
                Broadcast($"broadcast [{Name}] {Status} {myEnergy}");
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
            _turnsToWait = 2;

        }



        private void AllMessagesRescived() //when all messeges recived step1 is complete
        {
            step1 = true;
            //      Console.WriteLine($"1.-------------- [{Name}]: Messages recived:{messages.Count} OverallEnergy: {OverallEnergy + myEnergy}");
            _turnsToWait = 2;

        }



        private void Dutch_Auction_Announce()  // When (overall energy) is > 0
        /*     In a Dutch auction, an initial price is set that is very high, after which the price is gradually decreased.At any
            moment, any bidder can claim the item.*/
        {
            step2 = true;

            if (Status == "selling")

            {
                Extract_Buyers(messages);

                //         Console.WriteLine($"\n [{Name}]: im {Status} and i have {myEnergy} energy. Im about to start Dutch Auction.\n");
                //Thread.Sleep(1000);
                if (myEnergy > 0)
                {
                    SendToMany(buyers, $"dutchAuctionOffer {PriceDutch} {myEnergy}");
                    _turnsToWait = messages.Count;
                    DutchWait = true;
                    PriceDutch--;


                }
            }

        }

        private void Japaneese_Auction_Announce() // When(overall energy) is < 0
        /* In a Japanese auction, the initial price is zero; the price is then gradually increased.A bidder
can leave the room when the price becomes too high for her.Once there is only one bidder remaining,
that bidder wins the item, and pays the price at which the last other bidder left the room.*/
        {
            SendToMany(buyers, "japanAuction");

        }

        private void Extract_Buyers(List<String> messages) //for each message, get the buyer's name and strip "[" and  "]"
        {

            for (var i = 0; i < messages.Count; i++)

            {
                string[] householdSplit = messages[i].Split(" ");
                if (householdSplit[1] == "buying")
                {
                    string housholdStripped = householdSplit[0].Replace("[", "").Replace("]", "");

                    buyers.Add(housholdStripped);

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
                Send(message.Sender, "imOut");
            }
            else
            {
                string[] offerSplit = parameters.Split(" ");
                int PriceToBuy = myPriceBuyUT - 1;
                int OfferPrice = Int32.Parse(offerSplit[0]);
                int OfferEnergyAmount = Int32.Parse(offerSplit[1]);
                if (PriceToBuy >= OfferPrice) // if Buy price is better than UT price
                {
                    if ((myEnergy) + myPendingEnergy + OfferEnergyAmount <= 0) // my demand is bigger or matchesthe offer, im buying all the energy
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
                    myMoneyEarned = myMoneyEarned + (buyingPrice * buyingAmount);
                    myEnergy = myEnergy - buyingAmount;
                    Send(message.Sender, $"sendEnergy {buyingAmount} {(buyingPrice * buyingAmount)}");
                    //     Console.WriteLine($"[{Name}] I've just send to {message.Sender} This amount: {buyingAmount} for {buyingPrice * buyingAmount}. Now my energy balance is: {myEnergy} and i've earned {myMoneyEarned}");
                }
                else
                {
                    //  Console.WriteLine($"[{Name}] Offer Accepted by {message.Sender}; He wants to buy {buyingAmount} for {buyingPrice}. Total = {buyingPrice * buyingAmount}");
                    myMoneyEarned = myMoneyEarned + (buyingPrice * buyingAmount);
                    myEnergy = myEnergy - buyingAmount;
                    Send(message.Sender, $"sendEnergy {buyingAmount} {(buyingPrice * buyingAmount)}");
                    // Console.WriteLine($"[{Name}] I've just send to {message.Sender} This amount: {buyingAmount} for {buyingPrice * buyingAmount}. Now my energy balance is: {myEnergy} and i've earned {myMoneyEarned}");
                }

            }
            else
            {
                // Console.WriteLine($"ERRRRR:[{Name}] My energy is below zero - {myEnergy}");
                Send(message.Sender, "refuseOffer");
            }
        }
    }
}

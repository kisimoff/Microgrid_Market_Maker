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
        private int myMoneyEarned;
        private int myMoneySpent;
        private int AmountToBuy = 0;
        private bool stopBuying = false;
        private bool finished = false;
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
                    DutchWait = false;
                    Dutch_Auction_Announce();
                }
            }
            /*else if (finished)
            {
                if (--_turnsToWait <= 0)
                {
                    Broadcast_online();
                    finished = false;
                }
            }*/



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

                    case "imOut":

                        buyers.Remove(message.Sender);
                        if (buyers.Count == 0)
                        {
                            Broadcast("noBuyersLeft");
                        }

                        break;
                    case "noBuyersLeft":
                        if (Energy != 0)
                        {
                            while (0 < Energy)
                            {
                                myMoneyEarned = myMoneyEarned + MyPriceSellUT;
                                Energy--;
                            }
                            Console.WriteLine($"Sold it to UC! {Name} Im {Status} and my energy now is: {Energy}. Money: {myMoneyEarned}");

                        }
                        else
                        {
                            Console.WriteLine($"{Energy}. Is it 0? Shoould be. Ah yea, im a {Status} and my name is {Name}. ");

                        }

                        break;
                    case "dutchAuctionOffer":

                        if (Status == "buying")
                        {
                            if (Energy == 0)
                            {
                                Send(message.Sender, "imOut");
                            }
                            string[] offerSplit = parameters.Split(" ");
                            int PriceToBuy = MyPriceBuyUT - 1;
                            int OfferPrice = Int32.Parse(offerSplit[0]);
                            int OfferEnergyAmount = Int32.Parse(offerSplit[1]);

                            if (PriceToBuy <= OfferPrice)
                            {


                                if ((Energy) + AmountToBuy + OfferEnergyAmount < 0)
                                {
                                    Send(message.Sender, $"dutchAuctionOfferAccept {OfferPrice} {OfferEnergyAmount}");
                                    AmountToBuy = AmountToBuy + OfferEnergyAmount;

                                }

                                else if ((Energy) + AmountToBuy + OfferEnergyAmount > 0)
                                {
                                    if (AmountToBuy == 0)
                                    {
                                        Send(message.Sender, $"dutchAuctionOfferAccept {OfferPrice} {Math.Abs(Energy)}");
                                        AmountToBuy = AmountToBuy + Math.Abs(Energy);
                                    }
                                    else if (AmountToBuy > 0)
                                    {               //-5 + 2
                                        int toBuy = (Energy) + AmountToBuy;
                                        if (toBuy < 0)
                                        {
                                            Send(message.Sender, $"dutchAuctionOfferAccept {OfferPrice} {Math.Abs(toBuy)}");
                                            AmountToBuy = AmountToBuy + Math.Abs(toBuy);

                                        }
                                    }

                                }
                                else if ((Energy) + AmountToBuy + OfferEnergyAmount == 0)
                                {  //in that case after transanction is complete, energy would be 0
                                    // so we stop future transactions
                                    //stopBuying = true;
                                    break;
                                }

                                // Send(message.Sender, $"dutchAuctionOfferAccept {OfferPrice} {AmountToBuy}");
                            }
                            else
                            {
                                Console.WriteLine($"[{Name}] Price Too High. My Price to buy is: {PriceToBuy} My Energy is: {Energy} My Amount to buy is: {AmountToBuy} My stop buying status is {stopBuying}");
                                break;
                            }




                            /*  if (OfferEnergyAmount > Energy)
                              {
                                  OfferEnergyAmount = OfferEnergyAmount - Energy;
                              }*/


                            //Console.WriteLine($"\n [{Name}]: im {Status} and i have {Energy} energy.\n");
                        }
                        break;

                    case "refuseOffer":
                        Console.WriteLine($"[{Name}] My energy is {Energy} Offer refused from: {message.Sender}");
                        AmountToBuy = 0;
                        break;


                    case "dutchAuctionOfferAccept":
                        string[] buyingSplit = parameters.Split(" ");
                        int buyingPrice = Int32.Parse(buyingSplit[0]);
                        int buyingAmount = Int32.Parse(buyingSplit[1]);
                        Console.WriteLine($"[{Name}] Offer Accepted by {message.Sender}; He is buying {buyingAmount} for {buyingPrice}. Total = {buyingPrice * buyingAmount}");
                        if (Energy <= 0)
                        {
                            Console.WriteLine($"[{Name}] My energy is {Energy} Im refusing the offer to {message.Sender}");
                            Send(message.Sender, "refuseOffer");
                            break;
                        }

                        myMoneyEarned = myMoneyEarned + (buyingPrice * buyingAmount);
                        Energy = Energy - buyingAmount;
                        Send(message.Sender, $"sendEnergy {buyingAmount} {(buyingPrice * buyingAmount)}");
                        Console.WriteLine($"[{Name}] I've just send to {message.Sender} This amount: {buyingAmount} for {buyingPrice * buyingAmount}. Now my energy balance is: {Energy} and i've earned {myMoneyEarned}");
                        if (Energy == 0)
                        {
                            Console.WriteLine($"[{Name}] My part is done! My energy balance is {Energy} and i've Earned {myMoneyEarned}");
                            //Stop();
                        }
                        break;


                    case "sendEnergy":
                        string[] sendEnergySplit = parameters.Split(" ");
                        int sendEnergyBuyingAmount = Int32.Parse(sendEnergySplit[0]);
                        int sendEnergyTotalPrice = Int32.Parse(sendEnergySplit[1]);
                        Energy = (Energy) + sendEnergyBuyingAmount;
                        myMoneySpent = myMoneySpent + sendEnergyTotalPrice;
                        Console.WriteLine($"[{Name}] I've just bought  from {message.Sender} This amount: {sendEnergyBuyingAmount} for {sendEnergyTotalPrice}. Now my energy balance is: {Energy} and i've spend {myMoneySpent}");
                        if (Energy == 0)
                        {
                            Console.WriteLine($"[{Name}] My part is done! My energy balance is {Energy} and i've spent {myMoneySpent}");
                            Send(message.Sender, "imOut");

                            // Stop();

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
                if (Status == "selling")
                {
                    Console.WriteLine($"\n [{Name}]: im {Status} and i have {Energy} energy. Im about to start Dutch Auction.\n");
                    //Thread.Sleep(1000);
                    Dutch_Auction_Announce();

                }
            }
            else if ((OverallEnergy + Energy) < 0) // Japaneese Auction
            {
                if (Status == "selling")
                {
                    Console.WriteLine($"\n [{Name}]: im {Status} and i have {Energy} energy. Im about to start Japaneese  Auction.\n");
                    Japaneese_Auction_Announce();

                }
            }
            else
            {
                Console.WriteLine("We are self-sutainable, the energy company would starve.");


            }

        }
        private void Dutch_Auction_Announce()  // When (overall energy) is > 0
        /*     In a Dutch auction, an initial price is set that is very high, after which the price is gradually decreased.At any
moment, any bidder can claim the item.*/
        {
            if (Energy > 0)
            {
                SendToMany(buyers, $"dutchAuctionOffer {PriceDutch} {Energy}");
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

        private void Extract_Buyers(List<String> messages) //for each message, get the buyer's name and strip "[" and  "]"
        {
            // List<string> players = new List<string>();
            /*Console.WriteLine("Messages count:" + messages.Count);
            foreach (string household in messages)
            {
                string[] householdSplit = household.Split(" ");
                if (householdSplit[1] == "buying")
                {
                     Console.WriteLine($" {Name} : Buyer:" + householdSplit[1] + householdSplit[0]);

                    // string housholdStripped = householdSplit[0].Replace("[", "").Replace("]", "");
                    //buyers.Add(housholdStripped);
                    //Console.WriteLine("Buyer Added" + housholdStripped);
                }
            }*/

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


    }


}

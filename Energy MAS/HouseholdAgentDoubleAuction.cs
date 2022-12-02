using ActressMas;

//IMPORTNAT: FORMAT TO SEND - 
//Send("Household:03", "testing");

namespace Energy_MAS
{

    public class HouseholdAgentDoubleAuction : Agent
    {

        private int buyPrice, _turnsWaited, sellPrice, myGeneration, myDemand, agentsCount, myPriceBuyUT, myPriceSellUT, myEnergy, myMoneyEarned, myMoneySpent, OverallEnergy, _turnsToWait;
        private bool step1, step2, DutchWait = false; //flow control bools
        private string? Status; // sustainable, buying, selling. Declared after initiation phase
        private int PriceDutch = 23; //(= Max price to Buy from UT) starting price for Dutch Auction 
        private int myPendingEnergy = 0; //updates on accepted offer (pending energy to be recived), used to make descisions of future offers
        List<string> messages = new List<string>(); //gets all the messages, recives one meassage from each participant with his information
        List<string> buyers = new List<string>(); //filters the buyers from messages list
        private double halfPrice = 0;
        private double coefLess = 0.4;
        private double coefMore = 0.6;
        private bool Once = true;
        private int Unclean = 0;
        private bool showUnclean = false; //change to true to see the unclean energy bought
        private string agentNumber = "20";
        public override void Setup() // initialisation phase
        {

            Send("environment", "start");
            _turnsToWait = 2;


        }

        public override void ActDefault() //step control, to initiate auction after everyone recived the first broadcast. 
        {

            if (step1)
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
                                buyersSpent = buyersSpent + myMoneySpent;
                                uncleanSent = uncleanSent + Unclean;

                                Send($"Household:{houseHoldCount:D2}", $"infoReport {houseHoldCount} {buyersCount} {buyersSpent} {sellersCount} {sellersProfit} {uncleanSent} {OverallEnergy}");
                                step1 = false;
                                _turnsWaited = 0;
                                Console.WriteLine($"\n[Report Agent]: {Name}: Role: Buyer; Money:-{myMoneySpent};\n");

                            }
                            else if (Status == "selling")
                            {
                                houseHoldCount = houseHoldCount + 1;
                                sellersCount = sellersCount + 1;
                                sellersProfit = sellersProfit + myMoneyEarned;
                                Send($"Household:{houseHoldCount:D2}", $"infoReport {houseHoldCount} {buyersCount} {buyersSpent} {sellersCount} {sellersProfit} {uncleanSent} {OverallEnergy}");
                                step1 = false;
                                _turnsWaited = 0;
                                Console.WriteLine($"\n[Report Agent]: {Name}: Role: Seller; Money:{myMoneyEarned};\n");

                            }
                            else
                            {
                                houseHoldCount = houseHoldCount + 1;

                                Send($"Household:{houseHoldCount:D2}", $"infoReport {houseHoldCount} {buyersCount} {buyersSpent} {sellersCount} {sellersProfit} {uncleanSent} {OverallEnergy}");
                                step1 = false;
                                _turnsWaited = 0;
                                Console.WriteLine($"\n[Report Agent]: {Name}: Role: Sustainable; Money:0;\n");

                            }

                        }

                    }
                    _turnsWaited = 0;
                }
            }



            if (step2)
            {
                _turnsWaited = _turnsWaited + 1;
                if (_turnsWaited >= 300)
                {
                    if (myEnergy == 0)
                    {
                        string report;

                        if (Status == "buying")
                        {
                            report = $"{{\"name\": \"{Name}\", \"status\": \"{Status}\", \"money\": \"-{myMoneySpent}\"}},";
                            Console.WriteLine(report);



                            // FileWriteAllAgentsReport(report);
                            step1 = false;

                        }
                        else if (Status == "selling")
                        {
                            report = $"{{\"name\": \"{Name}\", \"status\": \"{Status}\", \"money\": \"{myMoneyEarned}\"}},";
                            Console.WriteLine(report);
                            // FileWriteAllAgentsReport(report);
                            step1 = false;


                        }
                        else
                        {
                            report = $"{{\"name\": \"{Name}\", \"status\": \"{Status}\", \"money\": \"{myMoneyEarned}\"}},";
                            // FileWriteAllAgentsReport(report);
                            Console.WriteLine(report);
                            step1 = false;


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
                    case "calculatePriceBuy":
                        CalculatePriceBuy(parameters);
                        Send("central", $"priceBuy [{Name}] {myEnergy} {buyPrice}");
                        break;
                    case "calculatePriceSell":
                        CalculatePriceSell(parameters);
                        Send("central", $"priceSell [{Name}] {myEnergy} {sellPrice}");
                        break;
                    case "requestSendEnergy":
                        HandleRequestSendEnergy(parameters);
                        break;
                    case "sendEnergy":
                        HandleSendEnergy(parameters);
                        break;
                    case "noMatchBuyer":
                        // Console.WriteLine($"[OLD PRICE Buyer {Name}]:{buyPrice}");
                        HandleNoMatchBuyer(parameters);
                        // Console.WriteLine($"[NEW PRICE Buyer {Name}]:{buyPrice}");
                        Send("central", $"priceBuy [{Name}] {myEnergy} {buyPrice}");
                        break;
                    case "noMatchSeller":
                        // Console.WriteLine($"[OLD PRICE Seller {Name}]:{sellPrice}");
                        HandleNoMatchSeller(parameters);
                        // Console.WriteLine($"[NEW PRICE Seller {Name}]:{sellPrice}");
                        Send("central", $"priceSell [{Name}] {myEnergy} {sellPrice}");
                        break;
                    case "noBuyersLeft":
                        if (myEnergy > 0)
                        {
                            // Console.WriteLine($" \t \t {Name} Before selling to UC, Money: {myMoney}");

                            myMoneyEarned = myMoneyEarned + (myPriceSellUT * (myEnergy));
                            myEnergy = 0;
                            Console.WriteLine($" \t \t {Name} Sold to UC, Money: {myMoneyEarned}");
                            // string report = Name + " My Energy: " + myEnergy + "; Status: " + Status + "; Demand: " + myDemand + "; Generation: " + myGeneration + "; Price to buy from UT: " + myPriceBuyUT + "; Price to sell to UT: " + myPriceSellUT + "; Money:" + myMoney;
                        }
                        break;
                    case "noSellersLeft":
                        if (myEnergy < 0)
                        {
                            Unclean = myEnergy;
                            Console.WriteLine($" \t \t {Name} Before buying from UC, Money: {myMoneySpent}");
                            myMoneySpent = myMoneySpent + (myPriceBuyUT * Math.Abs(myEnergy));

                            myEnergy = 0;
                            Console.WriteLine($" \t \t {Name} Bought from UC, Money: {myMoneySpent}");

                            // string report = Name + " My Energy: " + myEnergy + "; Status: " + Status + "; Demand: " + myDemand + "; Generation: " + myGeneration + "; Price to buy from UT: " + myPriceBuyUT + "; Price to sell to UT: " + myPriceSellUT + "; Money:" + myMoney;


                        }
                        break;



                    case "infoReport":
                        string[] infoSplit = parameters.Split(" ");
                        int houseHoldCount = Int32.Parse(infoSplit[0]);
                        int buyersCount = Int32.Parse(infoSplit[1]);
                        int buyersSpent = Int32.Parse(infoSplit[2]);
                        int sellersCount = Int32.Parse(infoSplit[3]);
                        int sellersProfit = Int32.Parse(infoSplit[4]);
                        int uncleanSent = Int32.Parse(infoSplit[5]);
                        int OverallEnergyCheck = Int32.Parse(infoSplit[6]);

                        if (Name == $"Household:{agentNumber}") //20
                        {
                            if (Status == "buying")
                            {
                                buyersCount = buyersCount + 1;
                                buyersSpent = buyersSpent + myMoneySpent;
                                uncleanSent = uncleanSent + Unclean;
                                OverallEnergyCheck = OverallEnergy;
                                Console.WriteLine($"\n[Report Agent]: {Name}: Role: Buyer; Money:-{myMoneySpent};\n");


                            }
                            if (Status == "selling")
                            {
                                sellersCount = sellersCount + 1;
                                sellersProfit = sellersProfit + myMoneyEarned;
                                OverallEnergyCheck = OverallEnergy;
                                Console.WriteLine($"\n[Report Agent]: {Name}: Role: Seller; Money:{myMoneyEarned};\n");


                            }
                            if (Status == "sustainable")
                            {
                                Console.WriteLine($"\n[Report Agent]: {Name}: Role: Sustainable; Money:0;\n");

                            }

                            double averageBuyerSpend = buyersSpent / buyersCount;
                            double averageSellerEarned = sellersProfit / sellersCount;



                            string report = $"{{\"Session\": \"{DateTime.Now.ToString("yyyyMMddHHmmssfff")}\", \"average buyer money\": \"-{averageBuyerSpend}\", \"average seller money\": \"{averageSellerEarned}\", \"overall energy\": \"{OverallEnergyCheck}\", \"unclean energy bought\": \"{Math.Abs(uncleanSent)}\"  }},";
                            FileWriteSingleLineReport(report);

                            Console.WriteLine($"[Report Summary]: Average Buyer Money: -{averageBuyerSpend}; Average Seller Money: {averageSellerEarned}; Overall Energy: {OverallEnergyCheck}; Unclean Energy Bought: {Math.Abs(uncleanSent)};");

                            return;

                        }

                        if (Status == "buying")
                        {
                            houseHoldCount = houseHoldCount + 1;
                            buyersCount = buyersCount + 1;
                            buyersSpent = buyersSpent + myMoneySpent;
                            uncleanSent = uncleanSent + Unclean;
                            OverallEnergyCheck = OverallEnergy;

                            Send($"Household:{houseHoldCount:D2}", $"infoReport {houseHoldCount} {buyersCount} {buyersSpent} {sellersCount} {sellersProfit} {uncleanSent} {OverallEnergyCheck}");
                            Console.WriteLine($"\n[Report Agent]: {Name}: Role: Buyer; Money:-{myMoneySpent};\n");

                            break;
                        }
                        if (Status == "selling")
                        {
                            houseHoldCount = houseHoldCount + 1;
                            sellersCount = sellersCount + 1;
                            sellersProfit = sellersProfit + myMoneyEarned;
                            OverallEnergyCheck = OverallEnergy;

                            Send($"Household:{houseHoldCount:D2}", $"infoReport {houseHoldCount} {buyersCount} {buyersSpent} {sellersCount} {sellersProfit} {uncleanSent} {OverallEnergyCheck}");
                            Console.WriteLine($"\n[Report Agent]: {Name}: Role: Seller; Money:{myMoneyEarned};\n");

                            break;
                        }
                        else
                        {
                            houseHoldCount = houseHoldCount + 1;
                            Send($"Household:{houseHoldCount:D2}", $"infoReport {houseHoldCount} {buyersCount} {buyersSpent} {sellersCount} {sellersProfit} {uncleanSent} {OverallEnergyCheck}");
                            Console.WriteLine($"\n[Report Agent]: {Name}: Role: Sustainable; Money:0;\n");
                            break;
                        }
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
        private void HandleNoMatchBuyer(string parameters)
        {
            // Console.WriteLine($"Im {Status} recived NOMATCH with: {parameters}");


            if (OverallEnergy > 0)// advantage
            {
                if (halfPrice == 0)
                {
                    halfPrice = (double)buyPrice + coefLess;

                }
                else
                {
                    halfPrice = halfPrice + coefLess;
                }

                buyPrice = Convert.ToInt32(halfPrice);

            }
            else // disadvantage
            {
                if (halfPrice == 0)
                {
                    halfPrice = (double)buyPrice + coefMore;

                }
                else
                {
                    halfPrice = halfPrice + coefMore;
                }

                buyPrice = Convert.ToInt32(halfPrice);

            }
        }

        private void HandleNoMatchSeller(string parameters)
        {
            // Console.WriteLine($"Im {Status} recived NOMATCH with: {parameters}");

            if (OverallEnergy > 0)// disadvantage
            {
                if (halfPrice == 0)
                {
                    halfPrice = (double)sellPrice - coefMore;

                }
                else
                {
                    halfPrice = halfPrice - coefMore;
                }

                sellPrice = Convert.ToInt32(halfPrice);

            }
            else // advantage
            {
                if (halfPrice == 0)
                {
                    halfPrice = (double)sellPrice - coefLess;

                }
                else
                {
                    halfPrice = halfPrice - coefLess;
                }

                sellPrice = Convert.ToInt32(halfPrice);

            }
        }
        private void HandleRequestSendEnergy(string parameters)
        {
            string[] buyersToSend = parameters.Split(";");
            int loops = buyersToSend.Length - 1; //last element is blank space
            for (int i = 0; i < loops; i++)
            {
                string[] infoSplit = buyersToSend[i].Split(","); //0-name 1-price 2-amount
                Send(infoSplit[0], $"sendEnergy {infoSplit[1]} {infoSplit[2]}");
                myMoneyEarned = myMoneyEarned + (Int32.Parse(infoSplit[1]) * Int32.Parse(infoSplit[2]));
                myEnergy = myEnergy - Int32.Parse(infoSplit[2]);
            }
            // Console.WriteLine($"Seller: {Name} SENT  Earned:{myMoneyEarned}, energy:{myEnergy}");

        }

        private void HandleSendEnergy(string parameters)
        {
            string[] infoSplit = parameters.Split(" ");
            int recivePrice = Int32.Parse(infoSplit[0]);
            int reciveAmount = Int32.Parse(infoSplit[1]);
            myMoneySpent = myMoneySpent + (recivePrice * reciveAmount);
            myEnergy = myEnergy + reciveAmount;
            //Console.WriteLine($"RECIVED ENERGY Buyer:{Name} Spend:{myMoneySpent}, energy:{myEnergy}");

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
                Send("central", $"centralInform [{Name}] {Status} {myEnergy}");

            }
            _turnsToWait = 100;
            step1 = true;
            //step2 = true;

        }




        private void CalculatePriceBuy(string parameters) //split the parameters, calculate energy balance and broadcast its name status and energy
        {
            string[] messageSplit = parameters.Split(" ");
            OverallEnergy = Int32.Parse(messageSplit[0]);
            double midPriceFormula = ((double)myPriceBuyUT - (double)myPriceBuyUT / 3);
            double tempPrice = midPriceFormula + (midPriceFormula * (((double)OverallEnergy * -1) / 100));
            int tempPriceInt = Convert.ToInt32(tempPrice);
            if (tempPriceInt > myPriceBuyUT)
            {
                buyPrice = myPriceBuyUT;
            }
            else if (tempPriceInt < 2)
            {
                buyPrice = 2;
            }
            else
            {
                buyPrice = tempPriceInt;
            }



            string report = $"Buyer {Name}; BuyUC: {myPriceBuyUT}; Buy: {buyPrice} ; Mid: {midPriceFormula} OE: {OverallEnergy}; ";
            FileWriteReport(report);
        }


        private void CalculatePriceSell(string parameters) //split the parameters, calculate energy balance and broadcast its name status and energy
        {
            OverallEnergy = Int32.Parse(parameters);
            double midPriceFormula = myPriceSellUT + 10;
            double tempPrice = midPriceFormula + (midPriceFormula * (((double)OverallEnergy * -1) / 100));
            int tempPriceInt = Convert.ToInt32(tempPrice);

            int MaxSellPrice = 22;

            if (tempPriceInt > MaxSellPrice)
            {
                sellPrice = MaxSellPrice;
            }
            else if (tempPriceInt < myPriceSellUT)
            {
                sellPrice = myPriceSellUT;
            }
            else
            {
                sellPrice = tempPriceInt;
            }


            string report = $"Seller {Name}; SellUC: {myPriceSellUT}; Sell: {sellPrice}; MidPrice: {midPriceFormula}; OE: {OverallEnergy}; ";
            FileWriteReport2(report);
            //CalculatePriceSellerFormula1();
            double aggressivnesCoeficient = 1;


            // Console.WriteLine($"[{Name}] My price to sell is {sellPrice} Sell to UC: {myPriceSellUT}");

            /*
                        string report = $"Seller {Name}; SellUC${myPriceSellUT}; Sell{sellPrice};  Temp${tempSellPrice}; MidPrice${midPriceFormula}; OE:{OverallEnergy}; Agents:{agentsCount}; Coef:{Convert.ToInt32(coeficient)}%";
                        FileWriteReport2(report)*/
            //  string report;
            //    FileWriteReport(report);
        }



        public static async Task FileWriteReport(string report)
        {
            using StreamWriter file = new("C:/Users/Vincent/Desktop/Buyers.txt", append: true);

            await file.WriteLineAsync(report);
        }
        public static async Task FileWriteReport2(string report)
        {
            using StreamWriter file = new("C:/Users/Vincent/Desktop/Sellers.txt", append: true);

            await file.WriteLineAsync(report);
        }
        public static async Task FileWriteMessages(string report)
        {
            using StreamWriter file = new("C:/Users/Vincent/Desktop/AllMessages.txt", append: true);

            await file.WriteLineAsync(report);
        }
        public static async Task FileWriteAllAgentsReport(string report)
        {

            using StreamWriter file = new($"C:/Users/Vincent/Desktop/AllAgentsReport.txt", append: true);
            await file.WriteLineAsync(report);
        }
        public static async Task FileWriteSingleLineReport(string report)
        {

            using StreamWriter file = new($"C:/Users/Vincent/Desktop/SingleLineReport.txt", append: true);
            await file.WriteLineAsync(report);
        }


    }
}

//May be useful for future development, gave up on the idea, because of the price fluctuation. Can be useful if the ratio Agents/OverallPrice is more not that dyanamic
/*private int PriceFormula1(double aggressivnesCoeficient, double midPrice)
        {
            double tempPrice = ((double)OverallEnergy / (double)agentsCount);
            tempPrice = tempPrice * aggressivnesCoeficient;
            Console.WriteLine($"BEFORE: {Name}: Temprice: {tempPrice}; OE:{OverallEnergy}");

            if (OverallEnergy < 0)
            {
                tempPrice = tempPrice * -1;
                tempPrice = midPrice * tempPrice;
                tempPrice = tempPrice + midPrice;

            }
            else if (OverallEnergy > 0)
            {

                tempPrice = midPrice * tempPrice;
                tempPrice = tempPrice + midPrice;
            }
            else if (OverallEnergy == 0)
            {
                tempPrice = midPrice;
            }
            Console.WriteLine($"AFTER {Name}: Temprice: {tempPrice} ");
            int tempPriceInt = Convert.ToInt32(tempPrice);
            Console.WriteLine($"AFTER INT {Name}: Temprice: {tempPriceInt} COEF: {((double)OverallEnergy / (double)agentsCount)}");

            return tempPriceInt;
        }*/

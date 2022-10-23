using ActressMas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System;
using static System.Net.Mime.MediaTypeNames;


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

        public override void Setup()
        {
            Send("environment", "start");
        }

        public override void Act(Message message)
        {
            try
            {
                Console.WriteLine($"\t{message.Format()}");
                message.Parse(out string action, out string parameters);

                switch (action)
                {
                    case "inform": //activates when Setup: send start to env
                        HandleInfromation(parameters);
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
            MyPriceSellUT  = Int32.Parse(infoSplit[3]);
            Energy = MyGeneraton - MyDemand;
            if (Energy > 0) { Status = "selling"; } else if (Energy == 0) { Status = "sustainable"; } else { Status = "buying"; }
            Console.WriteLine($"[{Name}] Energy Balance: {Energy}; Status: {Status}; \nInfo - Demand: {MyDemand}; Generation: {MyGeneraton}; Price to buy from UT: {MyPriceBuyUT}; Price to sell to UT: {MyPriceSellUT}; \n");
            /*
                1. Send(status and energy balance) if not sustainable
                2. Implement Auction
                3. Fist part done.
             */
        }

    }


}

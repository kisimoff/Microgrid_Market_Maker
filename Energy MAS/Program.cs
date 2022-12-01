/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: Vickrey auction using the ActressMas framework           *
 *  Copyright:   (c) 2018, Florin Leon                                    *
 *                                                                        *
 *  This program is free software; you can redistribute it and/or modify  *
 *  it under the terms of the GNU General Public License as published by  *
 *  the Free Software Foundation. This program is distributed in the      *
 *  hope that it will be useful, but WITHOUT ANY WARRANTY; without even   *
 *  the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR   *
 *  PURPOSE. See the GNU General Public License for more details.         *
 *                                                                        *
 **************************************************************************/

using ActressMas;

namespace Energy_MAS
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var env = new EnvironmentMas();

            var rand = new Random();

            for (int i = 1; i <= Settings.NoHouseholds; i++)
            {
                if (Settings.isDutchAuction)
                {
                    var householdAgent = new HouseholdAgentDutch();
                    // var householdAgent = new HouseholdAgentDutchAuction();
                    env.Add(householdAgent, $"Household:{i:D2}");
                }
                else if (!Settings.isDutchAuction)
                {
                    //double auction
                    var householdAgent = new HouseholdAgentDoubleAuction();
                    // var householdAgent = new HouseholdAgentDutchAuction();
                    env.Add(householdAgent, $"Household:{i:D2}");
                }



            }

            var environmentAgent = new EnvironmentAgent();
            env.Add(environmentAgent, "environment");
            if (!Settings.isDutchAuction)
            {
                var centralAgent = new CentralAgent();
                // var householdAgent = new HouseholdAgentDutchAuction();
                env.Add(centralAgent, "central");
            }
            env.Start();
        }
    }
}
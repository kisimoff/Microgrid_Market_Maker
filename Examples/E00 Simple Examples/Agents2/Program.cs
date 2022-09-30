﻿/**************************************************************************
 *                                                                        *
 *  Description: Simple example of using the ActressMas framework         *
 *  Website:     https://github.com/florinleon/ActressMas                 *
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
using System;
using System.Threading;

namespace Agents2
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var env = new EnvironmentMas(noTurns: 100, parallel: false);
            var a1 = new MyAgent1(); env.Add(a1, "agent1");
            var a2 = new MyAgent2(); env.Add(a2, "agent2");
            env.Start();

            Console.ReadLine();
        }
    }

    public class MyAgent1 : Agent
    {
        public override void Setup()
        {
            Send("agent2", "start from 1");

            for (int i = 0; i < 10; i++)
            {
                Send("agent2", $"in setup for: {i}");
                Thread.Sleep(100);
            }
        }

        public override void Act(Message m)
        {
            Console.WriteLine(m.Format());

            for (int i = 0; i < 10; i++)
            {
                Send("agent2", $"in act for: {i}");
                Thread.Sleep(100);
            }
        }
    }

    public class MyAgent2 : Agent
    {
        public override void Setup()
        {
            Send("agent1", "start from 2");
        }

        public override void Act(Message m)
        {
            Console.WriteLine(m.Format());
        }
    }
}
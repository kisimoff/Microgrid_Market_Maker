/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: The reactive architecture using the ActressMas framework *
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
using System.Collections.Generic;

namespace Reactive
{
    public class ExplorerAgent : Agent
    {
        private int _x, _y;
        private State _state;
        private string _resourceCarried;
        private int _size;
        List<int> historyX = new List<int>();
        List<int> historyY = new List<int>();
        public bool edge_complete = false;
        public string closest_edge;
        int loopNumber = 1;
        int sideNumber = 1;

        private static Random _rand = new Random();

        private enum State { Free, Carrying };

        public override void Setup()
        {
            Console.WriteLine($"Starting {Name}");

            _size = Environment.Memory["Size"];

            _x = _size / 2;
            _y = _size / 2;
            _state = State.Free;

            while (IsAtBase())
            {
                _x = _rand.Next(_size);
                _y = _rand.Next(_size);
            }
            closest_edge = EdgeDetection();
            Send("planet", $"position {_x} {_y}");
        }

        private bool IsAtBase()
        {
            return (_x == _size / 2 && _y == _size / 2); // the position of the base
        }

        public override void Act(Message message)
        {
            try
            {
                Console.WriteLine($"\t{message.Format()}");
                message.Parse(out string action, out List<string> parameters);

                if (action == "block")
                {
                    // R1. If you detect an obstacle, then change direction
                    MoveRandomly();
                    Send("planet", $"change {_x} {_y}");
                }
                else if (action == "move" && _state == State.Carrying && IsAtBase())
                {
                    // R2. If carrying samples and at the base, then unload samples
                    _state = State.Free;
                    Send("planet", $"unload {_resourceCarried}");
                }
                else if (action == "move" && _state == State.Carrying && !IsAtBase())
                {
                    // R3. If carrying samples and not at the base, then travel up gradient
                    MoveToBase();
                    Send("planet", $"carry {_x} {_y}");

                }
                else if (action == "rock")
                {
                    // R4. If you detect a sample, then pick sample up
                    _state = State.Carrying;
                    _resourceCarried = parameters[0];
                    Send("planet", $"pick-up {_resourceCarried}");
                }
                else if (action == "move")
                {
                    // R5. If (true), then move randomly
                    MoveRandomly();
                    Send("planet", $"change {_x} {_y}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //Console.WriteLine(ex.ToString()); // for debugging
            }
        }




        public void loopCounter()
        {



            sideNumber++;
                if (sideNumber % 4 == 0)
            {
                loopNumber++;
                Console.WriteLine("LOOP NUMBER INCREASEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE:");


            }
            Console.WriteLine("SIDE NUMBER:");

            Console.WriteLine(sideNumber);

            Console.WriteLine("LOOP NUMBER:");

            Console.WriteLine(loopNumber);

        }


        public string EdgeDetection()
        {
            string Edge = "";

            int res_lr = Math.Abs(_size) - Math.Abs(_x);
            int res_tb = Math.Abs(_y) - Math.Abs(_size);

            if (!edge_complete) {
            if (Math.Abs(res_tb) > (_size / 2))
            {
                Console.WriteLine("Closer to the top");
                Edge = Edge + "T";

            }
            else
            {
                Console.WriteLine("Closer to the bot");
                Edge = Edge + "B";

            }

            if (Math.Abs(res_lr) < (_size / 2))
            {
                Console.WriteLine("Closer to the right");
                Edge = Edge + "R";
            }
            else
            {
                Console.WriteLine("Closer to the left");
                Edge = Edge + "L";

            }
            }

            if (_x == loopNumber - 1 && _y == loopNumber - 1)
            {
                Console.WriteLine("Top Left Reached!");

                loopCounter();

                Edge = "TLR";
                closest_edge = Edge;
                edge_complete = true;
            }

            else if (_x == _size - loopNumber && _y == loopNumber - 1)
            {
                Console.WriteLine("Top Right Reached!");
                loopCounter();

                Edge = "TRR";
                closest_edge = Edge;
                edge_complete = true;

            }
            else if (_x == loopNumber -1 && _y == _size - loopNumber)
            {
                Console.WriteLine("Top Right Reached!");
                loopCounter();



                Edge = "BLR";
                closest_edge = Edge;
                edge_complete = true;

            }
            else if (_x == _size - loopNumber && _y == _size - loopNumber)
            {
                Console.WriteLine("Top Right Reached!");
                loopCounter();


                Edge = "BRR";
                closest_edge = Edge;
                edge_complete = true;

            }

            return Edge;

        }

        private void MoveRandomly2()
        //0 - 7 = 7                <                  11 - 7 = 4

        {


        //size = 11

        a:
            int d = _rand.Next(4);
            int validMove = 0;
            switch (d)
            {
                case 0:
                    Console.WriteLine(_x);
                    Console.WriteLine(_y);
                    Console.WriteLine(d);

                    if (_x > 0) _x--;
                    { break; }

                case 1:
                    Console.WriteLine(_x);
                    Console.WriteLine(_y);
                    Console.WriteLine(d);

                    if (_x < _size - 1) _x++;
                    { break; }


                case 2:
                    Console.WriteLine(_x);
                    Console.WriteLine(_y);
                    Console.WriteLine(d);

                    if (_y > 0) _y--;
                    { break; }

                case 3:
                    Console.WriteLine(_x);
                    Console.WriteLine(_y);
                    Console.WriteLine(d);

                    if (_y < _size - 1) _y++;
                    { break; }

            }
        }



        private void MoveRandomly()
        {


            Console.WriteLine(EdgeDetection());
            Console.WriteLine(loopNumber);

                switch (closest_edge)
                {
                    case "TL": //00

                        if (_x > 0 && _y > 0)
                        {
                            _x--;
                            _y--; break;
                        }
                        else if (_x > 0) { _x--; break; }
                        else if (_y > 0) { _y--; break; }
                        else
                        {
                            break;
                        }

                    case "TR": //11 0

                        if (_x < _size - 1 && _y > 0)
                        {
                            _x++;
                            _y--; break;
                        }
                        else if (_x < _size - 1) { _x++; break; }
                        else if (_y > 0) { _y--; break; }
                        else
                        {
                            break;
                        }

                    case "BL": // 0 11
                        if (_x > 0 && _y < _size - 1)
                        {
                            _x--;
                            _y++; break;
                        }
                        else if (_x > 0) { _x--; break; }
                        else if (_y < _size - 1) { _y++; break; }
                        else
                        {
                            break;
                        }


                    case "BR": // 11 , 11
                        if (_x < _size - 1 && _y < _size - 1)
                        {
                            _x++;
                            _y++; break;
                        }
                        else if (_x < _size - 1) { _x++; break; }
                        else if (_y < _size - 1) { _y++; break; }
                        else
                        {
                            break;
                        }

                case "TLR":
                    if (_x == _size - loopNumber)
                    {
                        closest_edge = "TRR";
                    }
                        else { _x++; }
                        

                    break;

                case "TRR":
                    if  (_y == _size - loopNumber)
                    {
                        closest_edge = "BRR";
                    }
                    else { _y++; }
                    break;
                
                
                case "BRR":
                    if (_x == loopNumber - 1)
                    {
                        closest_edge = "BLR";
                    }
                    else { _x--; }
                    break;

                case "BLR":
                    if (_y == loopNumber - 1)
                    {
                        closest_edge = "TLR";
                    }
                    else { _y--; }
                   
                    break;
            }
   




            




        }








        private int CheckHistory(int x, int y)
        {
            string Move = x + "" + y;
            int MoveInt = Int32.Parse(Move);
            if (historyX.Contains(MoveInt) == true)
            {
                return 1;
            }
            else
            {
                historyX.Add(MoveInt);
                return 0;
            }

        }
        private void MoveToBase()
        {
            int dx = _x - _size / 2;
            int dy = _y - _size / 2;

            if (Math.Abs(dx) > Math.Abs(dy))
                _x -= Math.Sign(dx);
            else
                _y -= Math.Sign(dy);
        }
    }
}
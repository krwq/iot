﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Tm1637;
using System;
using System.Threading;

namespace Tm1637Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Tm1637!");
            Tm1637 tm1637 = new Tm1637(20, 21);
            tm1637.Brightness = 7;
            tm1637.ScreenOn = true;
            tm1637.ClearDisplay();
            // Displays 4 Characters
            // If you have a 4 display, all 4 will be displayed as well as on a 6
            Character[] toDisplay = new Character[4] {
                Character.Digit4,
                Character.Digit2 | Character.Dot,
                Character.Digit3,
                Character.Digit8
            };
            tm1637.Display(toDisplay);
            Thread.Sleep(3000);

            // Changing order of the segments
            tm1637.SegmentOrder = new byte[] { 2, 1, 0, 5, 4, 3 };

            // Displays couple of raw data
            Character[] rawData = new Character[6] {
                // All led on including the dot
                (Character)0b1111_1111, 
                // All led off
                (Character)0b0000_0000,
                // top blanck, right on, turning like this including dot
                (Character)0b1010_1010,
                // top on, right black, turning like this no dot
                (Character)0b0101_0101,
                // half one half off
                Character.SegmentTop | Character.SegmentTopRight | Character.SegmentBottomRight | Character.SegmentBottom, 
                // half off half on
                Character.SegmentTopLeft|Character.SegmentBottomLeft|Character.SegmentMiddle | Character.Dot,
            };
            // If you have a 4 display, only the fisrt 4 will be displayed
            // on a 6 segment one, all 6 will be displayed
            tm1637.Display(rawData);
            Thread.Sleep(3000);
            for(int i=0; i<6; i++)
                rawData[i] = (Character)Enum.Parse(typeof(Character), $"Char{i}");
            tm1637.Display(rawData);
            Thread.Sleep(3000);

            // Blink the screen by switching on and off
            for (int i = 0; i < 10; i++)
            {
                tm1637.ScreenOn = !tm1637.ScreenOn;
                tm1637.Display(rawData);
                Thread.Sleep(500);
            }
            tm1637.ScreenOn = true;

            long bright = 0;
            while (!Console.KeyAvailable)
            {
                var dt = DateTime.Now;
                toDisplay[0] = (Character)Enum.Parse(typeof(Character), $"Char{dt.Minute / 10}");
                toDisplay[1] = (Character)Enum.Parse(typeof(Character), $"Char{dt.Minute % 10}") | Character.Dot;
                toDisplay[2] = (Character)Enum.Parse(typeof(Character), $"Char{dt.Second / 10}");
                toDisplay[3] = (Character)Enum.Parse(typeof(Character), $"Char{dt.Second % 10}");
                tm1637.Brightness = (byte)(bright++ % 8);
                tm1637.Display(toDisplay);
                Thread.Sleep(100);
            }
            tm1637.ScreenOn = false;
            tm1637.ClearDisplay();

        }
    }
}

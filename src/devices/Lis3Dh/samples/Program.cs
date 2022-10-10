// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.IO;
using System.Numerics;
using System.Text;
using System.Threading;
using Iot.Device.Ft4222;
using Iot.Device.Lis3DhAccelerometer;

List<Ft4222Device> ft4222s = Ft4222Device.GetFt4222();
if (ft4222s.Count == 0)
{
    Console.WriteLine("FT4222 not plugged in");
    return;
}

Ft4222Device ft4222 = ft4222s[0];
using var accelerometer = Lis3Dh.Create(ft4222.CreateI2cDevice(new I2cConnectionSettings(0, Lis3Dh.DefaultI2cAddress)), dataRate: DataRate.DataRate25Hz, accelerationScale: AccelerationScale.Scale02G);

OneEuroFilter[] oneEuFilter = new OneEuroFilter[3] { CreateOneEuroFilter(), CreateOneEuroFilter(), CreateOneEuroFilter() };
MovingAverage[] maFilter = new MovingAverage[3] { CreateMovingAverage(), CreateMovingAverage(), CreateMovingAverage() };

Console.WriteLine("If you orient sensor so that two axes are close to 0");
Console.WriteLine("the remaining one should equal close to 1 or -1 (1G) which is gravitational force");

while (!Console.KeyAvailable || Console.ReadKey().Key != ConsoleKey.Enter)
{
    Vector3 acceleration = accelerometer.Acceleration;
    DateTimeOffset now = DateTimeOffset.Now;

    Vector3 oneEuFiltAcc = new Vector3(
       (float)oneEuFilter[0].Apply(acceleration.X, now),
       (float)oneEuFilter[1].Apply(acceleration.Y, now),
       (float)oneEuFilter[2].Apply(acceleration.Z, now));

    Vector3 maFilterAcc = new Vector3(
       (float)maFilter[0].Apply(acceleration.X),
       (float)maFilter[1].Apply(acceleration.Y),
       (float)maFilter[2].Apply(acceleration.Z));

    Console.SetCursorPosition(0, 2);
    Console.WriteLine($"1EU Filter: {FmtV3(oneEuFiltAcc)}");
    Console.WriteLine($" MA Filter: {FmtV3(maFilterAcc)}");
    Console.WriteLine($"  Original: {FmtV3(acceleration)}");

    Vector3 accOneEuDiff = Vector3.Abs(Vector3.Subtract(acceleration, oneEuFiltAcc));
    Console.WriteLine($"  1EU Diff: {FmtV3(accOneEuDiff)}");

    Vector3 accMaDiff = Vector3.Abs(Vector3.Subtract(acceleration, maFilterAcc));
    Console.WriteLine($"MovAvgDiff: {FmtV3(accMaDiff)}");
    Thread.Sleep(40);
}

static OneEuroFilter CreateOneEuroFilter()
{
    const double delta = 0.0;
    const double minCutOff = 1.0;
    const double beta = 0.0001;
    const double cutOff = 0.3;
    return new OneEuroFilter(delta: delta, minCutoff: minCutOff, beta: beta, cutoff: cutOff);
}

static MovingAverage CreateMovingAverage()
{
    return new MovingAverage(4);
}

static string FmtV3(Vector3 vec)
{
    return $"{vec.X:0.00} {vec.Y:0.00} {vec.Z:0.00}".PadRight(30);
}

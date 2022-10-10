// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable

using System;

public class MovingAverage
{
    private int _current = 0;
    private int _total = 0;
    private float _sum;
    private float[] _samples;

    public MovingAverage(int samples)
    {
        _samples = new float[samples];
    }

    public float Apply(float value)
    {
        if (_total < _samples.Length)
        {
            _total++;
        }
        else
        {
            _sum -= _samples[_current];
        }

        _sum += value;
        _samples[_current++] = value;
        _current %= _samples.Length;
        return _sum / _total;
    }
}

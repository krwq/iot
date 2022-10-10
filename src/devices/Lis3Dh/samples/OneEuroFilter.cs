// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable

using System;

/// <summary>
/// Smoothing filter adapted from https://github.com/jaantollander/OneEuroFilter
/// Filter demo: https://gery.casiez.net/1euro/InteractiveDemo/
/// </summary>
public class OneEuroFilter
{
    private DateTimeOffset _previousTime;
    private double _previousValue;
    private double _previousDelta;
    private readonly double _minCutoff;
    private readonly double _beta;
    private readonly double _cutoff;
    private double _integral = 0;

    public OneEuroFilter(DateTimeOffset? currentTime = null, double initialValue = double.NaN, double delta = 0.0, double minCutoff = 1.0, double beta = 0.0,
        double cutoff = 1.0)
    {
        _previousTime = currentTime ?? DateTimeOffset.UtcNow;
        _previousValue = initialValue;
        _previousDelta = delta;
        _minCutoff = minCutoff;
        _beta = beta;
        _cutoff = cutoff;
    }

    public double Apply(double value, DateTimeOffset? now = null)
    {
        now = now?.ToUniversalTime() ?? DateTimeOffset.UtcNow;

        if (double.IsNaN(_previousValue))
        {
            _previousValue = value;
            _previousTime = now.Value;
            return value;
        }

        var elapsedMilliseconds = (now.Value - _previousTime).TotalMilliseconds;

        // The filtered derivative of the signal.
        var alpha = SmoothingFactor(elapsedMilliseconds, _cutoff);
        var dx = (value - _previousValue) / elapsedMilliseconds;
        var dxHat = ExponentialSmoothing(alpha, dx, _previousDelta);

        // The filtered signal.
        var cutoff = _minCutoff + _beta * Math.Abs(dxHat);
        alpha = SmoothingFactor(elapsedMilliseconds, cutoff);
        var xHat = ExponentialSmoothing(alpha, value, _previousValue);

        // Memorize the previous values.
        _previousValue = xHat;
        _previousDelta = dxHat;
        _previousTime = now.Value;

        return xHat;
    }

    private static double SmoothingFactor(double elapsedMilliseconds, double cutoff)
    {
        var r = 2 * Math.PI * cutoff * elapsedMilliseconds;
        return r / (r + 1);
    }


    private static double ExponentialSmoothing(double alpha, double value, double previousValue)
    {
        return alpha * value + (1 - alpha) * previousValue;
    }
}

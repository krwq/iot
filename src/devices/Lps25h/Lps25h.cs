﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Diagnostics;
using Iot.Units;

namespace Iot.Device.Lps25h
{
    /// <summary>
    /// LPS25H - Piezoresistive pressure and thermometer sensor
    /// </summary>
    public class Lps25h : IDisposable
    {
        private const byte ReadMask = 0x80;
        private I2cDevice _i2c;

        /// <summary>
        /// Lps25h - Pressure and temperature sensor
        /// </summary>
        public Lps25h(I2cDevice i2cDevice)
        {
            if (i2cDevice == null)
                throw new ArgumentNullException(nameof(i2cDevice));

            _i2c = i2cDevice;

            // Highest resolution for both pressure and temperature sensor
            byte resolution = Read(Register.ResolutionMode);
            resolution |= 0b1111;
            WriteByte(Register.ResolutionMode, resolution);

            byte control1orig = Read(Register.Control1);
            // 7 - PD - power down control - 1 means active
            // 6-4 - output data rate - 0b100 means 25Hz for both sensors
            // 3 - interrupt circuit enable - 0 means disabled
            // 2 - block data update - 1 means update when both MSB and LSB are read
            // 1 - reset auto-zero - 0 means disable
            // 0 - SPI mode - we don't care what value since we use I2c, leave at default (0)
            byte control1 = 0b1100_0100;
            WriteByte(Register.Control1, control1);
        }

        /// <summary>
        /// Temperature
        /// </summary>
        public Temperature Temperature => Temperature.FromCelsius(42.5f + ReadInt16(Register.Temperature) / 480f);

        /// <summary>
        /// Pressure in hPa
        /// </summary>
        public float Pressure => ReadInt24(Register.Pressure) / 4096f;

        private void WriteByte(Register register, byte data)
        {
            Span<byte> buff = stackalloc byte[2]
            {
                (byte)register,
                data
            };

            _i2c.Write(buff);
        }

        private static int ReadInt24LittleEndian(ReadOnlySpan<byte> buff)
        {
            Debug.Assert(buff.Length == 3);

            byte mostSignificantByte = buff[2];
            Span<byte> b = stackalloc byte[4]
            {
                buff[0],
                buff[1],
                mostSignificantByte,
                (mostSignificantByte >> 7) != 0 ? (byte)0xff : (byte)0x00,
            };

            return BinaryPrimitives.ReadInt32LittleEndian(b);
        }

        private int ReadInt24(Register register)
        {
            Span<byte> val = stackalloc byte[3];
            Read(register, val);
            return ReadInt24LittleEndian(val);
        }

        private short ReadInt16(Register register)
        {
            Span<byte> val = stackalloc byte[2];
            Read(register, val);
            return BinaryPrimitives.ReadInt16LittleEndian(val);
        }

        private void Read(Register register, Span<byte> buffer)
        {
            _i2c.WriteByte((byte)((byte)register | ReadMask));
            _i2c.Read(buffer);
        }

        private byte Read(Register register)
        {
            _i2c.WriteByte((byte)((byte)register | ReadMask));
            return _i2c.ReadByte();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2c?.Dispose();
            _i2c = null;
        }
    }
}

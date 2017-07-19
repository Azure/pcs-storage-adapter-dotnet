// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Text;

namespace Services.Test.helpers
{
    public static class RandomExtension
    {
        public static string NextString(this Random rand, int length = 32, string characters = @"0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ")
        {
            var builder = new StringBuilder();

            while (builder.Length < length)
            {
                builder.Append(characters[rand.Next(0, characters.Length)]);
            }

            return builder.ToString();
        }

        public static DateTime NextDateTime(this Random rand)
        {
            var min = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var max = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            return min + TimeSpan.FromSeconds(rand.Next(0, (int)(max - min).TotalSeconds));
        }
    }
}

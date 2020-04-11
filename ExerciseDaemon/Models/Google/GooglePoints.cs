﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ExerciseDaemon.Models.Google
{
    public static class GooglePoints
    {
        public static IEnumerable<CoordinateEntity> Decode(string encodedPoints)
        {
            if (string.IsNullOrEmpty(encodedPoints))
            {
                throw new ArgumentNullException(nameof(encodedPoints));
            }

            var polylineChars = encodedPoints.ToCharArray();
            var index = 0;

            var currentLat = 0;
            var currentLng = 0;

            while (index < polylineChars.Length)
            {
                var sum = 0;
                var shifter = 0;
                int next5Bits;
                
                do
                {
                    next5Bits = (int)polylineChars[index++] - 63;
                    sum |= (next5Bits & 31) << shifter;
                    shifter += 5;

                } while (next5Bits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length)
                {
                    break;
                }

                currentLat += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                sum = 0;
                shifter = 0;

                do
                {
                    next5Bits = (int)polylineChars[index++] - 63;
                    sum |= (next5Bits & 31) << shifter;
                    shifter += 5;

                } while (next5Bits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length && next5Bits >= 32)
                {
                    break;
                }

                currentLng += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                yield return new CoordinateEntity
                {
                    Latitude = Convert.ToDouble(currentLat) / 1E5,
                    Longitude = Convert.ToDouble(currentLng) / 1E5
                };
            }
        }

        public static string Encode(IEnumerable<CoordinateEntity> points)
        {
            var str = new StringBuilder();

            var encodeDiff = (Action<int>)(diff =>
            {
                var shifted = diff << 1;
                if (diff < 0)
                {
                    shifted = ~shifted;
                }

                var rem = shifted;

                while (rem >= 0x20)
                {
                    str.Append((char)((0x20 | (rem & 0x1f)) + 63));

                    rem >>= 5;
                }

                str.Append((char)(rem + 63));
            });

            var lastLat = 0;
            var lastLng = 0;

            foreach (var point in points)
            {
                var lat = (int)Math.Round(point.Latitude * 1E5);
                var lng = (int)Math.Round(point.Longitude * 1E5);

                encodeDiff(lat - lastLat);
                encodeDiff(lng - lastLng);

                lastLat = lat;
                lastLng = lng;
            }

            return str.ToString();
        }
    }
}

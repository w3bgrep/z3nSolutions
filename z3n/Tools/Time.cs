﻿using System;
using System.Globalization;
using System.Threading;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace z3n
{
    public static class Time
    {
        private static readonly object LockObject = new object();

        public static string Now(string format = "unix") // unix|iso
        {
            lock (LockObject)
            {
                if (format == "unix") return ((long)((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds)).ToString();   //Unix Epoch
                else if (format == "iso") return DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"); // ISO 8601 
                else if (format == "short") return DateTime.UtcNow.ToString("MM-ddTHH:mm");
                throw new ArgumentException("Invalid format. Use 'unix' or 'iso'.");
            }
        }
        public static string cd(object input = null, string o = "unix")
        {
            DateTime utcNow = DateTime.UtcNow;
            if (input == null)
            {
                DateTime todayEnd = utcNow.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                if (o == "unix") return ((int)(todayEnd - new DateTime(1970, 1, 1)).TotalSeconds).ToString();
                else if (o == "iso") return todayEnd.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"); // ISO 8601
            }
            else if (input is decimal || input is int)
            {
                decimal minutes = Convert.ToDecimal(input);
                int secondsToAdd = (int)Math.Round(minutes * 60);
                DateTime futureTime = utcNow.AddSeconds(secondsToAdd);
                if (o == "unix") return ((int)(futureTime - new DateTime(1970, 1, 1)).TotalSeconds).ToString();
                else if (o == "iso") return futureTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"); // ISO 8601
            }
            else if (input is string timeString)
            {
                TimeSpan parsedTime = TimeSpan.Parse(timeString);
                DateTime futureTime = utcNow.Add(parsedTime);
                if (o == "unix") return ((int)(futureTime - new DateTime(1970, 1, 1)).TotalSeconds).ToString();
                else if (o == "iso") return futureTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"); // ISO 8601
            }
            throw new ArgumentException("Неподдерживаемый тип входного параметра");
        }


        public static int TimeElapsed(this IZennoPosterProjectModel project, string varName = "varSessionId")
        {
            var start = project.Variables[varName].Value;
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long startTime = long.Parse(start);
            int difference = (int)(currentTime - startTime);

            return difference;
        }
        public static T Age<T>(this IZennoPosterProjectModel project)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            long start;
            try
            {
                start = long.Parse(project.Variables["varSessionId"].Value);
            }
            catch
            {
                project.Variables["varSessionId"].Value = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                start = long.Parse(project.Variables["varSessionId"].Value);
            }

            long Age = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - start;


            if (typeof(T) == typeof(string))
            {
                string result = TimeSpan.FromSeconds(Age).ToString();
                return (T)(object)result;
            }
            else if (typeof(T) == typeof(TimeSpan))
            {
                TimeSpan result = TimeSpan.FromSeconds(Age);
                return (T)(object)result;
            }
            else
            {
                return (T)Convert.ChangeType(Age, typeof(T));
            }

        }
        public static void TimeOut(this IZennoPosterProjectModel project, int min = 30)
        {
            if (project.TimeElapsed() > 60 * min) throw new Exception("GlobalTimeout");
        }
        public static void Deadline(this IZennoPosterProjectModel project, int sec = 0)
        {
            if (sec != 0)
            {
                var start = project.Variables[$"t0"].Value;
                long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                long startTime = long.Parse(start);
                int difference = (int)(currentTime - startTime);
                if (difference > sec) throw new Exception("Timeout");

            }
            else
            {
                project.Variables["t0"].Value = (DateTimeOffset.UtcNow.ToUnixTimeSeconds()).ToString();
            }
        }

    }
}

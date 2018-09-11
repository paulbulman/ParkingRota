﻿namespace ParkingRota.Middleware
{
    using System.Collections.Generic;

    public class SecurityHeadersPolicy
    {
        public SecurityHeadersPolicy() => this.Headers = new Dictionary<string, string>();

        public IDictionary<string, string> Headers { get; }

        public void AddHeader(string header, string value) => this.Headers.Add(header, value);
    }
}
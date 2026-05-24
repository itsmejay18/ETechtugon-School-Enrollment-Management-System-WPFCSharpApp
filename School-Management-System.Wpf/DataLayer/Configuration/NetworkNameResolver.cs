using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace School_Management_System.DataLayer.Configuration
{
    public static class NetworkNameResolver
    {
        public static string ResolveIpv4(string host)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                return null;
            }

            IPAddress parsed;
            if (IPAddress.TryParse(host.Trim(), out parsed))
            {
                return parsed.AddressFamily == AddressFamily.InterNetwork ? parsed.ToString() : null;
            }

            try
            {
                var addresses = Dns.GetHostAddresses(host.Trim());
                for (var i = 0; i < addresses.Length; i++)
                {
                    if (addresses[i].AddressFamily == AddressFamily.InterNetwork)
                    {
                        return addresses[i].ToString();
                    }
                }
            }
            catch
            {
            }

            return ResolveIpv4ViaPublicDns(host.Trim());
        }

        private static string ResolveIpv4ViaPublicDns(string host)
        {
            foreach (var dnsServer in new[] { "8.8.8.8", "1.1.1.1" })
            {
                var result = ResolveIpv4ViaDnsServer(host, dnsServer);
                if (!string.IsNullOrWhiteSpace(result))
                {
                    return result;
                }
            }

            return null;
        }

        private static string ResolveIpv4ViaDnsServer(string host, string dnsServer)
        {
            try
            {
                var query = BuildDnsQuery(host);
                using (var udp = new UdpClient())
                {
                    udp.Client.ReceiveTimeout = 3000;
                    udp.Client.SendTimeout = 3000;
                    udp.Connect(dnsServer, 53);
                    udp.Send(query, query.Length);

                    var endpoint = new IPEndPoint(IPAddress.Any, 0);
                    var response = udp.Receive(ref endpoint);
                    return ParseARecord(response);
                }
            }
            catch
            {
                return null;
            }
        }

        private static byte[] BuildDnsQuery(string host)
        {
            var bytes = new List<byte>();
            var id = unchecked((ushort)Environment.TickCount);

            AddUInt16(bytes, id);
            AddUInt16(bytes, 0x0100); // standard recursive query
            AddUInt16(bytes, 1);
            AddUInt16(bytes, 0);
            AddUInt16(bytes, 0);
            AddUInt16(bytes, 0);

            var labels = host.TrimEnd('.').Split('.');
            for (var i = 0; i < labels.Length; i++)
            {
                var labelBytes = System.Text.Encoding.ASCII.GetBytes(labels[i]);
                bytes.Add((byte)labelBytes.Length);
                bytes.AddRange(labelBytes);
            }

            bytes.Add(0);
            AddUInt16(bytes, 1); // A
            AddUInt16(bytes, 1); // IN

            return bytes.ToArray();
        }

        private static string ParseARecord(byte[] response)
        {
            if (response == null || response.Length < 12)
            {
                return null;
            }

            var questionCount = ReadUInt16(response, 4);
            var answerCount = ReadUInt16(response, 6);
            var offset = 12;

            for (var i = 0; i < questionCount; i++)
            {
                offset = SkipName(response, offset);
                offset += 4;
                if (offset > response.Length)
                {
                    return null;
                }
            }

            for (var i = 0; i < answerCount; i++)
            {
                offset = SkipName(response, offset);
                if (offset + 10 > response.Length)
                {
                    return null;
                }

                var type = ReadUInt16(response, offset);
                var dnsClass = ReadUInt16(response, offset + 2);
                var dataLength = ReadUInt16(response, offset + 8);
                offset += 10;

                if (offset + dataLength > response.Length)
                {
                    return null;
                }

                if (type == 1 && dnsClass == 1 && dataLength == 4)
                {
                    return new IPAddress(new[]
                    {
                        response[offset],
                        response[offset + 1],
                        response[offset + 2],
                        response[offset + 3]
                    }).ToString();
                }

                offset += dataLength;
            }

            return null;
        }

        private static int SkipName(byte[] response, int offset)
        {
            while (offset < response.Length)
            {
                var length = response[offset];
                if ((length & 0xC0) == 0xC0)
                {
                    return offset + 2;
                }

                offset++;
                if (length == 0)
                {
                    return offset;
                }

                offset += length;
            }

            return response.Length;
        }

        private static void AddUInt16(ICollection<byte> bytes, int value)
        {
            bytes.Add((byte)((value >> 8) & 0xFF));
            bytes.Add((byte)(value & 0xFF));
        }

        private static ushort ReadUInt16(byte[] bytes, int offset)
        {
            return (ushort)((bytes[offset] << 8) | bytes[offset + 1]);
        }
    }
}

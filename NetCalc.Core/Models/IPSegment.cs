using System;
using System.Collections.Generic;

namespace NetCalc.Core.Models
{
    public class IpSegment : IComparable<IpSegment>
    {
        // IP e máscara em formato uint
        private readonly uint _ip;
        private readonly uint _mask;

        // ReSharper disable once UnusedMember.Global
        public IpSegment(string ip, string mask)
        {
            _ip = ip.ParseIp();
            _mask = mask.ParseIp();
        }

        public IpSegment(string ip, byte cidr)
        {
            _ip = ip.ParseIp();
            _mask = CidrToMask(cidr);
        }

        public bool Rfc3021
        {
            get
            {
                if (Cidr == 31)
                {
                    return true;
                }
                return false;
            }
        }

        private byte MaskToCidr(UInt32 mask)
        {
            //uint __mask = mask;
            mask = mask - ((mask >> 1) & 0x55555555);
            mask = (mask & 0x33333333) + ((mask >> 2) & 0x33333333);
            return Convert.ToByte((((mask + (mask >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24);
        }

        private uint CidrToMask(byte cidr)
        {
            uint mask = 0xFFFFFFFF;
            mask = mask << (32 - cidr);
            return mask;
        }

        public byte Cidr
        {
            get
            {
                return MaskToCidr(_mask);
            }
        }

        public string SubnetMask
        {
            get
            {
                return _mask.ToIpString();
            }
        }

        public uint NumberOfHosts
        {
            get
            {
                uint allIPs = ~_mask + 1;

                uint hosts = 0;

                if (allIPs > 2)
                {
                    hosts = allIPs - 2;
                }
                else if (allIPs == 2 && Cidr == 31)
                {
                    hosts = 2;
                }
                else if (allIPs == 1)
                {
                    hosts = 1;
                }

                return hosts;
            }
        }

        public uint NetworkAddress
        {
            get { return _ip & _mask; }
        }

        // ReSharper disable once UnusedMember.Global
        public string NetworkAddressText
        {
            get { return NetworkAddress.ToIpString(); }
        }

        public uint BroadcastAddress
        {
            get { return NetworkAddress + ~_mask; }
        }

        // ReSharper disable once UnusedMember.Global
        public IEnumerable<uint> Hosts()
        {
            for (var host = NetworkAddress + 1; host < BroadcastAddress; host++)
            {
                yield return host;
            }
        }

        /// <summary>
        /// First usable IP address in Network
        /// </summary>
        public uint FirstUsable
        {
            get
            {
                if (Cidr == 31 || Cidr == 32)
                {
                    return NetworkAddress;
                }
                return (Usable <= 0) ? NetworkAddress : NetworkAddress + 1;
            }
        }

        /// <summary>
        /// Last usable IP address in Network
        /// </summary>
        public uint LastUsable
        {
            get
            {
                if (Cidr == 31 || Cidr == 32)
                {
                    return BroadcastAddress;
                }
                return (Usable <= 0) ? NetworkAddress : BroadcastAddress - 1;
            }
        }

        public string WildCardSubnetMask
        {
            get
            {
                return (~_mask).ToIpString();
            }
        }
        /// <summary>
        /// Number of usable IP adress in Network
        /// </summary>
        private uint Usable
        {
            get
            {
                //return (this.CIDR > 32) ? 0 : ((0xffffffff >> this.CIDR) - 1);
                return ((0xffffffff >> Cidr) - 1);
            }
        }

        public int CompareTo(IpSegment other)
        {
            int network = NetworkAddress.CompareTo(other.NetworkAddress);
            if (network != 0)
            {
                return network;
            }

            int cidr = Cidr.CompareTo(other.Cidr);
            return cidr;
        }
    }
}
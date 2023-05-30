using System;
using System.Collections;
using System.Collections.Generic;

namespace NetCalc.Core.Models
{
    public class IpSegmentCollection : IEnumerable<IpSegment>, IEnumerator<IpSegment>
    {
        private double _enumerator;
        private readonly byte _cidrSubnet;
        private readonly IpSegment _ipnetwork;

        private byte Cidr
        {
            get { return _ipnetwork.Cidr; }
        }

        // ReSharper disable once UnusedMember.Local
        private string Mask
        {
            get { return _ipnetwork.SubnetMask; }
        }
        private uint Broadcast
        {
            get { return _ipnetwork.BroadcastAddress; }
        }
        private uint Network
        {
            get { return _ipnetwork.NetworkAddress; }
        }

        public IpSegmentCollection(IpSegment ipnetwork, byte cidrSubnet)
        {

            if (cidrSubnet > 32)
            {
                throw new ArgumentOutOfRangeException("cidrSubnet");
            }

            if (cidrSubnet < ipnetwork.Cidr)
            {
                throw new ArgumentException("cidr");
            }

            _cidrSubnet = cidrSubnet;
            _ipnetwork = ipnetwork;
            _enumerator = -1;
        }

        #region Count, Array, Enumerator

        public double Count
        {
            get
            {
                double count = Math.Pow(2, _cidrSubnet - Cidr);
                return count;
            }
        }

        private IpSegment this[double i]
        {
            get
            {
                if (i - 1 >= Count)
                {
                    throw new ArgumentOutOfRangeException("i");
                }
                double size = Count;
                var increment = (int)((Broadcast - Network) / size);
                var uintNetwork = (uint)(Network + ((increment + 1) * i));
                var ipn = new IpSegment(uintNetwork.ToIpString(), _cidrSubnet);
                return ipn;
            }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator<IpSegment> IEnumerable<IpSegment>.GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }

        #region IEnumerator<IPNetwork> Members

        public IpSegment Current
        {
            get { return this[_enumerator]; }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            // nothing to dispose
        }

        #endregion

        #region IEnumerator Members

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public bool MoveNext()
        {
            // Por questões de performance só os primeiros 65536 itens são retornados
            _enumerator++;
            if (_enumerator >= Count || _enumerator >= 65536)
            {
                return false;
            }
            return true;

        }

        public void Reset()
        {
            _enumerator = -1;
        }

        #endregion

        #endregion

    }
}

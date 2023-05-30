using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using NetCalc.Core.Models;

namespace NetCalc.Core.ViewModels
{
    public class MainViewModel : MvxViewModel
    {
        private string _addressBlock = "10.0.0.0";

        public string AddressBlock
        {
            get { return _addressBlock; }
            set
            {
                _addressBlock = value;
                RaisePropertyChanged(() => AddressBlock);
            }
        }


        private KeyValuePair<uint, uint> _selectedHostAndSubnet;

        public KeyValuePair<uint, uint> SelectedHostsAndSubnets
        {
            get { return _selectedHostAndSubnet; }
            set
            {
                _selectedHostAndSubnet = value;
                RaisePropertyChanged(() => SelectedHostsAndSubnets);
            }
        }


        private ObservableCollection<IpSegment> _subnets;

        public ObservableCollection<IpSegment> Subnets 
        {
            get { return _subnets; }
            set
            {
                _subnets = value;
                RaisePropertyChanged(() => Subnets);
            }
        }
        


        private KeyValuePair<int, string> _selectedMask;

        public KeyValuePair<int, string> SelectedMask
        {
            get { return _selectedMask; }
            set
            {
                _selectedMask = value;
                RaisePropertyChanged(() => SelectedMask);
            }
        }

        public MvxCommand MaskSelectedCommand { get; set; }

        private void MaskSelected()
        {
            if (SelectedMask.Value != null && !string.IsNullOrEmpty(AddressBlock))
            {
                var ipNetwork = new IpSegment(AddressBlock, Convert.ToByte(SelectedMask.Key));
                var ipNetCollection = new IpSegmentCollection(ipNetwork, 32);

                uint maxSubnets = Convert.ToUInt32(ipNetCollection.Count);
                var sub = new List<uint>();

                do
                {
                    sub.Add(maxSubnets);
                    maxSubnets = maxSubnets/2;
                } while (maxSubnets >= 1);

                sub.Reverse();

                var numberOfHosts = new List<uint>();

                for (int i = 0; i < sub.Count; i++)
                {
                    var ipSegment = new IpSegment(
                        AddressBlock, Convert.ToByte(SelectedMask.Key + i));
                    numberOfHosts.Add(ipSegment.NumberOfHosts);
                }

                HostsAndSubnets = sub.ToDictionary(x => x, x => numberOfHosts[sub.IndexOf(x)]);
            }
        }


        public MvxCommand ListSubnetsCommand { get; set; }

        private void ListSubnets()
        {
            try
            {
                byte netBits = Convert.ToByte(SelectedMask.Key);

                var ipNetwork = new IpSegment(AddressBlock, netBits);

                // número de opções de números de subnets
                double additionalBitsToTheSubnetMask;

                additionalBitsToTheSubnetMask = Math.Round(
                    Math.Log(Convert.ToDouble(SelectedHostsAndSubnets.Key), 2));


                byte subnetting = (byte)(netBits + additionalBitsToTheSubnetMask);

                var ipNetCollection = new IpSegmentCollection(ipNetwork, subnetting);

                List<IpSegment> networks = new List<IpSegment>();

                foreach (var item in ipNetCollection)
                {
                    networks.Add(item);
                }

                Subnets =  new ObservableCollection<IpSegment>(networks);
            }
            catch
            {
                //MessageBox.Show (AppResources.InvalidIP);
            }
        }

        private Dictionary<uint, uint> _hostsAndSubnets;

        public Dictionary<uint, uint> HostsAndSubnets
        {
            get { return _hostsAndSubnets; }
            set
            {
                _hostsAndSubnets = value;
                RaisePropertyChanged(() => HostsAndSubnets);
            }
        }

        private IpSegment _selectedIpSegment;

        public IpSegment SelectedIpSegment
        {
            get { return _selectedIpSegment; }
            set
            {
                _selectedIpSegment = value;
                RaisePropertyChanged(() => SelectedIpSegment);
            }
        }


        public Dictionary<int, string> MasksList { get; set; }

        public MainViewModel()
        {
            PopulateMaskList();
            MaskSelectedCommand = new MvxCommand(MaskSelected);
            ListSubnetsCommand = new MvxCommand(ListSubnets);
        }

        private void PopulateMaskList()
        {
            MasksList = new Dictionary<int, string>
            {
                {1, "128.0.0.0"},
                {2, "192.0.0.0"},
                {3, "224.0.0.0"},
                {4, "240.0.0.0"},
                {5, "248.0.0.0"},
                {6, "252.0.0.0"},
                {7, "254.0.0.0"},
                {8, "255.0.0.0"},
                {9, "255.128.0.0"},
                {10, "255.192.0.0"},
                {11, "255.224.0.0"},
                {12, "255.240.0.0"},
                {13, "255.248.0.0"},
                {14, "255.252.0.0"},
                {15, "255.254.0.0"},
                {16, "255.255.0.0"},
                {17, "255.255.128.0"},
                {18, "255.255.192.0"},
                {19, "255.255.224.0"},
                {20, "255.255.240.0"},
                {21, "255.255.248.0"},
                {22, "255.255.252.0"},
                {23, "255.255.254.0"},
                {24, "255.255.255.0"},
                {25, "255.255.255.128"},
                {26, "255.255.255.192"},
                {27, "255.255.255.224"},
                {28, "255.255.255.240"},
                {29, "255.255.255.248"},
                {30, "255.255.255.252"},
                {31, "255.255.255.254 - (RFC 3021)"},
                {32, "255.255.255.255"}
            };
        }
    }
}

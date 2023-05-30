using System;
using System.Globalization;

namespace NetCalc.Core.Models
{
    public static class IpHelpers
    {
        // Extension methods

        /// <summary>
        /// Converte um endereço IP do formato uint para formato string
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToIpString(this uint value)
        {
            var bitmask = 0xff000000;
            var parts = new string[4];
            for (var i = 0; i < 4; i++)
            {
                var masked = (value & bitmask) >> ((3 - i) * 8);
                bitmask >>= 8;
                parts[i] = masked.ToString(CultureInfo.InvariantCulture);
            }
            return String.Join(".", parts);
        }

        /// <summary>
        /// Converte um endereço IP do formato string para o formato uint
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public static uint ParseIp(this string ipAddress)
        {
            try
            {
                ipAddress = ipAddress.Replace(",", ".");
                var splitted = ipAddress.Split('.');
                uint ip = 0;
                for (var i = 0; i < 4; i++)
                {
                    // verifica se cada octeto está entre 0 e 255
                    if (Convert.ToInt16(splitted[i]) > 255 || Convert.ToInt16(splitted[i]) < 0)
                    {
                        throw new ArgumentException("Invalid IP Address");
                    }
                    ip = (ip << 8) + uint.Parse(splitted[i]);
                }
                return ip;
            }
            catch (Exception)
            {
                throw new ArgumentException("Invalid IP Address");
            }
        }
    }
}
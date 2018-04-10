using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TcpServcerTest
{
    public static class CommonFunction
    {
        #region String和Hex转换方法
        public static string StrToHex(string Msg)
        {
            try
            {
                byte[] bytes = System.Text.Encoding.GetEncoding("GB2312").GetBytes(Msg);//System.Text.Encoding.Default.GetBytes(Msg);
                string str = "";
                for (int i = 0; i < bytes.Length; i++)
                {
                    str += string.Format("{0:X}", bytes[i]);
                }
                return str;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 字节数组转16进制字符串
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string byteToHexString(byte[] bytes) // 0xae00cf => "AE00CF "
        {
            string hexString = string.Empty;
            if (bytes != null)
            {
                StringBuilder strB = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    strB.Append(bytes[i].ToString("X2"));
                }
                hexString = strB.ToString();
            } return hexString;
        }

        /// <summary>
        /// 字符串转16进制字节数组
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] strToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }
        #endregion
    }
}

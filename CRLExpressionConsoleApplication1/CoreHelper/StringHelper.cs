using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
namespace CoreHelper
{
    /// <summary>
    /// 字符串加密类
    /// </summary>
    public class StringHelper
    {
        private static int[] iA = new int[]
		{
			-20319,
			-20317,
			-20304,
			-20295,
			-20292,
			-20283,
			-20265,
			-20257,
			-20242,
			-20230,
			-20051,
			-20036,
			-20032,
			-20026,
			-20002,
			-19990,
			-19986,
			-19982,
			-19976,
			-19805,
			-19784,
			-19775,
			-19774,
			-19763,
			-19756,
			-19751,
			-19746,
			-19741,
			-19739,
			-19728,
			-19725,
			-19715,
			-19540,
			-19531,
			-19525,
			-19515,
			-19500,
			-19484,
			-19479,
			-19467,
			-19289,
			-19288,
			-19281,
			-19275,
			-19270,
			-19263,
			-19261,
			-19249,
			-19243,
			-19242,
			-19238,
			-19235,
			-19227,
			-19224,
			-19218,
			-19212,
			-19038,
			-19023,
			-19018,
			-19006,
			-19003,
			-18996,
			-18977,
			-18961,
			-18952,
			-18783,
			-18774,
			-18773,
			-18763,
			-18756,
			-18741,
			-18735,
			-18731,
			-18722,
			-18710,
			-18697,
			-18696,
			-18526,
			-18518,
			-18501,
			-18490,
			-18478,
			-18463,
			-18448,
			-18447,
			-18446,
			-18239,
			-18237,
			-18231,
			-18220,
			-18211,
			-18201,
			-18184,
			-18183,
			-18181,
			-18012,
			-17997,
			-17988,
			-17970,
			-17964,
			-17961,
			-17950,
			-17947,
			-17931,
			-17928,
			-17922,
			-17759,
			-17752,
			-17733,
			-17730,
			-17721,
			-17703,
			-17701,
			-17697,
			-17692,
			-17683,
			-17676,
			-17496,
			-17487,
			-17482,
			-17468,
			-17454,
			-17433,
			-17427,
			-17417,
			-17202,
			-17185,
			-16983,
			-16970,
			-16942,
			-16915,
			-16733,
			-16708,
			-16706,
			-16689,
			-16664,
			-16657,
			-16647,
			-16474,
			-16470,
			-16465,
			-16459,
			-16452,
			-16448,
			-16433,
			-16429,
			-16427,
			-16423,
			-16419,
			-16412,
			-16407,
			-16403,
			-16401,
			-16393,
			-16220,
			-16216,
			-16212,
			-16205,
			-16202,
			-16187,
			-16180,
			-16171,
			-16169,
			-16158,
			-16155,
			-15959,
			-15958,
			-15944,
			-15933,
			-15920,
			-15915,
			-15903,
			-15889,
			-15878,
			-15707,
			-15701,
			-15681,
			-15667,
			-15661,
			-15659,
			-15652,
			-15640,
			-15631,
			-15625,
			-15454,
			-15448,
			-15436,
			-15435,
			-15419,
			-15416,
			-15408,
			-15394,
			-15385,
			-15377,
			-15375,
			-15369,
			-15363,
			-15362,
			-15183,
			-15180,
			-15165,
			-15158,
			-15153,
			-15150,
			-15149,
			-15144,
			-15143,
			-15141,
			-15140,
			-15139,
			-15128,
			-15121,
			-15119,
			-15117,
			-15110,
			-15109,
			-14941,
			-14937,
			-14933,
			-14930,
			-14929,
			-14928,
			-14926,
			-14922,
			-14921,
			-14914,
			-14908,
			-14902,
			-14894,
			-14889,
			-14882,
			-14873,
			-14871,
			-14857,
			-14678,
			-14674,
			-14670,
			-14668,
			-14663,
			-14654,
			-14645,
			-14630,
			-14594,
			-14429,
			-14407,
			-14399,
			-14384,
			-14379,
			-14368,
			-14355,
			-14353,
			-14345,
			-14170,
			-14159,
			-14151,
			-14149,
			-14145,
			-14140,
			-14137,
			-14135,
			-14125,
			-14123,
			-14122,
			-14112,
			-14109,
			-14099,
			-14097,
			-14094,
			-14092,
			-14090,
			-14087,
			-14083,
			-13917,
			-13914,
			-13910,
			-13907,
			-13906,
			-13905,
			-13896,
			-13894,
			-13878,
			-13870,
			-13859,
			-13847,
			-13831,
			-13658,
			-13611,
			-13601,
			-13406,
			-13404,
			-13400,
			-13398,
			-13395,
			-13391,
			-13387,
			-13383,
			-13367,
			-13359,
			-13356,
			-13343,
			-13340,
			-13329,
			-13326,
			-13318,
			-13147,
			-13138,
			-13120,
			-13107,
			-13096,
			-13095,
			-13091,
			-13076,
			-13068,
			-13063,
			-13060,
			-12888,
			-12875,
			-12871,
			-12860,
			-12858,
			-12852,
			-12849,
			-12838,
			-12831,
			-12829,
			-12812,
			-12802,
			-12607,
			-12597,
			-12594,
			-12585,
			-12556,
			-12359,
			-12346,
			-12320,
			-12300,
			-12120,
			-12099,
			-12089,
			-12074,
			-12067,
			-12058,
			-12039,
			-11867,
			-11861,
			-11847,
			-11831,
			-11798,
			-11781,
			-11604,
			-11589,
			-11536,
			-11358,
			-11340,
			-11339,
			-11324,
			-11303,
			-11097,
			-11077,
			-11067,
			-11055,
			-11052,
			-11045,
			-11041,
			-11038,
			-11024,
			-11020,
			-11019,
			-11018,
			-11014,
			-10838,
			-10832,
			-10815,
			-10800,
			-10790,
			-10780,
			-10764,
			-10587,
			-10544,
			-10533,
			-10519,
			-10331,
			-10329,
			-10328,
			-10322,
			-10315,
			-10309,
			-10307,
			-10296,
			-10281,
			-10274,
			-10270,
			-10262,
			-10260,
			-10256,
			-10254
		};
        private static string[] sA = new string[]
		{
			"a",
			"ai",
			"an",
			"ang",
			"ao",
			"ba",
			"bai",
			"ban",
			"bang",
			"bao",
			"bei",
			"ben",
			"beng",
			"bi",
			"bian",
			"biao",
			"bie",
			"bin",
			"bing",
			"bo",
			"bu",
			"ca",
			"cai",
			"can",
			"cang",
			"cao",
			"ce",
			"ceng",
			"cha",
			"chai",
			"chan",
			"chang",
			"chao",
			"che",
			"chen",
			"cheng",
			"chi",
			"chong",
			"chou",
			"chu",
			"chuai",
			"chuan",
			"chuang",
			"chui",
			"chun",
			"chuo",
			"ci",
			"cong",
			"cou",
			"cu",
			"cuan",
			"cui",
			"cun",
			"cuo",
			"da",
			"dai",
			"dan",
			"dang",
			"dao",
			"de",
			"deng",
			"di",
			"dian",
			"diao",
			"die",
			"ding",
			"diu",
			"dong",
			"dou",
			"du",
			"duan",
			"dui",
			"dun",
			"duo",
			"e",
			"en",
			"er",
			"fa",
			"fan",
			"fang",
			"fei",
			"fen",
			"feng",
			"fo",
			"fou",
			"fu",
			"ga",
			"gai",
			"gan",
			"gang",
			"gao",
			"ge",
			"gei",
			"gen",
			"geng",
			"gong",
			"gou",
			"gu",
			"gua",
			"guai",
			"guan",
			"guang",
			"gui",
			"gun",
			"guo",
			"ha",
			"hai",
			"han",
			"hang",
			"hao",
			"he",
			"hei",
			"hen",
			"heng",
			"hong",
			"hou",
			"hu",
			"hua",
			"huai",
			"huan",
			"huang",
			"hui",
			"hun",
			"huo",
			"ji",
			"jia",
			"jian",
			"jiang",
			"jiao",
			"jie",
			"jin",
			"jing",
			"jiong",
			"jiu",
			"ju",
			"juan",
			"jue",
			"jun",
			"ka",
			"kai",
			"kan",
			"kang",
			"kao",
			"ke",
			"ken",
			"keng",
			"kong",
			"kou",
			"ku",
			"kua",
			"kuai",
			"kuan",
			"kuang",
			"kui",
			"kun",
			"kuo",
			"la",
			"lai",
			"lan",
			"lang",
			"lao",
			"le",
			"lei",
			"leng",
			"li",
			"lia",
			"lian",
			"liang",
			"liao",
			"lie",
			"lin",
			"ling",
			"liu",
			"long",
			"lou",
			"lu",
			"lv",
			"luan",
			"lue",
			"lun",
			"luo",
			"ma",
			"mai",
			"man",
			"mang",
			"mao",
			"me",
			"mei",
			"men",
			"meng",
			"mi",
			"mian",
			"miao",
			"mie",
			"min",
			"ming",
			"miu",
			"mo",
			"mou",
			"mu",
			"na",
			"nai",
			"nan",
			"nang",
			"nao",
			"ne",
			"nei",
			"nen",
			"neng",
			"ni",
			"nian",
			"niang",
			"niao",
			"nie",
			"nin",
			"ning",
			"niu",
			"nong",
			"nu",
			"nv",
			"nuan",
			"nue",
			"nuo",
			"o",
			"ou",
			"pa",
			"pai",
			"pan",
			"pang",
			"pao",
			"pei",
			"pen",
			"peng",
			"pi",
			"pian",
			"piao",
			"pie",
			"pin",
			"ping",
			"po",
			"pu",
			"qi",
			"qia",
			"qian",
			"qiang",
			"qiao",
			"qie",
			"qin",
			"qing",
			"qiong",
			"qiu",
			"qu",
			"quan",
			"que",
			"qun",
			"ran",
			"rang",
			"rao",
			"re",
			"ren",
			"reng",
			"ri",
			"rong",
			"rou",
			"ru",
			"ruan",
			"rui",
			"run",
			"ruo",
			"sa",
			"sai",
			"san",
			"sang",
			"sao",
			"se",
			"sen",
			"seng",
			"sha",
			"shai",
			"shan",
			"shang",
			"shao",
			"she",
			"shen",
			"sheng",
			"shi",
			"shou",
			"shu",
			"shua",
			"shuai",
			"shuan",
			"shuang",
			"shui",
			"shun",
			"shuo",
			"si",
			"song",
			"sou",
			"su",
			"suan",
			"sui",
			"sun",
			"suo",
			"ta",
			"tai",
			"tan",
			"tang",
			"tao",
			"te",
			"teng",
			"ti",
			"tian",
			"tiao",
			"tie",
			"ting",
			"tong",
			"tou",
			"tu",
			"tuan",
			"tui",
			"tun",
			"tuo",
			"wa",
			"wai",
			"wan",
			"wang",
			"wei",
			"wen",
			"weng",
			"wo",
			"wu",
			"xi",
			"xia",
			"xian",
			"xiang",
			"xiao",
			"xie",
			"xin",
			"xing",
			"xiong",
			"xiu",
			"xu",
			"xuan",
			"xue",
			"xun",
			"ya",
			"yan",
			"yang",
			"yao",
			"ye",
			"yi",
			"yin",
			"ying",
			"yo",
			"yong",
			"you",
			"yu",
			"yuan",
			"yue",
			"yun",
			"za",
			"zai",
			"zan",
			"zang",
			"zao",
			"ze",
			"zei",
			"zen",
			"zeng",
			"zha",
			"zhai",
			"zhan",
			"zhang",
			"zhao",
			"zhe",
			"zhen",
			"zheng",
			"zhi",
			"zhong",
			"zhou",
			"zhu",
			"zhua",
			"zhuai",
			"zhuan",
			"zhuang",
			"zhui",
			"zhun",
			"zhuo",
			"zi",
			"zong",
			"zou",
			"zu",
			"zuan",
			"zui",
			"zun",
			"zuo"
		};
        /// <summary>
        /// 得到日期的汉字显示
        /// </summary>
        /// <param name="date">格式为：2011-10-05</param>
        /// <param name="isTraditional">是否繁体字显示</param>
        /// <returns></returns>
        public static string GetChineseDate(string date, bool isTraditional)
        {
            string[] array = date.Split(new char[]
			{
				'-'
			});
            string result;
            if (array.Length == 3)
            {
                string text = string.Empty;
                string text2 = array[0];
                for (int i = 0; i < text2.Length; i++)
                {
                    text += StringHelper.GetChineseNumber(text2[i].ToString(), isTraditional);
                }
                text += "年";
                text += StringHelper.getDBChinese(array[1], isTraditional);
                text += "月";
                text += StringHelper.getDBChinese(array[2], isTraditional);
                text += "日";
                result = text;
            }
            else
            {
                result = string.Empty;
            }
            return result;
        }
        /// <summary>
        /// 得到日期的汉字显示
        /// </summary>
        /// <param name="date">格式为：2011-10-05</param>
        /// <returns></returns>
        public static string GetChineseDate(string date)
        {
            return StringHelper.GetChineseDate(date, false);
        }
        private static string getDBChinese(string number, bool isTraditional)
        {
            string result;
            if (number.Length == 1)
            {
                result = StringHelper.GetChineseNumber(number, isTraditional);
            }
            else
            {
                string text = string.Empty;
                string text2 = number.Substring(0, 1);
                string text3 = number.Substring(1, 1);
                if (text2 == "0")
                {
                    result = StringHelper.GetChineseNumber(text3, isTraditional);
                }
                else
                {
                    if (text2 != "1")
                    {
                        text += StringHelper.GetChineseNumber(text2, isTraditional);
                    }
                    text += StringHelper.GetChineseNumber("10", isTraditional);
                    if (text3 != "0")
                    {
                        text += StringHelper.GetChineseNumber(text3, isTraditional);
                    }
                    result = text;
                }
            }
            return result;
        }
        /// <summary>
        /// BYTE转16进制
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string ByteToHex(byte[] data)
        {
            string text = System.BitConverter.ToString(data);
            return text.Replace("-", "");
        }
        /// <summary>
        /// 16进制转BYTE
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] HexToByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if (hexString.Length % 2 != 0)
            {
                hexString += " ";
            }
            byte[] array = new byte[hexString.Length / 2];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = System.Convert.ToByte(hexString.Substring(i * 2, 2).Trim(), 16);
            }
            return array;
        }
        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="instr">要加密的字体串</param>
        /// <returns></returns>
        public static string EncryptMD5(string instr, System.Text.Encoding enc = null)
        {
            string result;
            try
            {
                if (enc == null)
                {
                    enc = System.Text.Encoding.Default;
                }
                byte[] array = enc.GetBytes(instr);
                System.Security.Cryptography.MD5CryptoServiceProvider mD5CryptoServiceProvider = new System.Security.Cryptography.MD5CryptoServiceProvider();
                array = mD5CryptoServiceProvider.ComputeHash(array);
                result = System.BitConverter.ToString(array).Replace("-", "");
            }
            catch
            {
                result = string.Empty;
            }
            return result;
        }
        /// <summary>
        /// DES加密
        /// </summary>
        /// <param name="sourceDataBytes"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static byte[] Encrypt(byte[] sourceDataBytes, byte[] key, byte[] iv)
        {
            return sourceDataBytes;
        }
        /// <summary>
        /// DES解密
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static byte[] Decrypt(byte[] data, byte[] key, byte[] iv)
        {
            return data;
        }
        /// <summary>
        /// 加密字符串,返回16进制字符串
        /// </summary>
        /// <param name="sourceData"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Encrypt(string sourceData, string key)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(key.Substring(0, key.Length));
            byte[] iv = new byte[]
			{
				1,
				2,
				3,
				4,
				5,
				6,
				7,
				8
			};
            byte[] data = StringHelper.Encrypt(System.Text.Encoding.UTF8.GetBytes(sourceData), bytes, iv);
            return StringHelper.ByteToHex(data);
        }
        /// <summary>
        /// 解密 传入16进制字符串
        /// </summary>
        /// <param name="sourceData"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Decrypt(string sourceData, string key)
        {
            byte[] data = StringHelper.HexToByte(sourceData);
            byte[] iv = new byte[]
			{
				1,
				2,
				3,
				4,
				5,
				6,
				7,
				8
			};
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(key.Substring(0, key.Length));
            byte[] bytes2 = StringHelper.Decrypt(data, bytes, iv);
            return System.Text.Encoding.UTF8.GetString(bytes2);
        }
        /// <summary>
        /// 判断字符串是否在一个以‘_ , |’隔开的字符串里
        /// </summary>
        /// <param name="str">目标字符串</param>
        /// <param name="strs">要查找的字符串</param>
        /// <returns></returns>
        public static bool IsInStrs(string str, string strs)
        {
            return StringHelper.IsInParams(str, strs.Split(new char[]
			{
				'_',
				',',
				'|'
			}));
        }
        /// <summary>
        /// 判断字符串是否在一个数组里
        /// </summary>
        /// <param name="str">目标字符串</param>
        /// <param name="strs">要查找的字符串数组</param>
        /// <returns></returns>
        public static bool IsInParams(string str, params string[] strs)
        {
            bool result;
            for (int i = 0; i < strs.Length; i++)
            {
                string a = strs[i];
                if (a == str)
                {
                    result = true;
                    return result;
                }
            }
            result = false;
            return result;
        }
        /// <summary>
        /// 是否是null或空字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(string str)
        {
            return string.IsNullOrEmpty(str);
        }
        public static bool IsNullOrEmpty(object obj)
        {
            string value = string.Concat(obj);
            return string.IsNullOrEmpty(value);
        }
        /// <summary>
        /// 是否不为空字符串也不是null
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNotNullAndEmpty(string str)
        {
            return !string.IsNullOrEmpty(str);
        }
        /// <summary>
        /// 判断字符串里每个字符是否是十进制
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsDigital(string str)
        {
            bool result;
            if (IsNotNullAndEmpty(str))
            {
                for (int i = 0; i < str.Length; i++)
                {
                    char c = str[i];
                    if (!char.IsDigit(c))
                    {
                        result = false;
                        return result;
                    }
                }
                result = true;
            }
            else
            {
                result = false;
            }
            return result;
        }
        /// <summary>
        /// 是否是Integer
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsInteger(string str)
        {
            int num;
            return int.TryParse(str, out num);
        }
        [System.Obsolete("方法名写错了,请用相当的方法", false)]
        public static bool IsNum(string str)
        {
            double num;
            return double.TryParse(str, out num);
        }
        [System.Obsolete("方法名写错了，请用相当的方法")]
        public static bool IsNumber(string str)
        {
            double num;
            return double.TryParse(str, out num);
        }
        /// <summary>
        /// 是否是double
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsDouble(string str)
        {
            double num;
            return double.TryParse(str, out num);
        }
        /// <summary>
        /// 是否是single
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsSingle(string str)
        {
            float num;
            return float.TryParse(str, out num);
        }
        /// <summary>
        /// 是否是ip地址
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool IsIP(string ip)
        {
            return Regex.IsMatch(ip, "");
        }
        /// <summary>
        /// 是否是手机号
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsCellPhone(string str)
        {
            Regex regex = new Regex("");
            return regex.IsMatch(str);
        }
        /// <summary>
        /// 是否是固话号
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsPhone(string str)
        {
            Regex regex = new Regex("");
            return regex.IsMatch(str);
        }
        /// <summary>
        /// 是否是邮箱地址
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsEmail(string str)
        {
            Regex regex = new Regex("");
            return regex.IsMatch(str);
        }
        /// <summary>
        /// 是否是中国公民身份证号
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static bool IsIDCard(string Id)
        {
            bool result;
            if (Id.Length == 18)
            {
                result = StringHelper.isIDCard18(Id);
            }
            else
            {
                result = (Id.Length == 15 && StringHelper.isIDCard15(Id));
            }
            return result;
        }
        private static bool isIDCard18(string Id)
        {
            long num = 0L;
            bool result;
            if (!long.TryParse(Id.Remove(17), out num) || (double)num < System.Math.Pow(10.0, 16.0) || !long.TryParse(Id.Replace('x', '0').Replace('X', '0'), out num))
            {
                result = false;
            }
            else
            {
                string text = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
                if (text.IndexOf(Id.Remove(2)) == -1)
                {
                    result = false;
                }
                else
                {
                    string s = Id.Substring(6, 8).Insert(6, "-").Insert(4, "-");
                    System.DateTime dateTime = default(System.DateTime);
                    if (!System.DateTime.TryParse(s, out dateTime))
                    {
                        result = false;
                    }
                    else
                    {
                        string[] array = "1,0,x,9,8,7,6,5,4,3,2".Split(new char[]
						{
							','
						});
                        string[] array2 = "7,9,10,5,8,4,2,1,6,3,7,9,10,5,8,4,2".Split(new char[]
						{
							','
						});
                        char[] array3 = Id.Remove(17).ToCharArray();
                        int num2 = 0;
                        for (int i = 0; i < 17; i++)
                        {
                            num2 += int.Parse(array2[i]) * int.Parse(array3[i].ToString());
                        }
                        int num3 = -1;
                        System.Math.DivRem(num2, 11, out num3);
                        result = !(array[num3] != Id.Substring(17, 1).ToLower());
                    }
                }
            }
            return result;
        }
        private static bool isIDCard15(string Id)
        {
            long num = 0L;
            bool result;
            if (!long.TryParse(Id, out num) || (double)num < System.Math.Pow(10.0, 14.0))
            {
                result = false;
            }
            else
            {
                string text = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
                if (text.IndexOf(Id.Remove(2)) == -1)
                {
                    result = false;
                }
                else
                {
                    string s = Id.Substring(6, 6).Insert(4, "-").Insert(2, "-");
                    System.DateTime dateTime = default(System.DateTime);
                    result = System.DateTime.TryParse(s, out dateTime);
                }
            }
            return result;
        }
        public static bool CustomRegex(string inputStr, string express)
        {
            Regex regex = new Regex(express);
            return regex.IsMatch(inputStr);
        }
        /// <summary>
        /// 根据区位得到首字母
        /// </summary>
        /// <param name="GBCode">区位</param>
        /// <returns></returns>
        private static string GetX(int GBCode)
        {
            string result;
            if (GBCode >= 1601 && GBCode < 1637)
            {
                result = "A";
            }
            else if (GBCode >= 1637 && GBCode < 1833)
            {
                result = "B";
            }
            else if (GBCode >= 1833 && GBCode < 2078)
            {
                result = "C";
            }
            else if (GBCode >= 2078 && GBCode < 2274)
            {
                result = "D";
            }
            else if (GBCode >= 2274 && GBCode < 2302)
            {
                result = "E";
            }
            else if (GBCode >= 2302 && GBCode < 2433)
            {
                result = "F";
            }
            else if (GBCode >= 2433 && GBCode < 2594)
            {
                result = "G";
            }
            else if (GBCode >= 2594 && GBCode < 2787)
            {
                result = "H";
            }
            else if (GBCode >= 2787 && GBCode < 3106)
            {
                result = "J";
            }
            else if (GBCode >= 3106 && GBCode < 3212)
            {
                result = "K";
            }
            else if (GBCode >= 3212 && GBCode < 3472)
            {
                result = "L";
            }
            else if (GBCode >= 3472 && GBCode < 3635)
            {
                result = "M";
            }
            else if (GBCode >= 3635 && GBCode < 3722)
            {
                result = "N";
            }
            else if (GBCode >= 3722 && GBCode < 3730)
            {
                result = "O";
            }
            else if (GBCode >= 3730 && GBCode < 3858)
            {
                result = "P";
            }
            else if (GBCode >= 3858 && GBCode < 4027)
            {
                result = "Q";
            }
            else if (GBCode >= 4027 && GBCode < 4086)
            {
                result = "R";
            }
            else if (GBCode >= 4086 && GBCode < 4390)
            {
                result = "S";
            }
            else if (GBCode >= 4390 && GBCode < 4558)
            {
                result = "T";
            }
            else if (GBCode >= 4558 && GBCode < 4684)
            {
                result = "W";
            }
            else if (GBCode >= 4684 && GBCode < 4925)
            {
                result = "X";
            }
            else if (GBCode >= 4925 && GBCode < 5249)
            {
                result = "Y";
            }
            else if (GBCode >= 5249 && GBCode <= 5589)
            {
                result = "Z";
            }
            else if (GBCode >= 5601 && GBCode <= 8794)
            {
                string text = "cjwgnspgcenegypbtwxzdxykygtpjnmjqmbsgzscyjsyyfpggbzgydywjkgaljswkbjqhyjwpdzlsgmrybywwccgznkydgttngjeyekzydcjnmcylqlypyqbqrpzslwbdgkjfyxjwcltbncxjjjjcxdtqsqzycdxxhgckbphffsspybgmxjbbyglbhlssmzmpjhsojnghdzcdklgjhsgqzhxqgkezzwymcscjnyetxadzpmdssmzjjqjyzcjjfwqjbdzbjgdnzcbwhgxhqkmwfbpbqdtjjzkqhylcgxfptyjyyzpsjlfchmqshgmmxsxjpkdcmbbqbefsjwhwwgckpylqbgldlcctnmaeddksjngkcsgxlhzaybdbtsdkdylhgymylcxpycjndqjwxqxfyyfjlejbzrwccqhqcsbzkymgplbmcrqcflnymyqmsqtrbcjthztqfrxchxmcjcjlxqgjmshzkbswxemdlckfsydsglycjjssjnqbjctyhbftdcyjdgwyghqfrxwckqkxebpdjpxjqsrmebwgjlbjslyysmdxlclqkxlhtjrjjmbjhxhwywcbhtrxxglhjhfbmgykldyxzpplggpmtcbbajjzyljtyanjgbjflqgdzyqcaxbkclecjsznslyzhlxlzcghbxzhznytdsbcjkdlzayffydlabbgqszkggldndnyskjshdlxxbcghxyggdjmmzngmmccgwzszxsjbznmlzdthcqydbdllscddnlkjyhjsycjlkohqasdhnhcsgaehdaashtcplcpqybsdmpjlpcjaqlcdhjjasprchngjnlhlyyqyhwzpnccgwwmzffjqqqqxxaclbhkdjxdgmmydjxzllsygxgkjrywzwyclzmcsjzldbndcfcxyhlschycjqppqagmnyxpfrkssbjlyxyjjglnscmhcwwmnzjjlhmhchsyppttxrycsxbyhcsmxjsxnbwgpxxtaybgajcxlypdccwqocwkccsbnhcpdyznbcyytyckskybsqkkytqqxfcwchcwkelcqbsqyjqcclmthsywhmktlkjlychwheqjhtjhppqpqscfymmcmgbmhglgsllysdllljpchmjhwljcyhzjxhdxjlhxrswlwzjcbxmhzqxsdzpmgfcsglsdymjshxpjxomyqknmyblrthbcftpmgyxlchlhlzylxgsssscclsldclepbhshxyyfhbmgdfycnjqwlqhjjcywjztejjdhfblqxtqkwhdchqxagtlxljxmsljhdzkzjecxjcjnmbbjcsfywkbjzghysdcpqyrsljpclpwxsdwejbjcbcnaytmgmbapclyqbclzxcbnmsggfnzjjbzsfqyndxhpcqkzczwalsbccjxpozgwkybsgxfcfcdkhjbstlqfsgdslqwzkxtmhsbgzhjcrglyjbpmljsxlcjqqhzmjczydjwbmjklddpmjegxyhylxhlqyqhkycwcjmyhxnatjhyccxzpcqlbzwwwtwbqcmlbmynjcccxbbsnzzljpljxyztzlgcldcklyrzzgqtgjhhgjljaxfgfjzslcfdqzlclgjdjcsnclljpjqdcclcjxmyzftsxgcgsbrzxjqqcczhgyjdjqqlzxjyldlbcyamcstylbdjbyregklzdzhldszchznwczcllwjqjjjkdgjcolbbzppglghtgzcygezmycnqcycyhbhgxkamtxyxnbskyzzgjzlqjdfcjxdygjqjjpmgwgjjjpkjsbgbmmcjssclpqpdxcdyykypcjddyygywchjrtgcnyqldkljczzgzccjgdyksgpzmdlcphnjafyzdjcnmwescsglbtzcgmsdllyxqsxsbljsbbsgghfjlwpmzjnlyywdqshzxtyywhmcyhywdbxbtlmswyyfsbjcbdxxlhjhfpsxzqhfzmqcztqcxzxrdkdjhnnyzqqfnqdmmgnydxmjgdhcdycbffallztdltfkmxqzdngeqdbdczjdxbzgsqqddjcmbkxffxmkdmcsychzcmljdjynhprsjmkmpcklgdbqtfzswtfgglyplljzhgjjgypzltcsmcnbtjbhfkdhbyzgkpbbymtdlsxsbnpdkleycjnycdykzddhqgsdzsctarlltkzlgecllkjljjaqnbdggghfjtzqjsecshalqfmmgjnlyjbbtmlycxdcjpldlpcqdhsycbzsckbzmsljflhrbjsnbrgjhxpdgdjybzgdlgcsezgxlblgyxtwmabchecmwyjyzlljjshlgndjlslygkdzpzxjyyzlpcxszfgwyydlyhcljscmbjhblyjlycblydpdqysxktbytdkdxjypcnrjmfdjgklccjbctbjddbblblcdqrppxjcglzcshltoljnmdddlngkaqakgjgyhheznmshrphqqjchgmfprxcjgdychghlyrzqlcngjnzsqdkqjymszswlcfqjqxgbggxmdjwlmcrnfkkfsyyljbmqammmycctbshcptxxzzsmphfshmclmldjfyqxsdyjdjjzzhqpdszglssjbckbxyqzjsgpsxjzqznqtbdkwxjkhhgflbcsmdldgdzdblzkycqnncsybzbfglzzxswmsccmqnjqsbdqsjtxxmbldxcclzshzcxrqjgjylxzfjphymzqqydfqjjlcznzjcdgzygcdxmzysctlkphtxhtlbjxjlxscdqccbbqjfqzfsltjbtkqbsxjjljchczdbzjdczjccprnlqcgpfczlclcxzdmxmphgsgzgszzqjxlwtjpfsyaslcjbtckwcwmytcsjjljcqlwzmalbxyfbpnlschtgjwejjxxglljstgshjqlzfkcgnndszfdeqfhbsaqdgylbxmmygszldydjmjjrgbjgkgdhgkblgkbdmbylxwcxyttybkmrjjzxqjbhlmhmjjzmqasldcyxyqdlqcafywyxqhz";
                string text2 = GBCode.ToString();
                int num = (int)((System.Convert.ToInt16(text2.Substring(0, 2)) - 56) * 94 + System.Convert.ToInt16(text2.Substring(text2.Length - 2, 2)));
                result = text.Substring(num - 1, 1);
            }
            else
            {
                result = " ";
            }
            return result;
        }
        /// <summary>
        /// 得到单个汉字首字母大写
        /// </summary>
        /// <param name="OneIndexTxt"></param>
        /// <returns></returns>
        private static string GetOneIndex(string OneIndexTxt)
        {
            string result;
            if (System.Convert.ToChar(OneIndexTxt) >= '\0' && System.Convert.ToChar(OneIndexTxt) < 'Ā')
            {
                result = OneIndexTxt;
            }
            else
            {
                System.Text.Encoding encoding = System.Text.Encoding.GetEncoding("gb2312");
                byte[] bytes = System.Text.Encoding.Unicode.GetBytes(OneIndexTxt);
                byte[] array = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, encoding, bytes);
                result = StringHelper.GetX(System.Convert.ToInt32(string.Format("{0:D2}", (int)(System.Convert.ToInt16(array[0]) - 160)) + string.Format("{0:D2}", (int)(System.Convert.ToInt16(array[1]) - 160))));
            }
            return result;
        }
        /// <summary>
        /// 得到汉字字符串的首字母大写
        /// </summary>
        /// <param name="IndexTxt"></param>
        /// <returns></returns>
        public static string GetChineseIndex(string IndexTxt)
        {
            string text = null;
            for (int i = 0; i < IndexTxt.Length; i++)
            {
                text += StringHelper.GetOneIndex(IndexTxt.Substring(i, 1));
            }
            return text;
        }
        /// <summary>
        /// 得到汉字的拼音字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GetChineseSpell(string str)
        {
            byte[] array = new byte[2];
            string text = "";
            char[] array2 = str.ToCharArray();
            for (int i = 0; i < array2.Length; i++)
            {
                array = System.Text.Encoding.Default.GetBytes(array2[i].ToString());
                if (array[0] <= 160 && array[0] >= 0)
                {
                    text += array2[i];
                }
                else
                {
                    for (int j = StringHelper.iA.Length - 1; j >= 0; j--)
                    {
                        if (StringHelper.iA[j] <= (int)array[0] * 256 + (int)array[1] - 65536)
                        {
                            text += StringHelper.sA[j];
                            break;
                        }
                    }
                }
            }
            return text;
        }
        /// <summary>
        /// 前补0
        /// </summary>
        /// <param name="value">字符串</param>
        /// <param name="size">总长度</param>
        /// <returns></returns>
        public static string FillZero(string value, int size)
        {
            string str = "";
            for (int i = 0; i < size - value.Length; i++)
            {
                str += "0";
            }
            return str + value;
        }
        /// <summary>
        /// 截取字符串
        /// </summary>
        /// <param name="str">原字符串</param>
        /// <param name="number">截取数量</param>
        /// <returns></returns>
        public static string InterceptString(string str, int number)
        {
            string result;
            if (str == null || str.Length == 0 || number <= 0)
            {
                result = "";
            }
            else
            {
                int byteCount = System.Text.Encoding.GetEncoding("Shift_JIS").GetByteCount(str);
                if (byteCount > number)
                {
                    int num = 0;
                    for (int i = 0; i < str.Length; i++)
                    {
                        int byteCount2 = System.Text.Encoding.GetEncoding("Shift_JIS").GetByteCount(new char[]
						{
							str[i]
						});
                        num += byteCount2;
                        if (num == number)
                        {
                            str = str.Substring(0, i + 1);
                            break;
                        }
                        if (num > number)
                        {
                            str = str.Substring(0, i);
                            break;
                        }
                    }
                }
                result = str;
            }
            return result;
        }
        /// <summary>
        /// 截取字符串，以“.”结束
        /// </summary>
        /// <param name="str">原字符串</param>
        /// <param name="number">截取数量</param>
        /// <returns></returns>
        public static string InterceptStringEndDot(string str, int number)
        {
            string result;
            if (str == null || str.Length == 0 || number <= 0)
            {
                result = "";
            }
            else
            {
                int byteCount = System.Text.Encoding.GetEncoding("Shift_JIS").GetByteCount(str);
                if (byteCount > number)
                {
                    int num = 0;
                    for (int i = 0; i < str.Length; i++)
                    {
                        int byteCount2 = System.Text.Encoding.GetEncoding("Shift_JIS").GetByteCount(new char[]
						{
							str[i]
						});
                        num += byteCount2;
                        if (num == number)
                        {
                            str = str.Substring(0, i + 1) + "…";
                            break;
                        }
                        if (num > number)
                        {
                            str = str.Substring(0, i) + "…";
                            break;
                        }
                    }
                }
                result = str;
            }
            return result;
        }
        /// <summary>
        /// 判断字符串是否在几个字符之中
        /// </summary>
        /// <param name="str">要判断的字符串</param>
        /// <param name="strs">几个字符串，就是范围</param>
        /// <returns>如果在返回true，否则返回false</returns>
        public static bool IsIn(string str, params string[] strs)
        {
            bool result;
            for (int i = 0; i < strs.Length; i++)
            {
                string a = strs[i];
                if (a == str)
                {
                    result = true;
                    return result;
                }
            }
            result = false;
            return result;
        }
        /// <summary>
        /// 得到随机数
        /// </summary>
        /// <param name="count">个数</param>
        /// <returns></returns>
        public static string GetCheckCode(int count)
        {
            char[] array = new char[]
			{
				'1',
				'2',
				'3',
				'4',
				'5',
				'6',
				'8',
				'9',
				'A',
				'B',
				'C',
				'D',
				'E',
				'F',
				'G',
				'H',
				'I',
				'J',
				'K',
				'L',
				'M',
				'N',
				'P',
				'Q',
				'R',
				'S',
				'T',
				'U',
				'V',
				'W',
				'X',
				'Y',
				'Z',
				'a',
				'b',
				'c',
				'd',
				'e',
				'f',
				'g',
				'h',
				'i',
				'j',
				'k',
				'l',
				'm',
				'n',
				'p',
				'q',
				'r',
				's',
				't',
				'u',
				'v',
				'w',
				'x',
				'y',
				'z'
			};
            System.Random random = new System.Random();
            string text = string.Empty;
            for (int i = 0; i < count; i++)
            {
                text += array[random.Next(array.Length)];
            }
            return text;
        }
        /// <summary>
        /// 得到0-10的汉字显示
        /// </summary>
        /// <param name="number"></param>
        /// <param name="isTraditional"></param>
        /// <returns></returns>
        public static string GetChineseNumber(int number, bool isTraditional)
        {
            string text = number.ToString();
            string text2 = "";
            string text3 = text;
            for (int i = 0; i < text3.Length; i++)
            {
                text2 += StringHelper.GetChineseNumber(text3[i].ToString(), isTraditional);
            }
            return text2;
        }
        /// <summary>
        /// 得到0-10的汉字显示
        /// </summary>
        /// <param name="number">数字</param>
        /// <param name="isTraditional">是否是繁体</param>
        /// <returns></returns>
        public static string GetChineseNumber(string number, bool isTraditional)
        {
            string result;
            switch (number)
            {
                case "1":
                    result = (isTraditional ? "壹" : "一");
                    return result;
                case "2":
                    result = (isTraditional ? "贰" : "二");
                    return result;
                case "3":
                    result = (isTraditional ? "叁" : "三");
                    return result;
                case "4":
                    result = (isTraditional ? "肆" : "四");
                    return result;
                case "5":
                    result = (isTraditional ? "伍" : "五");
                    return result;
                case "6":
                    result = (isTraditional ? "陆" : "六");
                    return result;
                case "7":
                    result = (isTraditional ? "柒" : "七");
                    return result;
                case "8":
                    result = (isTraditional ? "捌" : "八");
                    return result;
                case "9":
                    result = (isTraditional ? "玖" : "九");
                    return result;
                case "10":
                    result = (isTraditional ? "拾" : "十");
                    return result;
                case "0":
                    result = (isTraditional ? "零" : "〇");
                    return result;
            }
            result = string.Empty;
            return result;
        }
        /// <summary>
        /// 根据DataTable生成json对象数组格式
        /// </summary>
        /// <param name="dt">DataTable</param>
        /// <returns></returns>
        public static string ToJSON(DataTable dt)
        {
            string result;
            if (dt.Rows.Count == 0)
            {
                result = string.Empty;
            }
            else
            {
                System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
                stringBuilder.Append("[");
                DataRowCollection rows = dt.Rows;
                for (int i = 0; i < rows.Count; i++)
                {
                    stringBuilder.Append("{");
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        stringBuilder.AppendFormat("\"{0}\":", dt.Columns[j].ColumnName);
                        string jSONValue = StringHelper.getJSONValue(rows[i][j]);
                        if (j == dt.Columns.Count - 1)
                        {
                            stringBuilder.Append(jSONValue);
                        }
                        else
                        {
                            stringBuilder.AppendFormat("{0},", jSONValue);
                        }
                    }
                    stringBuilder.Append("},");
                }
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
                stringBuilder.Append("]");
                result = stringBuilder.ToString();
            }
            return result;
        }
        /// <summary>
        /// 根据DataRow生成json对象格式
        /// </summary>
        /// <param name="dr">DataRow</param>
        /// <returns></returns>
        public static string ToJSON(DataRow dr)
        {
            DataColumnCollection columns = dr.Table.Columns;
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            stringBuilder.Append("{");
            for (int i = 0; i < columns.Count; i++)
            {
                stringBuilder.AppendFormat("\"{0}\":", columns[i].ColumnName);
                string jSONValue = StringHelper.getJSONValue(dr[i]);
                if (i == columns.Count - 1)
                {
                    stringBuilder.Append(jSONValue);
                }
                else
                {
                    stringBuilder.AppendFormat("{0},", jSONValue);
                }
            }
            stringBuilder.Append("}");
            return stringBuilder.ToString();
        }
        /// <summary>
        /// 根据对象生成json对象格式
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        public static string ToJSON(object obj)
        {
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            stringBuilder.Append("{");
            System.Type type = obj.GetType();
            System.Reflection.PropertyInfo[] properties = type.GetProperties();
            for (int i = 0; i < properties.Length; i++)
            {
                if (i == properties.Length - 1)
                {
                    stringBuilder.AppendFormat("\"{0}\":{1}", properties[i].Name, StringHelper.getPropertyValue(obj, properties[i]));
                }
                else
                {
                    stringBuilder.AppendFormat("\"{0}\":{1},", properties[i].Name, StringHelper.getPropertyValue(obj, properties[i]));
                }
            }
            stringBuilder.Append("}");
            return stringBuilder.ToString();
        }
        /// <summary>
        /// 根据对象集合生成json对象数组格式
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="list">对象集合</param>
        /// <returns></returns>
        public static string ToJSON<T>(System.Collections.Generic.IList<T> list)
        {
            string result;
            if (list.Count == 0)
            {
                result = string.Empty;
            }
            else
            {
                System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
                string name = typeof(T).Name;
                stringBuilder.Append("[");
                for (int i = 0; i < list.Count; i++)
                {
                    if (i == list.Count - 1)
                    {
                        stringBuilder.Append(StringHelper.ToJSON(list[i]));
                    }
                    else
                    {
                        stringBuilder.AppendFormat("{0},", StringHelper.ToJSON(list[i]));
                    }
                }
                stringBuilder.Append("]");
                result = stringBuilder.ToString();
            }
            return result;
        }
        private static string getPropertyValue(object obj, System.Reflection.PropertyInfo pinfo)
        {
            object value = pinfo.GetValue(obj, null);
            string result;
            if (value == null)
            {
                result = "null";
            }
            else
            {
                result = StringHelper.getJSONValue(value);
            }
            return result;
        }
        private static string getJSONValue(object value)
        {
            return "\"" + value.ToString().Replace("\"", "\\\"") + "\"";
        }

    }
}

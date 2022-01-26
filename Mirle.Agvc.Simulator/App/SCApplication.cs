using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using System.Data;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using GenericParsing;
using com.mirle.ibg3k0.sc.Data.DAO;
using com.mirle.ibg3k0.sc.BLL;
using Mirle.Agvc.Simulator;
using com.mirle.ibg3k0.sc.Data.VO;

namespace com.mirle.ibg3k0.sc.App
{
    public class SCApplication
    {
        private static NLog.Logger logger = LogManager.GetCurrentClassLogger();

        private static Object _lock = new Object();
        private static SCApplication application;

        //config DAO
        private BlockDataDao blockDataDao = null;
        public BlockDataDao BlockDataDao { get { return blockDataDao; } }

        private AddressDataDao addressDataDao = null;
        public AddressDataDao AddressDataDao { get { return addressDataDao; } }

        //BLL
        private BlockDataBLL blockDataBLL = null;
        public BlockDataBLL BlockDataBLL { get { return blockDataBLL; } }

        private AddressDataBLL addressDataBLL = null;
        public AddressDataBLL AddressDataBLL { get { return addressDataBLL; } }

        private DataTable blockData = null;
        public DataTable BlockData { get { return blockData; } }

        private DataTable addressData = null;
        public DataTable AddressData { get { return addressData; } }

        public Dictionary<string, MiddleAgent> AgentDic = new Dictionary<string, MiddleAgent>();
        public Dictionary<string, string> VhAddressDic = new Dictionary<string, string>();  //<Remote Port, Address>

        public AddressData address = null;

        private SCApplication()
        {
            init();
        }

        public static SCApplication getInstance()
        {
            if (application == null)
            {
                lock (_lock)
                {
                    if (application == null)
                    {
                        application = new SCApplication();
                    }
                    return application;
                }
            }
            return application;

        }

        private void init()
        {
            blockDataDao = new BlockDataDao();
            blockDataBLL = new BlockDataBLL();
            addressDataDao = new AddressDataDao();
            addressDataBLL = new AddressDataBLL();

            blockDataBLL.start(this);
            addressDataBLL.start(this);

            loadBlockDataConfig();
        }

        public void loadBlockDataConfig()
        {
            loadCSVToDataTable(ref blockData, "BLOCKDATA");
            loadCSVToDataTable(ref addressData, "ADDRESSDATA");
        }

        private void loadCSVToDataTable(ref DataTable dt, string tableName)
        {
            using (GenericParser parser = new GenericParser())
            {
                parser.SetDataSource(Environment.CurrentDirectory + this.getString("CsvConfig", "") + @"\Config\" +tableName + ".csv");

                parser.ColumnDelimiter = ',';
                parser.FirstRowHasHeader = true;
                parser.MaxBufferSize = 1024;

                dt = new System.Data.DataTable(tableName);

                bool isfirst = true;
                while (parser.Read())
                {

                    int cs = parser.ColumnCount;
                    if (isfirst)
                    {
                        for (int i = 0; i < cs; i++)
                        {
                            dt.Columns.Add(parser.GetColumnName(i), typeof(string));
                        }
                        isfirst = false;
                    }


                    DataRow dr = dt.NewRow();

                    for (int i = 0; i < cs; i++)
                    {
                        string val = parser[i];

                        dr[i] = val;
                    }
                    dt.Rows.Add(dr);
                }
            }
        }

        object _updateLock = new object();
        public void updateVhAddress(string remote_port, string address)
        {
            lock(_updateLock)
            {
                try
                {
                    if (VhAddressDic.ContainsKey(remote_port))
                    {
                        VhAddressDic[remote_port] = address;
                    }
                    else
                    {
                        VhAddressDic.Add(remote_port, address);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception:");
                }
            }
        }

        object _checkLock = new object();
        public bool checkVhAddress(string remote_port, string address)
        {
            bool result = true;

            lock (_checkLock)
            {
                try
                {
                    List<string> PortLst = VhAddressDic.Keys.ToList();

                    foreach (string port in PortLst)
                    {
                        if (isMatche(port, remote_port))
                        {
                            continue;
                        }

                        if (isMatche(VhAddressDic[port], address))
                        {
                            result = false;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception:");
                }
            }

            return result;
        }

        object _deleteLock = new object();
        public void deleteVhAddress(string remote_port)
        {
            lock (_deleteLock)
            {
                try
                {
                    if (VhAddressDic.ContainsKey(remote_port))
                    {
                        VhAddressDic.Remove(remote_port);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception:");
                }
            }
        }

        private int getInt(string key, int defaultValue)
        {
            int rtn = defaultValue;
            try
            {
                rtn = Convert.ToInt32(ConfigurationManager.AppSettings.Get(key));
            }
            catch (Exception e)
            {
                logger.Warn("Get Config error[key:{0}][Exception:{1}]", key, e);
            }
            return rtn;
        }

        private long getLong(string key, long defaultValue)
        {
            long rtn = defaultValue;
            try
            {
                rtn = long.Parse(ConfigurationManager.AppSettings.Get(key));
            }
            catch (Exception e)
            {
                logger.Warn("Get Config error[key:{0}][Exception:{1}]", key, e);
            }
            return rtn;
        }

        private string getString(string key, string defaultValue)
        {
            string rtn = defaultValue;
            try
            {
                rtn = ConfigurationManager.AppSettings.Get(key);
            }
            catch (Exception e)
            {
                logger.Warn("Get Config error[key:{0}][Exception:{1}]", key, e);
            }
            return rtn;
        }

        /// <summary>
        /// 表示兩個物件內容是否相同
        /// </summary>
        /// <param name="obj1">The obj1.</param>
        /// <param name="obj2">The obj2.</param>
        /// <returns>Boolean.</returns>
        public Boolean isMatche(Object obj1, Object obj2)
        {
            try
            {
                if (obj1 == obj2) return true;
                if (obj1 == null || obj2 == null) return false;
                if (obj1.GetType().BaseType == typeof(Array))
                {
                    return isMatche((Array)obj1, (Array)obj2);
                }
                if (obj1.GetType().BaseType == typeof(Enum))
                {
                    return (Enum.Equals(obj1, obj2));
                }
                return isMatche(obj1.ToString(), obj2.ToString());
            }
            catch (Exception e)
            {
                logger.Warn("isMatche Has Exception:{0}", e);
            }
            return false;
        }

        /// <summary>
        /// 比較兩個陣列內容是否相同
        /// </summary>
        /// <param name="ary1">The ary1.</param>
        /// <param name="ary2">The ary2.</param>
        /// <returns>Boolean.</returns>
        public static Boolean isMatche(Array ary1, Array ary2)
        {
            try
            {
                if (ary1.Length != ary2.Length) return false;

                if (ary1.GetType() == typeof(UInt16[]))
                {
                    UInt16[] uAry1;
                    UInt16[] uAry2;
                    uAry1 = (UInt16[])ary1;
                    uAry2 = (UInt16[])ary2;
                    for (int i = 0; i < uAry1.Length; i++)
                    {
                        if (uAry1[i] != uAry2[i]) return false;
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                logger.Warn("isMatche(Array) Has Exception:{0}", e);
            }
            return false;
        }

        /// <summary>
        /// 比較兩個字串內容是否相同
        /// </summary>
        /// <param name="str1">The STR1.</param>
        /// <param name="str2">The STR2.</param>
        /// <returns>Boolean.</returns>
        public static Boolean isMatche(String str1, String str2)
        {
            try
            {
                if (str1 == str2) return true;
                if (str1 == null || str2 == null) return false;
                return str1.Trim().Equals(str2.Trim(), StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception e)
            {
                logger.Warn("isMatche Has Exception:{0}", e);
            }
            return false;
        }

        /// <summary>
        /// Determines whether the specified object is empty.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>Boolean.</returns>
        public Boolean isEmpty(Object obj)
        {
            if (obj == null)
            {
                return true;
            }
            if (obj is String)
            {
                if ((obj as String).Trim().Length <= 0)
                {
                    return true;
                }
            }
            if (obj.GetType().BaseType == typeof(Array))
            {
                if ((obj as Array).Length <= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}


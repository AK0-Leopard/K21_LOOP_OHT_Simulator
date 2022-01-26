using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.mirle.ibg3k0.sc.App;
using System.IO;
using System.Reflection;
using NLog;
using com.mirle.ibg3k0.sc.Data.VO;

namespace com.mirle.ibg3k0.sc.Data.DAO
{
    public class AddressDataDao
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public AddressData loadAddressByID(string address)
        {
            try
            {
                SCApplication scApp = SCApplication.getInstance();
                DataTable dt = scApp.AddressData;
                AddressData rtnAddressData = null;

                lock (scApp.AddressData)
                {
                    var query = from c in dt.AsEnumerable()
                                where c.Field<string>("ADDRESS").Trim() == address.Trim()
                                select new AddressData
                                {
                                    ADDRESS = c.Field<string>("ADDRESS"),
                                    SNED_ZONE = c.Field<string>("SEND_ZONE"),
                                    ZONE_ID = c.Field<string>("ZONE_ID"),
                                };

                    rtnAddressData = query.FirstOrDefault();
                }

                return rtnAddressData;
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

        public AddressData loadAllAddress()
        {
            try
            {
                SCApplication scApp = SCApplication.getInstance();
                DataTable dt = scApp.AddressData;
                AddressData rtnAddressData = null;

                lock (scApp.AddressData)
                {
                    var query = from c in dt.AsEnumerable()
                                select new AddressData
                                {
                                    ADDRESS = c.Field<string>("ADDRESS"),
                                    SNED_ZONE = c.Field<string>("SEND_ZONE"),
                                };

                    rtnAddressData = query.FirstOrDefault();
                }

                return rtnAddressData;
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

    }
}

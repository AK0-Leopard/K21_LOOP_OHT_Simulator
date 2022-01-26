
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Data.DAO;
using com.mirle.ibg3k0.sc.Data.VO;
using NLog;

namespace com.mirle.ibg3k0.sc.BLL
{
    public class AddressDataBLL
    {
        private SCApplication scApp = null;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private AddressDataDao addressDataDao = null;

        public AddressDataBLL()
        {

        }

        public void start(SCApplication scApp)
        {
            this.scApp = scApp;
            addressDataDao = scApp.AddressDataDao;
        }

        public AddressData loadAddressByID(string address)
        {
            AddressData addressData = null;

            try
            {
                addressData = addressDataDao.loadAddressByID(address);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
            }

            return addressData;
        }

        public AddressData loadAllAddress()
        {
            AddressData addressData = null;

            try
            {
                addressData = addressDataDao.loadAllAddress();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
            }

            return addressData;
        }
    }
}

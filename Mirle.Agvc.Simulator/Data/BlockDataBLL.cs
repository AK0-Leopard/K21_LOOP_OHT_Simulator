
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
    public class BlockDataBLL
    {
        private SCApplication scApp = null;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private BlockDataDao blockDataDao = null;

        public BlockDataBLL()
        {

        }

        public void start(SCApplication scApp)
        {
            this.scApp = scApp;
            blockDataDao = scApp.BlockDataDao;
        }

        public BlockData loadBlockByID(string block_id)
        {
            BlockData blockData = null;

            try
            {
                blockData = blockDataDao.loadBlockByID(block_id);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
            }

            return blockData;
        }
    }
}

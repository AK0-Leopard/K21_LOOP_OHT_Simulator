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
    public class BlockDataDao
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public BlockData loadBlockByID(string block_id)
        {
            try
            {
                SCApplication scApp = SCApplication.getInstance();
                DataTable dt = scApp.BlockData;
                BlockData rtnBlockData = null;

                lock (scApp.BlockData)
                {
                    var query = from c in dt.AsEnumerable()
                                where c.Field<string>("BLOCK_ID").Trim() == block_id.Trim()
                                select new BlockData
                                {
                                    Block_ID = c.Field<string>("BLOCK_ID"),
                                    XAxis = c.Field<string>("XAxis"),
                                    YAxis = c.Field<string>("YAxis"),
                                    Angle = c.Field<string>("Angle")
                                };

                    rtnBlockData = query.FirstOrDefault();
                }

                return rtnBlockData;
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }
    }
}

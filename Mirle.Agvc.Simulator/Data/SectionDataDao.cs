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
    public class SectionDataDao
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public SectionData loadSectionByID(string sec_id)
        {
            try
            {
                SCApplication scApp = SCApplication.getInstance();
                DataTable dt = scApp.SectionData;
                SectionData rtnSectionData = null;

                lock (scApp.SectionData)
                {
                    var query = from c in dt.AsEnumerable()
                                where c.Field<string>("SEC_ID").Trim() == sec_id.Trim()
                                select new SectionData
                                {
                                    SEC_ID = c.Field<string>("SEC_ID"),
                                    FROM_ADR_ID = c.Field<string>("FROM_ADR_ID"),
                                    TO_ADR_ID = c.Field<string>("TO_ADR_ID"),
                                };

                    rtnSectionData = query.FirstOrDefault();
                }

                return rtnSectionData;
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }
        public SectionData loadSectionByTo_Add_ID(string to_add_id)
        {
            try
            {
                SCApplication scApp = SCApplication.getInstance();
                DataTable dt = scApp.SectionData;
                SectionData rtnSectionData = null;

                lock (scApp.SectionData)
                {
                    var query = from c in dt.AsEnumerable()
                                where c.Field<string>("TO_ADR_ID").Trim() == to_add_id.Trim()
                                select new SectionData
                                {
                                    SEC_ID = c.Field<string>("SEC_ID"),
                                    FROM_ADR_ID = c.Field<string>("FROM_ADR_ID"),
                                    TO_ADR_ID = c.Field<string>("TO_ADR_ID"),
                                };

                    rtnSectionData = query.FirstOrDefault();
                }

                return rtnSectionData;
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

        public SectionData loadSectionByFrom_Add_ID(string from_add_id)
        {
            try
            {
                SCApplication scApp = SCApplication.getInstance();
                DataTable dt = scApp.SectionData;
                SectionData rtnSectionData = null;

                lock (scApp.SectionData)
                {
                    var query = from c in dt.AsEnumerable()
                                where c.Field<string>("FROM_ADR_ID").Trim() == from_add_id.Trim()
                                select new SectionData
                                {
                                    SEC_ID = c.Field<string>("SEC_ID"),
                                    FROM_ADR_ID = c.Field<string>("FROM_ADR_ID"),
                                    TO_ADR_ID = c.Field<string>("TO_ADR_ID"),
                                };

                    rtnSectionData = query.FirstOrDefault();
                }

                return rtnSectionData;
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

        public SectionData loadAllSections()
        {
            try
            {
                SCApplication scApp = SCApplication.getInstance();
                DataTable dt = scApp.SectionData;
                SectionData rtnSectionData = null;

                lock (scApp.AddressData)
                {
                    var query = from c in dt.AsEnumerable()
                                select new SectionData
                                {
                                    SEC_ID = c.Field<string>("SEC_ID"),
                                    FROM_ADR_ID = c.Field<string>("FROM_ADR_ID"),
                                    TO_ADR_ID = c.Field<string>("TO_ADR_ID"),
                                };

                    rtnSectionData = query.FirstOrDefault();
                }

                return rtnSectionData;
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

    }
}

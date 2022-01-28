
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
    public class SectionDataBLL
    {
        private SCApplication scApp = null;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private SectionDataDao sectionDataDao = null;

        public SectionDataBLL()
        {

        }

        public void start(SCApplication scApp)
        {
            this.scApp = scApp;
            sectionDataDao = scApp.SectionDataDao;
        }

        public SectionData loadSectionByID(string sec_id)
        {
            SectionData sectionData = null;

            try
            {
                sectionData = sectionDataDao.loadSectionByID(sec_id);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
            }

            return sectionData;
        }
        public SectionData loadSectionByTo_Add_ID(string toadd_id)
        {
            SectionData sectionData = null;

            try
            {
                sectionData = sectionDataDao.loadSectionByTo_Add_ID(toadd_id);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
            }

            return sectionData;
        }

        public SectionData loadSectionByFrom_Add_ID(string from_add_id)
        {
            SectionData sectionData = null;

            try
            {
                sectionData = sectionDataDao.loadSectionByFrom_Add_ID(from_add_id);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
            }

            return sectionData;
        }


        public SectionData loadAllSections()
        {
            SectionData sectionData = null;

            try
            {
                sectionData = sectionDataDao.loadAllSections();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
            }

            return sectionData;
        }
    }
}

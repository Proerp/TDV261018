using System.Linq;
using System.Data.Entity;
using System.Collections.Generic;

using TotalModel.Models;
using TotalCore.Repositories.Productions;

namespace TotalDAL.Repositories.Productions
{
    public class PlannedOrderRepository : GenericWithDetailRepository<PlannedOrder, PlannedOrderDetail>, IPlannedOrderRepository
    {
        public PlannedOrderRepository(TotalSmartPortalEntities totalSmartPortalEntities)
            : base(totalSmartPortalEntities, "PlannedOrderEditable", "PlannedOrderApproved", null, "PlannedOrderVoidable")
        {
        }
    }








    public class PlannedOrderAPIRepository : GenericAPIRepository, IPlannedOrderAPIRepository
    {
        public PlannedOrderAPIRepository(TotalSmartPortalEntities totalSmartPortalEntities)
            : base(totalSmartPortalEntities, "GetPlannedOrderIndexes")
        {
        }

        public IEnumerable<PlannedOrderLog> GetPlannedOrderLogs(int? plannedOrderID, int? firmOrderID)
        {
            this.TotalSmartPortalEntities.Configuration.ProxyCreationEnabled = false;
            IEnumerable<PlannedOrderLog> plannedOrderLogs = base.TotalSmartPortalEntities.GetPlannedOrderLogs(plannedOrderID, firmOrderID).ToList();
            this.TotalSmartPortalEntities.Configuration.ProxyCreationEnabled = true;

            return plannedOrderLogs;
        }
    }


}
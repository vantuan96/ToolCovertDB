using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolConvertSqlToMongoDB.Models.eazy
{
  
    public class WORK_Task
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ProjectId { get; set; }
        public string ProjectAppCode { get; set; }

        public string DateStart { get; set; }

        public string DateEnd { get; set; }

        public int SubTaskCount { get; set; }

        public int SubTaskCompleted { get; set; }

        public float SubTaskPercent { get; set; }

        public string DashboardStageId { get; set; }

        public bool IsStore { get; set; }

        public string DateCreated { get; set; }

        public string DateUpdated { get; set; }

        public bool IsCompleted { get; set; }

        public int SortOrder { get; set; }

        public bool IsConfirm { get; set; }

        public bool IsInProgress { get; set; }

        public string EmployeeId { get; set; }

        public string ContactID { get; set; }

        public string EnterpriseID { get; set; }

        public string ContractID { get; set; }

        public string CompletedDate { get; set; }
        public int CompletedPercent { get; set; }
        public int TaskPriorityID { get; set; }

        public int TaskStatusID { get; set; }

        public int TaskTypeID { get; set; }

        public bool IsReminder { get; set; }
        public int ReminderInterval { get; set; }
        public string ReminderDate { get; set; }

        public Guid OwnerID { get; set; }  
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string CreatedDate { get; set; }  
        public string ModifiedDate { get; set; }
        public string EmployeeJoinTask { get; set; }
        public string InvoiceID { get; set; }
         public bool IsPublic { get; set; }
         public bool IsService { get; set; }
         public string CampaignID { get; set; }
       

      
    }
}

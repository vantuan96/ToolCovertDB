using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ToolConvertSqlToMongoDB.Models;
using OfficeOpenXml;
using ToolConvertSqlToMongoDB.Models.eazy;

namespace ToolConvertSqlToMongoDB
{
    public partial class Eazy : Form
    {
        private static bool ischeck1 = false;
        private static bool ischeck2 = false;
        private static SqlConnection conn1;

        private static MongoClient connect;
        protected static IMongoClient _client;
        protected static IMongoDatabase _db;
        public Eazy()
        {
            InitializeComponent();
        }
        private void Eazy_Load(object sender, EventArgs e)
        {
            txtServer1.Focus();
            txtServer1.Text = "125.212.226.137";
            txtLogin1.Text = "adminLogin";
            txtPassword1.Text = "kztek123456";
            txtDatabase1.Text = "eazyerp_testing_futech";
            txtServer2.Text = "125.212.226.137";
            txtLogin2.Text = "adminLogin";
            txtPassword2.Text = "kztek123456";
            txtDatabase2.Text = "eazyerp_testing_futech";
            btnTestConnect2.Enabled = false;

        }

        private void btnTestConnect2_Click(object sender, EventArgs e)
        {
            btnTestConnect2.Enabled = false;

            #region validate
            if (string.IsNullOrEmpty(txtServer2.Text))
            {
                MessageBox.Show("Vui lòng nhập tên server");
                txtServer2.Focus();
                return;
            }
            if (string.IsNullOrEmpty(txtLogin2.Text))
            {
                MessageBox.Show("Vui lòng nhập tài khoản");
                txtLogin2.Focus();
                return;
            }
            if (string.IsNullOrEmpty(txtPassword2.Text))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu");
                txtPassword2.Focus();
                return;
            }
            if (string.IsNullOrEmpty(txtDatabase2.Text))
            {
                MessageBox.Show("Vui lòng nhập tên database");
                txtDatabase2.Focus();
                return;
            }

            #endregion

            try
            {
                if (ischeck1)
                {
                    ischeck2 = DBConnect.CheckConnection(txtServer2.Text.Trim(), txtLogin2.Text.Trim(), txtPassword2.Text.Trim(), conn1);
                }

                MessageBox.Show("Kết nối thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ischeck2 = false;
            }

            btnTestConnect2.Enabled = true;

            showExchangeData();
        }

        private void btnTestConnect1_Click(object sender, EventArgs e)
        {
            btnTestConnect1.Enabled = false;

            #region validate
            if (string.IsNullOrEmpty(txtServer1.Text))
            {
                MessageBox.Show("Vui lòng nhập tên server");
                txtServer1.Focus();
                return;
            }
            if (string.IsNullOrEmpty(txtLogin1.Text))
            {
                MessageBox.Show("Vui lòng nhập tài khoản");
                txtLogin1.Focus();
                return;
            }
            if (string.IsNullOrEmpty(txtPassword1.Text))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu");
                txtPassword1.Focus();
                return;
            }
            if (string.IsNullOrEmpty(txtDatabase1.Text))
            {
                MessageBox.Show("Vui lòng nhập tên database");
                txtDatabase1.Focus();
                return;
            }


            #endregion

            conn1 = DBConnect.GetDBConnection(txtServer1.Text.Trim(), txtDatabase1.Text.Trim(), txtLogin1.Text.Trim(), txtPassword1.Text.Trim());
            try
            {
                conn1.Open();
                MessageBox.Show("Kết nối thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ischeck1 = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message + " Vui lòng kiểm tra thông tin kết nối", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ischeck1 = false;
            }
            finally
            {
                conn1.Close();
            }

            btnTestConnect1.Enabled = true;

            //ẩn hiển chuyển đổi dữ liệu
            showExchangeData();

            //ẩn hiện nút test 2
            if (ischeck1)
            {
                btnTestConnect2.Enabled = true;
            }
            else
            {
                btnTestConnect2.Enabled = false;
            }
        }

        private void btnExchangeData_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtServer1.Text.Trim()) && !string.IsNullOrEmpty(txtServer2.Text.Trim()) && ischeck1 && ischeck2)
            {
                Task.Run(() =>
                {
                    btnExchangeData.Invoke(new Action(delegate { btnExchangeData.Enabled = false; }));
                    statusStrip1.Invoke(new Action(delegate
                    {
                        toolStripProgressBar1.MarqueeAnimationSpeed = 30;
                        toolStripProgressBar1.Style = ProgressBarStyle.Marquee;
                    }));


                    ExchangeData(conn1);

                    statusStrip1.Invoke(new Action(delegate
                    {
                        toolStripProgressBar1.Style = ProgressBarStyle.Continuous;
                        toolStripProgressBar1.MarqueeAnimationSpeed = 0;
                    }));

                    btnExchangeData.Invoke(new Action(delegate { btnExchangeData.Enabled = true; }));
                });
            }
        }
        /// <summary>
        /// chuyển đổi dữ liệu
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="conn"></param>
        public void ExchangeData(SqlConnection conn)
        {

            var path1 = "[" + txtServer1.Text.Trim() + "].[" + txtDatabase1.Text.Trim() + "]";
            var path2 = "[" + txtServer2.Text.Trim() + "].[" + txtDatabase2.Text.Trim() + "]";
            string con2 = "Data Source=125.212.226.137;Initial Catalog=eazyerp_testing_futech;User Id=adminLogin;Password=kztek123456;";
            try
            {
                #region

                // Lấy dữ liệu từ tblTask

                conn.Open();
                var query = new StringBuilder();
                query.AppendLine(string.Format("select * from {0}.dbo.[tblTask]", path2));
                var obj = DBConnect.GetTable(query.ToString(), con2, false);
                var lst = new List<WORK_Task>();
                foreach (DataRow item in obj.Rows)
                {
                    var obj1 = new WORK_Task();
                    obj1.Id = item["TaskID"].ToString();
                    obj1.Title = item["Subject"].ToString();
                    obj1.Description = item["Description"].ToString();
                    obj1.ProjectId = "";
                    obj1.ProjectAppCode = "";
                    obj1.DateStart = (item["StartDate"].ToString());
                    obj1.DateEnd = (item["DueDate"].ToString());
                    obj1.SubTaskCount = 0;
                    obj1.SubTaskCompleted = 0;
                    obj1.SubTaskPercent = Convert.ToInt32(item["CompletedPercent"].ToString());
                    obj1.DashboardStageId = "";
                    obj1.IsStore = false;
                    obj1.DateCreated = (item["CreatedDate"].ToString());
                    obj1.DateUpdated = (item["ModifiedDate"].ToString());
                    obj1.IsCompleted = false;
                    obj1.SortOrder = 0;
                    obj1.IsConfirm = false;
                    obj1.IsInProgress = false;
                    obj1.EmployeeId = (item["EmployeeID"].ToString());
                    obj1.ContactID = (item["ContactID"].ToString());
                    obj1.EnterpriseID = (item["EnterpriseID"].ToString());
                    obj1.ContractID = (item["ContractID"].ToString());
                    obj1.CompletedDate = (item["CompletedDate"].ToString());
                    obj1.CompletedPercent = Convert.ToInt32(item["CompletedPercent"].ToString());
                    obj1.IsReminder = item["IsReminder"].ToString() != null ? false : true;
                    obj1.EmployeeJoinTask = (item["EmployeeJoinTask"].ToString());
                    obj1.ReminderInterval = Convert.ToInt32(item["ReminderInterval"].ToString());
                    obj1.ReminderDate = DateTime.Now.ToString();
                    obj1.OwnerID = Guid.Parse(item["OwnerID"].ToString());
                    obj1.CreatedBy = (item["CreatedBy"].ToString());
                    obj1.CreatedDate = (item["CreatedDate"].ToString());
                    obj1.ModifiedBy = (item["ModifiedBy"].ToString());
                    obj1.ModifiedDate = (item["ModifiedDate"].ToString());
                    obj1.InvoiceID = item["InvoiceID"].ToString();
                    obj1.IsPublic = item["IsPublic"].ToString() != null ? false : true;
                    obj1.IsService = item["IsService"].ToString() != null ? false : true;
                    obj1.CampaignID = (item["CampaignID"].ToString());
                    lst.Add(obj1);
                }


                foreach (var item1 in lst)
                {
                    #region
                    var str = new StringBuilder();
                    str.AppendLine("INSERT INTO [dbo].[WORK_Task]([Id], [Title], [Description], [ProjectId], [ProjectAppCode], [DateStart], [DateEnd], [SubTaskCount], [SubTaskCompleted], [SubTaskPercent], [DashboardStageId], [IsStore], [DateCreated], [DateUpdated],[IsCompleted],[SortOrder],[IsConfirm],[IsInProgress],[EmployeeId],[ContactID],[CompletedDate],[CompletedPercent],[IsReminder],[EmployeeJoinTask],[ReminderInterval],[ReminderDate],[OwnerID],[CreatedBy],[CreatedDate],[ModifiedBy],[ModifiedDate],[InvoiceID],[IsPublic],[IsService],[CampaignID],[ContractID],[EnterpriseID])");

                    str.AppendLine("VALUES (");

                    str.AppendLine(string.Format("'{0}'", item1.Id));
                    str.AppendLine(string.Format(", N'{0}'", item1.Title));
                    str.AppendLine(string.Format(", N'{0}'", item1.Description));
                    str.AppendLine(string.Format(", '{0}'", item1.ProjectId));
                    str.AppendLine(string.Format(", '{0}'", item1.ProjectAppCode));
                    str.AppendLine(string.Format(", '{0}'", Convert.ToDateTime(item1.DateStart).ToString("yyyy/MM/dd")));
                    str.AppendLine(string.Format(", '{0}'", Convert.ToDateTime(item1.DateEnd).ToString("yyyy/MM/dd")));
                    str.AppendLine(string.Format(", '{0}'", item1.SubTaskCount));
                    str.AppendLine(string.Format(", '{0}'", item1.SubTaskCompleted));
                    str.AppendLine(string.Format(", '{0}'", item1.SubTaskPercent));
                    str.AppendLine(string.Format(", '{0}'", item1.DashboardStageId));
                    str.AppendLine(string.Format(", '{0}'", item1.IsStore));
                    str.AppendLine(string.Format(", '{0}'", Convert.ToDateTime(item1.DateCreated).ToString("yyyy/MM/dd")));
                    str.AppendLine(string.Format(", '{0}'", Convert.ToDateTime(item1.DateUpdated).ToString("yyyy/MM/dd")));
                    str.AppendLine(string.Format(", '{0}'", item1.IsCompleted));
                    str.AppendLine(string.Format(", '{0}'", item1.SortOrder));
                    str.AppendLine(string.Format(", '{0}'", item1.IsConfirm));
                    str.AppendLine(string.Format(", '{0}'", item1.IsInProgress));

                    str.AppendLine(string.Format(", '{0}'", item1.EmployeeId));
                    str.AppendLine(string.Format(", '{0}'", item1.ContactID));
                    str.AppendLine(string.Format(", '{0}'", Convert.ToDateTime(item1.CompletedDate).ToString("yyyy/MM/dd")));
                    str.AppendLine(string.Format(", '{0}'", item1.CompletedPercent));
                    str.AppendLine(string.Format(", '{0}'", item1.IsReminder));
                    str.AppendLine(string.Format(", '{0}'", item1.EmployeeJoinTask));
                    str.AppendLine(string.Format(", '{0}'", item1.ReminderInterval));
                    str.AppendLine(string.Format(", '{0}'", Convert.ToDateTime(item1.ReminderDate).ToString("yyyy/MM/dd")));
                    str.AppendLine(string.Format(", '{0}'", item1.OwnerID));
                    str.AppendLine(string.Format(", '{0}'", item1.CreatedBy));
                    str.AppendLine(string.Format(", '{0}'", Convert.ToDateTime(item1.CreatedDate).ToString("yyyy/MM/dd")));
                    str.AppendLine(string.Format(", '{0}'", item1.ModifiedBy));
                    str.AppendLine(string.Format(", '{0}'", Convert.ToDateTime(item1.ModifiedDate).ToString("yyyy/MM/dd")));
                    str.AppendLine(string.Format(", '{0}'", item1.InvoiceID));
                    str.AppendLine(string.Format(", '{0}'", item1.IsPublic));
                    str.AppendLine(string.Format(", '{0}'", item1.IsService));
                    str.AppendLine(string.Format(", '{0}'", item1.CampaignID));
                    str.AppendLine(string.Format(", '{0}'", item1.ContractID));
                    str.AppendLine(string.Format(", '{0}'", item1.EnterpriseID));
                    #endregion
                    str.AppendLine(")");
                    SqlCommand sqlCmd = new SqlCommand(str.ToString(), conn);
                    sqlCmd.CommandTimeout = 21600;
                    sqlCmd.ExecuteNonQuery();
                    #region work_tag

                    List<string> lst1 = new List<string>();
                    if (!item1.EmployeeJoinTask.Equals(""))
                    {


                        var arrAcLevel = item1.EmployeeJoinTask.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                        var attAL = string.Join(",", arrAcLevel);

                        foreach (var item2 in arrAcLevel)
                        {
                            var str1 = new StringBuilder();
                            str1.AppendLine("INSERT INTO [dbo].[WORK_Task_Tag]([Id], [TaskId], [EmployeeId])");
                            str1.AppendLine("VALUES (");
                            str1.AppendLine(string.Format("'{0}'", Guid.NewGuid().ToString()));
                            str1.AppendLine(string.Format(", N'{0}'", item1.Id.ToString()));
                            str1.AppendLine(string.Format(", N'{0}'", item2));
                            str1.AppendLine(")");

                            SqlCommand sqlCmd1 = new SqlCommand(str1.ToString(), conn);
                            sqlCmd1.CommandTimeout = 21600;
                            sqlCmd1.ExecuteNonQuery();
                        }


                    }
                    #endregion
                  

                }  
                #endregion
                MessageBox.Show("Chuyển đổi dữ liệu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);



            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
            finally
            {
                conn.Close();
            }

        }

        private void txtServer1_TextChanged(object sender, EventArgs e)
        {
            btnTestConnect2.Enabled = false;
            btnExchangeData.Enabled = false;

            ischeck2 = false;
        }

        private void txtLogin1_TextChanged(object sender, EventArgs e)
        {
            btnTestConnect2.Enabled = false;
            btnExchangeData.Enabled = false;

            ischeck2 = false;
        }

        private void txtPassword1_TextChanged(object sender, EventArgs e)
        {
            btnTestConnect2.Enabled = false;
            btnExchangeData.Enabled = false;

            ischeck2 = false;
        }

        private void txtDatabase1_TextChanged(object sender, EventArgs e)
        {
            btnTestConnect2.Enabled = false;
            btnExchangeData.Enabled = false;

            ischeck2 = false;
        }

        //ẩn hiện nút chuyển đổi dữ liệu
        void showExchangeData()
        {
            if (ischeck1 && ischeck2)
            {
                btnExchangeData.Enabled = true;

            }
            else
            {
                btnExchangeData.Enabled = false;

            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ischeck1)
            {
                //xóa linked
                DBConnect.RemoveLinked(txtServer2.Text.Trim(), conn1);
            }

        }

        /// <summary>
        /// đọc danh sách thẻ từ excel
        /// </summary>
        /// <param name="path"></param>
        /// <param name="errorText"></param>
        /// <returns></returns>


        /// <summary>
        /// đọc danh sách thẻ từ excel
        /// </summary>
        /// <param name="path"></param>
        /// <param name="errorText"></param>
        /// <returns></returns>

        /// <summary>
        /// Cập nhật thẻ
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="dt"></param>
        public void UpdateCard(SqlConnection conn, DataTable dt)
        {
            string l = "";
            try
            {
                conn.Open();
                var query = new StringBuilder();

                foreach (DataRow dr in dt.Rows)
                {
                    //Thẻ
                    var plate = dr["3"].ToString().Trim();
                    var canho = dr["2"].ToString().Trim();
                    var vehiclename = dr["5"].ToString().Trim();
                    var expiredate = dr["6"].ToString().Trim();
                    var cardnumber = dr["CardNumber"].ToString().Trim();
                    var cardno = dr["CardNo"].ToString().Trim();
                    var cardgroup = dr["CardGroup"].ToString().Trim();
                    l = plate + "-" + expiredate + "-" + canho;
                    //DateTime d = DateTime.ParseExact(!string.IsNullOrEmpty(expiredate) ? expiredate : DateTime.Now.ToString(), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                    //var a = d.ToString("yyyy/MM/dd");

                    if (!string.IsNullOrEmpty(cardnumber))
                    {
                        if (!string.IsNullOrEmpty(cardgroup))
                        {
                            InsertCardGroup(conn, cardgroup);
                        }

                        InsertCard(conn, cardno, cardnumber, plate, vehiclename, expiredate, cardgroup, canho);
                    }

                }



                MessageBox.Show("Cập nhật thẻ thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message + " " + l, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
            finally
            {
                conn.Close();
            }

        }

        /// <summary>
        /// thêm nhóm kh
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="dt"></param>
        public void InsertCardGroup(SqlConnection conn, string name)
        {
            var path1 = "[" + txtServer1.Text.Trim() + "].[" + txtDatabase1.Text.Trim() + "]";

            try
            {
                //conn.Open();
                var query = new StringBuilder();

                query.AppendLine("BEGIN");
                query.AppendLine(string.Format("IF NOT EXISTS (SELECT * FROM tblCardGroup WHERE CardGroupName = N'{0}')", name));
                query.AppendLine("BEGIN");
                query.AppendLine("INSERT INTO tblCardGroup(CardGroupID,CardGroupCode, CardGroupName, Inactive,CardType)     ");
                query.AppendLine(string.Format("VALUES (NEWID(),'',N'{0}','false','0')", name));
                query.AppendLine("END");
                query.AppendLine("END");

                SqlCommand cmd = new SqlCommand(query.ToString(), conn);
                cmd.CommandTimeout = 21600;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
            finally
            {
                //conn.Close();
            }

        }

        /// <summary>
        /// thêm thẻ
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="dt"></param>
        public void InsertCard(SqlConnection conn, string cardno, string cardnumber, string plate, string vehiclename, string expiredate, string cardgroupname, string customercode)
        {
            var path1 = "[" + txtServer1.Text.Trim() + "].[" + txtDatabase1.Text.Trim() + "]";

            try
            {
                //conn.Open();
                var query = new StringBuilder();
                query.AppendLine("DECLARE @CardGroupId varchar(50);");
                query.AppendLine("DECLARE @CustomerId varchar(50);");
                query.AppendLine(string.Format("SET @CardGroupId =  ISNULL((SELECT Convert(varchar(50),CardGroupID) FROM tblCardGroup WHERE CardGroupName = N'{0}' ),'')", cardgroupname));
                query.AppendLine(string.Format("SET @CustomerId =  ISNULL((SELECT Convert(varchar(50),CustomerID) FROM tblCustomer WHERE CustomerCode = '{0}' ),'')", customercode));

                query.AppendLine("BEGIN");
                query.AppendLine(string.Format("IF NOT EXISTS (SELECT * FROM tblCard WHERE CardNumber = '{0}')", cardnumber));
                query.AppendLine("BEGIN");
                query.AppendLine("INSERT INTO tblCard(CardID, CardNo, CardNumber,CardGroupID,CustomerID,Plate1,VehicleName1,IsDelete,DateActive,ImportDate,ExpireDate)");
                query.AppendLine(string.Format("VALUES (NEWID(),'{0}','{1}',@CardGroupId,@CustomerId,'{2}','{3}','false',GETDATE(),GETDATE(),{4})", cardno, cardnumber, plate, vehiclename, !string.IsNullOrEmpty(expiredate) ? "convert(datetime,'" + expiredate + "')" : "GETDATE()"));
                query.AppendLine("END");
                query.AppendLine("END");

                SqlCommand cmd = new SqlCommand(query.ToString(), conn);
                cmd.CommandTimeout = 21600;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
            finally
            {
                //conn.Close();
            }

        }

        /// <summary>
        /// Cập nhật khách hàng
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="dt"></param>
        public void UpdateCustomer(SqlConnection conn, DataTable dt)
        {
            var path1 = "[" + txtServer1.Text.Trim() + "].[" + txtDatabase1.Text.Trim() + "]";

            try
            {
                conn.Open();
                var query = new StringBuilder();

                foreach (DataRow dr in dt.Rows)
                {
                    //Thẻ
                    var toa = dr["TÒA"].ToString().Trim();
                    var canho = dr["CĂN HỘ"].ToString().Trim();
                    var customer = dr["CHỦ HỘ"].ToString().Trim();
                    var description = dr["NGƯỜI Ở"].ToString().Trim();
                    var phone = dr["SĐT"].ToString().Trim();

                    //thêm tòa
                    InsertCustomerGroup(conn, toa, "", "0");

                    InsertCustomerGroup(conn, canho, toa, "1");
                    //thêm kh
                    InsertCustomer(conn, customer, canho, description, phone);
                }


                MessageBox.Show("Cập nhật khách hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
            finally
            {
                conn.Close();
            }

        }

        /// <summary>
        /// thêm nhóm kh
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="dt"></param>
        public void InsertCustomerGroup(SqlConnection conn, string name, string nameParent, string type)
        {
            var path1 = "[" + txtServer1.Text.Trim() + "].[" + txtDatabase1.Text.Trim() + "]";

            try
            {
                //conn.Open();
                var query = new StringBuilder();
                query.AppendLine("DECLARE @ParenId varchar(50);");
                query.AppendLine(string.Format("SET @ParenId =  CASE WHEN '{0}' = '0'", type));
                query.AppendLine("THEN '0' ELSE");
                query.AppendLine(string.Format("(SELECT Convert(varchar(50),CustomerGroupID) FROM tblCustomerGroup WHERE CustomerGroupName = '{0}' )", nameParent));
                query.AppendLine("END");
                query.AppendLine("BEGIN");
                query.AppendLine(string.Format("IF NOT EXISTS (SELECT * FROM tblCustomerGroup WHERE CustomerGroupName = '{0}')", name));
                query.AppendLine("BEGIN");
                query.AppendLine("INSERT INTO tblCustomerGroup(CustomerGroupID, CustomerGroupName, Inactive,ParentID)     ");
                query.AppendLine(string.Format("VALUES (NEWID(),N'{0}','false',@ParenId)", name));
                query.AppendLine("END");
                query.AppendLine("END");

                SqlCommand cmd = new SqlCommand(query.ToString(), conn);
                cmd.CommandTimeout = 21600;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
            finally
            {
                //conn.Close();
            }

        }

        /// <summary>
        /// thêm kh
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="dt"></param>
        public void InsertCustomer(SqlConnection conn, string name, string code, string description, string mobile)
        {
            var path1 = "[" + txtServer1.Text.Trim() + "].[" + txtDatabase1.Text.Trim() + "]";

            try
            {
                //conn.Open();
                var query = new StringBuilder();
                query.AppendLine("DECLARE @CustomerGroupId varchar(50);");
                query.AppendLine(string.Format("SET @CustomerGroupId = (SELECT Convert(varchar(50),CustomerGroupID) FROM tblCustomerGroup WHERE CustomerGroupName = '{0}' )", code));

                query.AppendLine("BEGIN");
                query.AppendLine(string.Format("IF NOT EXISTS (SELECT * FROM tblCustomer WHERE CustomerCode = '{0}')", code));
                query.AppendLine("BEGIN");
                query.AppendLine("INSERT INTO tblCustomer(CustomerID, CustomerCode, CustomerName,CustomerGroupID,Inactive,Address,Mobile)");
                query.AppendLine(string.Format("VALUES (NEWID(),'{0}',N'{1}',@CustomerGroupId,'false',N'{2}','{3}')", code, name, description, mobile));
                query.AppendLine("END");
                query.AppendLine("END");

                SqlCommand cmd = new SqlCommand(query.ToString(), conn);
                cmd.CommandTimeout = 21600;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
            finally
            {
                // conn.Close();
            }

        }

        /// <summary>
        /// lấy id nhóm kh
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="dt"></param>
        public string GetCustomerGroupId(SqlConnection conn, string name)
        {
            var path1 = "[" + txtServer1.Text.Trim() + "].[" + txtDatabase1.Text.Trim() + "]";
            string customergroupId = "";
            try
            {
                conn.Open();
                var query = new StringBuilder();

                query.AppendLine(string.Format("SELECT CustomerGroupID FROM tblCustomerGroup WHERE CustomerGroupName = '{0}')", name));

                customergroupId = DBConnect.GetDataSet(query.ToString(), conn, false).Tables[0].Rows[0].ToString();

            }
            catch (Exception ex)
            {
                //MessageBox.Show("Error: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
            finally
            {
                conn.Close();
            }

            return customergroupId;
        }

        //private void btnInsertData_Click(object sender, EventArgs e)
        //{
        //    var type = cbType.Text;
        //    using (OpenFileDialog fileDialog1 = new OpenFileDialog())
        //    {
        //        fileDialog1.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";
        //        if (fileDialog1.ShowDialog() == DialogResult.OK)
        //        {
        //            Task.Run(() =>
        //            {
        //                btnInsertData.Invoke(new Action(delegate { btnInsertData.Enabled = true; }));
        //                statusStrip1.Invoke(new Action(delegate
        //                {
        //                    toolStripProgressBar1.MarqueeAnimationSpeed = 30;
        //                    toolStripProgressBar1.Style = ProgressBarStyle.Marquee;
        //                }));


        //                string path = fileDialog1.FileName;

        //                if (File.Exists(path))
        //                {
        //                    string txtError = "";
        //                    var dt = new DataTable();
        //                    if (!string.IsNullOrEmpty(type))
        //                    {
        //                        ////đọc file
        //                        if (type.Equals("Customer"))
        //                        {
        //                            dt = ReadFromExcelCustomer(path, ref txtError);

        //                            if (dt != null && dt.Rows.Count > 0)
        //                            {
        //                                //cập nhật mã thẻ
        //                                UpdateCustomer(conn1, dt);
        //                            }
        //                            else
        //                            {
        //                                MessageBox.Show("Không đọc được dữ liệu Excel!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            dt = ReadFromExcelCard(path, ref txtError);

        //                            if (dt != null && dt.Rows.Count > 0)
        //                            {
        //                                //cập nhật mã thẻ
        //                                UpdateCard(conn1, dt);
        //                            }
        //                            else
        //                            {
        //                                MessageBox.Show("Không đọc được dữ liệu Excel!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //                            }
        //                        }



        //                    }
        //                    else
        //                    {
        //                        MessageBox.Show("Vui lòng chọn kiểu dữ liệu!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //                    }

        //                }

        //                statusStrip1.Invoke(new Action(delegate
        //                {
        //                    toolStripProgressBar1.Style = ProgressBarStyle.Continuous;
        //                    toolStripProgressBar1.MarqueeAnimationSpeed = 0;
        //                }));

        //                btnInsertData.Invoke(new Action(delegate { btnInsertData.Enabled = true; }));
        //            });

        //        }
        //    }
        //}

        private void txtServer1_TextChanged_1(object sender, EventArgs e)
        {

        }


    }
}

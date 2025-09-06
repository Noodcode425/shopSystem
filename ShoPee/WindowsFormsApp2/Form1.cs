using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        // 請確認這裡有宣告 tabPage2
        private TabPage tabPage2;
        private TabControl tabControl1;

        public Form1()
        {
            InitializeComponent();

            // 初始化 tabControl1
            this.tabControl1 = new TabControl();
            this.tabControl1.Dock = DockStyle.Fill;
            this.Controls.Add(this.tabControl1);


            // 設定為整列選取
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false; // 若只允許單列選取

            // 修正：初始化 tabPage2，並將控制項加入 TabPage2
            this.tabPage2 = new TabPage("TabPage2");
            this.tabPage2.Controls.Add(this.textBoxSearch);

            // 將 tabPage2 加入 tabControl1
            this.tabControl1.Controls.Add(this.tabPage2);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string connStr = "Data Source=DESKTOP-HVUT3JB\\SQLEXPRESS;Initial Catalog=ShoPee;Integrated Security=True";
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlDataAdapter sqlDataAdapter1 = new SqlDataAdapter("SELECT * FROM orders_items", conn);
                DataSet ds = new DataSet();
                sqlDataAdapter1.Fill(ds);

                dataGridView1.DataSource = ds.Tables[0];

                // 設定 order_item_id 欄位唯讀且隱藏
                if (dataGridView1.Columns.Contains("order_item_id"))
                {
                    dataGridView1.Columns["order_item_id"].ReadOnly = true;
                    dataGridView1.Columns["order_item_id"].Visible = false; // 隱藏主鍵欄位
                }

                // 不允許使用者新增行
                dataGridView1.AllowUserToAddRows = false;
            }
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            // TODO: 這行程式碼會將資料載入 'shoPeeDataSet3.products' 資料表。您可以視需要進行移動或移除。
            this.productsTableAdapter1.Fill(this.shoPeeDataSet3.products);
            // TODO: 這行程式碼會將資料載入 'shoPeeDataSet2.products' 資料表。您可以視需要進行移動或移除。
            this.productsTableAdapter.Fill(this.shoPeeDataSet2.products);
            // TODO: 這行程式碼會將資料載入 'shoPeeDataSet1.product_inventory' 資料表。您可以視需要進行移動或移除。
            this.product_inventoryTableAdapter.Fill(this.shoPeeDataSet1.product_inventory);
            // TODO: 這行程式碼會將資料載入 'shoPeeDataSet.payments' 資料表。您可以視需要進行移動或移除。
            this.paymentsTableAdapter.Fill(this.shoPeeDataSet.payments);
            // TODO: 這行程式碼會將資料載入 'shoPeeOrder_itemSet.order_items' 資料表。您可以視需要進行移動或移除。
            this.order_itemsTableAdapter.Fill(this.shoPeeOrder_itemSet.order_items);

        }

        private void Fillbtn_Click(object sender, EventArgs e)
        {
            string connStr = "Data Source=DESKTOP-HVUT3JB\\SQLEXPRESS;Initial Catalog=ShoPee;Integrated Security=True";
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                SqlDataAdapter sqlDataAdapter1 = new SqlDataAdapter("SELECT * FROM orders_items", conn);
                DataSet ds = new DataSet();
                sqlDataAdapter1.Fill(ds);

                // 將資料顯示在 DataGridView
                dataGridView1.DataSource = ds.Tables[0];
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string keyword = textBox1.Text.Trim();
            if (string.IsNullOrEmpty(keyword))
                return;

            // 取消所有選取
            dataGridView1.ClearSelection();
            dataGridView2.ClearSelection();

            // dataGridView1 搜尋
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.IsNewRow) continue;
                if (row.Cells["orderidDataGridViewTextBoxColumn"].Value != null &&
                    row.Cells["orderidDataGridViewTextBoxColumn"].Value.ToString() == keyword)
                {
                    row.Selected = true;
                    dataGridView1.CurrentCell = row.Cells[0];
                    break;
                }
            }

            // dataGridView2 搜尋
            foreach (DataGridViewRow row in dataGridView2.Rows)
            {
                if (row.IsNewRow) continue;
                if (row.Cells["orderidDataGridViewTextBoxColumn1"].Value != null &&
                    row.Cells["orderidDataGridViewTextBoxColumn1"].Value.ToString() == keyword)
                {
                    row.Selected = true;
                    dataGridView2.CurrentCell = row.Cells[0];
                    break;
                }
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var selectedRow = dataGridView1.Rows[e.RowIndex];
                // 請確認這裡的欄位名稱正確
                var productId = selectedRow.Cells["productidDataGridViewTextBoxColumn"].Value?.ToString();
                label7.Text = $"product_id: {productId}";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string connStr = "Data Source=DESKTOP-HVUT3JB\\SQLEXPRESS;Initial Catalog=ShoPee;Integrated Security=True";
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    foreach (DataGridViewRow row in dataGridView3.Rows)
                    {
                        if (row.IsNewRow) continue;

                        // 取得主鍵
                        var productId = row.Cells["productidDataGridViewTextBoxColumn1"].Value?.ToString();
                        var stockQuantity = row.Cells["stockquantityDataGridViewTextBoxColumn"].Value?.ToString();
                        var updatedAt = DateTime.Now;

                        if (string.IsNullOrEmpty(productId)) continue;

                        // 判斷資料庫是否已有此 product_id
                        string checkSql = "SELECT COUNT(*) FROM product_inventory WHERE product_id = @product_id";
                        SqlCommand checkCmd = new SqlCommand(checkSql, conn, transaction);
                        checkCmd.Parameters.AddWithValue("@product_id", productId);
                        int exists = (int)checkCmd.ExecuteScalar();

                        if (exists > 0)
                        {
                            // UPDATE
                            string updateSql = "UPDATE product_inventory SET stock_quantity = @stock_quantity, updated_at = @updated_at WHERE product_id = @product_id";
                            SqlCommand updateCmd = new SqlCommand(updateSql, conn, transaction);
                            updateCmd.Parameters.AddWithValue("@stock_quantity", stockQuantity);
                            updateCmd.Parameters.AddWithValue("@updated_at", updatedAt);
                            updateCmd.Parameters.AddWithValue("@product_id", productId);
                            updateCmd.ExecuteNonQuery();
                        }
                        else
                        {
                            // INSERT
                            string insertSql = "INSERT INTO product_inventory (product_id, stock_quantity, updated_at) VALUES (@product_id, @stock_quantity, @updated_at)";
                            SqlCommand insertCmd = new SqlCommand(insertSql, conn, transaction);
                            insertCmd.Parameters.AddWithValue("@product_id", productId);
                            insertCmd.Parameters.AddWithValue("@stock_quantity", stockQuantity);
                            insertCmd.Parameters.AddWithValue("@updated_at", updatedAt);
                            insertCmd.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                    MessageBox.Show("資料已更新或新增成功！");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("更新失敗：" + ex.Message);
                }
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void btn2_Click(object sender, EventArgs e)
        {
            string keyword = textBoxSearch.Text.Trim();
            if (string.IsNullOrEmpty(keyword))
                return;

            // 搜尋 dataGridView3
            dataGridView3.ClearSelection();
            foreach (DataGridViewRow row in dataGridView3.Rows)
            {
                if (row.IsNewRow) continue;
                if (row.Cells["productidDataGridViewTextBoxColumn1"].Value != null &&
                    row.Cells["productidDataGridViewTextBoxColumn1"].Value.ToString() == keyword)
                {
                    row.Selected = true;
                    dataGridView3.CurrentCell = row.Cells[0];
                    break;
                }
            }

            // 搜尋 dataGridView4
            dataGridView4.ClearSelection();
            foreach (DataGridViewRow row in dataGridView4.Rows)
            {
                if (row.IsNewRow) continue;
                if (row.Cells["productidDataGridViewTextBoxColumn2"].Value != null &&
                    row.Cells["productidDataGridViewTextBoxColumn2"].Value.ToString() == keyword)
                {
                    row.Selected = true;
                    dataGridView4.CurrentCell = row.Cells[0];
                    break;
                }
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void updatebtn_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("請選取要更新的資料列！");
                return;
            }

            var selectedRow = dataGridView1.SelectedRows[0];
            var orderItemId = selectedRow.Cells["order_item_id"].Value?.ToString();
            var itemName = selectedRow.Cells["item_name"].Value?.ToString();
            var quantity = selectedRow.Cells["quantity"].Value?.ToString();

            if (string.IsNullOrEmpty(orderItemId))
            {
                MessageBox.Show("無法取得主鍵 order_item_id！");
                return;
            }

            string connStr = "Data Source=DESKTOP-HVUT3JB\\SQLEXPRESS;Initial Catalog=ShoPee;Integrated Security=True";
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string updateSql = "UPDATE orders_items SET item_name = @item_name, quantity = @quantity WHERE order_item_id = @order_item_id";
                using (SqlCommand cmd = new SqlCommand(updateSql, conn))
                {
                    cmd.Parameters.AddWithValue("@item_name", itemName);
                    cmd.Parameters.AddWithValue("@quantity", quantity);
                    cmd.Parameters.AddWithValue("@order_item_id", orderItemId);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                        MessageBox.Show("資料更新成功！");
                    else
                        MessageBox.Show("更新失敗，請確認資料是否存在。");
                }
            }
        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView4_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}

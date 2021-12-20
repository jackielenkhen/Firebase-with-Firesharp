using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FireSharp.Config;
using FireSharp.Response;
using FireSharp.Interfaces;
using FireSharp;

namespace fireConfig
{
    public partial class Form1 : Form
    {

        bool is_loading_first = true;
        int is_delete = 0;
        int is_added = 0;
        string tmp_path = "";

        string table_path = "table/";
        string table_name = "student";
        public Form1()
        {
            InitializeComponent();
        }

        IFirebaseConfig ifc = new FirebaseConfig()
        {
            AuthSecret = "scJitfWLlRR4g8Fvm5UjdJpcnhoS6UyawF8nkIOF",
            BasePath = "https://fir-pos-133f0.firebaseio.com/"
        };
        IFirebaseClient client;
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                client = new FirebaseClient(ifc);
                var unused = EventStreaming();
            }
            catch
            {
                MessageBox.Show("there was a problem in your internet");
            }
        }   
        private void btnGet_Click(object sender, EventArgs e)
        {
            loading_data();
        }
        private void btnSet_Click(object sender, EventArgs e)
        {
            set_dataAsync();
        }   
        private void btnPush_Click(object sender, EventArgs e)
        {
           push_dataAsync();
        }
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            update_dataAsync();
        }
        private void btnDelete_Click(object sender, EventArgs e)
        {
            delete_dataAsync();
        }
        private async Task set_dataAsync()
        {
            try
            {
                student obj = new student
                {
                    id = txtID.Text,
                    name = txtName.Text,
                    age = Convert.ToInt32(txtAge.Text)
                };
              
                SetResponse response = await client.SetAsync(table_path+"student/" + txtID.Text, obj);
                Todo result = response.ResultAs<Todo>(); //The response will contain the data written
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }
        private async Task<student> Get_dataAsync(string path ="")
        {
            try
            {
                FirebaseResponse response = await client.GetAsync(path);
                student obj = response.ResultAs<student>(); 
                return obj;
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
            return new student();
        }
        private async Task push_dataAsync()
        {
            try
            {
                student obj = new student
                {
                    id = txtID.Text,
                    name = txtName.Text,
                    age = Convert.ToInt32(txtAge.Text)
                };

                PushResponse response = await client.PushAsync(table_path+ table_name +"/", obj);              
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private async Task update_dataAsync()
        {
            try
            {
                student obj = new student
                {
                    id = txtID.Text,
                    name = txtName.Text,
                    age = Convert.ToInt32(txtAge.Text)
                };
               
                FirebaseResponse response = await client.UpdateAsync(table_path + table_name + "/" +txtID.Tag.ToString(), obj);              
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private async Task delete_dataAsync()
        {
            try
            {
                FirebaseResponse response = await client.DeleteAsync(table_path + table_name + "/" + txtID.Tag.ToString()); //Deletes todos collection
                MessageBox.Show(response.StatusCode.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }   
        private  async Task EventStreaming()
        {          
            EventStreamResponse response = await client.OnAsync(table_path+"student",
            added: (s, args, context) =>
            {
                if (is_loading_first)
                    is_loading_first = false;
                else
                {
                    //added
                    if (is_added >= 1)
                    {                      
                        is_added = 0;
                        this.BeginInvoke((Action)delegate ()
                        {
                            tmp_path = args.Path;
                            added_to_dataGrid();
                        });
                        
                    }

                }
                is_delete = 0;
            },
            changed: (s, args, context) =>
            {
                //change             
                is_delete = 0;
                this.BeginInvoke((Action)delegate ()
                {
                    tmp_path = args.Path;
                    changed_to_dataGrid();
                });
                       
            },
            removed: (s, args, context) =>
            {             
                is_delete++;
                is_added++;
                if (is_delete >= 2)
                {
                    //delete                  
                    is_delete = 0;
                    is_delete = 0;
                    this.BeginInvoke((Action)delegate ()
                    {
                        remove_to_dataGrid();
                    });
                }

                tmp_path = args.Path;
            });
        }    

        // datagridview
        private async void added_to_dataGrid(string path ="")
        {
            try
            {



                student obj = await Get_dataAsync(table_path + table_name + path ==""? tmp_path: "/"+path);            
                if(obj != null)
                {
                    DataGridViewRow row = dgvStudent.Rows[dgvStudent.Rows.Add()];

                    row.Tag = tmp_path;
                    row.Cells[id.Name].Value = obj.id;
                    row.Cells["name"].Value = obj.name;
                    row.Cells[age.Name].Value = obj.age;
                }           
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private async void remove_to_dataGrid()
        {
            try
            {
                foreach(DataGridViewRow row in dgvStudent.Rows)
                {
                    if(row.Tag.ToString() == tmp_path)
                    {
                        dgvStudent.Rows.Remove(row);    
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private async void changed_to_dataGrid()
        {
            try
            {
                if (tmp_path == "") return;
                string[] str = tmp_path.Split('/');
                string paht_change = "";
                if(str.Length < 3)
                {
                    paht_change =  str[1];
                }
                else
                {
                    paht_change =  str[1];
                }

                bool update = false;
                foreach (DataGridViewRow row in dgvStudent.Rows)
                {
                    if (row.Tag.ToString() == paht_change)
                    {
                        student obj = await Get_dataAsync(table_path + "student/" + paht_change);

                        row.Tag = tmp_path;
                        row.Cells[id.Name].Value = obj.id;
                        row.Cells["name"].Value = obj.name;
                        row.Cells[age.Name].Value = obj.age;
                        update = true;
                        break;
                    }
                }

                if(update == false)
                {
                    added_to_dataGrid(paht_change);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private async void loading_data()
        {
            try
            {
                dgvStudent.Rows.Clear();

                var data = await client.GetAsync("table/student");

                Dictionary<string, student> stus = data.ResultAs<Dictionary<string, student>>();

                foreach (var obj in stus)
                {
                    DataGridViewRow row = dgvStudent.Rows[dgvStudent.Rows.Add()];
                    row.Tag = obj.Key;
                    row.Cells[id.Name].Value = obj.Value.id;
                    row.Cells["name"].Value = obj.Value.name;
                    row.Cells[age.Name].Value = obj.Value.age;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void dgvStudent_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int index = dgvStudent.CurrentRow.Index;
            if (index > -1)
            {
                DataGridViewRow row = dgvStudent.Rows[index];

                txtID.Tag = row.Tag;
                txtID.Text = row.Cells[id.Name].Value.ToString();
                txtName.Text = row.Cells["name"].Value.ToString();
                txtAge.Text = row.Cells[age.Name].Value.ToString();
            }
        }
    }
}

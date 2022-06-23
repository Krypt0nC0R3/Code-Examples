using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using MySql.Data.MySqlClient;

namespace TalosUsernameViewer
{
    public partial class Form1 : Form
    {

        private class Person
        {
            public string Name { get; set; }
            public int PersonalID { get; set; }
            public string Username { get; set; }
            public int DBID { get; set; }
        }

        MySqlConnection conn = null;

        private List<Person> persons = new();
        private List<Person> selected = new();

        Thread loadInfo = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            selected = persons.Where(x => x.Name.ToLower().Contains(textBox1.Text.ToLower()) || x.Username.ToLower().Contains(textBox1.Text.ToLower()) || x.PersonalID.ToString().ToLower().Contains(textBox1.Text.ToLower())).ToList();
            UpdateCombobox();
        }

        private void UpdateCombobox()
        {
            comboBox1.Items.Clear();
            foreach(var p in selected)
            {
                comboBox1.Items.Add(p.Name);
            }
            if (selected.Count > 0)
            {
                comboBox1.SelectedIndex = 0;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            loadInfo = new(()=>
            {
                textBox1.BeginInvoke(new Action(() =>
                {
                    textBox1.Enabled = false;
                }));
                conn = new("User Id=krypt0n;Host=192.168.0.12;password=dima160597;Character Set=utf8;database=talos_dnd");
                try
                {
                    conn.Open();
                    string selectionString = @"SELECT
  realNames.realName,
  realNames.u_id,
  users.username,
  users.personal_id
FROM realNames
  INNER JOIN users
    ON realNames.u_id = users.id";

                    MySqlCommand cmd = new(selectionString, conn);
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        string dbid_s = reader["u_id"].ToString();
                        string pid_s = reader["personal_id"].ToString();
                        int dbid = -1;
                        try
                        {
                            dbid = Convert.ToInt32(dbid_s);
                        }
                        catch{}
                        int pid = -1;
                        try
                        {
                            pid = Convert.ToInt32(pid_s);
                        }
                        catch { }
                        Person p = new()
                        {
                            Name = reader["realName"].ToString(),
                            DBID = dbid,
                            PersonalID = pid,
                            Username = reader["username"].ToString(),
                        };
                        persons.Add(p);
                    }
                    textBox1.BeginInvoke(new Action(() =>
                    {
                        textBox1.Enabled = true;
                    }));
                }
                catch(Exception e)
                {
                    textBox1.BeginInvoke(new Action(() =>
                    {
                        textBox1.Enabled = false;
                        MessageBox.Show(e.Message + "\n" + e.StackTrace);
                    }));
                }
            });
            loadInfo.Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //loadInfo?.Abort();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex != -1)
            {
                Person p = selected[comboBox1.SelectedIndex];
                richTextBox1.Text = $"Имя: {p.Name}\nЛогин: {p.Username}\nНомер карты: {p.PersonalID}\nId: {p.DBID}";
            }
            else
            {
                richTextBox1.Text = "";
            }
        }
    }
}

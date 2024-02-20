using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace ClothPatternGrouping
{
    public partial class Form1 : Form
    {
        MySqlConnection conn;
        MySqlConnection connRead;

        public Form1()
        {
            InitializeComponent();
            connectMysqlDatabase();
        }

        private void connectMysqlDatabase()
        {
            string myConnectionString;

            myConnectionString = "server=127.0.0.1;uid=root;pwd=;database=new_patterns";

            try
            {
                conn = new MySqlConnection(myConnectionString);
                conn.Open();

                connRead = new MySqlConnection(myConnectionString);
                connRead.Open();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        // 18000
        private void buttonStart_Click(object sender, EventArgs e)
        {
            int nFrom = (int)numericUpDownFrom.Value;
            int nTo = (int)numericUpDownTo.Value;
            string query = "SELECT * FROM `new_patterns`.`patterns` where `id`>=" + nFrom;
            if (nTo > nFrom && nTo > 1)
            {
                query += " AND `id`<=" + nTo;
            }
            MySqlCommand mySqlCommand = new MySqlCommand(query, connRead);
            MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();

            int id, categoryId;
            string strBodyType;
            string strSleeveType;
            string strType;
            while (mySqlDataReader.Read()) { 
                id = mySqlDataReader.GetInt32("id");
                strBodyType = mySqlDataReader.GetString("body_type");
                strSleeveType = mySqlDataReader.GetString("sleeve_type");
                
                string[] attributeDetails = strBodyType.Split(' ');
                if (attributeDetails.Length > 4)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        if (i < 2)
                        {
                            strType = attributeDetails[i];
                        } else if(i == 2)
                        {
                            strType = attributeDetails[2];
                            for (int j = 3; j < attributeDetails.Length - 1; j++)
                            {
                                strType += " " + attributeDetails[j];
                            }
                        } else if (i == 3)
                        {
                            strType = attributeDetails.Last();
                        } else
                        {
                            strType = strSleeveType;
                        }
                        categoryId = insertToDB(i+1, strType);
                        updatePatternTable(id, i + 1, categoryId);
                    }
                }
            }
            mySqlDataReader.Close();
        }

        private int getCurrentIdFromDB(int categoryIdx, string categoryName)
        {
            int id = 0;
            string query = "SELECT * FROM `new_patterns`.`category" + categoryIdx + "` where `name`='" + categoryName + "';";
            MySqlCommand mySqlCommand = new MySqlCommand(query, conn);
            MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
            if (mySqlDataReader.Read())
            {
                id = mySqlDataReader.GetInt32("id");
            }
            mySqlDataReader.Close();
            return id;
        }

        private int insertToDB(int categoryIdx, string categoryName)
        {
            try
            {
                int id = getCurrentIdFromDB(categoryIdx, categoryName);
                if (id > 0)
                {
                    return id;
                }

                string query = "INSERT INTO `new_patterns`.`category" + categoryIdx + "` (`name`) VALUES ('" + categoryName + "');";
                MySqlCommand mySqlCommand = new MySqlCommand(query, conn);
                mySqlCommand.ExecuteNonQuery();
                id = (int)mySqlCommand.LastInsertedId;

                return id;
            }
            catch
            {
                return -1;
            }
        }

        private int updatePatternTable(int id, int categoryIdx, int categoryId)
        {
            string query = "UPDATE `new_patterns`.`patterns` SET `category" + categoryIdx + "` = " + categoryId + " WHERE `id` = " + id;
            MySqlCommand mySqlCommand = new MySqlCommand(query, conn);
            mySqlCommand.ExecuteNonQuery();
            
            return 1;
        }

    }
}

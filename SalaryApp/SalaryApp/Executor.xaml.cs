﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Windows;
using System.Windows.Data;

namespace SalaryApp
{
    /// <summary>
    /// Логика взаимодействия для Executor.xaml
    /// </summary>
    public partial class Executor : Window
    {
        public int id = 0;
        public string login;
        public string grade;
        DataTable dt;
        ObservableCollection<TaskTable> tasksList;
        ICollectionView Itemlist;

        public Executor()
        {
            InitializeComponent();
            Loaded += Executor_Loaded;
        }

        private void Executor_Loaded(object sender, RoutedEventArgs e)
        {

            LoginLabel.Content = "Ваш логин: " + login;
            GradeLabel.Content = "Должность: " + grade;

            var statusList = new List<string>() { "Любой статус", "Запланирована", "Выполняется", "Завершена", "Отменена" };
            StatusCB.ItemsSource = statusList;

            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();
            try
            {
                GetTasks(conn);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }

        class TaskTable
        {

            public TaskTable(string Name, string Status, string Manager)
            {
                this.Name = Name;
                this.Status = Status;
                this.Manager = Manager;
            }

            public string Name { get; set; }
            public string Status { get; set; }
            public string Manager { get; set; }
        }

        private void GetTasks(MySqlConnection conn)
        {

            MySqlCommand command = new MySqlCommand("SELECT id, Performer, Name, Status, NeedTime, EndTime FROM `tasks` WHERE Performer = '" + id + "'", conn);
            command.ExecuteNonQuery();

            MySqlDataAdapter adapter = new MySqlDataAdapter(command);
            tasksList = new ObservableCollection<TaskTable>();
            dt = new DataTable("tasks");
            dbHandler db = new dbHandler();
            adapter.Fill(dt);

            foreach (DataRow row in dt.Rows)
            {
                var name = Convert.ToString(row[2]);
                var status = Convert.ToString(row[3]);
                var managerId = db.GetManager(Convert.ToInt32(row[1]));
                var manager = db.GetUser(managerId);

                tasksList.Add(new TaskTable(name, status, manager));
            }

            TasksDG.ItemsSource = tasksList;
            adapter.Update(dt);

        }

        private void StatusBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedIndex = TasksDG.SelectedIndex;
            var taskID = dt.Rows[selectedIndex][0]; 
            if (tasksList[selectedIndex].Status.Equals("Запланирована"))
            {
                tasksList[selectedIndex] = new TaskTable(tasksList[selectedIndex].Name, "Выполняется", tasksList[selectedIndex].Manager);
            }
            else if (tasksList[selectedIndex].Status.Equals("Выполняется"))
            {
                MessageBoxResult result = MessageBox.Show("Задача выполнена?", "My App", MessageBoxButton.YesNoCancel);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        if (DateTime.Now > Convert.ToDateTime(dt.Rows[selectedIndex][4]))
                        {
                            MessageBox.Show("Задание просрочено!");
                            tasksList[selectedIndex] = new TaskTable(tasksList[selectedIndex].Name, "Отменена", tasksList[selectedIndex].Manager);
                            break;
                        }
                        tasksList[selectedIndex] = new TaskTable(tasksList[selectedIndex].Name, "Завершена", tasksList[selectedIndex].Manager);
                        dt.Rows[selectedIndex][5] = DateTime.Now;
                        break;
                    case MessageBoxResult.No:
                        tasksList[selectedIndex] = new TaskTable(tasksList[selectedIndex].Name, "Отменена", tasksList[selectedIndex].Manager);
                        break;
                }
            }

            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();
            try
            {

                MySqlCommand command = new MySqlCommand("UPDATE `tasks` SET Status = @Status, EndTime = @ET WHERE id = '" + taskID + "'", conn);
                command.Parameters.Add("@Status", MySqlDbType.Enum).Value = tasksList[selectedIndex].Status;
                command.Parameters.Add("@ET", MySqlDbType.DateTime).Value = dt.Rows[selectedIndex][5];
                command.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }

        }

        private void UserBtn_Click(object sender, RoutedEventArgs e)
        {
            LoginForm lf = new LoginForm();
            lf.Show();
            Close();
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedIndex = TasksDG.SelectedIndex;
            var taskID = dt.Rows[selectedIndex][0];
            AddTask addForm = new AddTask
            {
                id = id,
                login = login,
                grade = grade,
                taskID = Convert.ToInt32(taskID)
            };
            addForm.Show();
            Close();
        }

        private void StatusCB_DropDownClosed(object sender, EventArgs e)
        {
            if (StatusCB.Text.Equals("Любой статус"))
            {
                TasksDG.ItemsSource = tasksList;
            }
            else
            {
                var _itemSourceList = new CollectionViewSource() { Source = tasksList };
                Itemlist = _itemSourceList.View;

                GroupFilter gf = new GroupFilter();

                var statusFilter = new Predicate<object>(item => ((TaskTable)item).Status.Equals(StatusCB.Text));
                gf.AddFilter(statusFilter);

                Itemlist.Filter = gf.Filter;
                TasksDG.ItemsSource = Itemlist;
            }
        }

        private void ExBtn_Click(object sender, RoutedEventArgs e)
        {
            ExList exForm = new ExList {
                id = id,
                login = login,
                grade = grade
            };
            exForm.Show();
            Close();
        }
    }
}

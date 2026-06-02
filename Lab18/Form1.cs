using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Lab18
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            ConfigureListView();
            LoadProcesses();
        }

        private void ConfigureListView()
        {
            lvProcesses.View = View.Details;
            lvProcesses.FullRowSelect = true;
            lvProcesses.MultiSelect = false;

            lvProcesses.Columns.Add("Назва процесу", 220);
            lvProcesses.Columns.Add("PID (ідентифікатор)", 100);

            lvProcesses.ContextMenuStrip = contextMenu;
        }

        private void LoadProcesses()
        {
            lvProcesses.Items.Clear();

            Process[] processList = Process.GetProcesses();

            foreach (Process proc in processList)
            {
                ListViewItem item = new ListViewItem(proc.ProcessName);

                item.SubItems.Add(proc.Id.ToString());

                item.Tag = proc;

                lvProcesses.Items.Add(item);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadProcesses();
        }

        private void menuInfo_Click(object sender, EventArgs e)
        {
            if (lvProcesses.SelectedItems.Count == 0) return;

            Process selectedProc = (Process)lvProcesses.SelectedItems[0].Tag;

            try
            {
                string info = $"Ім'я процесу: {selectedProc.ProcessName}\n" +
                              $"ID процесу (PID): {selectedProc.Id}\n" +
                              $"Обсяг віртуальної пам'яті: {selectedProc.VirtualMemorySize64 / 1024 / 1024} МБ\n" +
                              $"Час запуску: {selectedProc.StartTime.ToLongTimeString()}";

                MessageBox.Show(info, "Деталі процесу", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не вдалося отримати повну інформацію (обмеження ОС): {ex.Message}",
                                "Помилка доступу", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void menuThreadsModules_Click(object sender, EventArgs e)
        {
            if (lvProcesses.SelectedItems.Count == 0) return;

            Process selectedProc = (Process)lvProcesses.SelectedItems[0].Tag;
            string details = $"ПОТОКИ ПРОЦЕСУ ({selectedProc.ProcessName})\n";

            try
            {
                int threadCount = 0;
                foreach (ProcessThread thread in selectedProc.Threads)
                {
                    if (threadCount++ < 10)
                        details += $"ID Потоку: {thread.Id} | Поточний пріоритет: {thread.CurrentPriority}\n";
                }

                details += $"\nЗАВАНТАЖЕНІ МОДУЛІ (DLL)\n";
                int moduleCount = 0;

                foreach (ProcessModule module in selectedProc.Modules)
                {
                    if (moduleCount++ < 10)
                        details += $"Модуль: {module.ModuleName}\nШлях: {module.FileName}\n\n";
                }

                MessageBox.Show(details, "Потоки та модулі процесу", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка зчитування внутрішніх структур: {ex.Message}",
                                "Обмеження безпеки ОС", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void menuKill_Click(object sender, EventArgs e)
        {
            if (lvProcesses.SelectedItems.Count == 0) return;

            Process selectedProc = (Process)lvProcesses.SelectedItems[0].Tag;

            var confirmResult = MessageBox.Show($"Ви впевнені, що хочете примусово завершити процес {selectedProc.ProcessName}?",
                                                 "Підтвердження дії", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmResult == DialogResult.Yes)
            {
                try
                {
                    selectedProc.Kill();
                    MessageBox.Show("Процес успішно зупинено", "Операція завершена", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadProcesses();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не вдалося завершити процес: {ex.Message}", "Помилка виконання", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void menuExport_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Текстові файли (.txt)|.txt";
                sfd.Title = "Збереження списку процесів у файл";
                sfd.FileName = "System_Processes_Report.txt";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (StreamWriter writer = new StreamWriter(sfd.FileName))
                        {
                            writer.WriteLine($"Звіт про запущені процеси системи від: {DateTime.Now}");
                            writer.WriteLine(new string('=', 60));
                            writer.WriteLine($"{"PID",-12} | {"Назва активного процесу"}");
                            writer.WriteLine(new string('=', 60));

                            foreach (ListViewItem item in lvProcesses.Items)
                            {
                                string name = item.Text;
                                string pid = item.SubItems[1].Text;

                                writer.WriteLine($"{pid,-12} | {name}");
                            }
                        }
                        MessageBox.Show("Експорт звіту успішно завершено!", "Файл збережено", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Помилка запису на диск: {ex.Message}", "Критична помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
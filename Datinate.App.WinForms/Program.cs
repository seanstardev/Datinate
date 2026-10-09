namespace Datinate.App.WinForms
{
    static class Program
    {
        private static Mutex? singleInstanceMutex;

        [STAThread]
        static void Main()
        {
            const string mutexName = @"Local\Datinate.SingleInstance";

            singleInstanceMutex = new Mutex(
                initiallyOwned: true,
                name: mutexName,
                createdNew: out bool isFirstInstance);

            if (!isFirstInstance)
            {
                MessageBox.Show(
                    "Datinate is already running.\r\n\r\nOnly one instance of Datinate can be open at a time.",
                    "Datinate Already Running",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            MainForm mainForm = new MainForm();

            Application.Run(mainForm);
        }
    }
}
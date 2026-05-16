using datinate.app;

namespace com.RADIO.Datinate
{
    static class Program
    {

        [STAThread]
        static void Main() 
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            MainForm mainForm = new MainForm();
            
            Application.Run(mainForm);
            
        }
    }
}

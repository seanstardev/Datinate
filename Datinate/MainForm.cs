using com.RADIO.Datinate;
using com.RADIO.Datinate.App.View.compare;
using com.RADIO.Datinate.App.View.createDat;
using com.RADIO.Datinate.App.View.customList;
using com.RADIO.Datinate.App.View.datDetails.addToProject;
using com.RADIO.Datinate.App.View.problemList;
using com.RADIO.Datinate.App.View.projects.datPathsUpdate;
using com.RADIO.Datinate.RMVC.Shared;
using Datinate.App;
using System.Runtime.InteropServices;

namespace datinate.app
{
    public partial class MainForm : Form, IShell
    {
        public ProjectLoaderView ProjectLoaderView => ProjectsForm.ProjectsView.ProjectLoaderView;
        public DatGrouperView GameFamilyView => ProjectsForm.ProjectsView.DatGrouperView;
        public CompareView CompareView => CompareForm.CompareView;
        public CreateDatView CreateDatView => CreateDatForm.CreateDatView;
        public CustomListView CustomListView => CustomListForm.CustomListView;
        public ProblemListView ProblemListView => ProblemListForm.ProblemListView;
        public DatPathsUpdateView DatPathsUpdateView => DatPathsUpdateForm.DatPathsUpdateView;
        public ExportView ExportView => ProjectsForm.ProjectsView.ExportView;
        public AddToProjectView AddToProjectView => AddToProjectForm.AddToProjectView;
        public ProgressView ProgressView => ProgressForm.ProgressView;

        private CustomListForm CustomListForm;
        private ProblemListForm ProblemListForm;
        private CompareForm CompareForm;
        private CreateDatForm CreateDatForm;
        private ProjectsForm ProjectsForm;
        private DatPathsUpdateForm DatPathsUpdateForm;
        private AddToProjectForm AddToProjectForm;
        private ProgressForm ProgressForm;

        private List<Form>? progressDisabledForms;

        private readonly int uiThreadId;
        private readonly System.Threading.SynchronizationContext? uiContext;
        private static readonly IntPtr HWND_TOP = IntPtr.Zero;

        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_SHOWWINDOW = 0x0040;

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int X,
            int Y,
            int cx,
            int cy,
            uint uFlags);

        public MainForm()
        {
            uiThreadId = Environment.CurrentManagedThreadId;
            uiContext = System.Threading.SynchronizationContext.Current;

            Facade.Create(typeof(Facade), this);

            base.Text = Constants.APP_NAME + " " + Constants.APP_VERSION;

            CustomListForm = new CustomListForm();
            CustomListForm.formClosingEvent += OnCustomFormClose;

            ProblemListForm = new ProblemListForm();

            CompareForm = new CompareForm();
            CompareForm.formClosingEvt += HandleCompareFormClose;

            CreateDatForm = new CreateDatForm();

            ProjectsForm = new ProjectsForm();
            ProjectsForm.FormHiddenEvt += HandleProjectsFormClose;

            DatPathsUpdateForm = new DatPathsUpdateForm();

            AddToProjectForm = new AddToProjectForm();

            ProgressForm = new ProgressForm();
            WireProgressForm();

            InitializeComponent();

            sizeBar.BackColor = Color.Black;
            sizeBar.Visible = false;

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            UpdateStyles();
            CenterToScreen();
        }

        public void StartResizeMonitor()
        {
            WindowSizePresetBar.EnableOverlays = true;
            Ui(() => {
                sizeBar.Visible = true;
            });
        }

        public void ShowProgressForm(bool doShow) => SetProgressFormVisible(doShow);

        public void SetAddToProjectFormVisible(bool doShow)
        {
            Ui(() =>
            {
                SetCenteredModalFormVisible(AddToProjectForm, doShow);
            });
        }

        public void SetDatPathsUpdateFormVisible(bool doShow)
        {
            Ui(() =>
            {
                SetCenteredModalFormVisible(DatPathsUpdateForm, doShow);
            });
        }
        private void SetCenteredModalFormVisible(Form form, bool doShow)
        {
            if (form.IsDisposed)
                return;

            if (doShow)
            {
                if (form.Visible)
                {
                    form.BringToFront();
                    form.Activate();
                    return;
                }

                if (form.WindowState == FormWindowState.Minimized)
                    form.WindowState = FormWindowState.Normal;

                var owner = GetCenteredModalDialogOwner(form);

                form.StartPosition = FormStartPosition.Manual;
                form.ShowInTaskbar = false;

                PositionFormInScreen(form, owner);

                form.ShowDialog(owner);

                return;
            }

            if (form.Visible)
            {
                form.Hide();
                BringToFront();
            }
        }

        private Form GetCenteredModalDialogOwner(Form dialog)
        {
            if (IsValidCenteredModalDialogOwner(ProjectsForm, dialog))
                return ProjectsForm;

            if (IsValidCenteredModalDialogOwner(this, dialog))
                return this;

            for (int i = Application.OpenForms.Count - 1; i >= 0; i--)
            {
                Form form = Application.OpenForms[i];

                if (IsValidCenteredModalDialogOwner(form, dialog))
                    return form;
            }

            return this;
        }

        private static bool IsValidCenteredModalDialogOwner(Form? form, Form dialog)
        {
            if (form == null)
                return false;

            if (form == dialog)
                return false;

            if (form.IsDisposed)
                return false;

            if (!form.Visible)
                return false;

            if (!form.Enabled)
                return false;

            if (!form.IsHandleCreated)
                return false;

            return true;
        }

        private static void PositionFormInScreen(Form form, Form owner)
        {
            var screen = owner.IsHandleCreated
                ? Screen.FromControl(owner)
                : Screen.FromPoint(Cursor.Position);

            Rectangle area = screen.WorkingArea;

            form.Location = new Point(
                area.Left + (area.Width - form.Width) / 2,
                area.Top + (area.Height - form.Height) / 2);
        }

        public void SetCompareFormVisible(bool doShow)
        {
            if (doShow)
                CompareForm.ShowAndBringToFront();
            else if (CompareForm.Visible)
            {
                CompareForm.Hide();
                BringToFront();
            }
        }

        public void SetCustomFormVisible(bool doShow)
        {
            if (doShow)
            {
                CustomListForm.Show();
                CustomListForm.BringToFront();
            }
            else if (CustomListForm.Visible)
            {
                CustomListForm.Hide();
                BringToFront();
            }
        }

        public void SetCreateDatFormVisible(bool doShow)
        {
            if (doShow)
                CreateDatForm.ShowAndBringToFront();
            else if (CreateDatForm.Visible)
            {
                CreateDatForm.Hide();
            }
        }

        public void SetProblemListFormVisible(bool doShow)
        {
            Ui(() =>
            {
                if (doShow)
                    ProblemListForm.ShowAndBringToFront();
                else
                    ProblemListForm.Hide();
            });
        }

        public void ShowRbProjectsView()
        {
            ProjectsForm.ProjectsView.ShowProjectsView();
        }

        public void ShowContentPathsView()
        {
            ProjectsForm.ProjectsView.ShowRbContentPathsView();
        }

        public void ShowDatGrouperWindowView()
        {
            ProjectsForm.ProjectsView.ShowCurationView();
        }

        public void ShowExportView()
        {
            ProjectsForm.ProjectsView.ShowExportView();
        }
        public bool CurrentProjectsPageIsProjectLoaderPage
            => ProjectsForm.ProjectsView.CurrentProjectsPageIsProjectLoaderPage;
        public void SetAppEnabled(bool doEnable)
        {
        }

        public Task<bool> ShowMessageBox(string title, string message, bool isYesNo = false)
        {
            MessageBoxButtons messageBoxButtons = isYesNo ? MessageBoxButtons.OKCancel : MessageBoxButtons.OK;
            MessageBoxIcon messageBoxIcon = MessageBoxIcon.Information;
            var result = MessageBox.Show(message, title, messageBoxButtons, messageBoxIcon);
            return Task.FromResult(result == DialogResult.OK);
        }

        public void SetMainFormsSizeBarBackColor(Color colour)
        {
            Ui(() => { 
                sizeBar.BackColor = colour;
            });
        }

        public void HandleCompareFormClose()
        {
            Facade.Instance?.HandleCompareFormClose();
            BringToFront();
        }

        public void HandleProjectsFormClose()
        {
            Facade.Instance?.HandleProjectsFormClose();
            BringToFront();
        }

        public void SetProjectsFormVisible(bool doShow)
        {
            if (doShow)
                ProjectsForm.ShowProjectsFormAndBringToFront();
        }

        public void SetProjectsFormTitle(string title)
        {
            ProjectsForm.SetProjectTitle(title);
        }
        public void SetProgressFormVisible(bool doShow)
        {
            Ui(() =>
            {
                if (doShow)
                {
                    EnsureProgressFormInstance();

                    Form? owner = GetProgressOwner();

                    if (!ProgressForm.Visible)
                    {
                        ShowProgressFormInternal(owner);
                        DisableAllOtherFormsForProgress();
                        BringProgressFormAboveDatinateWindows();
                    }
                    else
                    {
                        PositionProgressForm(owner);
                        DisableAllOtherFormsForProgress();
                        BringProgressFormAboveDatinateWindows();
                    }

                    return;
                }

                if (!ProgressForm.IsDisposed && ProgressForm.Visible)
                    ProgressForm.Hide();

                RestoreFormsAfterProgress();
            });
        }

        private void EnsureProgressFormInstance()
        {
            if (!ProgressForm.IsDisposed)
                return;

            ProgressForm = new ProgressForm();
            WireProgressForm();
        }

        private void ShowProgressFormInternal(Form? owner)
        {
            if (owner != null)
            {
                ProgressForm.StartPosition = FormStartPosition.Manual;
                PositionProgressForm(owner);
                ProgressForm.Show(owner);
            }
            else
            {
                ProgressForm.StartPosition = FormStartPosition.Manual;
                PositionProgressForm(null);
                ProgressForm.Show();
            }

            BringProgressFormAboveDatinateWindows();
        }

        private void PositionProgressForm(Form? preferredAnchor)
        {
            Form? anchor = GetProgressAnchor(preferredAnchor);

            if (anchor != null)
            {
                Rectangle b = anchor.Bounds;
                ProgressForm.Location = new Point(
                    b.Left + (b.Width - ProgressForm.Width) / 2,
                    b.Top + (b.Height - ProgressForm.Height) / 2);
                return;
            }

            Rectangle area = Screen.FromPoint(Cursor.Position).WorkingArea;
            ProgressForm.Location = new Point(
                area.Left + (area.Width - ProgressForm.Width) / 2,
                area.Top + (area.Height - ProgressForm.Height) / 2);
        }

        private Form? GetProgressAnchor(Form? preferredAnchor)
        {
            if (IsValidProgressAnchor(preferredAnchor))
                return preferredAnchor;

            if (IsValidProgressAnchor(ProgressForm.Owner))
                return ProgressForm.Owner;

            if (IsValidProgressAnchor(this))
                return this;

            return null;
        }

        private Form? GetProgressOwner()
        {
            if (TryGetForegroundDatinateForm(out Form? foregroundForm) && IsValidProgressOwner(foregroundForm))
                return foregroundForm;

            Form? active = Form.ActiveForm;
            if (IsValidProgressOwner(active))
                return active;

            if (IsValidProgressOwner(this))
                return this;

            for (int i = Application.OpenForms.Count - 1; i >= 0; i--)
            {
                Form form = Application.OpenForms[i];
                if (IsValidProgressOwner(form))
                    return form;
            }

            return null;
        }

        private bool TryGetForegroundDatinateForm(out Form? form)
        {
            IntPtr hwnd = GetForegroundWindow();

            if (hwnd != IntPtr.Zero)
            {
                foreach (Form openForm in Application.OpenForms)
                {
                    if (openForm == ProgressForm)
                        continue;

                    if (openForm.IsDisposed || !openForm.IsHandleCreated)
                        continue;

                    if (openForm.Handle == hwnd)
                    {
                        form = openForm;
                        return true;
                    }
                }
            }

            form = null;
            return false;
        }

        private bool IsValidProgressOwner(Form? form)
        {
            if (!IsValidProgressAnchor(form))
                return false;

            if (!form!.Enabled)
                return false;

            return true;
        }

        private bool IsValidProgressAnchor(Form? form)
        {
            if (form == null)
                return false;

            if (form == ProgressForm)
                return false;

            if (form.IsDisposed)
                return false;

            if (!form.Visible)
                return false;

            if (!form.IsHandleCreated)
                return false;

            return true;
        }

        private void BringProgressFormAboveDatinateWindows()
        {
            if (ProgressForm.IsDisposed || !ProgressForm.Visible || !ProgressForm.IsHandleCreated)
                return;

            ProgressForm.BringToFront();

            if (TryGetForegroundDatinateForm(out _))
            {
                SetWindowPos(
                    ProgressForm.Handle,
                    HWND_TOP,
                    0,
                    0,
                    0,
                    0,
                    SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
            }
        }

        private void DisableAllOtherFormsForProgress()
        {
            progressDisabledForms ??= new List<Form>();

            foreach (Form form in Application.OpenForms)
            {
                if (form == ProgressForm)
                    continue;

                if (!form.Visible)
                    continue;

                if (!form.Enabled)
                    continue;

                if (!progressDisabledForms.Contains(form))
                    progressDisabledForms.Add(form);

                form.Enabled = false;
            }
        }

        private void RestoreFormsAfterProgress()
        {
            if (progressDisabledForms == null)
                return;

            foreach (Form form in progressDisabledForms)
            {
                if (!form.IsDisposed)
                    form.Enabled = true;
            }

            progressDisabledForms = null;
        }

        private void WireProgressForm()
        {
            ProgressForm.ShowInTaskbar = false;
            ProgressForm.TopMost = false;

            ProgressForm.VisibleChanged += (_, __) =>
            {
                if (ProgressForm.Visible)
                    BringProgressFormAboveDatinateWindows();
                else
                    RestoreFormsAfterProgress();
            };

            ProgressForm.FormClosed += (_, __) =>
            {
                RestoreFormsAfterProgress();
            };
        }

        private void OnCustomFormClose(object? sender, EventArgs e)
        {
            SetCustomFormVisible(false);
        }

        private void Ui(Action action)
        {
            if (Environment.CurrentManagedThreadId == uiThreadId)
            {
                action();
                return;
            }

            if (IsDisposed)
                return;

            if (IsHandleCreated)
            {
                BeginInvoke(action);
                return;
            }

            if (uiContext != null)
            {
                uiContext.Post(_ =>
                {
                    if (!IsDisposed)
                        action();
                }, null);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            StartAppExit();
        }

        private void StartAppExit()
        {
            AddToProjectForm.AppClosing = true;
            CompareForm.AppClosing = true;
            CreateDatForm.AppClosing = true;
            CustomListForm.AppClosing = true;
            DatPathsUpdateForm.AppClosing = true;
            ProblemListForm.AppClosing = true;
            ProgressForm.AppClosing = true;
            ProjectsForm.AppClosing = true;

            Ui(() =>
            {
                AddToProjectForm.Close();
                CompareForm.Close();
                CreateDatForm.Close();
                CustomListForm.Close();
                DatPathsUpdateForm.Close();
                ProblemListForm.Close();
                ProgressForm.Close();
                ProjectsForm.Close();

                sizeBar.Dispose();
            });
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            StartAppExit();
        }
    }
}
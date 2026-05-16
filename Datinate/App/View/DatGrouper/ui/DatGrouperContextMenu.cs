using com.RADIO.Datinate.RMVC.Shared;
using RadioLibCore.RadioDat;
using static System.Net.Mime.MediaTypeNames;

namespace datinate.app
{
    public partial class DatGrouperContextMenu : UserControl
    {
        public event Action? UndoEvt;
        public event Action? RedoEvt;
        public event Action<bool, IGame>? GameMoveTopOrBottomEvt;
        public event Action<bool, IGamePart>? PartIncludeExcludeEvt;
        public event Action? ToggleAliasesShowHideEvt;
        public event Action? ToggleExcludedFamiliesShowHideEvt;
        public event Action<string>? CopyNameEvt;

        private readonly ContextMenuStrip _menu = new();
        
        private readonly ToolStripMenuItem _undoItem = new("Undo");
        private readonly ToolStripMenuItem _redoItem = new("Redo");

        private readonly ToolStripMenuItem _gameMoveTopItem = new("Move Game to Top");
        private readonly ToolStripMenuItem _gameMoveBottomItem = new("Move Game to Bottom");
        private readonly ToolStripMenuItem _partIncludeExcludeItem = new("Exclude Part");

        private readonly ToolStripMenuItem _copyNameItem = new("Copy Name to Clipboard");
        private readonly ToolStripMenuItem _copyNameFullItem = new("Copy Fullname to Clipboard");

        private readonly ToolStripMenuItem _aliasesShowHideItem = new("Show Aliases");
        private readonly ToolStripMenuItem _excludedFamiliesShowHideItem = new("Show Excluded FamiliesAliases");

        private const string TEXT_PART_INCLUDE = "Include Part";
        private const string TEXT_PART_EXCLUDE = "Exclude Part";

        private const string TEXT_ALIASES_SHOW = "Show Aliases";
        private const string TEXT_ALIASES_HIDE = "Hide Aliases";

        private const string TEXT_EXCLUDED_FAMILIES_SHOW = "Show Excluded Families";
        private const string TEXT_EXCLUDED_FAMILIES_HIDE = "Hide Excluded Families";


        private IGameEntity? _activeEntity = null;

        public DatGrouperContextMenu(bool isCurated)
        {
            InitializeComponent();
            var items = new List<ToolStripItem>()
            {
                _undoItem,
                _redoItem,
                new ToolStripSeparator(),
                _excludedFamiliesShowHideItem,
                _aliasesShowHideItem,
                new ToolStripSeparator(),
                _copyNameItem,
                _copyNameFullItem
            };
            if (isCurated)
            {
                items.AddRange(new List<ToolStripItem>()
                {                
                    new ToolStripSeparator(),
                    _gameMoveTopItem,
                    _gameMoveBottomItem,
                    new ToolStripSeparator(),
                    _partIncludeExcludeItem
                });
            }
            _menu.Items.AddRange(items.ToArray());

            _menu.Opening += Menu_Opening;

            ResetMenuItemEventHandlers();

            _undoItem.Enabled = false;
            _redoItem.Enabled = false;
            
            _excludedFamiliesShowHideItem.Enabled = false;
            _aliasesShowHideItem.Enabled = false;
            
            _copyNameItem.Enabled = false;  
            _copyNameFullItem.Enabled = false;

            _partIncludeExcludeItem.Enabled = false;

            _gameMoveTopItem.Enabled = false;
            _gameMoveBottomItem.Enabled = false;
        }
        public void DisableSetPartIncludeExclude() =>
            _partIncludeExcludeItem.Enabled = false;

        public void SetPartIncludeExclude(bool include)
        {
            _partIncludeExcludeItem.Enabled = true;

            _partIncludeExcludeItem.Text = include
                ? TEXT_PART_INCLUDE
                : TEXT_PART_EXCLUDE;
        }
        public void SetAliasesShowHide(bool show)
        {
            _aliasesShowHideItem.Enabled = true;
            _aliasesShowHideItem.Text = show
                ? TEXT_ALIASES_SHOW
                : TEXT_ALIASES_HIDE;
        }
        public void SetExcludedFamiliesShowHide(bool show)
        {
            _excludedFamiliesShowHideItem.Enabled = true;
            _excludedFamiliesShowHideItem.Text = show
                ? TEXT_EXCLUDED_FAMILIES_SHOW
                : TEXT_EXCLUDED_FAMILIES_HIDE;
        }

        internal void SetCopyNameEnabled(bool doEnable)
        {
            _copyNameItem.Enabled = doEnable;
            _copyNameFullItem.Enabled = doEnable;
        }
        public void SetUndoRedoEnabled(bool enableUndo, bool enableRedo)
        {
            _undoItem.Enabled = enableUndo;
            _redoItem.Enabled = enableRedo;
        }

        internal void SetCanMoveGameTopBottom(bool allowMoveToBottom, bool allowMoveToTop)
        {
            _gameMoveBottomItem.Enabled = allowMoveToBottom;
            _gameMoveTopItem.Enabled = allowMoveToTop;
        }
        public void Reset()
        {
            _undoItem.Enabled = false;
            _redoItem.Enabled = false;
            _partIncludeExcludeItem.Enabled = false;
            _excludedFamiliesShowHideItem.Enabled = false;
            _aliasesShowHideItem.Enabled = false;
            _copyNameFullItem.Enabled = false;
            _copyNameItem.Enabled = false;

            _activeEntity = null;
        }

        public void Show(TreeView treeView, Point location, IGameEntity? entity)
        {
            _activeEntity = entity;
            _menu.Show(treeView, location);
        }

        private void ResetMenuItemEventHandlers()
        {
            _undoItem.Click -= OnUndo;
            _redoItem.Click -= OnRedo;
            _excludedFamiliesShowHideItem.Click -= OnExcludedFamiliesShowHide;
            _aliasesShowHideItem.Click -= OnAliasesShowHide;
            _copyNameItem.Click -= OnCopyName;
            _copyNameFullItem.Click -= OnCopyFullname;
            _gameMoveTopItem.Click -= OnGameMoveTop;
            _gameMoveBottomItem.Click -= OnGameMoveBottom;
            _partIncludeExcludeItem.Click -= OnPartIncludeExclude;

            _undoItem.Click += OnUndo;
            _redoItem.Click += OnRedo;
            _excludedFamiliesShowHideItem.Click += OnExcludedFamiliesShowHide;
            _aliasesShowHideItem.Click += OnAliasesShowHide;
            _copyNameItem.Click += OnCopyName;
            _copyNameFullItem.Click += OnCopyFullname;
            _gameMoveTopItem.Click += OnGameMoveTop;
            _gameMoveBottomItem.Click += OnGameMoveBottom; 
            _partIncludeExcludeItem.Click += OnPartIncludeExclude;
        }
        private void OnAliasesShowHide(object? sender, EventArgs e)
            => ToggleAliasesShowHideEvt?.Invoke();

        private void OnExcludedFamiliesShowHide(object? sender, EventArgs e)
            => ToggleExcludedFamiliesShowHideEvt?.Invoke();

        private void OnCopyFullname(object? sender, EventArgs e)
            => InvokeCopy(true);

        private void OnCopyName(object? sender, EventArgs e)
            => InvokeCopy(false);
        private void InvokeCopy(bool sendNameWithFlags)
        {
            if (_activeEntity is IGameEntity && _activeEntity is not IGameEntityProxy)
            {
                string? name = DatinateHelper.GetGameEntityName(_activeEntity);

                if (sendNameWithFlags == false && !string.IsNullOrWhiteSpace(name))
                    name = DatinateHelper.GetFlaglessName(name);

                if (!string.IsNullOrWhiteSpace(name))
                    CopyNameEvt?.Invoke(name);
            }
        }
        private void OnPartIncludeExclude(object? sender, EventArgs e)
        {
            if (_activeEntity is IGamePart part)
            {
                PartIncludeExcludeEvt?.Invoke(
                    _partIncludeExcludeItem.Text == TEXT_PART_INCLUDE,
                    part);
            }
        }
        private void OnGameMoveBottom(object? sender, EventArgs e)
            => InvokeMoveGameTopBottom(false);

        private void OnGameMoveTop(object? sender, EventArgs e)
            => InvokeMoveGameTopBottom(true);
        private void InvokeMoveGameTopBottom(bool moveTop)
        {
            if (_activeEntity is IGame game)
                GameMoveTopOrBottomEvt?.Invoke(moveTop, game);
        }

        private void OnRedo(object? sender, EventArgs e)
            => RedoEvt?.Invoke();

        private void OnUndo(object? sender, EventArgs e)
            => UndoEvt?.Invoke();

        private void Menu_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
        {
        }
    }
}
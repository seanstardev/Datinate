using System.Diagnostics;
using System.Runtime.CompilerServices;
using Timer = System.Windows.Forms.Timer;

namespace datinate.app
{
    internal static class DatGrouperUiAnimationHelper
    {
        private sealed class NodePulse
        {
            public long StartTicks { get; }
            public long EndTicks { get; }
            public Color? OverrideColor { get; }

            public NodePulse(long startTicks, long endTicks, Color? overrideColor)
            {
                StartTicks = startTicks;
                EndTicks = endTicks;
                OverrideColor = overrideColor;
            }
        }

        private sealed class State
        {
            public WeakReference<DatGrouperTreeView> TreeRef { get; }
            public Timer Timer { get; }

            public Dictionary<TreeNode, NodePulse> Pulses { get; } = new();
            public Dictionary<string, Color> PulseColorByImageKey { get; } = new(StringComparer.Ordinal);

            public Action<DatGrouperTreeView, DrawTreeNodeEventArgs>? DrawCore { get; set; }
            public Action<DatGrouperTreeView, DrawTreeNodeEventArgs>? DrawAdorner { get; set; }

            public State(DatGrouperTreeView treeView)
            {
                TreeRef = new WeakReference<DatGrouperTreeView>(treeView);
                Timer = new Timer { Interval = NodePulseTickMs };
                Timer.Tick += (_, __) => Tick(this);
            }
        }

        private static readonly ConditionalWeakTable<DatGrouperTreeView, State> StateByTree = new();

        private const int NodePulseDurationMs = 700;
        private const int NodePulseTickMs = 33;

        private const int HaloMaxAlpha = 110;
        private const int DotMaxAlpha = 230;
        private const int RingMaxAlpha = 180;

        public static void DrawPulseOverlay(DatGrouperTreeView treeView, DrawTreeNodeEventArgs e)
        {
            if (e.Node == null)
                return;

            if (!TryGetState(treeView, out var state))
                return;

            if (!TryGetPulseParams(state, e.Node, out var intensity))
                return;

            var pulseColor = GetPulseColorForNode(state, treeView, e.Node);

            var saved = e.Graphics.Save();

            var row = GetRowRect(treeView, e.Node);
            if (!row.IsEmpty)
                e.Graphics.SetClip(row);

            DrawWideRowHalo(e, row, intensity, pulseColor);
            DrawIconPulse(treeView, e, intensity, pulseColor);

            e.Graphics.Restore(saved);
        }

        public static void DrawNode(DatGrouperTreeView treeView, DrawTreeNodeEventArgs e)
        {
            if (e.Node == null)
                return;

            if (!TryGetState(treeView, out var state) || state.DrawCore == null || state.DrawAdorner == null)
            {
                return;
            }

            if (!TryGetPulseParams(state, e.Node, out var intensity))
            {
                state.DrawCore(treeView, e);
                state.DrawAdorner(treeView, e);
                return;
            }

            state.DrawCore(treeView, e);

            var pulseColor = GetPulseColorForNode(state, treeView, e.Node);

            var saved = e.Graphics.Save();

            var row = GetRowRect(treeView, e.Node);
            if (!row.IsEmpty)
                e.Graphics.SetClip(row);

            DrawWideRowHalo(e, row, intensity, pulseColor);
            DrawIconPulse(treeView, e, intensity, pulseColor);

            e.Graphics.Restore(saved);

            state.DrawAdorner(treeView, e);
        }

        public static void StartNodePulseAmbiguous(DatGrouperTreeView treeView, TreeNode node) =>
            StartNodePulseCore(treeView, node, Color.LightBlue);

        public static void StartNodePulse(DatGrouperTreeView treeView, TreeNode node) =>
            StartNodePulseCore(treeView, node, null);

        public static void StartNodePulse(DatGrouperTreeView treeView, TreeNode node, Color pulseColor) =>
            StartNodePulseCore(treeView, node, pulseColor);

        private static void StartNodePulseCore(DatGrouperTreeView treeView, TreeNode node, Color? pulseColorOverride)
        {
            if (treeView.IsDisposed || !treeView.IsHandleCreated)
                return;

            if (!ReferenceEquals(node.TreeView, treeView))
                return;

            var state = StateByTree.GetValue(treeView, static tv => new State(tv));

            var now = Stopwatch.GetTimestamp();
            state.Pulses[node] = new NodePulse(now, now + MsToTicks(NodePulseDurationMs), pulseColorOverride);

            if (!state.Timer.Enabled)
                state.Timer.Start();

            InvalidateRow(treeView, node);

            if (treeView.IsHandleCreated)
            {
                treeView.BeginInvoke(new Action(() =>
                {
                    if (!treeView.IsDisposed && treeView.IsHandleCreated)
                        InvalidateRow(treeView, node);
                }));
            }
        }

        private static void Tick(State state)
        {
            if (!state.TreeRef.TryGetTarget(out var tv) || tv.IsDisposed || !tv.IsHandleCreated)
            {
                state.Pulses.Clear();
                state.Timer.Stop();
                return;
            }

            var now = Stopwatch.GetTimestamp();

            List<TreeNode>? alive = null;
            List<TreeNode>? dead = null;

            foreach (var kv in state.Pulses)
            {
                var node = kv.Key;
                var pulse = kv.Value;

                if (!ReferenceEquals(node.TreeView, tv))
                {
                    dead ??= new List<TreeNode>();
                    dead.Add(node);
                    continue;
                }

                if (now >= pulse.EndTicks)
                {
                    dead ??= new List<TreeNode>();
                    dead.Add(node);
                    continue;
                }

                alive ??= new List<TreeNode>();
                alive.Add(node);
            }

            if (alive != null)
            {
                for (int i = 0; i < alive.Count; i++)
                    InvalidateRow(tv, alive[i]);
            }

            if (dead != null)
            {
                for (int i = 0; i < dead.Count; i++)
                {
                    var n = dead[i];
                    state.Pulses.Remove(n);
                    InvalidateRow(tv, n);
                }
            }

            if (state.Pulses.Count == 0)
                state.Timer.Stop();
        }

        private static bool TryGetState(DatGrouperTreeView tv, out State state)
        {
            return StateByTree.TryGetValue(tv, out state!);
        }

        private static long MsToTicks(int ms) =>
            (long)((Stopwatch.Frequency * (double)ms) / 1000.0);

        private static bool TryGetPulseParams(State state, TreeNode node, out float intensity)
        {
            intensity = 0f;

            if (!state.Pulses.TryGetValue(node, out var pulse))
                return false;

            var now = Stopwatch.GetTimestamp();

            if (now < pulse.StartTicks || now > pulse.EndTicks)
                return false;

            var span = (double)(pulse.EndTicks - pulse.StartTicks);
            if (span <= 0)
                return false;

            var t = (now - pulse.StartTicks) / span;
            if (t < 0 || t > 1)
                return false;

            intensity = (float)Math.Sin(Math.PI * t);
            return intensity > 0.001f;
        }

        private static void InvalidateRow(DatGrouperTreeView tv, TreeNode node)
        {
            var row = GetRowRect(tv, node);
            if (!row.IsEmpty)
            {
                tv.Invalidate(row, false);
                return;
            }

            tv.Invalidate(tv.ClientRectangle, false);
        }

        private static Rectangle GetRowRect(TreeView tv, TreeNode node)
        {
            var b = node.Bounds;
            if (b.Height <= 0)
                return Rectangle.Empty;

            var client = tv.ClientRectangle;
            if (client.Width <= 0)
                return Rectangle.Empty;

            var top = Math.Max(0, b.Top);

            var bottom = b.Bottom;
            var minBottom = b.Top + Math.Max(tv.ItemHeight, 1);

            if (bottom < minBottom)
                bottom = minBottom;

            bottom = Math.Min(client.Bottom, bottom);

            if (bottom <= top)
                return Rectangle.Empty;

            return Rectangle.FromLTRB(0, top, client.Right, bottom);
        }

        private static bool TryGetNodeIconRect(TreeView tv, TreeNode node, out Rectangle iconRect)
        {
            iconRect = Rectangle.Empty;

            var il = tv.ImageList;
            if (il == null)
                return false;

            var b = node.Bounds;
            if (b.Height <= 0)
                return false;

            var sz = il.ImageSize;
            if (sz.Width <= 0 || sz.Height <= 0)
                return false;

            var x = b.Left - sz.Width - 3;
            var y = b.Top + Math.Max(0, (b.Height - sz.Height) / 2);

            iconRect = new Rectangle(x, y, sz.Width, sz.Height);
            return true;
        }

        private static void DrawWideRowHalo(DrawTreeNodeEventArgs e, Rectangle row, float intensity, Color pulseColor)
        {
            if (row.IsEmpty)
                return;

            var h = Math.Max(6, row.Height - 4);
            var y = row.Top + ((row.Height - h) / 2);

            var w = Math.Max(20, (int)((row.Width - 12) * Math.Min(1.0, 0.25 + (0.75 * intensity))));
            var x = row.Left + ((row.Width - w) / 2);

            var halo = new Rectangle(x, y, w, h);

            var a = Math.Min(255, (int)(HaloMaxAlpha * intensity));
            if (a <= 0)
                return;

            var r = h / 2;
            if (r <= 0)
                return;

            var old = e.Graphics.SmoothingMode;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using (var b = new SolidBrush(Color.FromArgb(a, pulseColor.R, pulseColor.G, pulseColor.B)))
            {
                var midW = halo.Width - (r * 2);
                if (midW > 0)
                    e.Graphics.FillRectangle(b, halo.Left + r, halo.Top, midW, halo.Height);

                e.Graphics.FillEllipse(b, halo.Left, halo.Top, r * 2, halo.Height);
                e.Graphics.FillEllipse(b, halo.Right - (r * 2), halo.Top, r * 2, halo.Height);
            }

            e.Graphics.SmoothingMode = old;
        }

        private static void DrawIconPulse(TreeView tv, DrawTreeNodeEventArgs e, float intensity, Color pulseColor)
        {
            Rectangle iconRect;

            if (!TryGetNodeIconRect(tv, e.Node, out iconRect))
                return;

            var cx = iconRect.Left + (iconRect.Width / 2);
            var cy = iconRect.Top + (iconRect.Height / 2);

            var dotAlpha = Math.Min(255, (int)(DotMaxAlpha * intensity));
            var ringAlpha = Math.Min(255, (int)(RingMaxAlpha * intensity));

            var baseRadius = Math.Max(6, iconRect.Width / 2);
            var dotRadius = baseRadius - 1 + (int)(7f * intensity);
            var ringRadius = baseRadius + 6 + (int)(16f * intensity);

            var old = e.Graphics.SmoothingMode;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using (var fill = new SolidBrush(Color.FromArgb(dotAlpha, pulseColor.R, pulseColor.G, pulseColor.B)))
                e.Graphics.FillEllipse(fill, cx - dotRadius, cy - dotRadius, dotRadius * 2, dotRadius * 2);

            using (var pen = new Pen(Color.FromArgb(ringAlpha, pulseColor.R, pulseColor.G, pulseColor.B), 2f))
                e.Graphics.DrawEllipse(pen, cx - ringRadius, cy - ringRadius, ringRadius * 2, ringRadius * 2);

            e.Graphics.SmoothingMode = old;
        }

        private static Color GetPulseColorForNode(State state, TreeView tv, TreeNode node)
        {
            if (state.Pulses.TryGetValue(node, out var pulse) && pulse.OverrideColor.HasValue)
                return pulse.OverrideColor.Value;

            var key = node.SelectedImageKey;
            if (string.IsNullOrEmpty(key))
                key = node.ImageKey;

            if (string.IsNullOrEmpty(key))
                return Color.FromArgb(255, 191, 0);

            if (state.PulseColorByImageKey.TryGetValue(key, out var cached))
                return cached;

            var il = tv.ImageList;
            if (il == null || !il.Images.ContainsKey(key))
            {
                var fallback = Color.FromArgb(255, 191, 0);
                state.PulseColorByImageKey[key] = fallback;
                return fallback;
            }

            var img = il.Images[key];

            if (img is not Bitmap bmp)
            {
                var fallback = Color.FromArgb(255, 191, 0);
                state.PulseColorByImageKey[key] = fallback;
                return fallback;
            }

            var c = ExtractDominantIconColor(bmp);
            state.PulseColorByImageKey[key] = c;
            return c;
        }

        private static Color ExtractDominantIconColor(Bitmap bmp)
        {
            var w = bmp.Width;
            var h = bmp.Height;

            if (w <= 0 || h <= 0)
                return Color.FromArgb(255, 191, 0);

            int bestScore = -1;
            Color best = Color.FromArgb(255, 191, 0);

            var cx = w / 2;
            var cy = h / 2;

            var startX = Math.Max(0, cx - 4);
            var endX = Math.Min(w - 1, cx + 4);
            var startY = Math.Max(0, cy - 4);
            var endY = Math.Min(h - 1, cy + 4);

            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    var c = bmp.GetPixel(x, y);
                    if (c.A < 40)
                        continue;

                    int max = Math.Max(c.R, Math.Max(c.G, c.B));
                    int min = Math.Min(c.R, Math.Min(c.G, c.B));
                    int sat = max - min;

                    int score = (c.A * 2) + (sat * 5);

                    if (score > bestScore)
                    {
                        bestScore = score;
                        best = Color.FromArgb(255, c.R, c.G, c.B);
                    }
                }
            }

            return best;
        }
    }
}
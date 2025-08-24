using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using Microsoft.VisualBasic;

namespace SA_DE_OM_Changer
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _timer;
        private readonly MemorySession _mem = new();
        private AddressConfig _config;

        private IntPtr _baseAddress = IntPtr.Zero;
        private IntPtr _targetAddress = IntPtr.Zero;
        private long _offset;
        private string? _version;

        private const int HOTKEY_ID = 1;
        private const uint VK_F6 = 0x75;
        private const uint WM_HOTKEY = 0x0312;

        public MainWindow()
        {
            InitializeComponent();
            _config = new AddressConfig(AppContext.BaseDirectory);

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _timer.Tick += (_, __) => Tick();
            _timer.Start();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var handle = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(handle)?.AddHook(WndProc);
            Native.RegisterHotKey(handle, HOTKEY_ID, 0, VK_F6);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            var handle = new WindowInteropHelper(this).Handle;
            Native.UnregisterHotKey(handle, HOTKEY_ID);
            _mem.Detach();
        }

        private void Tick()
        {
            if (!_mem.IsAttached)
            {
                var proc = Process.GetProcessesByName("SanAndreas").FirstOrDefault();
                if (proc != null) Attach(proc);
            }
            else
            {
                if (_mem.ProcessHasExited)
                {
                    Detach("Process exited.");
                    return;
                }

                if (_targetAddress != IntPtr.Zero && _mem.ReadByte(_targetAddress, out byte val))
                {
                    ValueText.Text = $"on_mission Value: {val}";
                    StatusText.Text = "Status: Attached. Use F6 to toggle on_mission.";
                    StatusText.Foreground = System.Windows.Media.Brushes.White;
                }
                else
                {
                    Detach("Failed to read value.");
                }
            }
        }

        private void Attach(Process proc)
        {
            if (!_mem.Attach(proc))
            {
                StatusText.Text = "Status: Failed to attach";
                StatusText.Foreground = System.Windows.Media.Brushes.OrangeRed;
                return;
            }

            try
            {
                _baseAddress = proc.MainModule?.BaseAddress ?? IntPtr.Zero;
                var exePath = proc.MainModule?.FileName ?? string.Empty;
                var verInfo = FileVersionInfo.GetVersionInfo(exePath);
                string fileVersion = string.Format("{0}.{1}.{2}.{3}", verInfo.FileMajorPart,
                                                      verInfo.FileMinorPart,
                                                      verInfo.FileBuildPart,
                                                      verInfo.FilePrivatePart);
                _version = fileVersion;

                if (!_config.TryGetOffset(_version, out _offset))
                {
                    Detach("Unsupported version (add via button).");
                    return;
                }

                _targetAddress = new IntPtr(_baseAddress.ToInt64() + _offset);
                StatusText.Text = "Status: Attached. Use F6 to toggle on_mission.";
                StatusText.Foreground = System.Windows.Media.Brushes.White;
            }
            catch (Exception ex)
            {
                Detach($"Attach error: {ex.Message}");
            }
        }

        private void Detach(string reason)
        {
            _mem.Detach();
            _baseAddress = IntPtr.Zero;
            _targetAddress = IntPtr.Zero;
            StatusText.Text = $"Status: {reason}";
            StatusText.Foreground = System.Windows.Media.Brushes.Red;
            ValueText.Text = "on_mission Value: —";
        }

        private void Toggle()
        {
            if (!_mem.IsAttached || _targetAddress == IntPtr.Zero) return;
            if (_mem.ReadByte(_targetAddress, out byte current))
            {
                byte normalized = current != 0 ? (byte)1 : (byte)0;
                byte next = normalized == 0 ? (byte)1 : (byte)0;
                _mem.WriteByte(_targetAddress, next);
                ValueText.Text = $"on_mission Value: {next}";
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                Toggle();
                handled = true;
            }
            return IntPtr.Zero;
        }

        private void AddVersion_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new AddVersionWindow { Owner = this };
            if (dlg.ShowDialog() == true)
            {
                if (_config.AddAdditionalVersion(dlg.GameVersion, dlg.AddressHex))
                {
                    MessageBox.Show($"Version {dlg.GameVersion} added.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Failed to add version (maybe already exists).", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    public sealed class MemorySession
    {
        private IntPtr _handle = IntPtr.Zero;
        private Process? _proc;

        public bool IsAttached => _handle != IntPtr.Zero;
        public bool ProcessHasExited => _proc == null || _proc.HasExited;

        public bool Attach(Process proc)
        {
            Detach();
            _proc = proc;
            _handle = Native.OpenProcess(Native.ProcessAccessFlags.All, false, proc.Id);
            return _handle != IntPtr.Zero;
        }

        public void Detach()
        {
            if (_handle != IntPtr.Zero)
            {
                Native.CloseHandle(_handle);
                _handle = IntPtr.Zero;
            }
            _proc = null;
        }

        public bool ReadByte(IntPtr addr, out byte val)
        {
            val = 0;
            var buffer = new byte[1];
            if (!IsAttached) return false;
            if (Native.ReadProcessMemory(_handle, addr, buffer, buffer.Length, out _))
            {
                val = buffer[0];
                return true;
            }
            return false;
        }

        public bool WriteByte(IntPtr addr, byte val)
        {
            if (!IsAttached) return false;
            var buffer = new[] { val };
            return Native.WriteProcessMemory(_handle, addr, buffer, buffer.Length, out _);
        }
    }

    public sealed class AddressConfig
    {
        // Hardcoded offsets – not editable
        private readonly Dictionary<string, string> _hardcoded = new(StringComparer.OrdinalIgnoreCase)
        {
            {"1.0.0.14296", "500CD78"}, // Base Version 1.0
            {"1.0.0.14388", "5010878"}, // Title Update 1.01
            {"1.0.0.14718", "501CB78"}, // Title Update 1.03
            {"1.0.0.15483", "501E838"}, // Title Update 1.04
            {"1.0.8.11827", "5095E08"}, // Title Update 1.04.5
            {"1.0.17.38838", "513003C"}, // Steam Release
            {"1.0.17.39540", "5137698"}, // Epic Release
            {"1.0.112.6680", "0x51BE148"}, // Title Update 1.112
            {"1.0.113.21181", "51BF148"}, // Steam Only 1.113 Update (released 10 minutes after 1.112)
        };

        private readonly string _customPath;
        private Dictionary<string, string> _custom = new(StringComparer.OrdinalIgnoreCase);

        public AddressConfig(string folder)
        {
            _customPath = Path.Combine(folder, "additional_addresses.json");
            LoadCustom();
        }

        private void LoadCustom()
        {
            _custom.Clear();
            if (File.Exists(_customPath))
            {
                try
                {
                    var json = File.ReadAllText(_customPath);
                    _custom = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                              ?? new(StringComparer.OrdinalIgnoreCase);
                }
                catch
                {
                    _custom = new(StringComparer.OrdinalIgnoreCase);
                }
            }
        }

        public bool TryGetOffset(string? version, out long offset)
        {
            offset = 0;
            if (string.IsNullOrWhiteSpace(version)) return false;

            // 1. exact match in hardcoded
            if (_hardcoded.TryGetValue(version, out var raw) && TryParseHex(raw, out offset))
                return true;

            // 2. exact match in custom
            if (_custom.TryGetValue(version, out raw) && TryParseHex(raw, out offset))
                return true;

            // 3. prefix match in hardcoded
            var prefix = string.Join('.', version.Split('.').Take(3));
            var kv = _hardcoded.FirstOrDefault(k => k.Key.StartsWith(prefix + ".", StringComparison.OrdinalIgnoreCase));
            if (!kv.Equals(default(KeyValuePair<string, string>)) && TryParseHex(kv.Value, out offset))
                return true;

            // 4. prefix match in custom
            kv = _custom.FirstOrDefault(k => k.Key.StartsWith(prefix + ".", StringComparison.OrdinalIgnoreCase));
            if (!kv.Equals(default(KeyValuePair<string, string>)) && TryParseHex(kv.Value, out offset))
                return true;

            return false;
        }

        public bool AddAdditionalVersion(string version, string hexAddr)
        {
            if (string.IsNullOrWhiteSpace(version) || string.IsNullOrWhiteSpace(hexAddr))
                return false;

            LoadCustom(); // reload current file

            if (_custom.ContainsKey(version))
                return false;

            // Strip '0x' prefix if present before storing
            string cleanedHexAddr = hexAddr.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                ? hexAddr.Substring(2)
                : hexAddr;
            _custom[version] = cleanedHexAddr;
            var opts = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(_customPath, JsonSerializer.Serialize(_custom, opts));

            return true;
        }

        private static bool TryParseHex(string raw, out long value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(raw)) return false;

            raw = raw.Trim();
            // Remove '0x' prefix if present
            if (raw.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                raw = raw.Substring(2);

            return long.TryParse(raw, System.Globalization.NumberStyles.HexNumber, null, out value);
        }
    }


    internal static class Native
    {
        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x1F0FFF
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(ProcessAccessFlags access, bool inherit, int pid);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr h);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr addr, [Out] byte[] buffer, int size, out IntPtr read);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr addr, byte[] buffer, int size, out IntPtr written);

        [DllImport("user32.dll")] public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")] public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }
}

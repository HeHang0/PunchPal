using PunchPal.Tools;
using SQLite.CodeFirst;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace PunchPal.Core.Models
{
    public class PunchDbContext : DbContext
    {
        private static readonly string DbPath = Path.Combine(PathTools.AppDataPath, "data.sqlite3");
        private static readonly string ConnectionText = $"Data Source={DbPath};";

        public DbSet<PunchRecord> PunchRecords { get; set; }
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
        public DbSet<AttendanceType> AttendanceTypes { get; set; }
        public DbSet<User> Users { get; set; }

        public PunchDbContext() : base(new SQLiteConnection(ConnectionText), true)
        {
            DbConfiguration.SetConfiguration(new SQLiteConfiguration());
            Database.Log = WriteDBLog;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var sqliteConnectionInitializer = new SqliteCreateDatabaseIfNotExists<PunchDbContext>(modelBuilder, true);
            Database.SetInitializer(sqliteConnectionInitializer);
            modelBuilder.Entity<PunchRecord>().Property(e => e.PunchTime).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
        }

        private void WriteDBLog(string text)
        {
        }

        private struct SYSTEM_INFO
        {
            internal ushort wProcessorArchitecture;

            private readonly ushort wReserved;

            private readonly int dwPageSize;

            private readonly IntPtr lpMinimumApplicationAddress;

            private readonly IntPtr lpMaximumApplicationAddress;

            private readonly IntPtr dwActiveProcessorMask;

            private readonly int dwNumberOfProcessors;

            private readonly int dwProcessorType;

            private readonly int dwAllocationGranularity;

            private readonly short wProcessorLevel;

            private readonly short wProcessorRevision;
        }

        public enum ProcessorArchitecture : ushort
        {
            x86 = 0,
            x64 = 9,
            ARM64 = 12,
            Unknown = ushort.MaxValue
        }

        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true, ThrowOnUnmappableChar = true, BestFitMapping = false)]
        internal static extern IntPtr LoadLibrary(string fileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern void GetSystemInfo(out SYSTEM_INFO lpSystemInfo);

        static PunchDbContext()
        {
            string loaderPath = Path.Combine(PathTools.SqlitePath, "SQLite.Interop.dll");
            if (OSVersionTools.IsWindowsNT)
            {
                GetSystemInfo(out var lpSystemInfo);
                var architecture = (ProcessorArchitecture)lpSystemInfo.wProcessorArchitecture;
                switch (architecture)
                {
                    case ProcessorArchitecture.x86:
                        File.WriteAllBytes(loaderPath, Properties.Resources.SQLiteInterop_x86);
                        break;
                    default:
                        File.WriteAllBytes(loaderPath, Properties.Resources.SQLiteInterop_x64);
                        break;
                }
            }
            if (!File.Exists(loaderPath)) return;
            var handle = LoadLibrary(loaderPath);
            if (handle == IntPtr.Zero) return;
            var type = Type.GetType("System.Data.SQLite.UnsafeNativeMethods, System.Data.SQLite");
            var fieldInfo = type?.GetField("_SQLiteNativeModuleHandle", BindingFlags.NonPublic | BindingFlags.Static);
            fieldInfo?.SetValue(null, handle);
        }
    }
}

using Newtonsoft.Json;
using PunchPal.Tools;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PunchPal.Core.ViewModels
{
    public class DataSourceModel : NotifyPropertyBase
    {
        private static readonly DataSourceModel _dataSource;

        static DataSourceModel()
        {
            try
            {
                _dataSource = JsonConvert.DeserializeObject<DataSourceModel>(File.ReadAllText(PathTools.DataSourcePath));
            }
            catch (Exception)
            {
            }
        }

        private DataSourceModel()
        {
        }

        public static DataSourceModel Load()
        {
            return _dataSource;
        }

        public override string ToString()
        {
            try
            {
                return JsonConvert.SerializeObject(this);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private CancellationTokenSource saveCts;

        public void Save()
        {
            saveCts?.Cancel();
            saveCts = new CancellationTokenSource();
            _ = SaveReal(saveCts.Token);
        }

        public async Task SaveReal(CancellationToken? token = null)
        {
            try
            {
                if (token != null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), token.Value);
                }
                File.WriteAllText(PathTools.DataSourcePath, ToString());
                saveCts?.Cancel();
            }
            catch (Exception)
            {
                // ignore
            }
        }
    }
}

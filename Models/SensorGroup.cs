using System.Collections.ObjectModel;
using System.Collections.Specialized;
using HardwareMonitorWinUI3.Core;

namespace HardwareMonitorWinUI3.Models
{
    public class SensorGroup : BaseViewModel
    {
        #region Events

        public event EventHandler<string>? ExpansionStateChanged;

        #endregion

        private string _categoryName = string.Empty;
        private string _categoryIcon = string.Empty;
        private bool _isExpanded = true;
        private string _parentHardwareName = string.Empty;

        public SensorGroup()
        {
            Sensors.CollectionChanged += OnSensorsCollectionChanged;
        }

        public string CategoryName
        {
            get => _categoryName;
            set => SetProperty(ref _categoryName, value);
        }

        public string CategoryIcon
        {
            get => _categoryIcon;
            set => SetProperty(ref _categoryIcon, value);
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (SetProperty(ref _isExpanded, value))
                {
                    var key = $"{_parentHardwareName}|{CategoryName}";
                    ExpansionStateChanged?.Invoke(this, key);
                }
            }
        }

        public string ParentHardwareName
        {
            get => _parentHardwareName;
            set => SetProperty(ref _parentHardwareName, value);
        }

        public int SensorCount => Sensors.Count;

        public ObservableCollection<SensorData> Sensors { get; } = new();

        private void OnSensorsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(SensorCount));
        }

        protected override void DisposeManaged()
        {
            Sensors.CollectionChanged -= OnSensorsCollectionChanged;
            base.DisposeManaged();
        }
    }
}

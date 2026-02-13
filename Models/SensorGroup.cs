using System.Collections.ObjectModel;
using HardwareMonitorWinUI3.Core;

namespace HardwareMonitorWinUI3.Models
{
    /// <summary>
    /// Groupe de capteurs organisés par catégorie
    /// </summary>
    public class SensorGroup : BaseViewModel
    {
        private string _categoryName = string.Empty;
        private string _categoryIcon = string.Empty;
        private bool _isExpanded = true;

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
            set => SetProperty(ref _isExpanded, value);
        }

        public ObservableCollection<SensorData> Sensors { get; } = new();
    }
} 
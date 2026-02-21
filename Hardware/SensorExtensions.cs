using System;
using System.Collections.Generic;
using LibreHardwareMonitor.Hardware;
using HardwareMonitorWinUI3.Models;

namespace HardwareMonitorWinUI3.Hardware
{
    public static class SensorExtensions
    {
        private static int _currentTemperatureUnit = (int)TemperatureUnit.Celsius;
        private static readonly WeakEventManager WeakEventManager = new();

        public static event EventHandler? TemperatureUnitChanged
        {
            add => WeakEventManager.Subscribe(value);
            remove => WeakEventManager.Unsubscribe(value);
        }

        public static TemperatureUnit CurrentTemperatureUnit
        {
            get => (TemperatureUnit)Interlocked.CompareExchange(ref _currentTemperatureUnit, 0, 0);
            set
            {
                var oldValue = (TemperatureUnit)Interlocked.CompareExchange(ref _currentTemperatureUnit, 0, 0);
                if (oldValue != value)
                {
                    Interlocked.Exchange(ref _currentTemperatureUnit, (int)value);
                    WeakEventManager.Raise(null, EventArgs.Empty);
                }
            }
        }

        private static readonly string[] SensorUnits;
        private static readonly string[] SensorPrecisions;
        private static readonly string[] SensorIcons;

        static SensorExtensions()
        {
            var maxType = (int)SensorType.Humidity + 1;
            SensorUnits = new string[maxType];
            SensorPrecisions = new string[maxType];
            SensorIcons = new string[maxType];

            Array.Fill(SensorUnits, "");
            Array.Fill(SensorPrecisions, "F1");
            Array.Fill(SensorIcons, "\uE950");

            SensorUnits[(int)SensorType.Temperature] = "\u00b0C";
            SensorUnits[(int)SensorType.Clock] = "MHz";
            SensorUnits[(int)SensorType.Voltage] = "V";
            SensorUnits[(int)SensorType.Current] = "A";
            SensorUnits[(int)SensorType.Power] = "W";
            SensorUnits[(int)SensorType.Data] = "GB";
            SensorUnits[(int)SensorType.SmallData] = "MB";
            SensorUnits[(int)SensorType.Load] = "%";
            SensorUnits[(int)SensorType.Fan] = "RPM";
            SensorUnits[(int)SensorType.Flow] = "L/h";
            SensorUnits[(int)SensorType.Control] = "%";
            SensorUnits[(int)SensorType.Level] = "%";
            SensorUnits[(int)SensorType.Frequency] = "Hz";
            SensorUnits[(int)SensorType.Throughput] = "B/s";
            SensorUnits[(int)SensorType.Timing] = "ns";
            SensorUnits[(int)SensorType.Energy] = "mWh";
            SensorUnits[(int)SensorType.Noise] = "dBA";
            SensorUnits[(int)SensorType.Conductivity] = "\u00b5S/cm";
            SensorUnits[(int)SensorType.Humidity] = "%";

            SensorPrecisions[(int)SensorType.Voltage] = "F3";
            SensorPrecisions[(int)SensorType.Current] = "F3";
            SensorPrecisions[(int)SensorType.Fan] = "F0";
            SensorPrecisions[(int)SensorType.Timing] = "F3";
            SensorPrecisions[(int)SensorType.Factor] = "F3";
            SensorPrecisions[(int)SensorType.Energy] = "F0";
            SensorPrecisions[(int)SensorType.Noise] = "F0";
            SensorPrecisions[(int)SensorType.Humidity] = "F0";
            SensorPrecisions[(int)SensorType.TimeSpan] = "g";

            SensorIcons[(int)SensorType.Temperature] = "\uE9CA";
            SensorIcons[(int)SensorType.Clock] = "\uE823";
            SensorIcons[(int)SensorType.Voltage] = "\uE945";
            SensorIcons[(int)SensorType.Current] = "\uE945";
            SensorIcons[(int)SensorType.Power] = "\uE83E";
            SensorIcons[(int)SensorType.Data] = "\uE8B7";
            SensorIcons[(int)SensorType.SmallData] = "\uE8B7";
            SensorIcons[(int)SensorType.Load] = "\uE9D9";
            SensorIcons[(int)SensorType.Fan] = "\uE71E";
            SensorIcons[(int)SensorType.Flow] = "\uE81E";
            SensorIcons[(int)SensorType.Control] = "\uE713";
            SensorIcons[(int)SensorType.Level] = "\uE9D9";
            SensorIcons[(int)SensorType.Frequency] = "\uE823";
            SensorIcons[(int)SensorType.Throughput] = "\uE8AB";
            SensorIcons[(int)SensorType.TimeSpan] = "\uE823";
            SensorIcons[(int)SensorType.Timing] = "\uE823";
            SensorIcons[(int)SensorType.Energy] = "\uE83E";
            SensorIcons[(int)SensorType.Noise] = "\uE7F4";
            SensorIcons[(int)SensorType.Conductivity] = "\uE71E";
            SensorIcons[(int)SensorType.Humidity] = "\uE759";
        }

        public static string GetSensorUnit(this SensorType type) =>
            (int)type < SensorUnits.Length ? SensorUnits[(int)type] : "";

        public static string GetSensorPrecision(this SensorType type) =>
            (int)type < SensorPrecisions.Length ? SensorPrecisions[(int)type] : "F1";

        public static string GetSensorIcon(this SensorType type) =>
            (int)type < SensorIcons.Length ? SensorIcons[(int)type] : "\uE950";

        public static string ToFormattedString(this ISensor sensor)
        {
            if (sensor is null || !sensor.Value.HasValue)
                return "N/A";

            float value = sensor.Value.Value;
            var sensorType = sensor.SensorType;

            return sensorType switch
            {
                SensorType.Temperature => FormatTemperature(value),
                SensorType.Throughput => FormatThroughput(value, sensor.Name),
                SensorType.TimeSpan => TimeSpan.FromSeconds(value).ToString("g"),
                _ => $"{value.ToString(GetSensorPrecision(sensorType))} {GetSensorUnit(sensorType)}".TrimEnd()
            };
        }

        private static string FormatTemperature(float celsius)
        {
            if (CurrentTemperatureUnit == TemperatureUnit.Fahrenheit)
            {
                float fahrenheit = celsius * 1.8f + 32f;
                return $"{fahrenheit:F1} \u00b0F";
            }
            return $"{celsius:F1} \u00b0C";
        }

        public static float CelsiusToFahrenheit(float celsius) => celsius * 1.8f + 32f;

        private static string FormatThroughput(float value, string sensorName)
        {
            const int KB = 1024;
            const int MB = 1048576;
            const int GB = 1073741824;

            if (sensorName == "Connection Speed")
            {
                return value switch
                {
                    < KB => $"{value:F0} bps",
                    < MB => $"{value / KB:F1} Kbps",
                    < GB => $"{value / MB:F1} Mbps",
                    _ => $"{value / GB:F1} Gbps"
                };
            }

            return value < MB
                ? $"{value / KB:F1} KB/s"
                : $"{value / MB:F1} MB/s";
        }

        public static string FormatThroughputValue(float value, string sensorName) =>
            FormatThroughput(value, sensorName);

        public static SensorData CreateSensorData(this ISensor sensor)
        {
            if (sensor == null)
            {
                return new SensorData
                {
                    Name = "Unknown Sensor",
                    Icon = SensorType.Load.GetSensorIcon(),
                    Value = "N/A"
                };
            }

            var sensorData = new SensorData
            {
                Name = sensor.Name ?? "Unknown Sensor",
                Icon = sensor.SensorType.GetSensorIcon(),
                SensorType = sensor.SensorType.ToString()
            };

            sensorData.Value = sensor.ToFormattedString();

            if (sensor.Value.HasValue)
            {
                var rawValue = sensor.Value.Value;
                var sensorType = sensor.SensorType;

                switch (sensorType)
                {
                    case SensorType.Throughput:
                        sensorData.UpdateMinMaxThroughput(rawValue, sensor.Name ?? "");
                        break;
                    case SensorType.TimeSpan:
                        sensorData.UpdateMinMaxTimeSpan(rawValue);
                        break;
                    case SensorType.Temperature:
                        sensorData.UpdateMinMaxTemperature(rawValue);
                        break;
                    default:
                        sensorData.UpdateMinMax(rawValue, GetSensorUnit(sensorType), GetSensorPrecision(sensorType));
                        break;
                }
            }

            return sensorData;
        }
    }

    internal sealed class WeakEventManager
    {
        private readonly List<WeakReference<EventHandler>> _handlers = new();
        private readonly object _lock = new();

        public void Subscribe(EventHandler? handler)
        {
            if (handler == null) return;
            lock (_lock)
            {
                _handlers.Add(new WeakReference<EventHandler>(handler));
                CleanupDeadReferences();
            }
        }

        public void Unsubscribe(EventHandler? handler)
        {
            if (handler == null) return;
            lock (_lock)
            {
                _handlers.RemoveAll(wr =>
                {
                    if (wr.TryGetTarget(out var existing))
                        return existing == handler;
                    return true;
                });
            }
        }

        public void Raise(object? sender, EventArgs e)
        {
            List<EventHandler> toInvoke;
            lock (_lock)
            {
                toInvoke = new List<EventHandler>(_handlers.Count);
                foreach (var wr in _handlers)
                {
                    if (wr.TryGetTarget(out var handler) && handler != null)
                    {
                        toInvoke.Add(handler);
                    }
                }
                CleanupDeadReferences();
            }

            foreach (var handler in toInvoke)
            {
                handler(sender, e);
            }
        }

        private void CleanupDeadReferences()
        {
            _handlers.RemoveAll(wr => !wr.TryGetTarget(out _));
        }
    }
}

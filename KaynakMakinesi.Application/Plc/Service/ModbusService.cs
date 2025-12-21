using System;
using System.Threading;
using System.Threading.Tasks;
using KaynakMakinesi.Core.Abstractions;
using KaynakMakinesi.Core.Logging;
using KaynakMakinesi.Core.Plc;
using KaynakMakinesi.Core.Plc.Addressing;
using KaynakMakinesi.Core.Plc.Codec;
using KaynakMakinesi.Core.Plc.Service;
using KaynakMakinesi.Core.Settings;
using ValueType = KaynakMakinesi.Core.Plc.ValueType;

namespace KaynakMakinesi.Application.Plc.Service
{
    public sealed class ModbusService : IModbusService
    {
        private readonly IPlcClient _plc;
        private readonly IAddressResolver _resolver;
        private readonly IModbusCodec _codec;
        private readonly ISettingsStore<AppSettings> _settings;
        private readonly IAppLogger _log;

        public ModbusService(IPlcClient plc, IAddressResolver resolver, IModbusCodec codec,
                             ISettingsStore<AppSettings> settings, IAppLogger log)
        {
            _plc = plc;
            _resolver = resolver;
            _codec = codec;
            _settings = settings;
            _log = log;
        }

        public async Task<bool> WriteTextAsync(string input, string valueText, CancellationToken ct)
        {
            var rr = _resolver.Resolve(input);
            if (!rr.Success) return false;

            var a = rr.Address;

            object value;

            // Bool: "1/0/true/false/on/off" hepsini kabul et
            if (a.Type == ValueType.Bool)
            {
                value = ParseBool(valueText);
                return await WriteAutoAsync(input, value, ct);
            }

            // Sayısal tipler
            if (a.Type == ValueType.UShort)
            {
                value = ushort.Parse(valueText);
                return await WriteAutoAsync(input, value, ct);
            }

            if (a.Type == ValueType.Int32)
            {
                value = int.Parse(valueText);
                return await WriteAutoAsync(input, value, ct);
            }

            if (a.Type == ValueType.Float)
            {
                // Türkçe ondalık desteği
                valueText = valueText.Replace(',', '.');
                value = float.Parse(valueText, System.Globalization.CultureInfo.InvariantCulture);
                return await WriteAutoAsync(input, value, ct);
            }

            return false;
        }

        private bool ParseBool(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            s = s.Trim().ToLowerInvariant();

            if (s == "1" || s == "true" || s == "on" || s == "yes" || s == "evet") return true;
            if (s == "0" || s == "false" || s == "off" || s == "no" || s == "hayır" || s == "hayir") return false;

            // son çare
            return bool.Parse(s);
        }


        public async Task<ModbusReadResult> ReadAutoAsync(string input, CancellationToken ct)
        {
            var rr = _resolver.Resolve(input);
            if (!rr.Success)
                return new ModbusReadResult { Success = false, Error = rr.Error };

            var a = rr.Address;
            var s = _settings.Load();
            var unit = s.Plc.UnitId;

            try
            {
                if (a.Type == ValueType.Bool)
                {
                    bool val;
                    if (a.Area == ModbusArea.Coil)
                        val = (await _plc.ReadCoilsAsync(unit, a.Start0, 1, ct))[0];
                    else if (a.Area == ModbusArea.DiscreteInput)
                        val = (await _plc.ReadDiscreteInputsAsync(unit, a.Start0, 1, ct))[0];
                    else
                        return new ModbusReadResult { Success = false, Error = "Bool sadece Coil/DI alanında okunur." };

                    return new ModbusReadResult { Success = true, Value = val, Address = a };
                }
                else
                {
                    ushort[] regs;
                    if (a.Area == ModbusArea.HoldingRegister)
                        regs = await _plc.ReadHoldingRegistersAsync(unit, a.Start0, a.Length, ct);
                    else if (a.Area == ModbusArea.InputRegister)
                        regs = await _plc.ReadInputRegistersAsync(unit, a.Start0, a.Length, ct);
                    else
                        return new ModbusReadResult { Success = false, Error = "Register okuma için alan uygun değil." };

                    var value = _codec.Decode(a.Type, regs);
                    return new ModbusReadResult { Success = true, Value = value, Address = a };
                }
            }
            catch (Exception ex)
            {
                _log.Error(nameof(ModbusService), $"ReadAuto fail input={input}", ex);
                return new ModbusReadResult { Success = false, Error = ex.Message, Address = a };
            }
        }

        public async Task<bool> WriteAutoAsync(string input, object value, CancellationToken ct)
        {
            var rr = _resolver.Resolve(input);
            if (!rr.Success) return false;

            var a = rr.Address;
            if (a.ReadOnly) return false;

            var s = _settings.Load();
            var unit = s.Plc.UnitId;

            try
            {
                if (a.Type == ValueType.Bool)
                {
                    if (a.Area != ModbusArea.Coil) return false;
                    await _plc.WriteSingleCoilAsync(unit, a.Start0, Convert.ToBoolean(value), ct);
                    return true;
                }
                else
                {
                    var regs = _codec.Encode(a.Type, value);
                    if (regs.Length == 1)
                    {
                        await _plc.WriteSingleRegisterAsync(unit, a.Start0, regs[0], ct);
                        return true;
                    }

                    await _plc.WriteMultipleRegistersAsync(unit, a.Start0, regs, ct);
                    return true;
                 
                }
            }
            catch (Exception ex)
            {
                _log.Error(nameof(ModbusService), $"WriteAuto fail input={input}", ex);
                return false;
            }
        }
    }
}

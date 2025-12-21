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
            try
            {
                var rr = _resolver.Resolve(input);
                if (!rr.Success) return false;

                var a = rr.Address;
                if (a.ReadOnly) return false;

                valueText = (valueText ?? "").Trim();

                object value;

                if (a.Type == ValueType.Bool)
                {
                    if (!TryParseBool(valueText, out var b)) return false;
                    value = b;
                    return await WriteAutoAsync(input, value, ct);
                }

                if (a.Type == ValueType.UShort)
                {
                    if (!ushort.TryParse(valueText, out var us)) return false;
                    value = us;
                    return await WriteAutoAsync(input, value, ct);
                }

                if (a.Type == ValueType.Int32)
                {
                    if (!int.TryParse(valueText, out var i)) return false;
                    value = i;
                    return await WriteAutoAsync(input, value, ct);
                }

                if (a.Type == ValueType.Float)
                {
                    // Türkçe ondalık desteği + güvenli parse
                    valueText = valueText.Replace(',', '.');
                    if (!float.TryParse(valueText, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out var f))
                        return false;

                    value = f;
                    return await WriteAutoAsync(input, value, ct);
                }

                return false;
            }
            catch (Exception ex)
            {
                _log.Error(nameof(ModbusService), $"WriteText başarısız input={input}", ex);
                return false;
            }
        }

        private bool TryParseBool(string s, out bool value)
        {
            value = false;
            if (string.IsNullOrWhiteSpace(s)) return false;

            s = s.Trim().ToLowerInvariant();
            if (s == "1" || s == "true" || s == "on" || s == "yes" || s == "evet") { value = true; return true; }
            if (s == "0" || s == "false" || s == "off" || s == "no" || s == "hayır" || s == "hayir") { value = false; return true; }

            return bool.TryParse(s, out value);
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
                    {
                        var arr = await _plc.ReadCoilsAsync(unit, a.Start0, 1, ct).ConfigureAwait(false);
                        if (arr == null || arr.Length == 0)
                        {
                            _log?.Warn(nameof(ModbusService), $"Coil okuması boş döndü: {input}");
                            return new ModbusReadResult { Success = false, Error = "PLC okuması başarısız", Address = a };
                        }
                        val = arr[0];
                    }
                    else if (a.Area == ModbusArea.DiscreteInput)
                    {
                        var arr = await _plc.ReadDiscreteInputsAsync(unit, a.Start0, 1, ct).ConfigureAwait(false);
                        if (arr == null || arr.Length == 0)
                        {
                            _log?.Warn(nameof(ModbusService), $"Discrete input okuması boş döndü: {input}");
                            return new ModbusReadResult { Success = false, Error = "PLC okuması başarısız", Address = a };
                        }
                        val = arr[0];
                    }
                    else
                        return new ModbusReadResult { Success = false, Error = "Bool sadece Coil/DI alanında okunur." };

                    return new ModbusReadResult { Success = true, Value = val, Address = a };
                }
                else
                {
                    ushort[] regs;
                    if (a.Area == ModbusArea.HoldingRegister)
                        regs = await _plc.ReadHoldingRegistersAsync(unit, a.Start0, a.Length, ct).ConfigureAwait(false);
                    else if (a.Area == ModbusArea.InputRegister)
                        regs = await _plc.ReadInputRegistersAsync(unit, a.Start0, a.Length, ct).ConfigureAwait(false);
                    else
                        return new ModbusReadResult { Success = false, Error = "Register okuma için alan uygun değil." };

                    if (regs == null || regs.Length < a.Length || regs.Length == 0)
                    {
                        _log?.Warn(nameof(ModbusService), $"Register okuması beklenenden kısa/dolu değil: {input} beklenen={a.Length} alındı={(regs==null?0:regs.Length)}");
                        return new ModbusReadResult { Success = false, Error = "PLC okuması başarısız", Address = a };
                    }

                    object value;
                    try
                    {
                        value = _codec.Decode(a.Type, regs);
                    }
                    catch (Exception ex)
                    {
                        _log?.Error(nameof(ModbusService), $"Decode hatası: {input}", ex);
                        return new ModbusReadResult { Success = false, Error = "Decode hatası: " + ex.Message, Address = a };
                    }

                    return new ModbusReadResult { Success = true, Value = value, Address = a };
                }
            }
            catch (Exception ex)
            {
                _log.Error(nameof(ModbusService), $"ReadAuto başarısız input={input}", ex);
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
                    await _plc.WriteSingleCoilAsync(unit, a.Start0, Convert.ToBoolean(value), ct).ConfigureAwait(false);
                    return true;
                }
                else
                {
                    var regs = _codec.Encode(a.Type, value);
                    if (regs.Length == 1)
                    {
                        await _plc.WriteSingleRegisterAsync(unit, a.Start0, regs[0], ct).ConfigureAwait(false);
                        return true;
                    }

                    await _plc.WriteMultipleRegistersAsync(unit, a.Start0, regs, ct).ConfigureAwait(false);
                    return true;
                 
                }
            }
            catch (Exception ex)
            {
                _log.Error(nameof(ModbusService), $"WriteAuto başarısız input={input}", ex);
                return false;
            }
        }
    }
}
